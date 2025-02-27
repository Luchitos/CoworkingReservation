using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Application.Jobs;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using CoworkingReservation.Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CoworkingReservation.Application.Services
{
    public class CoworkingSpaceService : ICoworkingSpaceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CoworkingApprovalJob _approvalJob;


        public CoworkingSpaceService(IUnitOfWork unitOfWork, CoworkingApprovalJob approvalJob)
        {
            _unitOfWork = unitOfWork;
            _approvalJob = approvalJob;
        }

        #region Create Coworking Space
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
                    Name = spaceDto.Name,
                    Description = spaceDto.Description,
                    Capacity = spaceDto.Capacity,
                    PricePerDay = spaceDto.PricePerDay,
                    HosterId = userId,
                    Status = CoworkingStatus.Pending,
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

                // Asociar Áreas de Coworking
                if (spaceDto.Areas?.Any() == true)
                {
                    coworkingSpace.Areas = spaceDto.Areas.Select(areaDto => new CoworkingArea
                    {
                        Type = areaDto.Type,
                        Description = areaDto.Description,
                        Capacity = areaDto.Capacity,
                        PricePerDay = areaDto.PricePerDay
                    }).ToList();
                }

                await _unitOfWork.CoworkingSpaces.AddAsync(coworkingSpace);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.AuditLogs.LogAsync(new AuditLog
                {
                    Action = "CreateCoworkingSpace",
                    UserId = userId,
                    UserRole = user.Role,
                    Success = true,
                    Description = $"Coworking space '{coworkingSpace.Name}' created successfully."
                });

                await AddPhotosToCoworkingSpace(spaceDto.Photos, coworkingSpace.Id);

                await transaction.CommitAsync();

                // Ejecutar el job de aprobación en segundo plano
                _ = Task.Run(async () => await _approvalJob.Run());

                return coworkingSpace;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _unitOfWork.AuditLogs.LogAsync(new AuditLog
                {
                    Action = "CreateCoworkingSpace",
                    UserId = userId,
                    Success = false,
                    Description = $"Error creating coworking space: {ex.Message}"
                });

                throw;
            }
        }
        #endregion

        #region Get Coworking Spaces

        public async Task<IEnumerable<CoworkingSpaceResponseDTO>> GetAllByHosterIdAsync(int hosterId)
        {
            var spaces = await _unitOfWork.CoworkingSpaces
                .GetQueryable(includeProperties: "Address,Photos,Services,Benefits,Areas")
                .Where(cs => cs.HosterId == hosterId)
                .ToListAsync();

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
                }).ToList() ?? new List<PhotoResponseDTO>(),
                Services = cs.Services?.Select(s => new ServiceOfferedDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                }).ToList() ?? new List<ServiceOfferedDTO>(),
                Benefits = cs.Benefits?.Select(b => new BenefitDTO
                {
                    Id = b.Id,
                    Name = b.Name,
                }).ToList() ?? new List<BenefitDTO>(),
                Areas = cs.Areas?.Select(a => new CoworkingAreaResponseDTO
                {
                    Id = a.Id,
                    Type = a.Type,
                    Description = a.Description,
                    Capacity = a.Capacity,
                    PricePerDay = a.PricePerDay
                }).ToList() ?? new List<CoworkingAreaResponseDTO>()
            }).ToList();
        }

        public async Task<IEnumerable<CoworkingSpaceResponseDTO>> GetAllActiveSpacesAsync()
        {
            var spaces = await _unitOfWork.CoworkingSpaces
                .GetAllAsync(includeProperties: "Address,Photos,Services,Benefits");

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
                    }).ToList() ?? new List<PhotoResponseDTO>(),
                    Services = cs.Services?.Select(s => new ServiceOfferedDTO
                    {
                        Id = s.Id,
                        Name = s.Name,
                    }).ToList() ?? new List<ServiceOfferedDTO>(),
                    Benefits = cs.Benefits?.Select(b => new BenefitDTO
                    {
                        Id = b.Id,
                        Name = b.Name,
                    }).ToList() ?? new List<BenefitDTO>()
                })
                .ToList();
        }

        public async Task<IEnumerable<CoworkingSpaceResponseDTO>> GetAllFilteredAsync(int? capacity, string? location)
        {
            var query = _unitOfWork.CoworkingSpaces
                .GetQueryable(includeProperties: "Address,Photos,Services,Benefits")
                .Where(cs => cs.Status == CoworkingStatus.Approved && cs.IsActive);

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
                }).ToList() ?? new List<PhotoResponseDTO>(),
                Services = cs.Services?.Select(s => new ServiceOfferedDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                }).ToList() ?? new List<ServiceOfferedDTO>(),
                Benefits = cs.Benefits?.Select(b => new BenefitDTO
                {
                    Id = b.Id,
                    Name = b.Name,
                }).ToList() ?? new List<BenefitDTO>()
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
        #endregion

        #region Update Coworking Space
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

            // Actualizar propiedades principales
            coworkingSpace.Name = dto.Name;
            coworkingSpace.Description = dto.Description;
            coworkingSpace.PricePerDay = dto.PricePerDay;
            coworkingSpace.Capacity = dto.Capacity;

            // Actualizar dirección
            coworkingSpace.Address.City = dto.Address.City;
            coworkingSpace.Address.Country = dto.Address.Country;
            coworkingSpace.Address.Apartment = dto.Address.Apartment;
            coworkingSpace.Address.Floor = dto.Address.Floor;
            coworkingSpace.Address.Number = dto.Address.Number;
            coworkingSpace.Address.Province = dto.Address.Province;
            coworkingSpace.Address.Street = dto.Address.Street;
            coworkingSpace.Address.ZipCode = dto.Address.ZipCode;

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
        #endregion

        #region Delete Coworking Space 
        public async Task DeleteAsync(int id, int hosterId)
        {
            var coworkingSpace = await _unitOfWork.CoworkingSpaces.GetByIdAsync(id);

            if (coworkingSpace == null)
            {
                await _unitOfWork.AuditLogs.LogAsync(new AuditLog
                {
                    Action = "DeleteCoworkingSpace",
                    UserId = hosterId,
                    Success = false,
                    Description = $"Attempted to delete a non-existent coworking space (ID: {id})."
                });

                throw new KeyNotFoundException("Coworking space not found");
            }

            await _unitOfWork.CoworkingSpaces.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.AuditLogs.LogAsync(new AuditLog
            {
                Action = "DeleteCoworkingSpace",
                UserId = hosterId,
                Success = true,
                Description = $"Coworking space '{coworkingSpace.Name}' (ID: {id}) deleted successfully."
            });
        }
        #endregion

        #region Private 
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
        #endregion

    }
}