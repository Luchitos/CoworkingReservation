using CoworkingReservation.Application.DTOs.Reservation;
using CoworkingReservation.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoworkingReservation.Application.Services.Interfaces
{
    public interface IReservationService
    {
        Task<ReservationResponseDTO> CreateReservationAsync(CreateReservationDTO dto, int userId);
        Task<ReservationResponseDTO> GetReservationByIdAsync(int id, int userId);
        Task<IEnumerable<ReservationResponseDTO>> GetUserReservationsAsync(int userId);
        Task<ReservationResponseDTO> CancelReservationAsync(int id, int userId);
        Task<bool> CheckAvailabilityAsync(int coworkingSpaceId, DateTime startDate, DateTime endDate, List<int> areaIds);
        Task<decimal> CalculateTotalPriceAsync(int coworkingSpaceId, List<int> areaIds, DateTime startDate, DateTime endDate);
    }
}
