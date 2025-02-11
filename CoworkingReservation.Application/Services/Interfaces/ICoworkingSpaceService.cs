using CoworkingReservation.Application.DTOs.CoworkingSpace;
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
        Task<IEnumerable<CoworkingSpace>> GetAllActiveSpacesAsync();

        //Task<IEnumerable<CoworkingSpace>> GetAllAsync();
        Task<CoworkingSpace> GetByIdAsync(int id);
        Task<CoworkingSpace> CreateAsync(CreateCoworkingSpaceDTO space, int hosterId);
        Task UpdateAsync(int id, CreateCoworkingSpaceDTO dto, int hosterId);
        Task DeleteAsync(int id, int hosterId);

    }
}