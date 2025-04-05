using CoworkingReservation.Application.DTOs.Address;
using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Application.DTOs.Photo;
using CoworkingReservation.Application.Jobs;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.DTOs;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using CoworkingReservation.Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using AddressDTO = CoworkingReservation.Application.DTOs.Address.AddressDTO;
using CoworkingSpaceResponseDTO = CoworkingReservation.Application.DTOs.CoworkingSpace.CoworkingSpaceResponseDTO;
using CoworkingSpaceSummaryDTO = CoworkingReservation.Application.DTOs.CoworkingSpace.CoworkingSpaceSummaryDTO;
using PhotoResponseDTO = CoworkingReservation.Application.DTOs.Photo.PhotoResponseDTO;

namespace CoworkingReservation.Application.Services
{
    public class CoworkingSpaceService : ICoworkingSpaceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICoworkingAreaService _coworkingAreaService;
        private readonly CoworkingApprovalJob _approvalJob;

        public CoworkingSpaceService(IUnitOfWork unitOfWork, CoworkingApprovalJob approvalJob, ICoworkingAreaService coworkingAreaService)
        {
            _unitOfWork = unitOfWork;
            _approvalJob = approvalJob;
            _coworkingAreaService = coworkingAreaService;
        }

        public async Task<CoworkingSpace> CreateAsync(CreateCoworkingSpaceDTO spaceDto, int userId)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new UnauthorizedAccessException("User not found.");
                
                bool hasPendingCoworking = await _unitOfWork.CoworkingSpaces.ExistsAsync(
                    c => c.HosterId == userId && c.Status == CoworkingStatus.Pending
                );

                if (hasPendingCoworking)
                    throw new InvalidOperationException("You already have a pending coworking space approval.");

                bool userWasClient = user.Role != "Hoster";
                if (userWasClient)
                {
                    user.Role = "Hoster";
                    await _unitOfWork.Users.UpdateAsync(user);
                }

                var coworkingSpace = new CoworkingSpace
                {
                    Name = spaceDto.Title,
                    Description = spaceDto.Description,
                    CapacityTotal = spaceDto.CapacityTotal,
                    HosterId = userId,
                    Status = CoworkingStatus.Pending,
                    Rate = spaceDto.Rate,
                    Address = new Address
                    {
                        City = spaceDto.Address.City,
                        Country = spaceDto.Address.Country,
                        Apartment = spaceDto.Address.Apartment,
                        Floor = spaceDto.Address.Floor,
                        Number = spaceDto.Address.Number,
                        Province = spaceDto.Address.Province,
                        Street = spaceDto.Address.Street,
                        ZipCode = spaceDto.Address.ZipCode,
                        Latitude = spaceDto.Address.Latitude,
                        Longitude = spaceDto.Address.Longitude,
                    }
                };

                bool addressExists = await _unitOfWork.Addresses.ExistsAsync(
                    a => a.Street == spaceDto.Address.Street &&
                         a.Number == spaceDto.Address.Number &&
                         a.City == spaceDto.Address.City &&
                         a.Province == spaceDto.Address.Province
                );

                if (addressExists)
                    throw new InvalidOperationException("A coworking space with this address already exists.");

                if (spaceDto.ServiceIds?.Any() == true)
                {
                    coworkingSpace.Services = (await _unitOfWork.Services
                        .GetAllAsync(s => spaceDto.ServiceIds.Contains(s.Id))).ToList();
                }

                if (spaceDto.BenefitIds?.Any() == true)
                {
                    coworkingSpace.Benefits = (await _unitOfWork.Benefits
                        .GetAllAsync(b => spaceDto.BenefitIds.Contains(b.Id))).ToList();
                }

                if (spaceDto.SafetyElementsIds?.Any() == true)
                {
                    coworkingSpace.SafetyElements = (await _unitOfWork.SafetyElements
                        .GetAllAsync(b => spaceDto.SafetyElementsIds.Contains(b.Id))).ToList();
                }
                if (spaceDto.SpeacialFeatureIds?.Any() == true)
                {
                    coworkingSpace.SpecialFeatures = (await _unitOfWork.SpecialFeatures
                        .GetAllAsync(b => spaceDto.SafetyElementsIds.Contains(b.Id))).ToList();
                }

                await _unitOfWork.CoworkingSpaces.AddAsync(coworkingSpace);
                await _unitOfWork.SaveChangesAsync();

                await AddPhotosToCoworkingSpace(spaceDto.Photos, coworkingSpace.Id);
                // Nueva línea: agregar áreas con servicio externo
                if (spaceDto.Areas?.Any() == true)
                {
                    await _coworkingAreaService.AddAreasToCoworkingAsync(spaceDto.Areas, coworkingSpace.Id, userId);
                }
                await transaction.CommitAsync();

                _ = Task.Run(async () => await _approvalJob.Run());
                return coworkingSpace;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateAsync(int id, UpdateCoworkingSpaceDTO dto, int hosterId, string userRole)
        {
            var coworkingSpace = await _unitOfWork.CoworkingSpaces
                .GetByIdAsync(id, "Address,Photos,Services,Benefits");

            if (coworkingSpace == null)
            {
                await _unitOfWork.AuditLogs.LogAsync(new AuditLog
                {
                    Action = "UpdateCoworkingSpace",
                    UserId = hosterId,
                    UserRole = userRole,
                    Success = false,
                    Description = $"Coworking space with ID {id} not found."
                });

                throw new KeyNotFoundException("Coworking space not found.");
            }

            // Validar permisos (Solo el hoster del espacio o un Admin pueden modificarlo)
            bool isAdmin = userRole == "Admin";
            if (coworkingSpace.HosterId != hosterId && !isAdmin)
            {
                await _unitOfWork.AuditLogs.LogAsync(new AuditLog
                {
                    Action = "UpdateCoworkingSpace",
                    UserId = hosterId,
                    UserRole = userRole,
                    Success = false,
                    Description = $"User {hosterId} attempted to modify coworking space {id} without permission."
                });

                throw new UnauthorizedAccessException("You do not have permission to perform this action.");
            }

            // **Actualizar propiedades principales**
            coworkingSpace.Name = dto.Name;
            coworkingSpace.Description = dto.Description;
            coworkingSpace.CapacityTotal = dto.CapacityTotal;

            // **Actualizar Dirección**
            coworkingSpace.Address.City = dto.Address.City;
            coworkingSpace.Address.Country = dto.Address.Country;
            coworkingSpace.Address.Apartment = dto.Address.Apartment;
            coworkingSpace.Address.Floor = dto.Address.Floor;
            coworkingSpace.Address.Number = dto.Address.Number;
            coworkingSpace.Address.Province = dto.Address.Province;
            coworkingSpace.Address.Street = dto.Address.Street;
            coworkingSpace.Address.ZipCode = dto.Address.ZipCode;

            // **Actualizar Servicios**
            coworkingSpace.Services.Clear();
            if (dto.ServiceIds?.Any() == true)
            {
                coworkingSpace.Services = (await _unitOfWork.Services
                    .GetAllAsync(s => dto.ServiceIds.Contains(s.Id))).ToList();
            }

            // **Actualizar Beneficios**
            coworkingSpace.Benefits.Clear();
            if (dto.BenefitIds?.Any() == true)
            {
                coworkingSpace.Benefits = (await _unitOfWork.Benefits
                    .GetAllAsync(b => dto.BenefitIds.Contains(b.Id))).ToList();
            }

            await _unitOfWork.CoworkingSpaces.UpdateAsync(coworkingSpace);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.AuditLogs.LogAsync(new AuditLog
            {
                Action = "UpdateCoworkingSpace",
                UserId = hosterId,
                UserRole = userRole,
                Success = true,
                Description = $"Coworking space {coworkingSpace.Name} (ID: {id}) updated successfully."
            });
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
                    CapacityTotal = cs.CapacityTotal,
                    IsActive = cs.IsActive,
                    Rate = cs.Rate,
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
                    }).ToList() ?? new List<PhotoResponseDTO>(),
                })
                .ToList();
        }

        public async Task<IEnumerable<CoworkingSpaceResponseDTO>> GetAllFilteredAsync(int? capacity, string? location)
        {
            var query = _unitOfWork.CoworkingSpaces
                .GetQueryable(includeProperties: "Address")
                .AsNoTracking()
                .Where(cs => cs.Status == CoworkingStatus.Approved && cs.IsActive);

            if (capacity.HasValue)
            {
                query = query.Where(cs => cs.CapacityTotal >= capacity.Value);
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
                CapacityTotal = cs.CapacityTotal,
                IsActive = cs.IsActive,
                Rate = cs.Rate,
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
                }).ToList() ?? new List<PhotoResponseDTO>(),
                
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
                CapacityTotal = cs.CapacityTotal,
                IsActive = cs.IsActive,
                Rate = cs.Rate,
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
            {
                await _unitOfWork.AuditLogs.LogAsync(new AuditLog
                {
                    Action = "ToggleActiveStatus",
                    UserId = userId,
                    UserRole = userRole,
                    Success = false,
                    Description = "Coworking space not found."
                });
                throw new KeyNotFoundException("Coworking space not found.");
            }

            // Solo el hoster del espacio o un admin pueden cambiar el estado
            if (coworkingSpace.HosterId != userId && userRole != "Admin")
            {
                await _unitOfWork.AuditLogs.LogAsync(new AuditLog
                {
                    Action = "ToggleActiveStatus",
                    UserId = userId,
                    UserRole = userRole,
                    Success = false,
                    Description = "User does not have permission."
                });
                throw new UnauthorizedAccessException("You do not have permission to perform this action.");
            }

            // Cambiar estado
            coworkingSpace.IsActive = !coworkingSpace.IsActive;
            await _unitOfWork.CoworkingSpaces.UpdateAsync(coworkingSpace);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.AuditLogs.LogAsync(new AuditLog
            {
                Action = "ToggleActiveStatus",
                UserId = userId,
                UserRole = userRole,
                Success = true,
                Description = $"Coworking space {coworkingSpace.Name} status changed to {(coworkingSpace.IsActive ? "Active" : "Inactive")}."
            });
        }

        public async Task<IEnumerable<CoworkingSpaceSummaryDTO>> GetAllSummariesAsync()
        {
            var query = _unitOfWork.CoworkingSpaces
                .GetQueryable(includeProperties: "Address,Photos")
                .AsNoTracking()
                .Where(cs => cs.IsActive && cs.Status == CoworkingStatus.Approved);

            return await query.Select(cs => new CoworkingSpaceSummaryDTO
            {
                Id = cs.Id,
                Name = cs.Name,
                Address = new AddressDTO
                {
                    City = cs.Address.City,
                    Country = cs.Address.Country,
                    Number = cs.Address.Number,
                    Province = cs.Address.Province,
                    Street = cs.Address.Street,
                    ZipCode = cs.Address.ZipCode
                },
                CoverPhoto = cs.Photos
                    .Where(p => p.IsCoverPhoto)
                    .Select(p => new PhotoResponseDTO
                    {
                        FileName = p.FileName,
                        IsCoverPhoto = p.IsCoverPhoto,
                        ContentType = p.MimeType,
                        FilePath = p.FilePath
                    })
                    .FirstOrDefault()
            }).ToListAsync();
        }

        public async Task<IEnumerable<CoworkingSpaceSummaryDTO>> GetFilteredSummariesAsync(int? capacity, string? location)
        {
            var query = _unitOfWork.CoworkingSpaces
                .GetQueryable(includeProperties: "Address,Photos")
                .AsNoTracking()
                .Where(cs => cs.IsActive && cs.Status == CoworkingStatus.Approved);

            if (capacity.HasValue)
                query = query.Where(cs => cs.CapacityTotal >= capacity.Value);

            if (!string.IsNullOrEmpty(location))
                query = query.Where(cs =>
                    cs.Address.City.Contains(location) ||
                    cs.Address.Province.Contains(location) ||
                    cs.Address.Street.Contains(location));

            return await query.Select(cs => new CoworkingSpaceSummaryDTO
            {
                Id = cs.Id,
                Name = cs.Name,
                Address = new AddressDTO
                {
                    City = cs.Address.City,
                    Country = cs.Address.Country,
                    Number = cs.Address.Number,
                    Province = cs.Address.Province,
                    Street = cs.Address.Street,
                    ZipCode = cs.Address.ZipCode
                },
                CoverPhoto = cs.Photos
                    .Where(p => p.IsCoverPhoto)
                    .Select(p => new PhotoResponseDTO
                    {
                        FileName = p.FileName,
                        IsCoverPhoto = p.IsCoverPhoto,
                        ContentType = p.MimeType,
                        FilePath = p.FilePath
                    })
                    .FirstOrDefault()
            }).ToListAsync();
        }

        public async Task<IEnumerable<CoworkingSpaceListItemDTO>> GetAllLightweightAsync()
        {
            return await _unitOfWork.CoworkingSpaces.GetFilteredLightweightAsync(null, null);
        }

        public async Task<IEnumerable<CoworkingSpaceListItemDTO>> GetFilteredLightweightAsync(int? capacity, string? location)
        {
            return await _unitOfWork.CoworkingSpaces.GetFilteredLightweightAsync(capacity, location);
        }

        //Task<IEnumerable<CoworkingSpaceResponseDTO>> ICoworkingSpaceService.GetAllActiveSpacesAsync()
        //{
        //    throw new NotImplementedException();
        //}

        //Task<CoworkingSpaceResponseDTO> ICoworkingSpaceService.GetByIdAsync(int id)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<IEnumerable<CoworkingSpaceLightDTO>> GetAllLightFilteredAsync(int? capacity, string? location)
        //{
        //    if (!string.IsNullOrEmpty(location))
        //        return await _unitOfWork.CoworkingSpaces.GetAllFilteredOptimizedAsync(location);

        //    fallback si no hay location(se puede extender)
        //    return await GetAllActiveSpacesAsync();
        //}

        //Task<IEnumerable<DTOs.CoworkingSpace.CoworkingSpaceResponseDTO>> ICoworkingSpaceService.GetAllFilteredAsync(int? capacity, string? location)
        //{
        //    throw new NotImplementedException();
        //}
    }
}