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
    public class SpecialFeatureRepository : ISpecialFeatureRepository
    {
        private readonly ApplicationDbContext _context;

        public SpecialFeatureRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task AddAsync(SpecialFeature entity)
        {
            throw new NotImplementedException();
        }

        public Task AddRangeAsync(IEnumerable<SpecialFeature> entities)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<SpecialFeature>> GetAllAsync()
        {
            return await _context.SpecialFeatures.AsNoTracking().ToListAsync();
        }

        public Task<IEnumerable<SpecialFeature>> GetAllAsync(string includeProperties = "")
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<SpecialFeature>> GetAllAsync(Expression<Func<SpecialFeature, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public Task<SpecialFeature> GetByIdAsync(int id, string includeProperties = "")
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(SpecialFeature entity)
        {
            throw new NotImplementedException();
        }
    }
}