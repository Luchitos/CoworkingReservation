using CoworkingReservation.Domain.DTOs;
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
        Task<IEnumerable<CoworkingSpaceResponseDTO>> GetAllFilteredAsync(int? capacity, string? location);
        IQueryable<CoworkingSpace> GetFilteredQuery();
        IQueryable<CoworkingSpace> GetQueryable(string includeProperties = "");
        Task<bool> ExistsAsync(Expression<Func<CoworkingSpace, bool>> predicate);
        Task<IEnumerable<CoworkingSpaceListItemDTO>> GetFilteredLightweightAsync(int? capacity, string? location);
        IQueryable<CoworkingSpace> GetAll();


    }
}
