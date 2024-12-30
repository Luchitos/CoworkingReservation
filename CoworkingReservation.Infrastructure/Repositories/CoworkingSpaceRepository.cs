using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Infrastructure.Repositories
{
    public class CoworkingSpaceRepository : Repository<CoworkingSpace>, ICoworkingSpaceRepository
    {
        public CoworkingSpaceRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<CoworkingSpace>> GetActiveSpacesAsync()
        {
            return await _dbSet.Where(cs => cs.IsActive).ToListAsync();
        }
    }
}