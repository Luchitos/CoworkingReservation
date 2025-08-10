using CoworkingReservation.API.Models;
using CoworkingReservation.Application.DTOs.Reservation;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoworkingReservation.API.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ICoworkingAreaRepository _areaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ReservationService(IReservationRepository reservationRepository,
                                  ICoworkingAreaRepository areaRepository,
                                  IUnitOfWork unitOfWork)
        {
            _reservationRepository = reservationRepository;
            _areaRepository = areaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ReservationBySpaceResponseDTO>> GetReservationsByCoworkingAsync(int hosterId)
        {
            var coworkingSpaces = await _unitOfWork.CoworkingSpaces
                .GetQueryable()
                .Where(cs => cs.HosterId == hosterId)
                .Include(cs => cs.Reservations)
                    .ThenInclude(r => r.User)
                .Include(cs => cs.Reservations)
                    .ThenInclude(r => r.ReservationDetails)
                        .ThenInclude(rd => rd.CoworkingArea)
                .AsNoTracking()
                .ToListAsync();

            var result = coworkingSpaces.Select(space => new ReservationBySpaceResponseDTO
            {
                CoworkingSpaceId = space.Id,
                CoworkingSpaceName = space.Name,
                Reservations = space.Reservations.Select(r => new ReservationSummaryDTO
                {
                    ReservationId = r.Id,
                    UserName = r.User?.Name + " " + r.User?.Lastname,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    Status = r.Status.ToString(),
                    AreaTypes = r.ReservationDetails
                        .Select(rd => rd.CoworkingArea.Type.ToString())
                        .Distinct()
                        .ToList()
                }).ToList()
            }).ToList();

            return result;
        }

        public async Task<object> CreateReservationAsync(CreateReservationRequest request)
        {
            try
            {
                // Log para debugging
                Console.WriteLine($"🔍 DEBUG: Método de pago recibido: {request.PaymentMethod}");
                Console.WriteLine($"🔍 DEBUG: Tipo de método de pago: {request.PaymentMethod.GetType().Name}");
                
                // Validaciones básicas
                if (request.CoworkingSpaceId <= 0)
                {
                    throw new ArgumentException("ID de espacio de coworking inválido");
                }

                if (request.AreaIds == null || !request.AreaIds.Any())
                {
                    throw new ArgumentException("Debe seleccionar al menos un área para reservar");
                }

                // Normalizar las fechas para ignorar la hora
                request.StartDate = request.StartDate.Date;
                request.EndDate = request.EndDate.Date;

                // Validaciones básicas
                if (request.StartDate > request.EndDate)
                {
                    throw new InvalidOperationException("La fecha de inicio no puede ser posterior a la fecha de fin");
                }

                if (request.StartDate < DateTime.UtcNow.Date)
                {
                    throw new InvalidOperationException("No se pueden reservar fechas en el pasado");
                }

                // 1. Verificar disponibilidad
                bool isAvailable = await _reservationRepository.CheckAvailabilityAsync(
                    request.CoworkingSpaceId,
                    request.StartDate,
                    request.EndDate,
                    request.AreaIds);

                if (!isAvailable)
                {
                    throw new InvalidOperationException("Las áreas seleccionadas no están disponibles para las fechas especificadas.");
                }
                // Obtener coworking
                var coworking = await _unitOfWork.CoworkingSpaces.GetByIdAsync(request.CoworkingSpaceId);
                if (coworking == null || coworking.Status != CoworkingStatus.Approved)
                    throw new InvalidOperationException("No se puede reservar en un coworking inactivo o no aprobado.");

                // Obtener usuario
                var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
                if (user != null && user.Role == "Hoster" && coworking.HosterId == user.Id)
                    throw new InvalidOperationException("El hoster no puede reservar en su propio coworking.");
                // 2. Obtener las áreas seleccionadas para calcular precio
                var areas = await _areaRepository.GetAreasAsync(request.AreaIds);

                // Verificar que todas las áreas existan
                if (areas.Count != request.AreaIds.Count)
                {
                    var foundAreaIds = areas.Select(a => a.Id).ToList();
                    var missingAreaIds = request.AreaIds.Where(id => !foundAreaIds.Contains(id)).ToList();

                    throw new InvalidOperationException($"No se encontraron las siguientes áreas: {string.Join(", ", missingAreaIds)}");
                }

                // Verificar que todas las áreas pertenezcan al espacio seleccionado
                var invalidAreas = areas.Where(a => a.CoworkingSpaceId != request.CoworkingSpaceId).ToList();
                if (invalidAreas.Any())
                {
                    var invalidAreaIds = invalidAreas.Select(a => a.Id).ToList();
                    throw new InvalidOperationException($"Las siguientes áreas no pertenecen al espacio seleccionado: {string.Join(", ", invalidAreaIds)}");
                }

                // 3. Calcular precio total
                var totalPrice = areas.Sum(area => area.PricePerDay);

                // 4. Crear nueva reserva
                var reservation = new Reservation
                {
                    CoworkingSpaceId = request.CoworkingSpaceId,
                    UserId = request.UserId, // Usar el ID del usuario de la solicitud
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Status = ReservationStatus.Pending, // Confirmar automáticamente por ahora
                    TotalPrice = totalPrice,
                    PaymentMethod = request.PaymentMethod, // Usar el método de pago enviado por el frontend
                    CreatedAt = DateTime.UtcNow,
                    ReservationDetails = new List<ReservationDetail>()
                };

                Console.WriteLine($"🔍 DEBUG: Método de pago asignado a la reserva: {reservation.PaymentMethod}");

                // 5. Agregar detalles de reserva
                foreach (var area in areas)
                {
                    reservation.ReservationDetails.Add(new ReservationDetail
                    {
                        CoworkingAreaId = area.Id,
                        PricePerDay = area.PricePerDay
                    });
                }

                // 6. Guardar en la base de datos
                await _reservationRepository.AddAsync(reservation);
                await _unitOfWork.SaveChangesAsync();

                Console.WriteLine($"✅ DEBUG: Reserva creada con ID {reservation.Id} y método de pago {reservation.PaymentMethod}");

                // 7. Retornar resultado
                return new
                {
                    Id = reservation.Id,
                    Message = "Reserva creada correctamente"
                };
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"❌ ERROR al crear reserva: {ex.Message}");
                throw;
            }
        }

        public async Task<object> GetReservationByIdAsync(int id)
        {
            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(id);
            if (reservation == null)
            {
                return null;
            }

            // Mapear a DTO para evitar referencias circulares
            var reservationDTO = new Models.ReservationResponseDTO
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                UserName = reservation.User?.Name + " " + reservation.User?.Lastname,
                CoworkingSpaceId = reservation.CoworkingSpaceId,
                CoworkingSpaceName = reservation.CoworkingSpace?.Name,
                StartDate = reservation.StartDate,
                EndDate = reservation.EndDate,
                Status = reservation.Status.ToString(),
                TotalPrice = reservation.TotalPrice,
                PaymentMethod = (int)reservation.PaymentMethod, // Enviar como número en lugar de string
                CreatedAt = reservation.CreatedAt,
                UpdatedAt = reservation.UpdatedAt,
                Areas = reservation.ReservationDetails.Select(rd => new ReservationAreaDTO
                {
                    Id = rd.Id,
                    CoworkingAreaId = rd.CoworkingAreaId,
                    AreaName = $"Área tipo {rd.CoworkingArea?.Type}",
                    AreaType = rd.CoworkingArea?.Type.ToString(),
                    PricePerDay = rd.PricePerDay
                }).ToList()
            };

            return reservationDTO;
        }

        public async Task<object> GetUserReservationsAsync(string userId)
        {
            if (!int.TryParse(userId, out int userIdInt))
            {
                throw new ArgumentException("ID de usuario inválido");
            }

            var reservations = await _reservationRepository.GetUserReservationsAsync(userIdInt);

            // Mapear a DTOs para evitar referencias circulares
            var reservationDTOs = reservations.Select(reservation => new Models.ReservationResponseDTO
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                UserName = reservation.User?.Name + " " + reservation.User?.Lastname,
                CoworkingSpaceId = reservation.CoworkingSpaceId,
                CoworkingSpaceName = reservation.CoworkingSpace?.Name,
                StartDate = reservation.StartDate,
                EndDate = reservation.EndDate,
                Status = reservation.Status.ToString(),
                TotalPrice = reservation.TotalPrice,
                PaymentMethod = (int)reservation.PaymentMethod, // Enviar como número en lugar de string
                CreatedAt = reservation.CreatedAt,
                UpdatedAt = reservation.UpdatedAt,
                Areas = reservation.ReservationDetails.Select(rd => new ReservationAreaDTO
                {
                    Id = rd.Id,
                    CoworkingAreaId = rd.CoworkingAreaId,
                    AreaName = $"Área tipo {rd.CoworkingArea?.Type}",
                    AreaType = rd.CoworkingArea?.Type.ToString(),
                    PricePerDay = rd.PricePerDay
                }).ToList()
            }).ToList();

            // Agrupar las reservas por estado para mejor presentación
            var result = new
            {
                Active = reservationDTOs.Where(r => r.Status == ReservationStatus.Confirmed.ToString() &&
                                                 DateTime.Parse(r.EndDate.ToString()) >= DateTime.UtcNow)
                                         .OrderBy(r => r.StartDate)
                                         .ToList(),
                Past = reservationDTOs.Where(r => r.Status == ReservationStatus.Completed.ToString() ||
                                              (r.Status == ReservationStatus.Confirmed.ToString() &&
                                               DateTime.Parse(r.EndDate.ToString()) < DateTime.UtcNow))
                                        .OrderByDescending(r => r.EndDate)
                                        .ToList(),
                Cancelled = reservationDTOs.Where(r => r.Status == ReservationStatus.Cancelled.ToString())
                                          .OrderByDescending(r => r.UpdatedAt)
                                          .ToList()
            };

            return result;
        }

        public async Task CancelReservationAsync(int id, string userId)
        {
            if (!int.TryParse(userId, out int userIdInt))
            {
                throw new ArgumentException("ID de usuario inválido");
            }

            // Obtener la reserva completa con sus detalles
            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(id);
            if (reservation == null)
            {
                throw new KeyNotFoundException("Reserva no encontrada");
            }

            // Verificar que la reserva pertenezca al usuario
            if (reservation.UserId != userIdInt)
            {
                throw new UnauthorizedAccessException("No tienes permiso para cancelar esta reserva");
            }

            // No permitir cancelar reservas ya canceladas
            if (reservation.Status == Domain.Enums.ReservationStatus.Cancelled)
            {
                throw new InvalidOperationException("La reserva ya fue cancelada previamente");
            }

            // No permitir cancelar reservas ya completadas
            if (reservation.Status == Domain.Enums.ReservationStatus.Completed)
            {
                throw new InvalidOperationException("No se puede cancelar una reserva ya completada");
            }

            // No permitir cancelar reservas cuyo EndDate ya pasó (en el pasado)
            if (reservation.EndDate < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("No se puede cancelar una reserva ya finalizada.");
            }
            // Cambiar el estado a cancelado
            reservation.Status = ReservationStatus.Cancelled;
            reservation.UpdatedAt = DateTime.UtcNow;

            // Guardar los cambios
            await _reservationRepository.UpdateAsync(reservation);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<object> CheckAvailabilityAsync(CheckAvailabilityRequest request)
        {
            // 1. Validaciones básicas

            // Normalizar las fechas para ignorar la hora
            request.StartDate = request.StartDate.Date;
            request.EndDate = request.EndDate.Date;

            // Verificar que las fechas sean válidas
            if (request.StartDate > request.EndDate)
            {
                throw new InvalidOperationException("La fecha de inicio no puede ser posterior a la fecha de fin");
            }

            // Verificar que las fechas no sean en el pasado
            if (request.StartDate < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("No se pueden reservar fechas en el pasado");
            }

            // Verificar duración máxima (opcional, ejemplo: máximo 30 días)
            int maxDurationDays = 30;
            int requestedDays = (int)(request.EndDate - request.StartDate).TotalDays + 1;
            if (requestedDays > maxDurationDays)
            {
                throw new InvalidOperationException($"La duración máxima de una reserva es de {maxDurationDays} días");
            }

            // 2. Verificar que las áreas existan y pertenezcan al espacio seleccionado
            if (request.AreaIds == null || !request.AreaIds.Any())
            {
                throw new InvalidOperationException("Debe seleccionar al menos un área para verificar disponibilidad");
            }

            var areas = await _areaRepository.GetAreasAsync(request.AreaIds);

            // Verificar que todas las áreas existan
            if (areas.Count != request.AreaIds.Count)
            {
                var foundAreaIds = areas.Select(a => a.Id).ToList();
                var missingAreaIds = request.AreaIds.Where(id => !foundAreaIds.Contains(id)).ToList();

                throw new InvalidOperationException($"No se encontraron las siguientes áreas: {string.Join(", ", missingAreaIds)}");
            }

            // Verificar que las áreas pertenezcan al espacio de coworking
            var invalidAreas = areas.Where(a => a.CoworkingSpaceId != request.CoworkingSpaceId).ToList();
            if (invalidAreas.Any())
            {
                var invalidAreaIds = invalidAreas.Select(a => a.Id).ToList();
                return new
                {
                    IsAvailable = false,
                    Message = "Una o más áreas no pertenecen al espacio de coworking",
                    InvalidAreas = invalidAreaIds
                };
            }

            // 3. Verificar disponibilidad real
            bool isAvailable = await _reservationRepository.CheckAvailabilityAsync(
                request.CoworkingSpaceId,
                request.StartDate,
                request.EndDate,
                request.AreaIds);

            // 4. Preparar respuesta detallada
            var availabilityResponse = new
            {
                IsAvailable = isAvailable,
                RequestInfo = new
                {
                    CoworkingSpaceId = request.CoworkingSpaceId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    AreaIds = request.AreaIds,
                    Days = requestedDays
                },
                Message = isAvailable
                    ? "Las áreas seleccionadas están disponibles para las fechas especificadas"
                    : "Una o más áreas no están disponibles para las fechas especificadas"
            };

            return availabilityResponse;
        }

        public async Task<UserReservationsGroupedDTO> GetUserReservationsGroupedAsync(int userId)
        {
            var reservations = await _reservationRepository.GetUserReservationsAsync(userId);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var past = reservations
                .Where(r => DateOnly.FromDateTime(r.EndDate) < today)
                .Select(MapToApplicationResponseDTO)
                .ToList();

            var currentAndFuture = reservations
                .Where(r => DateOnly.FromDateTime(r.EndDate) >= today)
                .Select(MapToApplicationResponseDTO)
                .ToList();

            return new UserReservationsGroupedDTO
            {
                PastReservations = past,
                CurrentAndFutureReservations = currentAndFuture
            };
        }

        private Models.ReservationResponseDTO MapToResponseDTO(Reservation reservation) => new Models.ReservationResponseDTO
        {
            Id = reservation.Id,
            CoworkingSpaceName = reservation.CoworkingSpace.Name,
            StartDate = reservation.StartDate,
            EndDate = reservation.EndDate,
            Status = reservation.Status.ToString(),
            TotalPrice = reservation.TotalPrice,
            PaymentMethod = (int)reservation.PaymentMethod, // Enviar como número en lugar de string
            CreatedAt = reservation.CreatedAt,
            Details = reservation.ReservationDetails.Select(d => new Models.ReservationDetailDTO
            {
                Id = d.Id,
                CoworkingAreaId = d.CoworkingAreaId,
                AreaType = d.CoworkingArea.Type.ToString(),
                PricePerDay = d.PricePerDay
            }).ToList()
        };

        private Application.DTOs.Reservation.ReservationResponseDTO MapToApplicationResponseDTO(Reservation reservation) => new Application.DTOs.Reservation.ReservationResponseDTO
        {
            Id = reservation.Id,
            CoworkingSpaceName = reservation.CoworkingSpace.Name,
            StartDate = reservation.StartDate,
            EndDate = reservation.EndDate,
            Status = reservation.Status,
            TotalPrice = reservation.TotalPrice,
            PaymentMethod = reservation.PaymentMethod,
            CreatedAt = reservation.CreatedAt,
            Details = reservation.ReservationDetails.Select(d => new Application.DTOs.Reservation.ReservationDetailDTO
            {
                Id = d.Id,
                CoworkingAreaId = d.CoworkingAreaId,
                AreaType = d.CoworkingArea.Type.ToString(),
                PricePerDay = d.PricePerDay
            }).ToList()
        };

    }
}