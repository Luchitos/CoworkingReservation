using CoworkingReservation.Application.DTOs.Review;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using CoworkingReservation.Domain.IRepository;

namespace CoworkingReservation.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly ICoworkingSpaceRepository _coworkingSpaceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ReviewService(
            IReviewRepository reviewRepository,
            IReservationRepository reservationRepository,
            ICoworkingSpaceRepository coworkingSpaceRepository,
            IUnitOfWork unitOfWork)
        {
            _reviewRepository = reviewRepository;
            _reservationRepository = reservationRepository;
            _coworkingSpaceRepository = coworkingSpaceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CanUserReviewAsync(int userId, int reservationId)
        {
            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(reservationId);

            if (reservation == null || reservation.UserId != userId)
                return false;

            var isCompleted = reservation.Status == ReservationStatus.Completed;
            var datePassed = DateOnly.FromDateTime(reservation.EndDate) < DateOnly.FromDateTime(DateTime.UtcNow);
            var alreadyReviewed = await _reviewRepository.ExistsByReservationIdAsync(reservationId);

            return isCompleted && datePassed && !alreadyReviewed;
        }

        public async Task<ReviewValidationResult> ValidateReviewCreationAsync(int userId, int reservationId)
        {
            // Verificar que la reserva existe
            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(reservationId);
            if (reservation == null)
            {
                return ReviewValidationResult.Failure("La reserva especificada no existe.", 404);
            }

            // Verificar que la reserva pertenece al usuario
            if (reservation.UserId != userId)
            {
                return ReviewValidationResult.Failure("No tienes autorización para reseñar esta reserva.", 403);
            }

            // Verificar que la reserva está completada
            if (reservation.Status != ReservationStatus.Completed)
            {
                return ReviewValidationResult.Failure("Solo se pueden reseñar reservas completadas.", 400);
            }

            // Verificar que la fecha de finalización ya pasó
            var datePassed = DateOnly.FromDateTime(reservation.EndDate) < DateOnly.FromDateTime(DateTime.UtcNow);
            if (!datePassed)
            {
                return ReviewValidationResult.Failure("Solo se pueden reseñar reservas cuya fecha de finalización ya haya pasado.", 400);
            }

            // Verificar que no existe una reseña previa para esta reserva
            var alreadyReviewed = await _reviewRepository.ExistsByReservationIdAsync(reservationId);
            if (alreadyReviewed)
            {
                return ReviewValidationResult.Failure("Ya existe una reseña para esta reserva.", 400);
            }

            return ReviewValidationResult.Success();
        }

        public async Task<int> CreateReviewAsync(CreateReviewRequest request, int userId)
        {
            // Validar antes de crear
            var validationResult = await ValidateReviewCreationAsync(userId, request.ReservationId);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException(validationResult.ErrorMessage);
            }

            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(request.ReservationId);
            
            // Esta validación ya se hizo arriba, pero por seguridad
            if (reservation == null)
                throw new KeyNotFoundException("Reserva no encontrada.");

            if (reservation.UserId != userId)
                throw new UnauthorizedAccessException("No autorizado para crear reseña para esta reserva.");

            // Crear la reseña
            var review = new Review
            {
                UserId = userId,
                CoworkingSpaceId = reservation.CoworkingSpaceId,
                ReservationId = reservation.Id,
                Rating = request.Rating,
                Comment = request.Comment?.Trim(), // Limpiar espacios en blanco
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);

            // Guardar primero la reseña
            await _unitOfWork.SaveChangesAsync();
            
            // 🆕 Ahora actualizar el rating promedio del coworking space
            await UpdateCoworkingSpaceRatingAsync(reservation.CoworkingSpaceId);
            
            // Guardar la actualización del rating
            await _unitOfWork.SaveChangesAsync();

            return review.Id;
        }

        public async Task<CoworkingReviewResponse> GetReviewsByCoworkingSpaceAsync(int coworkingSpaceId)
        {
            var reviews = await _reviewRepository.GetReviewsByCoworkingSpaceAsync(coworkingSpaceId);

            var response = new CoworkingReviewResponse
            {
                CoworkingSpaceId = coworkingSpaceId,
                TotalReviews = reviews.Count,
                AverageRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0,
                Reviews = reviews.Select(r => new ReviewItemDto
                {
                    UserName = $"{r.User?.Name ?? "Usuario"} {r.User?.Lastname ?? ""}".Trim(),
                    Rating = r.Rating,
                    Comment = r.Comment ?? "",
                    CreatedAt = r.CreatedAt
                }).ToList()
            };

            return response;
        }

        /// <summary>
        /// Obtiene una reseña específica por usuario y reserva.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="reservationId">ID de la reserva</param>
        /// <returns>La reseña si existe, null si no existe</returns>
        public async Task<ReviewResponseDTO?> GetReviewByUserAndReservationAsync(int userId, int reservationId)
        {
            // Primero verificar que la reserva existe y pertenece al usuario
            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(reservationId);
            if (reservation == null)
            {
                throw new KeyNotFoundException("La reserva especificada no existe.");
            }

            if (reservation.UserId != userId)
            {
                throw new UnauthorizedAccessException("No tienes autorización para acceder a esta reserva.");
            }

            // Buscar la reseña usando el método específico del repositorio
            var review = await _reviewRepository.GetByUserAndReservationAsync(userId, reservationId);

            if (review == null)
            {
                return null;
            }

            return new ReviewResponseDTO
            {
                Id = review.Id,
                UserId = review.UserId,
                CoworkingSpaceId = review.CoworkingSpaceId,
                ReservationId = review.ReservationId,
                Rating = review.Rating,
                Comment = review.Comment ?? "",
                CreatedAt = review.CreatedAt,
                UserName = $"{review.User?.Name ?? "Usuario"} {review.User?.Lastname ?? ""}".Trim(),
                CoworkingSpaceName = review.CoworkingSpace?.Name ?? "Espacio de coworking"
            };
        }

        /// <summary>
        /// Calcula y actualiza el rating promedio de un espacio de coworking
        /// basado en todas sus reseñas existentes.
        /// </summary>
        /// <param name="coworkingSpaceId">ID del espacio de coworking</param>
        private async Task UpdateCoworkingSpaceRatingAsync(int coworkingSpaceId)
        {
            try
            {
                // Obtener todas las reseñas del coworking space (incluirá la nueva reseña que se acaba de agregar)
                var reviews = await _reviewRepository.GetReviewsByCoworkingSpaceAsync(coworkingSpaceId);
                
                // Calcular el promedio
                float averageRating = 0;
                if (reviews.Count > 0)
                {
                    averageRating = (float)reviews.Average(r => r.Rating);
                    // Redondear a 1 decimal para tener mejor presentación
                    averageRating = (float)Math.Round(averageRating, 1);
                }

                // Actualizar el rating en la tabla CoworkingSpace (sin guardar aún)
                await _coworkingSpaceRepository.UpdateRatingAsync(coworkingSpaceId, averageRating);
                
                Console.WriteLine($"✅ Rating calculado para CoworkingSpace {coworkingSpaceId}: {averageRating} (basado en {reviews.Count} reseñas)");
            }
            catch (Exception ex)
            {
                // Log del error pero no fallar la creación de la reseña
                Console.WriteLine($"❌ Error al actualizar rating para CoworkingSpace {coworkingSpaceId}: {ex.Message}");
                // Re-lanzar la excepción para que se maneje apropiadamente
                throw new InvalidOperationException($"Error al actualizar rating del espacio de coworking: {ex.Message}", ex);
            }
        }

    }
}
