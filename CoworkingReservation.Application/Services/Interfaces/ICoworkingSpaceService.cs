using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Application.Services.Interfaces
{
    public interface ICoworkingSpaceService
    {
        Task<IEnumerable<CoworkingSpaceResponseDTO>> GetAllActiveSpacesAsync();
        Task<CoworkingSpaceResponseDTO> GetByIdAsync(int id);
        Task<IEnumerable<CoworkingSpaceResponseDTO>> GetAllFilteredAsync(int? capacity, string? location);
        Task<CoworkingSpaceResponseDTO> CreateAsync(CreateCoworkingSpaceDTO spaceDto, int hosterId);
        Task UpdateAsync(int id, UpdateCoworkingSpaceDTO dto, int hosterId, string userRole);
        Task DeleteAsync(int id, int hosterId);
        Task ToggleActiveStatusAsync(int coworkingSpaceId, int userId, string userRole);

        Task<IEnumerable<CoworkingSpaceSummaryDTO>> GetAllSummariesAsync();
        Task<IEnumerable<CoworkingSpaceSummaryDTO>> GetFilteredSummariesAsync(int? capacity, string? location);

        Task<IEnumerable<CoworkingSpaceListItemDTO>> GetFilteredLightweightAsync(int? capacity, string? location, int? userId = null);
        Task<IEnumerable<CoworkingSpaceListItemDTO>> GetAllLightweightAsync(int? userId = null);
        /// <summary>
        /// Obtiene todos los espacios de coworking creados por un hoster.
        /// </summary>
        /// <param name="hosterId">ID del hoster.</param>
        /// <returns>Lista de coworkings creados.</returns>
        Task<IEnumerable<CoworkingSpace>> GetByHosterAsync(int hosterId);
        Task<SpaceFilterResponseDTO> GetAdvancedFilteredAsync(
            int? capacity,
            string? location,
            DateTime? date,
            decimal? minPrice,
            decimal? maxPrice,
            bool? individualDesk,
            bool? privateOffice,
            bool? hybridSpace,
            List<string> services,
            List<string> benefits,
            int? userId = null);

    }
}

//Task<IEnumerable<CoworkingSpaceResponseDTO>> GetAllLightFilteredAsync(int? capacity, string? location);
//public Task<IEnumerable<CoworkingSpaceLightDTO>> GetAllLightweightAsync();