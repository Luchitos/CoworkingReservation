using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public IQueryable<CoworkingArea> GetQueryable(string includeProperties = "")
        {
            IQueryable<CoworkingArea> query = _dbSet;
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }
            return query;
        }

        public async Task<bool> ExistsAsync(Expression<Func<CoworkingArea, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
        
        public async Task<List<CoworkingArea>> GetAreasAsync(List<int> areaIds)
        {
            return await _dbSet
                .Where(a => areaIds.Contains(a.Id))
                .ToListAsync();
        }
    }
}
