using CoworkingReservation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Domain.IRepository
{
    public interface ICoworkingSpaceRepository : IRepository<CoworkingSpace>
    {
        Task<IEnumerable<CoworkingSpace>> GetActiveSpacesAsync();
        // Task<IEnumerable<CoworkingSpace>> GetAllFilteredAsync(int? capacity, string? location);
        IQueryable<CoworkingSpace> GetFilteredQuery();
        IQueryable<CoworkingSpace> GetQueryable(string includeProperties = "");
        Task<bool> ExistsAsync(Expression<Func<CoworkingSpace, bool>> predicate);
        // Task<IEnumerable<CoworkingSpace>> GetFilteredLightweightAsync(int? capacity, string? location, int? userId = null);
        // Task<IEnumerable<CoworkingSpace>> GetAllLightweightByIdsAsync(IEnumerable<int> ids, int? userId = null);
        Task UpdateRatingAsync(int coworkingSpaceId, float newRating);
    }
}
