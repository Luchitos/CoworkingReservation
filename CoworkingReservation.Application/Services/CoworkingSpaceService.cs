using CoworkingReservation.Application.DTOs.Address;
using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Application.DTOs.Photo;
using CoworkingReservation.Application.DTOs.CoworkingArea;
using CoworkingReservation.Application.DTOs.Benefit;
using CoworkingReservation.Application.Jobs;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using CoworkingReservation.Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using AddressDTO = CoworkingReservation.Application.DTOs.Address.AddressDTO;
using CoworkingSpaceResponseDTO = CoworkingReservation.Application.DTOs.CoworkingSpace.CoworkingSpaceResponseDTO;
using CoworkingSpaceSummaryDTO = CoworkingReservation.Application.DTOs.CoworkingSpace.CoworkingSpaceSummaryDTO;
using PhotoResponseDTO = CoworkingReservation.Application.DTOs.Photo.PhotoResponseDTO;
using ServiceOfferedDTO = CoworkingReservation.Application.DTOs.CoworkingSpace.ServiceOfferedDTO;
using CoworkingSpaceListItemDTO = CoworkingReservation.Domain.DTOs.CoworkingSpaceListItemDTO;
using CoworkingReservation.Infrastructure.Data;

namespace CoworkingReservation.Application.Services
{
    public class CoworkingSpaceService : ICoworkingSpaceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICoworkingAreaService _coworkingAreaService;
        private readonly CoworkingApprovalJob _approvalJob;
        private readonly IImageUploadService _imageUploadService;
        private readonly ApplicationDbContext _context;

        public CoworkingSpaceService(IUnitOfWork unitOfWork, CoworkingApprovalJob approvalJob, ICoworkingAreaService coworkingAreaService, IImageUploadService imageUploadService, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _approvalJob = approvalJob;
            _coworkingAreaService = coworkingAreaService;
            _imageUploadService = imageUploadService;
            _context = context;
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
                        StreetOne = spaceDto.Address.StreetOne,
                        StreetTwo = spaceDto.Address.StreetTwo,
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
            coworkingSpace.Address.StreetOne = dto.Address.StreetOne;
            coworkingSpace.Address.StreetTwo = dto.Address.StreetTwo;
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
                    // Renombrar el archivo antes de subir para garantizar que el índice sea explícito
                    var originalFileName = photos[i].FileName;
                    string extension = Path.GetExtension(originalFileName);
                    string tempFileName = $"photo{i+1}{extension}"; // Asegura índices secuenciales (1-based)
                    
                    // Usar el nuevo servicio para subir a ImgBB con organización por carpetas
                    string imageUrl = await _imageUploadService.UploadCoworkingSpaceImageAsync(photos[i], coworkingSpaceId);

                    var coworkingPhoto = new CoworkingSpacePhoto
                    {
                        FileName = tempFileName, // Guardar el nombre con índice
                        FilePath = imageUrl, // URL de ImgBB con organización por carpetas
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
                .GetAllAsync(includeProperties: "Address,Photos,Services,Benefits,SafetyElements,SpecialFeatures");

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
                        City = cs.Address.City ?? "",
                        Country = cs.Address.Country ?? "",
                        Number = cs.Address.Number ?? "",
                        Province = cs.Address.Province ?? "",
                        Street = cs.Address.Street ?? "",
                        ZipCode = cs.Address.ZipCode ?? "",
                        Apartment = cs.Address.Apartment ?? "",
                        Floor = cs.Address.Floor ?? "",
                        StreetOne = cs.Address.StreetOne ?? "",
                        StreetTwo = cs.Address.StreetTwo ?? "",
                        Latitude = cs.Address.Latitude ?? "",
                        Longitude = cs.Address.Longitude ?? ""
                    } : new AddressDTO(),
                    PhotoUrls = cs.Photos?.Select(p => p.FilePath).ToList() ?? new List<string>(),
                    ServiceNames = cs.Services?.Select(s => s.Name).ToList() ?? new List<string>(),
                    BenefitNames = cs.Benefits?.Select(b => b.Name).ToList() ?? new List<string>(),
                    SafetyElementNames = cs.SafetyElements?.Select(se => se.Name).ToList() ?? new List<string>(),
                    SpecialFeatureNames = cs.SpecialFeatures?.Select(sf => sf.Name).ToList() ?? new List<string>(),
                    Areas = cs.Areas?.Select(a => new CoworkingAreaResponseDTO
                    {
                        Id = a.Id,
                        Type = a.Type,
                        Description = a.Description ?? "",
                        Capacity = a.Capacity,
                        PricePerDay = a.PricePerDay,
                        Available = a.Available
                    }).ToList() ?? new List<CoworkingAreaResponseDTO>()
                })
                .ToList();
        }

        public async Task<IEnumerable<CoworkingSpaceResponseDTO>> GetAllFilteredAsync(int? capacity, string? location)
        {
            var query = _unitOfWork.CoworkingSpaces
                .GetQueryable(includeProperties: "Address,Photos,Services,Benefits,SafetyElements,SpecialFeatures")
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

            return spaces.Select(cs => 
            {
                // Asegurarse de que la dirección tenga todos los campos necesarios
                var address = cs.Address ?? new Domain.Entities.Address();
                
                return new CoworkingSpaceResponseDTO
                {
                    Id = cs.Id,
                    Name = cs.Name,
                    Description = cs.Description,
                    CapacityTotal = cs.CapacityTotal,
                    IsActive = cs.IsActive,
                    Rate = cs.Rate,
                    Address = new AddressDTO
                    {
                        City = address.City ?? "",
                        Country = address.Country ?? "",
                        Number = address.Number ?? "",
                        Province = address.Province ?? "",
                        Street = address.Street ?? "",
                        ZipCode = address.ZipCode ?? "",
                        Apartment = address.Apartment ?? "",
                        Floor = address.Floor ?? "",
                        StreetOne = address.StreetOne ?? "",
                        StreetTwo = address.StreetTwo ?? "",
                        Latitude = address.Latitude ?? "",
                        Longitude = address.Longitude ?? ""
                    },
                    PhotoUrls = cs.Photos?.Select(p => p.FilePath).ToList() ?? new List<string>(),
                    ServiceNames = cs.Services?.Select(s => s.Name).ToList() ?? new List<string>(),
                    BenefitNames = cs.Benefits?.Select(b => b.Name).ToList() ?? new List<string>(),
                    SafetyElementNames = cs.SafetyElements?.Select(se => se.Name).ToList() ?? new List<string>(),
                    SpecialFeatureNames = cs.SpecialFeatures?.Select(sf => sf.Name).ToList() ?? new List<string>(),
                    Areas = cs.Areas?.Select(a => new CoworkingAreaResponseDTO
                    {
                        Id = a.Id,
                        Type = a.Type,
                        Description = a.Description ?? "",
                        Capacity = a.Capacity,
                        PricePerDay = a.PricePerDay,
                        Available = a.Available
                    }).ToList() ?? new List<CoworkingAreaResponseDTO>()
                };
            }).ToList();
        }

        public async Task<CoworkingSpaceResponseDTO> GetByIdAsync(int id)
        {
            try
            {
                // Cargar el espacio de coworking básico
                var cs = await _unitOfWork.CoworkingSpaces.GetByIdAsync(id);
                if (cs == null) throw new KeyNotFoundException("Espacio de coworking no encontrado");

                // Cargar áreas usando el método seguro del repositorio
                var areas = await _unitOfWork.CoworkingAreas.GetByCoworkingSpaceIdAsync(id);

                // Cargar fotos directamente desde la base de datos - ahora solo obtenemos los filePath
                var photoUrls = await _context.CoworkingSpacePhotos
                    .Where(p => p.CoworkingSpaceId == id)
                    .OrderBy(p => !p.IsCoverPhoto) // Poner las fotos de portada primero
                    .Select(p => p.FilePath)
                    .ToListAsync();

                // Cargar nombres de servicios directamente
                var serviceNames = await _context.ServicesOffered
                    .FromSqlRaw(@"
                        SELECT so.* FROM ServicesOffered so
                        INNER JOIN CoworkingSpaceServiceOffered csso ON so.Id = csso.ServicesId
                        WHERE csso.CoworkingSpacesId = {0}", id)
                    .Select(s => s.Name ?? "")
                    .ToListAsync();

                // Cargar nombres de beneficios directamente
                var benefitNames = await _context.Benefits
                    .FromSqlRaw(@"
                        SELECT b.* FROM Benefits b
                        INNER JOIN BenefitCoworkingSpace bcs ON b.Id = bcs.BenefitsId
                        WHERE bcs.CoworkingSpacesId = {0}", id)
                    .Select(b => b.Name ?? "")
                    .ToListAsync();

                // Cargar nombres de elementos de seguridad directamente
                var safetyElementNames = await _context.SafetyElements
                    .FromSqlRaw(@"
                        SELECT se.* FROM SafetyElements se
                        INNER JOIN CoworkingSpaceSafetyElement csse ON se.Id = csse.SafetyElementsId
                        WHERE csse.CoworkingSpacesId = {0}", id)
                    .Select(se => se.Name ?? "")
                    .ToListAsync();

                // Cargar nombres de características especiales directamente
                var specialFeatureNames = await _context.SpecialFeatures
                    .FromSqlRaw(@"
                        SELECT sf.* FROM SpecialFeatures sf
                        INNER JOIN CoworkingSpaceSpecialFeature cssf ON sf.Id = cssf.SpecialFeaturesId
                        WHERE cssf.CoworkingSpacesId = {0}", id)
                    .Select(sf => sf.Name ?? "")
                    .ToListAsync();

                // Asegurarse de que la dirección tenga todos los campos necesarios
                // incluso si algunos campos son nulos en la base de datos
                var address = cs.Address ?? new Domain.Entities.Address();

                // Mapear a DTO
                var result = new CoworkingSpaceResponseDTO
                {
                    Id = cs.Id,
                    Name = cs.Name,
                    Description = cs.Description,
                    CapacityTotal = cs.CapacityTotal,
                    IsActive = cs.IsActive,
                    Rate = cs.Rate,
                    // Crear un AddressDTO asegurándonos que todos los campos estén presentes
                    Address = new AddressDTO
                    {
                        City = address.City ?? "",
                        Country = address.Country ?? "",
                        Number = address.Number ?? "",
                        Province = address.Province ?? "",
                        Street = address.Street ?? "",
                        ZipCode = address.ZipCode ?? "",
                        Apartment = address.Apartment ?? "",
                        Floor = address.Floor ?? "",
                        StreetOne = address.StreetOne ?? "",
                        StreetTwo = address.StreetTwo ?? "",
                        Latitude = address.Latitude ?? "",
                        Longitude = address.Longitude ?? ""
                    },
                    // Asignar las colecciones como listas de strings
                    PhotoUrls = photoUrls,
                    ServiceNames = serviceNames,
                    BenefitNames = benefitNames,
                    SafetyElementNames = safetyElementNames,
                    SpecialFeatureNames = specialFeatureNames,
                    Areas = areas.Select(a => new CoworkingAreaResponseDTO
                    {
                        Id = a.Id,
                        Type = a.Type,
                        Description = a.Description ?? "",
                        Capacity = a.Capacity,
                        PricePerDay = a.PricePerDay,
                        Available = a.Available
                    }).ToList()
                };

                return result;
            }
            catch (Exception ex)
            {
                // Registrar el error y relanzarlo
                Console.WriteLine($"Error al cargar el espacio de coworking: {ex.Message}");
                throw;
            }
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
                CoverPhotoUrl = cs.Photos
                    .Where(p => p.IsCoverPhoto)
                    .Select(p => p.FilePath)
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
                CoverPhotoUrl = cs.Photos
                    .Where(p => p.IsCoverPhoto)
                    .Select(p => p.FilePath)
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