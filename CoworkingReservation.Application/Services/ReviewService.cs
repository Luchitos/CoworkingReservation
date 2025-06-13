using System;
using System.Threading.Tasks;
using CoworkingReservation.Application.DTOs.Review;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.UnitOfWork;

namespace CoworkingReservation.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ReviewService(
            IReviewRepository reviewRepository,
            IReservationRepository reservationRepository,
            IUnitOfWork unitOfWork)
        {
            _reviewRepository = reviewRepository;
            _reservationRepository = reservationRepository;
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

        public async Task CreateReviewAsync(CreateReviewRequest request, int userId)
        {
            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(request.ReservationId);

            if (reservation == null || reservation.UserId != userId)
                throw new UnauthorizedAccessException("No se puede crear una review para esta reserva.");

            var review = new Review
            {
                UserId = userId,
                CoworkingSpaceId = reservation.CoworkingSpaceId,
                ReservationId = reservation.Id,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();
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
                    UserName = $"{r.User.Name} {r.User.Lastname}",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).ToList()
            };

            return response;
        }

    }
}
