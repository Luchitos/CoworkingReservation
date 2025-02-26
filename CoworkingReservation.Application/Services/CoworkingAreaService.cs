using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.DTOs;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using Microsoft.EntityFrameworkCore;

namespace CoworkingReservation.Application.Services
{
    public class CoworkingAreaService : ICoworkingAreaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CoworkingAreaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CoworkingArea> CreateAsync(CreateCoworkingAreaDTO areaDto, int coworkingSpaceId, int hosterId)
        {
            var coworkingSpace = await _unitOfWork.CoworkingSpaces.GetByIdAsync(coworkingSpaceId);
            if (coworkingSpace == null)
                throw new KeyNotFoundException("Coworking space not found");

            // Validar que el usuario sea el hoster del coworking space
            if (coworkingSpace.HosterId != hosterId)
                throw new UnauthorizedAccessException("You do not have permission to add an area to this coworking space.");
            
            var currentTotalCapacity = await GetTotalCapacityByCoworkingSpaceIdAsync(coworkingSpaceId);
            if (currentTotalCapacity + areaDto.Capacity > coworkingSpace.Capacity)
            {
                throw new InvalidOperationException("Total area capacity exceeds the coworking space's maximum capacity.");
            }
            var area = new CoworkingArea
            {
                Type = areaDto.Type,
                Description = areaDto.Description,
                Capacity = areaDto.Capacity,
                PricePerDay = areaDto.PricePerDay,
                CoworkingSpaceId = coworkingSpaceId
            };



            await _unitOfWork.CoworkingAreas.AddAsync(area);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.AuditLogs.LogAsync(new AuditLog
            {
                Action = "CreateCoworkingArea",
                UserId = hosterId,
                Success = true,
                Description = $"Coworking area created for space ID {coworkingSpaceId}."
            });

            return area;
        }

        public async Task UpdateAsync(int id, UpdateCoworkingAreaDTO dto, int hosterId)
        {
            var area = await _unitOfWork.CoworkingAreas.GetByIdAsync(id);
            if (area == null)
                throw new KeyNotFoundException("Coworking area not found");

            var coworkingSpace = await _unitOfWork.CoworkingSpaces.GetByIdAsync(area.CoworkingSpaceId);
            if (coworkingSpace.HosterId != hosterId)
                throw new UnauthorizedAccessException("You do not have permission to update this area.");

            area.Description = dto.Description;
            area.Capacity = dto.Capacity;
            area.PricePerDay = dto.PricePerDay;
            area.Type = dto.Type;

            await _unitOfWork.CoworkingAreas.UpdateAsync(area);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.AuditLogs.LogAsync(new AuditLog
            {
                Action = "UpdateCoworkingArea",
                UserId = hosterId,
                Success = true,
                Description = $"Updated coworking area ID {id}."
            });
        }

        public async Task DeleteAsync(int id, int hosterId)
        {
            var area = await _unitOfWork.CoworkingAreas.GetByIdAsync(id);
            if (area == null)
                throw new KeyNotFoundException("Coworking area not found");

            var coworkingSpace = await _unitOfWork.CoworkingSpaces.GetByIdAsync(area.CoworkingSpaceId);
            if (coworkingSpace.HosterId != hosterId)
                throw new UnauthorizedAccessException("You do not have permission to delete this area.");

            await _unitOfWork.CoworkingAreas.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.AuditLogs.LogAsync(new AuditLog
            {
                Action = "DeleteCoworkingArea",
                UserId = hosterId,
                Success = true,
                Description = $"Deleted coworking area ID {id}."
            });
        }

        public async Task<IEnumerable<CoworkingArea>> GetByCoworkingSpaceIdAsync(int coworkingSpaceId)
        {
            return await _unitOfWork.CoworkingAreas.GetByCoworkingSpaceIdAsync(coworkingSpaceId);
        }

        public async Task<CoworkingArea> GetByIdAsync(int id)
        {
            var area = await _unitOfWork.CoworkingAreas.GetByIdAsync(id);
            if (area == null)
                throw new KeyNotFoundException("Coworking area not found");
            return area;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _unitOfWork.CoworkingAreas.ExistsAsync(ca => ca.Id == id);
        }

        public async Task<IEnumerable<CoworkingArea>> GetAllAsync()
        {
            return await _unitOfWork.CoworkingAreas.GetAllAsync();
        }

        public async Task<int> GetTotalCapacityByCoworkingSpaceIdAsync(int coworkingSpaceId)
        {
            return await _unitOfWork.CoworkingAreas
                .GetQueryable()
                .Where(ca => ca.CoworkingSpaceId == coworkingSpaceId)
                .SumAsync(ca => ca.Capacity);
        }

        public async Task<bool> HasAvailableCapacity(int coworkingSpaceId, int requiredCapacity)
        {
            int totalCapacity = await GetTotalCapacityByCoworkingSpaceIdAsync(coworkingSpaceId);
            return totalCapacity >= requiredCapacity;
        }
    }
}
