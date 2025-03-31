using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Domain.IRepository
{
    public interface ICoworkingAvailabilityRepository : IRepository<CoworkingAvailability>
    {
        Task<IEnumerable<CoworkingAvailability>> GetAvailabilityByCoworkingSpaceIdAsync(int coworkingSpaceId);

        Task<CoworkingAvailability> FindAsync(int coworkingAreaId, DateTime date);
    }
}
