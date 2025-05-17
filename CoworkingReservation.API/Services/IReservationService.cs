using CoworkingReservation.API.Models;
using System.Threading.Tasks;

namespace CoworkingReservation.API.Services
{
    public interface IReservationService
    {
        Task<object> CreateReservationAsync(CreateReservationRequest request);
        Task<object> GetReservationByIdAsync(int id);
        Task<object> GetUserReservationsAsync(string userId);
        Task CancelReservationAsync(int id, string userId);
        Task<object> CheckAvailabilityAsync(CheckAvailabilityRequest request);
    }
} 