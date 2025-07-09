using System.Text.Json;
using CoworkingReservation.Application.DTOs.Address;
using CoworkingReservation.Application.DTOs.CoworkingArea;
using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Application.DTOs.Review;
using CoworkingReservation.Application.Jobs;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CoworkingReservation.Application.Services
{
    public class CoworkingSpaceService : ICoworkingSpaceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICoworkingAreaService _coworkingAreaService;
        private readonly ICoworkingApprovalJob _approvalJob;
        private readonly IImageUploadService _imageUploadService;
        private readonly ApplicationDbContext _context;

        public CoworkingSpaceService(IUnitOfWork unitOfWork, ICoworkingApprovalJob approvalJob, ICoworkingAreaService coworkingAreaService, IImageUploadService imageUploadService, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _approvalJob = approvalJob;
            _coworkingAreaService = coworkingAreaService;
            _imageUploadService = imageUploadService;
            _context = context;
        }

        // // TODO: Revisar los dto en el servicio porque se estan mezclando
        // public async Task<CoworkingSpaceResponseDTO> CreateAsync(CreateCoworkingSpaceDTO spaceDto, int userId)
        // {
        //     await using var transaction = await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        //     try
        //     {
        //         var user = await _unitOfWork.Users.GetByIdAsync(userId);
        //         if (user == null)
        //             throw new UnauthorizedAccessException("User not found.");

        //         bool hasPendingCoworking = await _unitOfWork.CoworkingSpaces.ExistsAsync(
        //             c => c.HosterId == userId && c.Status == CoworkingStatus.Pending
        //         );

        //         if (hasPendingCoworking)
        //             throw new InvalidOperationException("You already have a pending coworking space approval.");

        //         bool userWasClient = user.Role != "Hoster";
        //         if (userWasClient)
        //         {
        //             user.Role = "Hoster";
        //             await _unitOfWork.Users.UpdateAsync(user);
        //         }

        //         var coworkingSpace = new CoworkingSpace
        //         {
        //             Name = spaceDto.Title,
        //             Description = spaceDto.Description,
        //             CapacityTotal = spaceDto.CapacityTotal,
        //             HosterId = userId,
        //             Status = CoworkingStatus.Pending,
        //             Rate = spaceDto.Rate,
        //             Address = new Address
        //             {
        //                 City = spaceDto.Address.City,
        //                 Country = spaceDto.Address.Country,
        //                 Apartment = spaceDto.Address.Apartment,
        //                 Floor = spaceDto.Address.Floor,
        //                 Number = spaceDto.Address.Number,
        //                 Province = spaceDto.Address.Province,
        //                 Street = spaceDto.Address.Street,
        //                 StreetOne = spaceDto.Address.StreetOne,
        //                 StreetTwo = spaceDto.Address.StreetTwo,
        //                 ZipCode = spaceDto.Address.ZipCode,
        //                 Latitude = spaceDto.Address.Latitude,
        //                 Longitude = spaceDto.Address.Longitude,
        //             }
        //         };

        //         bool addressExists = await _unitOfWork.Addresses.ExistsAsync(
        //             a => a.Street == spaceDto.Address.Street &&
        //                  a.Number == spaceDto.Address.Number &&
        //                  a.City == spaceDto.Address.City &&
        //                  a.Province == spaceDto.Address.Province
        //         );

        //         if (addressExists)
        //             throw new InvalidOperationException("A coworking space with this address already exists.");

        //         // Deserialize the JSON strings into lists of integers
        //         List<int> serviceIds = string.IsNullOrWhiteSpace(spaceDto.Services)
        //             ? new List<int>()
        //             : JsonSerializer.Deserialize<List<int>>(spaceDto.Services, new JsonSerializerOptions
        //             {
        //                 PropertyNameCaseInsensitive = true
        //             }) ?? new List<int>();

        //         List<int> benefitIds = string.IsNullOrWhiteSpace(spaceDto.Benefits)
        //             ? new List<int>()
        //             : JsonSerializer.Deserialize<List<int>>(spaceDto.Benefits, new JsonSerializerOptions
        //             {
        //                 PropertyNameCaseInsensitive = true
        //             }) ?? new List<int>();

        //         List<int> safetyElementIds = string.IsNullOrWhiteSpace(spaceDto.SafetyElements)
        //             ? new List<int>()
        //             : JsonSerializer.Deserialize<List<int>>(spaceDto.SafetyElements, new JsonSerializerOptions
        //             {
        //                 PropertyNameCaseInsensitive = true
        //             }) ?? new List<int>();

        //         List<int> specialFeatureIds = string.IsNullOrWhiteSpace(spaceDto.SpeacialFeatures)
        //             ? new List<int>()
        //             : JsonSerializer.Deserialize<List<int>>(spaceDto.SpeacialFeatures, new JsonSerializerOptions
        //             {
        //                 PropertyNameCaseInsensitive = true
        //             }) ?? new List<int>();

        //         List<CoworkingAreaDTO> areas = string.IsNullOrWhiteSpace(spaceDto.AreasJson)
        //             ? new List<CoworkingAreaDTO>()
        //             : JsonSerializer.Deserialize<List<CoworkingAreaDTO>>(spaceDto.AreasJson, new JsonSerializerOptions
        //             {
        //                 PropertyNameCaseInsensitive = true
        //             }) ?? new List<CoworkingAreaDTO>();

        //         // Create the coworking space first, without related entities
        //         await _unitOfWork.CoworkingSpaces.AddAsync(coworkingSpace);
        //         await _unitOfWork.SaveChangesAsync();

        //         // Now that we have an ID, we can add the related entities in separate steps

        //         // Add services using the context directly to avoid EF Core navigation property issues
        //         if (serviceIds.Any())
        //         {
        //             var services = await _unitOfWork.Services.GetAllAsync(s => serviceIds.Contains(s.Id));
        //             foreach (var service in services)
        //             {
        //                 await _context.Database.ExecuteSqlRawAsync(
        //                     "INSERT INTO CoworkingSpaceServiceOffered (CoworkingSpacesId, ServicesId) VALUES ({0}, {1})",
        //                     coworkingSpace.Id, service.Id);
        //             }
        //         }

        //         // Add benefits using the context directly
        //         if (benefitIds.Any())
        //         {
        //             var benefits = await _unitOfWork.Benefits.GetAllAsync(b => benefitIds.Contains(b.Id));
        //             foreach (var benefit in benefits)
        //             {
        //                 await _context.Database.ExecuteSqlRawAsync(
        //                     "INSERT INTO BenefitCoworkingSpace (CoworkingSpacesId, BenefitsId) VALUES ({0}, {1})",
        //                     coworkingSpace.Id, benefit.Id);
        //             }
        //         }

        //         // Add safety elements using the context directly
        //         if (safetyElementIds.Any())
        //         {
        //             var safetyElements = await _unitOfWork.SafetyElements.GetAllAsync(se => safetyElementIds.Contains(se.Id));
        //             foreach (var safetyElement in safetyElements)
        //             {
        //                 await _context.Database.ExecuteSqlRawAsync(
        //                     "INSERT INTO CoworkingSpaceSafetyElement (CoworkingSpacesId, SafetyElementsId) VALUES ({0}, {1})",
        //                     coworkingSpace.Id, safetyElement.Id);
        //             }
        //         }

        //         // Add special features using the context directly
        //         if (specialFeatureIds.Any())
        //         {
        //             var specialFeatures = await _unitOfWork.SpecialFeatures.GetAllAsync(sf => specialFeatureIds.Contains(sf.Id));
        //             foreach (var specialFeature in specialFeatures)
        //             {
        //                 await _context.Database.ExecuteSqlRawAsync(
        //                     "INSERT INTO CoworkingSpaceSpecialFeature (CoworkingSpacesId, SpecialFeaturesId) VALUES ({0}, {1})",
        //                     coworkingSpace.Id, specialFeature.Id);
        //             }
        //         }

        //         await AddPhotosToCoworkingSpace(spaceDto.Photos, coworkingSpace.Id);
        //         // Nueva línea: agregar áreas con servicio externo
        //         if (areas.Any())
        //         {
        //             await _coworkingAreaService.AddAreasToCoworkingAsync(areas, coworkingSpace.Id, userId);
        //         }
        //         await transaction.CommitAsync();

        //         _ = Task.Run(async () => await _approvalJob.Run());
        //         return coworkingSpace;
        //     }
        //     catch (Exception)
        //     {
        //         await transaction.RollbackAsync();
        //         throw;
        //     }
        // }

        public async Task<CoworkingSpaceResponseDTO> CreateAsync(CreateCoworkingSpaceDTO spaceDto, int userId)
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
                    IsActive = true,
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

                // Deserialize JSON
                List<int> serviceIds = string.IsNullOrWhiteSpace(spaceDto.Services)
                    ? new List<int>()
                    : JsonSerializer.Deserialize<List<int>>(spaceDto.Services, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<int>();

                List<int> benefitIds = string.IsNullOrWhiteSpace(spaceDto.Benefits)
                    ? new List<int>()
                    : JsonSerializer.Deserialize<List<int>>(spaceDto.Benefits, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<int>();

                List<int> safetyElementIds = string.IsNullOrWhiteSpace(spaceDto.SafetyElements)
                    ? new List<int>()
                    : JsonSerializer.Deserialize<List<int>>(spaceDto.SafetyElements, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<int>();

                List<int> specialFeatureIds = string.IsNullOrWhiteSpace(spaceDto.SpeacialFeatures)
                    ? new List<int>()
                    : JsonSerializer.Deserialize<List<int>>(spaceDto.SpeacialFeatures, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<int>();

                List<CoworkingAreaDTO> areas = string.IsNullOrWhiteSpace(spaceDto.AreasJson)
                    ? new List<CoworkingAreaDTO>()
                    : JsonSerializer.Deserialize<List<CoworkingAreaDTO>>(spaceDto.AreasJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<CoworkingAreaDTO>();

                await _unitOfWork.CoworkingSpaces.AddAsync(coworkingSpace);
                await _unitOfWork.SaveChangesAsync();

                // Relacionar servicios, beneficios, etc. (tu código de inserción en tablas de relación aquí, como ya lo hacías)
                if (serviceIds.Any())
                {
                    var services = await _unitOfWork.Services.GetAllAsync(s => serviceIds.Contains(s.Id));
                    foreach (var service in services)
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "INSERT INTO CoworkingSpaceServiceOffered (CoworkingSpacesId, ServiceOfferedId) VALUES ({0}, {1})",
                            coworkingSpace.Id, service.Id);
                    }
                }
                if (benefitIds.Any())
                {
                    var benefits = await _unitOfWork.Benefits.GetAllAsync(b => benefitIds.Contains(b.Id));
                    foreach (var benefit in benefits)
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "INSERT INTO BenefitCoworkingSpace (CoworkingSpacesId, BenefitId) VALUES ({0}, {1})",
                            coworkingSpace.Id, benefit.Id);
                    }
                }
                if (safetyElementIds.Any())
                {
                    var safetyElements = await _unitOfWork.SafetyElements.GetAllAsync(se => safetyElementIds.Contains(se.Id));
                    foreach (var safetyElement in safetyElements)
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "INSERT INTO CoworkingSpaceSafetyElement (CoworkingSpacesId, SafetyElementsId) VALUES ({0}, {1})",
                            coworkingSpace.Id, safetyElement.Id);
                    }
                }
                if (specialFeatureIds.Any())
                {
                    var specialFeatures = await _unitOfWork.SpecialFeatures.GetAllAsync(sf => specialFeatureIds.Contains(sf.Id));
                    foreach (var specialFeature in specialFeatures)
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "INSERT INTO CoworkingSpaceSpecialFeature (CoworkingSpacesId, SpecialFeaturesId) VALUES ({0}, {1})",
                            coworkingSpace.Id, specialFeature.Id);
                    }
                }

                await AddPhotosToCoworkingSpace(spaceDto.Photos, coworkingSpace.Id);

                if (areas.Any())
                {
                    await _coworkingAreaService.AddAreasToCoworkingAsync(areas, coworkingSpace.Id, userId);
                }

                await transaction.CommitAsync();

                // --------- MAPEO AL DTO DE RESPUESTA ---------
                var fullSpace = await _unitOfWork.CoworkingSpaces.GetByIdWithAllDetailsAsync(coworkingSpace.Id);

                var response = new CoworkingSpaceResponseDTO
                {
                    Id = fullSpace.Id,
                    Rate = fullSpace.Rate,
                    Name = fullSpace.Name,
                    Description = fullSpace.Description,
                    CapacityTotal = fullSpace.CapacityTotal,
                    IsActive = fullSpace.IsActive,
                    Address = new AddressDTO
                    {
                        City = fullSpace.Address?.City,
                        Country = fullSpace.Address?.Country,
                        Apartment = fullSpace.Address?.Apartment,
                        Floor = fullSpace.Address?.Floor,
                        Number = fullSpace.Address?.Number,
                        Province = fullSpace.Address?.Province,
                        Street = fullSpace.Address?.Street,
                        StreetOne = fullSpace.Address?.StreetOne,
                        StreetTwo = fullSpace.Address?.StreetTwo,
                        ZipCode = fullSpace.Address?.ZipCode,
                        Latitude = fullSpace.Address?.Latitude,
                        Longitude = fullSpace.Address?.Longitude
                    },
                    PhotoUrls = fullSpace.Photos?.Select(p => p.FilePath).ToList() ?? new List<string>(),
                    ServiceNames = fullSpace.Services?.Select(s => s.Name).ToList() ?? new List<string>(),
                    BenefitNames = fullSpace.Benefits?.Select(b => b.Name).ToList() ?? new List<string>(),
                    SafetyElementNames = fullSpace.SafetyElements?.Select(se => se.Name).ToList() ?? new List<string>(),
                    SpecialFeatureNames = fullSpace.SpecialFeatures?.Select(sf => sf.Name).ToList() ?? new List<string>(),
                    Areas = fullSpace.Areas?.Select(a => new CoworkingAreaResponseDTO
                    {
                        Id = a.Id,
                        Type = a.Type,
                        Description = a.Description,
                        Capacity = a.Capacity,
                        PricePerDay = a.PricePerDay,
                        Available = a.Available
                    }).ToList() ?? new List<CoworkingAreaResponseDTO>(),
                    Reviews = fullSpace.Reviews?.Select(r => new ReviewResponseDTO
                    {
                        Id = r.Id,
                        UserId = r.UserId,
                        UserName = r.User?.Name,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt
                    }).ToList() ?? new List<ReviewResponseDTO>()
                };
                _ = Task.Run(async () => await _approvalJob.Run());

                return response;
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
            if (dto.Benefits?.Any() == true)
            {
                coworkingSpace.Benefits = (await _unitOfWork.Benefits
                    .GetAllAsync(b => dto.Benefits.Contains(b.Id))).ToList();
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
                    string tempFileName = $"photo{i + 1}{extension}"; // Asegura índices secuenciales (1-based)

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
                var cs = await _unitOfWork.CoworkingSpaces.GetByIdAsync(id, "Address");
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
        INNER JOIN CoworkingSpaceServiceOffered csso ON so.Id = csso.ServiceOfferedId
        WHERE csso.CoworkingSpacesId = {0}", id)
                    .Select(s => s.Name ?? "")
                    .ToListAsync();

                // Cargar nombres de beneficios directamente
                var benefitNames = await _context.Benefits
                    .FromSqlRaw(@"
        SELECT b.* FROM Benefits b
        INNER JOIN BenefitCoworkingSpace bcs ON b.Id = bcs.BenefitId
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

                // Cargar reviews con información del usuario
                var reviews = await _context.Reviews
                    .Where(r => r.CoworkingSpaceId == id)
                    .Include(r => r.User)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new ReviewResponseDTO
                    {
                        Id = r.Id,
                        UserId = r.UserId,
                        UserName = r.User.Name + " " + r.User.Lastname,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt
                    })
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
                    }).ToList(),
                    // Agregar los reviews
                    Reviews = reviews
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

        public async Task<IEnumerable<CoworkingSpaceListItemDTO>> GetAllLightweightAsync(int? userId = null)
        {
            var spaces = await _unitOfWork.CoworkingSpaces.GetFilteredLightweightAsync(null, null, userId);

            // Traer favoritos del usuario si corresponde
            var favoriteSpaceIds = new List<int>();
            if (userId.HasValue)
            {
                favoriteSpaceIds = (await _unitOfWork.FavoriteCoworkingSpaces
                    .GetAllAsync(f => f.UserId == userId.Value))
                    .Select(f => f.CoworkingSpaceId).ToList();

            }

            // Mapeo manual de entidad a DTO
            return spaces.Select(space =>
            {
                var availableAreas = space.Areas?.Where(a => a.Available).ToList() ?? new List<CoworkingArea>();
                return new CoworkingSpaceListItemDTO
                {
                    Id = space.Id,
                    Name = space.Name,
                    Address = space.Address != null ? new AddressDTO
                    {
                        City = space.Address.City,
                        Province = space.Address.Province,
                        Street = space.Address.Street,
                        Number = space.Address.Number,
                        Country = space.Address.Country,
                        ZipCode = space.Address.ZipCode,
                        Latitude = space.Address?.Latitude,
                        Longitude = space.Address?.Longitude
                    } : null,
                    CoverPhotoUrl = space.Photos?.FirstOrDefault(p => p.IsCoverPhoto)?.FilePath
                        ?? space.Photos?.FirstOrDefault()?.FilePath,
                    Rate = space.Rate,
                    TotalCapacity = space.CapacityTotal,
                    HasConfiguredAreas = availableAreas.Any(),
                    PrivateOfficesCount = availableAreas.Count(a => a.Type == CoworkingAreaType.PrivateOffice),
                    IndividualDesksCount = availableAreas.Count(a => a.Type == CoworkingAreaType.IndividualDesk),
                    SharedDesksCount = availableAreas.Count(a => a.Type == CoworkingAreaType.SharedDesks),
                    MinPrivateOfficePrice = availableAreas
                        .Where(a => a.Type == CoworkingAreaType.PrivateOffice)
                        .Select(a => (decimal?)a.PricePerDay)
                        .DefaultIfEmpty()
                        .Min(),
                    MaxPrivateOfficePrice = availableAreas
                        .Where(a => a.Type == CoworkingAreaType.PrivateOffice)
                        .Select(a => (decimal?)a.PricePerDay)
                        .DefaultIfEmpty()
                        .Max(),
                    MinIndividualDeskPrice = availableAreas
                        .Where(a => a.Type == CoworkingAreaType.IndividualDesk)
                        .Select(a => (decimal?)a.PricePerDay)
                        .DefaultIfEmpty()
                        .Min(),
                    MaxIndividualDeskPrice = availableAreas
                        .Where(a => a.Type == CoworkingAreaType.IndividualDesk)
                        .Select(a => (decimal?)a.PricePerDay)
                        .DefaultIfEmpty()
                        .Max(),
                    SharedDeskPrice = availableAreas
                        .Where(a => a.Type == CoworkingAreaType.SharedDesks)
                        .Select(a => (decimal?)a.PricePerDay)
                        .FirstOrDefault(),
                    IsFavorite = userId.HasValue && favoriteSpaceIds.Contains(space.Id)
                };
            }).ToList();
        }

        public async Task<IEnumerable<CoworkingSpaceListItemDTO>> GetFilteredLightweightAsync(int? capacity, string? location, int? userId = null)
        {
            var spaces = await _unitOfWork.CoworkingSpaces.GetFilteredLightweightAsync(capacity, location, userId);

            // Si hay usuario, traigo favoritos
            var favoriteSpaceIds = new List<int>();
            if (userId.HasValue)
            {
                favoriteSpaceIds = (await _unitOfWork.FavoriteCoworkingSpaces
                    .GetAllAsync(f => f.UserId == userId.Value))
                    .Select(f => f.CoworkingSpaceId).ToList();

            }

            return spaces.Select(space =>
            {
                var availableAreas = space.Areas?.Where(a => a.Available).ToList() ?? new List<CoworkingArea>();
                return new CoworkingSpaceListItemDTO
                {
                    Id = space.Id,
                    Name = space.Name,
                    Address = space.Address != null ? new AddressDTO
                    {
                        City = space.Address.City,
                        Province = space.Address.Province,
                        Street = space.Address.Street,
                        Number = space.Address.Number,
                        Country = space.Address.Country,
                        ZipCode = space.Address.ZipCode,
                        Latitude = space.Address?.Latitude,
                        Longitude = space.Address?.Longitude
                    } : null,
                    CoverPhotoUrl = space.Photos?.FirstOrDefault(p => p.IsCoverPhoto)?.FilePath
                        ?? space.Photos?.FirstOrDefault()?.FilePath,
                    Rate = space.Rate,
                    TotalCapacity = space.CapacityTotal,
                    HasConfiguredAreas = availableAreas.Any(),
                    PrivateOfficesCount = availableAreas.Count(a => a.Type == CoworkingAreaType.PrivateOffice),
                    IndividualDesksCount = availableAreas.Count(a => a.Type == CoworkingAreaType.IndividualDesk),
                    SharedDesksCount = availableAreas.Count(a => a.Type == CoworkingAreaType.SharedDesks),
                    MinPrivateOfficePrice = availableAreas
                        .Where(a => a.Type == CoworkingAreaType.PrivateOffice)
                        .Select(a => (decimal?)a.PricePerDay)
                        .DefaultIfEmpty()
                        .Min(),
                    MaxPrivateOfficePrice = availableAreas
                        .Where(a => a.Type == CoworkingAreaType.PrivateOffice)
                        .Select(a => (decimal?)a.PricePerDay)
                        .DefaultIfEmpty()
                        .Max(),
                    MinIndividualDeskPrice = availableAreas
                        .Where(a => a.Type == CoworkingAreaType.IndividualDesk)
                        .Select(a => (decimal?)a.PricePerDay)
                        .DefaultIfEmpty()
                        .Min(),
                    MaxIndividualDeskPrice = availableAreas
                        .Where(a => a.Type == CoworkingAreaType.IndividualDesk)
                        .Select(a => (decimal?)a.PricePerDay)
                        .DefaultIfEmpty()
                        .Max(),
                    SharedDeskPrice = availableAreas
                        .Where(a => a.Type == CoworkingAreaType.SharedDesks)
                        .Select(a => (decimal?)a.PricePerDay)
                        .FirstOrDefault(),
                    IsFavorite = userId.HasValue && favoriteSpaceIds.Contains(space.Id)
                };
            }).ToList();
        }

        public async Task<SpaceFilterResponseDTO> GetAdvancedFilteredAsync(
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
           int? userId = null)
        {
            // 1. Asegurar listas no nulas
            services ??= new List<string>();
            benefits ??= new List<string>();

            // 2. Metadata de filtros aplicados
            var appliedFilters = new Dictionary<string, object>();
            if (capacity.HasValue) appliedFilters["capacity"] = capacity.Value;
            if (!string.IsNullOrWhiteSpace(location)) appliedFilters["location"] = location;
            if (date.HasValue) appliedFilters["date"] = date.Value;
            if (minPrice.HasValue) appliedFilters["minPrice"] = minPrice.Value;
            if (maxPrice.HasValue) appliedFilters["maxPrice"] = maxPrice.Value;
            if (individualDesk == true) appliedFilters["individualDesk"] = true;
            if (privateOffice == true) appliedFilters["privateOffice"] = true;
            if (hybridSpace == true) appliedFilters["hybridSpace"] = true;
            if (services.Any()) appliedFilters["services"] = string.Join(", ", services);
            if (benefits.Any()) appliedFilters["benefits"] = string.Join(", ", benefits);

            // 3. Mapear tipo de área según enum
            var areaTypes = new List<int>();
            if (individualDesk == true) areaTypes.Add((int)CoworkingAreaType.IndividualDesk);
            if (privateOffice == true) areaTypes.Add((int)CoworkingAreaType.PrivateOffice);
            if (hybridSpace == true) areaTypes.Add((int)CoworkingAreaType.SharedDesks);

            // 4. Query base (puede ir a un repositorio para aún más clean, pero así está bien para Application Service)
            var baseQuery = _context.CoworkingSpaces
                .AsNoTracking()
                .Include(cs => cs.Address)
                .Include(cs => cs.Areas)
                .Include(cs => cs.Photos)
                .Include(cs => cs.Services)
                .Include(cs => cs.Benefits)
                .Where(cs => cs.IsActive && cs.Status == CoworkingStatus.Approved);

            if (capacity.HasValue)
                baseQuery = baseQuery.Where(cs => cs.CapacityTotal >= capacity.Value);

            if (!string.IsNullOrWhiteSpace(location))
                baseQuery = baseQuery.Where(cs =>
                    cs.Address.City.Contains(location) ||
                    cs.Address.Province.Contains(location) ||
                    cs.Address.Street.Contains(location));

            if (areaTypes.Any())
                baseQuery = baseQuery.Where(cs =>
                    cs.Areas.Any(a => a.Available && areaTypes.Contains((int)a.Type)));

            if (minPrice.HasValue || maxPrice.HasValue)
                baseQuery = baseQuery.Where(cs =>
                    cs.Areas.Any(a =>
                        a.Available &&
                        (!minPrice.HasValue || a.PricePerDay >= minPrice.Value) &&
                        (!maxPrice.HasValue || a.PricePerDay <= maxPrice.Value) &&
                        (!areaTypes.Any() || areaTypes.Contains((int)a.Type))
                    ));

            if (services.Any())
                foreach (var service in services)
                {
                    var lowerService = service.Trim().ToLower();
                    baseQuery = baseQuery.Where(cs =>
                        cs.Services.Any(so => so.Name.ToLower().Contains(lowerService)));
                }

            if (benefits.Any())
                foreach (var benefit in benefits)
                {
                    var lowerBenefit = benefit.Trim().ToLower();
                    baseQuery = baseQuery.Where(cs =>
                        cs.Benefits.Any(b => b.Name.ToLower().Contains(lowerBenefit)));
                }

            // 5. Ejecutar query
            var rawSpaces = await baseQuery.ToListAsync();

            // 6. Filtro de disponibilidad por fecha (en memoria)
            if (date.HasValue)
            {
                var reservedAreaIds = await _context.ReservationDetails
                    .Where(rd =>
                        rd.Reservation.Status != ReservationStatus.Cancelled &&
                        date.Value.Date >= rd.Reservation.StartDate.Date &&
                        date.Value.Date <= rd.Reservation.EndDate.Date)
                    .Select(rd => rd.CoworkingAreaId)
                    .Distinct()
                    .ToListAsync();

                rawSpaces = rawSpaces
                    .Where(cs => cs.Areas.Any(a => a.Available && !reservedAreaIds.Contains(a.Id)))
                    .ToList();
            }

            // 7. Favoritos del usuario
            var favoriteSpaceIds = new List<int>();
            if (userId.HasValue)
            {
                favoriteSpaceIds = await _context.FavoriteCoworkingSpaces
                    .Where(f => f.UserId == userId.Value)
                    .Select(f => f.CoworkingSpaceId)
                    .ToListAsync();
            }

            // 8. Mapeo a DTO
            var result = rawSpaces.Select(space =>
            {
                var availableAreas = space.Areas?.Where(a => a.Available).ToList() ?? new List<CoworkingArea>();
                return new CoworkingSpaceListItemDTO
                {
                    Id = space.Id,
                    Name = space.Name,
                    Address = space.Address != null ? new AddressDTO
                    {
                        City = space.Address.City,
                        Province = space.Address.Province,
                        Street = space.Address.Street,
                        Number = space.Address.Number,
                        Country = space.Address.Country,
                        ZipCode = space.Address.ZipCode,
                        Latitude = space.Address.Latitude,
                        Longitude = space.Address.Longitude
                    } : null,
                    CoverPhotoUrl = space.Photos?.FirstOrDefault(p => p.IsCoverPhoto)?.FilePath
                        ?? space.Photos?.FirstOrDefault()?.FilePath,
                    Rate = space.Rate,
                    TotalCapacity = space.CapacityTotal,
                    HasConfiguredAreas = availableAreas.Any(),
                    PrivateOfficesCount = availableAreas.Count(a => a.Type == CoworkingAreaType.PrivateOffice),
                    IndividualDesksCount = availableAreas.Count(a => a.Type == CoworkingAreaType.IndividualDesk),
                    SharedDesksCount = availableAreas.Count(a => a.Type == CoworkingAreaType.SharedDesks),
                    MinPrivateOfficePrice = availableAreas
                        .Where(a => a.Type == CoworkingAreaType.PrivateOffice)
                        .Select(a => (decimal?)a.PricePerDay)
                        .DefaultIfEmpty()
                        .Min(),
                    MaxPrivateOfficePrice = availableAreas
                        .Where(a => a.Type == CoworkingAreaType.PrivateOffice)
                        .Select(a => (decimal?)a.PricePerDay)
                        .DefaultIfEmpty()
                        .Max(),
                    MinIndividualDeskPrice = availableAreas
                        .Where(a => a.Type == CoworkingAreaType.IndividualDesk)
                        .Select(a => (decimal?)a.PricePerDay)
                        .DefaultIfEmpty()
                        .Min(),
                    MaxIndividualDeskPrice = availableAreas
                        .Where(a => a.Type == CoworkingAreaType.IndividualDesk)
                        .Select(a => (decimal?)a.PricePerDay)
                        .DefaultIfEmpty()
                        .Max(),
                    SharedDeskPrice = availableAreas
                        .Where(a => a.Type == CoworkingAreaType.SharedDesks)
                        .Select(a => (decimal?)a.PricePerDay)
                        .FirstOrDefault(),
                    IsFavorite = userId.HasValue && favoriteSpaceIds.Contains(space.Id)
                };
            }).ToList();

            // 9. Retornar estructura compuesta
            return new SpaceFilterResponseDTO
            {
                Spaces = result,
                Metadata = new MetadataDTO
                {
                    RequestedAt = DateTime.UtcNow,
                    Version = "1.1",
                    AppliedFilters = appliedFilters
                }
            };
        }

        public async Task<IEnumerable<CoworkingSpace>> GetByHosterAsync(int hosterId)
        {
            return await _unitOfWork.CoworkingSpaces
                .GetQueryable()
                .Where(cs => cs.HosterId == hosterId)
                .AsNoTracking()
                .ToListAsync();
        }
    }

    // Clase para estadísticas de reservas
    public class ReservationStat
    {
        public int CoworkingSpaceId { get; set; }
        public string SpaceName { get; set; }
        public int TotalAreas { get; set; }
        public int AreasDisponibles { get; set; }
    }

    // Clase para información de debug de espacios
    public class DebugSpaceInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AreaCount { get; set; }
    }

    // Clase para información de áreas reservadas
    public class ReservedAreaInfo
    {
        public int AreaId { get; set; }
        public int CoworkingSpaceId { get; set; }
        public string SpaceName { get; set; }
    }

}