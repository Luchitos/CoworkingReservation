using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using CoworkingReservation.Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            var hoster = await _unitOfWork.Users.GetByIdAsync(hosterId);
            if (hoster == null || hoster.Role != "Hoster")
                throw new UnauthorizedAccessException("Only hosters can create coworking spaces");

            var coworkingSpace = new CoworkingSpace
            {
                Name = spaceDto.Name,
                Description = spaceDto.Description,
                Capacity = spaceDto.Capacity,
                PricePerDay = spaceDto.PricePerDay,
                HosterId = hosterId,
                Address = new Address
                {
                    City = spaceDto.Address.City,
                    Country = spaceDto.Address.Country,
                    Apartment = spaceDto.Address.Apartment,
                    Floor = spaceDto.Address.Floor,
                    Number = spaceDto.Address.Number,
                    Province = spaceDto.Address.Province,
                    Street = spaceDto.Address.Street,
                    ZipCode = spaceDto.Address.ZipCode
                }
            };

            await _unitOfWork.CoworkingSpaces.AddAsync(coworkingSpace);
            await _unitOfWork.SaveChangesAsync(); // Guardar espacio antes de manejar las fotos

            // Agregar fotos si hay
            await AddPhotosToCoworkingSpace(spaceDto.Photos, coworkingSpace.Id);

            return coworkingSpace;
        }

        public async Task UpdateAsync(int id, CreateCoworkingSpaceDTO dto, int hosterId)
        {
            var coworkingSpace = await _unitOfWork.CoworkingSpaces.GetByIdAsync(id, "Photos");
            if (coworkingSpace == null)
                throw new KeyNotFoundException("Coworking space not found");

            if (coworkingSpace.Id != hosterId)
                throw new UnauthorizedAccessException("You can only update your own coworking spaces.");

            // Actualizar propiedades
            coworkingSpace.Name = dto.Name;
            coworkingSpace.Description = dto.Description;
            coworkingSpace.PricePerDay = dto.PricePerDay;
            coworkingSpace.Capacity = dto.Capacity;

            coworkingSpace.Address.City = dto.Address.City;
            coworkingSpace.Address.Country = dto.Address.Country;
            coworkingSpace.Address.Apartment = dto.Address.Apartment;
            coworkingSpace.Address.Floor = dto.Address.Floor;
            coworkingSpace.Address.Number = dto.Address.Number;
            coworkingSpace.Address.Province = dto.Address.Province;
            coworkingSpace.Address.Street = dto.Address.Street;
            coworkingSpace.Address.ZipCode = dto.Address.ZipCode;

            // Eliminar fotos anteriores
            if (dto.Photos != null && dto.Photos.Count > 0)
            {
                coworkingSpace.Photos.Clear();
                await AddPhotosToCoworkingSpace(dto.Photos, coworkingSpace.Id);
            }

            await _unitOfWork.CoworkingSpaces.UpdateAsync(coworkingSpace);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task AddPhotosToCoworkingSpace(List<IFormFile> photos, int coworkingSpaceId)
        {
            if (photos != null && photos.Count > 0)
            {
                if (photos.Count > 6)
                    throw new ArgumentException("You can upload up to 6 photos only.");

                var coworkingPhotos = new List<CoworkingSpacePhoto>();

                for (int i = 0; i < photos.Count; i++)
                {
                    using var memoryStream = new MemoryStream();
                    await photos[i].CopyToAsync(memoryStream);

                    var coworkingPhoto = new CoworkingSpacePhoto
                    {
                        FileName = photos[i].FileName,
                        FilePath = Convert.ToBase64String(memoryStream.ToArray()),
                        MimeType = photos[i].ContentType,
                        UploadedAt = DateTime.UtcNow,
                        CoworkingSpaceId = coworkingSpaceId,
                        IsCoverPhoto = (i == 0) // La primera imagen será la portada
                    };

                    coworkingPhotos.Add(coworkingPhoto);
                }

                await _unitOfWork.CoworkingSpacePhotos.AddRangeAsync(coworkingPhotos);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id, int hosterId)
        {
            var coworkingSpace = await _unitOfWork.CoworkingSpaces.GetByIdAsync(id);
            if (coworkingSpace == null) throw new KeyNotFoundException("Coworking space not found");

            await _unitOfWork.CoworkingSpaces.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<CoworkingSpaceResponseDTO>> GetAllActiveSpacesAsync()
        {
            var spaces = await _unitOfWork.CoworkingSpaces
                .GetAllAsync(includeProperties: "Address,Photos");

            return spaces
                .Where(cs => cs.IsActive && cs.Status == Domain.Enums.CoworkingStatus.Approved)
                .Select(cs => new CoworkingSpaceResponseDTO
                {
                    Id = cs.Id,
                    Name = cs.Name,
                    Description = cs.Description,
                    Capacity = cs.Capacity,
                    PricePerDay = cs.PricePerDay,
                    IsActive = cs.IsActive,
                    Address = cs.Address != null ? new AddressDTO
                    {
                        City = cs.Address.City,
                        Country = cs.Address.Country,
                        Number = cs.Address.Number,
                        Province = cs.Address.Province,
                        Street = cs.Address.Street,
                        ZipCode = cs.Address.ZipCode
                    } : null,
                    Photos = cs.Photos?.Select(p => new PhotoResponseDTO
                    {
                        FileName = p.FileName,
                        IsCoverPhoto = p.IsCoverPhoto,
                        FilePath = p.FilePath,
                        ContentType = p.MimeType
                    }).ToList() ?? new List<PhotoResponseDTO>()
                })
                .ToList();
        }

        public async Task<IEnumerable<CoworkingSpaceResponseDTO>> GetAllFilteredAsync(int? capacity, string? location)
        {
            var query = _unitOfWork.CoworkingSpaces.GetFilteredQuery();

            if (capacity.HasValue)
            {
                query = query.Where(cs => cs.Capacity >= capacity.Value);
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(cs =>
                    cs.Address.City.Contains(location) ||
                    cs.Address.Province.Contains(location) ||
                    cs.Address.Street.Contains(location));
            }

            var spaces = await query.ToListAsync();

            return spaces.Select(cs => new CoworkingSpaceResponseDTO
            {
                Id = cs.Id,
                Name = cs.Name,
                Description = cs.Description,
                Capacity = cs.Capacity,
                PricePerDay = cs.PricePerDay,
                IsActive = cs.IsActive,
                Address = cs.Address != null ? new AddressDTO
                {
                    City = cs.Address.City,
                    Country = cs.Address.Country,
                    Number = cs.Address.Number,
                    Province = cs.Address.Province,
                    Street = cs.Address.Street,
                    ZipCode = cs.Address.ZipCode
                } : null,
                Photos = cs.Photos?.Select(p => new PhotoResponseDTO
                {
                    FileName = p.FileName,
                    ContentType = p.MimeType
                }).ToList() ?? new List<PhotoResponseDTO>()
            }).ToList();
        }

        public async Task<CoworkingSpaceResponseDTO> GetByIdAsync(int id)
        {
            var cs = await _unitOfWork.CoworkingSpaces
                .GetByIdAsync(id, includeProperties: "Address,Photos");

            if (cs == null) throw new KeyNotFoundException("Coworking space not found");

            return new CoworkingSpaceResponseDTO
            {
                Id = cs.Id,
                Name = cs.Name,
                Description = cs.Description,
                Capacity = cs.Capacity,
                PricePerDay = cs.PricePerDay,
                IsActive = cs.IsActive,
                Address = cs.Address != null ? new AddressDTO
                {
                    City = cs.Address.City,
                    Country = cs.Address.Country,
                    Number = cs.Address.Number,
                    Province = cs.Address.Province,
                    Street = cs.Address.Street,
                    ZipCode = cs.Address.ZipCode
                } : null,
                Photos = cs.Photos?.Select(p => new PhotoResponseDTO
                {
                    FileName = p.FileName,
                    IsCoverPhoto = p.IsCoverPhoto,
                    FilePath = p.FilePath,
                    ContentType = p.MimeType
                }).ToList() ?? new List<PhotoResponseDTO>()
            };
        }

        public async Task ToggleActiveStatusAsync(int coworkingSpaceId, int userId, string userRole)
        {
            var coworkingSpace = await _unitOfWork.CoworkingSpaces.GetByIdAsync(coworkingSpaceId);
            if (coworkingSpace == null)
                throw new KeyNotFoundException("Coworking space not found.");

            // Solo el hoster del espacio o un admin pueden cambiar el estado
            if (coworkingSpace.HosterId != userId && userRole != "Admin")
                throw new UnauthorizedAccessException("You do not have permission to perform this action.");

            // Cambiar estado
            coworkingSpace.IsActive = !coworkingSpace.IsActive;

            await _unitOfWork.CoworkingSpaces.UpdateAsync(coworkingSpace);
            await _unitOfWork.SaveChangesAsync();
        }

    }
}