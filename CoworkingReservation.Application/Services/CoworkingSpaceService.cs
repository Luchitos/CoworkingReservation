using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;

namespace CoworkingReservation.Application.Services
{
    public class CoworkingSpaceService : ICoworkingSpaceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CoworkingSpaceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<CoworkingSpace> CreateAsync(CreateCoworkingSpaceDTO spaceDto, int hosterId)
        {
            var hoster = await _unitOfWork.CoworkingSpaces.GetByIdAsync(hosterId);
            if (hoster == null) throw new UnauthorizedAccessException("Only hosters can create coworking spaces");

            var coworkingSpace = new CoworkingSpace
            {
                Name = spaceDto.Name,
                Description = spaceDto.Description,
                Capacity = spaceDto.Capacity,
                AddressId = spaceDto.AddressId,
                Id = hosterId,
                PricePerDay = spaceDto.PricePerDay
            };

            await _unitOfWork.CoworkingSpaces.AddAsync(coworkingSpace);
            await _unitOfWork.SaveChangesAsync();
            return coworkingSpace;
        }

        public async Task DeleteAsync(int id, int hosterId)
        {
            var coworkingSpace = await _unitOfWork.CoworkingSpaces.GetByIdAsync(id);

            if (coworkingSpace == null) throw new UnauthorizedAccessException("Coworking space not found");

            if (coworkingSpace.Id != hosterId) throw new UnauthorizedAccessException("You can only delete your own coworking spaces.");

            await _unitOfWork.CoworkingSpaces.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<CoworkingSpace>> GetAllActiveSpacesAsync()
        {
            return await _unitOfWork.CoworkingSpaces.GetAllAsync();
        }

        public async Task<CoworkingSpace> GetByIdAsync(int id)
        {
            return await _unitOfWork.CoworkingSpaces.GetByIdAsync(id);
        }

        public async Task UpdateAsync(int id, CreateCoworkingSpaceDTO dto, int hosterId)
        {
            var coworkingSpace = await _unitOfWork.CoworkingSpaces.GetByIdAsync(id);
            if (coworkingSpace == null)
                throw new KeyNotFoundException("Coworking space not found");

            if(coworkingSpace.Id != hosterId) throw new UnauthorizedAccessException("You can only update your own coworking spaces.");

            coworkingSpace.Name = dto.Name;
            coworkingSpace.Description = dto.Description;
            coworkingSpace.PricePerDay = dto.PricePerDay;
            coworkingSpace.AddressId = dto.AddressId;
            coworkingSpace.Capacity = dto.Capacity;

            _unitOfWork.CoworkingSpaces.UpdateAsync(coworkingSpace);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
