using CoworkingReservation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoworkingReservation.Domain.IRepository
{
    public interface IReservationRepository : IRepository<Reservation>
    {
        Task<IEnumerable<Reservation>> GetUserReservationsAsync(int userId);
        Task<bool> CheckAvailabilityAsync(int coworkingSpaceId, DateTime startDate, DateTime endDate, List<int> areaIds);
        Task<Reservation> GetByIdWithDetailsAsync(int id);
    }
}
