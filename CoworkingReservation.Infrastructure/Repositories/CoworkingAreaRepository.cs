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
    public class CoworkingAreaRepository : Repository<CoworkingArea>, ICoworkingAreaRepository
    {
        public CoworkingAreaRepository(ApplicationDbContext context) : base(context) { }

        public async Task<List<CoworkingArea>> GetByCoworkingSpaceIdAsync(int coworkingSpaceId)
        {
            return await _dbSet.Where(ca => ca.CoworkingSpaceId == coworkingSpaceId)
                               .Include(ca => ca.Availabilities)
                               .ToListAsync();
        }
    }
}
