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

        /// <summary>
        /// Obtiene todas las reservas agrupadas por coworking space del hoster.
        /// </summary>
        /// <param name="hosterId">ID del hoster autenticado.</param>
        /// <returns>Reservas agrupadas por espacio.</returns>
        Task<List<ReservationBySpaceResponseDTO>> GetReservationsByCoworkingAsync(int hosterId);

                /// <summary>
        /// Obtiene las reservas del usuario agrupadas en pasadas y actuales/futuras.
        /// </summary>
        /// <param name="userId">ID del usuario autenticado</param>
        /// <returns>Objeto con dos listas de reservas</returns>
        Task<UserReservationsGroupedDTO> GetUserReservationsGroupedAsync(int userId);

    }
} 