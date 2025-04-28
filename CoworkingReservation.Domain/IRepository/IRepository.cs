using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Domain.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(string includeProperties = "");
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter);

        Task<T> GetByIdAsync(int id, string includeProperties = "");
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task RemoveAsync(T entity);
        Task RemoveRangeAsync(IEnumerable<T> entities);
    }
}
