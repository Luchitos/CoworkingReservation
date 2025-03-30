using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoworkingReservation.Infrastructure.Repositories
{
    public class CoworkingAvailabilityRepository : Repository<CoworkingAvailability>, ICoworkingAvailabilityRepository
    {
        public CoworkingAvailabilityRepository(ApplicationDbContext context) : base(context) { }

        public async Task<CoworkingAvailability> GetAvailabilityAsync(int coworkingAreaId, DateTime date)
        {
            return await _dbSet.FirstOrDefaultAsync(ca => ca.CoworkingAreaId == coworkingAreaId && ca.Date == date);
        }

        public async Task<IEnumerable<CoworkingAvailability>> GetAvailabilityByCoworkingSpaceIdAsync(int coworkingSpaceId)
        {
            return await _dbSet
                .Include(a => a.CoworkingArea) // Incluir la relación con CoworkingArea
                .Where(a => a.CoworkingArea.CoworkingSpaceId == coworkingSpaceId) // Acceder a CoworkingSpaceId a través de CoworkingArea
                .ToListAsync();
        }

        public async Task<CoworkingAvailability> FindAsync(int coworkingAreaId, DateTime date)
        {
            return await _dbSet.FirstOrDefaultAsync(ca => ca.CoworkingAreaId == coworkingAreaId && ca.Date == date);
        }
    }
}

