using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Domain.DTOs;
using CoworkingReservation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Application.Services.Interfaces
{
    public interface ICoworkingSpaceService
    {
        Task<IEnumerable<CoworkingSpaceResponseDTO>> GetAllActiveSpacesAsync();
        Task<CoworkingSpaceResponseDTO> GetByIdAsync(int id);
        Task<IEnumerable<CoworkingSpaceResponseDTO>> GetAllFilteredAsync(int? capacity, string? location);
        Task<CoworkingSpace> CreateAsync(CreateCoworkingSpaceDTO spaceDto, int hosterId);
        Task UpdateAsync(int id, UpdateCoworkingSpaceDTO dto, int hosterId, string userRole);
        Task DeleteAsync(int id, int hosterId);
        Task ToggleActiveStatusAsync(int coworkingSpaceId, int userId, string userRole);

        Task<IEnumerable<CoworkingSpaceSummaryDTO>> GetAllSummariesAsync();
        Task<IEnumerable<CoworkingSpaceSummaryDTO>> GetFilteredSummariesAsync(int? capacity, string? location);
        //Task<IEnumerable<CoworkingSpaceResponseDTO>> GetAllLightFilteredAsync(int? capacity, string? location);
        //public Task<IEnumerable<CoworkingSpaceLightDTO>> GetAllLightweightAsync();

        Task<IEnumerable<CoworkingSpaceListItemDTO>> GetAllLightweightAsync();
        Task<IEnumerable<CoworkingSpaceListItemDTO>> GetFilteredLightweightAsync(int? capacity, string? location);


    }
}