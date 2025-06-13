using System.Threading.Tasks;
using CoworkingReservation.Application.DTOs.Review;

namespace CoworkingReservation.Application.Services.Interfaces
{
    public interface IReviewService
    {
        Task<bool> CanUserReviewAsync(int userId, int reservationId);
        Task CreateReviewAsync(CreateReviewRequest request, int userId);
        Task<CoworkingReviewResponse> GetReviewsByCoworkingSpaceAsync(int coworkingSpaceId);

    }
}
