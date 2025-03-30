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
    public class SafetyElementRepository : ISafetyElementRepository
    {
        private readonly ApplicationDbContext _context;

        public SafetyElementRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task AddAsync(SafetyElement entity)
        {
            throw new NotImplementedException();
        }

        public Task AddRangeAsync(IEnumerable<SafetyElement> entities)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<SafetyElement>> GetAllAsync()
        {
            return await _context.SafetyElements.AsNoTracking().ToListAsync();
        }

        public Task<IEnumerable<SafetyElement>> GetAllAsync(string includeProperties = "")
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<SafetyElement>> GetAllAsync(Expression<Func<SafetyElement, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public Task<SafetyElement> GetByIdAsync(int id, string includeProperties = "")
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(SafetyElement entity)
        {
            throw new NotImplementedException();
        }
    }
}