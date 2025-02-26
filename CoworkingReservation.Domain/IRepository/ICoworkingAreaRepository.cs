using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Domain.IRepository
{
    public interface ICoworkingAreaRepository : IRepository<CoworkingArea>
    {
        Task<List<CoworkingArea>> GetByCoworkingSpaceIdAsync(int coworkingSpaceId);
        Task<bool> ExistsAsync(Expression<Func<CoworkingArea, bool>> predicate);
        IQueryable<CoworkingArea> GetQueryable(string includeProperties = "");
    }
}
