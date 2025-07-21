using CoworkingReservation.Application.DTOs.Address;
using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Application.DTOs.Photo;
using CoworkingReservation.Application.DTOs.CoworkingArea;
using CoworkingReservation.Application.DTOs.Benefit;
using CoworkingReservation.Application.DTOs.Review;
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
using System.Text.Json;

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

  // TODO: Revisar los dto en el servicio porque se estan mezclando
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
                        // Manejar campos opcionales con valores por defecto
                        // Si no se envía StreetOne, usar Street como valor por defecto
                        StreetOne = spaceDto.Address.StreetOne ?? spaceDto.Address.Street ?? "",
                        // Si no se envía StreetTwo, usar string vacío
                        StreetTwo = spaceDto.Address.StreetTwo ?? "",
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

                // Deserialize the JSON strings into lists of integers
                List<int> serviceIds = string.IsNullOrWhiteSpace(spaceDto.Services)
                    ? new List<int>()
                    : JsonSerializer.Deserialize<List<int>>(spaceDto.Services, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<int>();

                List<int> benefitIds = string.IsNullOrWhiteSpace(spaceDto.Benefits)
                    ? new List<int>()
                    : JsonSerializer.Deserialize<List<int>>(spaceDto.Benefits, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<int>();

                List<int> safetyElementIds = string.IsNullOrWhiteSpace(spaceDto.SafetyElements)
                    ? new List<int>()
                    : JsonSerializer.Deserialize<List<int>>(spaceDto.SafetyElements, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<int>();

                List<int> specialFeatureIds = string.IsNullOrWhiteSpace(spaceDto.SpeacialFeatures)
                    ? new List<int>()
                    : JsonSerializer.Deserialize<List<int>>(spaceDto.SpeacialFeatures, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<int>();

                List<CoworkingAreaDTO> areas = string.IsNullOrWhiteSpace(spaceDto.AreasJson)
                    ? new List<CoworkingAreaDTO>()
                    : JsonSerializer.Deserialize<List<CoworkingAreaDTO>>(spaceDto.AreasJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<CoworkingAreaDTO>();

                // Create the coworking space first, without related entities
                await _unitOfWork.CoworkingSpaces.AddAsync(coworkingSpace);
                await _unitOfWork.SaveChangesAsync();

                // Now that we have an ID, we can add the related entities in separate steps

                // Add services using the context directly to avoid EF Core navigation property issues
                if (serviceIds.Any())
                {
                    var services = await _unitOfWork.Services.GetAllAsync(s => serviceIds.Contains(s.Id));
                    foreach (var service in services)
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "INSERT INTO CoworkingSpaceServiceOffered (CoworkingSpacesId, ServicesId) VALUES ({0}, {1})",
                            coworkingSpace.Id, service.Id);
                    }
                }

                // Add benefits using the context directly
                if (benefitIds.Any())
                {
                    var benefits = await _unitOfWork.Benefits.GetAllAsync(b => benefitIds.Contains(b.Id));
                    foreach (var benefit in benefits)
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "INSERT INTO BenefitCoworkingSpace (CoworkingSpacesId, BenefitsId) VALUES ({0}, {1})",
                            coworkingSpace.Id, benefit.Id);
                    }
                }

                // Add safety elements using the context directly
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

                // Add special features using the context directly
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
                // Nueva línea: agregar áreas con servicio externo
                if (areas.Any())
                {
                    await _coworkingAreaService.AddAreasToCoworkingAsync(areas, coworkingSpace.Id, userId);
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
            Console.WriteLine($"🔍 DEBUG: UpdateAsync called with id={id}, hosterId={hosterId}, userRole={userRole}");

            // **1. Validar que el espacio existe (cargando la dirección para poder actualizarla)**
            var coworkingSpace = await _unitOfWork.CoworkingSpaces.GetByIdAsync(id, "Address");
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

            // **2. Validar permisos**
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

            Console.WriteLine("🔍 DEBUG: Permissions validated successfully");

            // **3. Actualizar datos básicos del espacio**
            coworkingSpace.Name = dto.Name;
            coworkingSpace.Description = dto.Description;
            coworkingSpace.CapacityTotal = dto.CapacityTotal;

            // **4. Actualizar dirección directamente en la entidad cargada**
            if (dto.Address != null)
            {
                var address = coworkingSpace.Address;
                if (address != null)
                {
                    Console.WriteLine($"🔍 DEBUG: Updating address - Old City: {address.City}, New City: {dto.Address.City}");
                    Console.WriteLine($"🔍 DEBUG: Updating address - Old Street: {address.Street}, New Street: {dto.Address.Street}");
                    
                    address.City = dto.Address.City;
                    address.Country = dto.Address.Country;
                    address.Apartment = dto.Address.Apartment;
                    address.Floor = dto.Address.Floor;
                    address.Number = dto.Address.Number;
                    address.Province = dto.Address.Province;
                    address.Street = dto.Address.Street;
                    // Manejar campos opcionales con valores por defecto
                    // Si no se envía StreetOne, usar Street como valor por defecto
                    address.StreetOne = dto.Address.StreetOne ?? dto.Address.Street ?? "";
                    // Si no se envía StreetTwo, usar string vacío
                    address.StreetTwo = dto.Address.StreetTwo ?? "";
                    address.ZipCode = dto.Address.ZipCode;
                    address.Latitude = dto.Address.Latitude;
                    address.Longitude = dto.Address.Longitude;
                    
                    Console.WriteLine($"🔍 DEBUG: Address properties updated - City: {address.City}, Street: {address.Street}");
                }
                else
                {
                    Console.WriteLine("🔍 DEBUG: Address is null, cannot update");
                }
            }
            else
            {
                Console.WriteLine("🔍 DEBUG: DTO Address is null, skipping address update");
            }

            Console.WriteLine("🔍 DEBUG: Basic data and address updated");

            // **5. Actualizar relaciones many-to-many por separado**
            if (dto.Services?.Any() == true)
            {
                await UpdateCoworkingSpaceServices(id, dto.Services.ToList());
                Console.WriteLine("🔍 DEBUG: Services updated");
            }

            if (dto.Benefits?.Any() == true)
            {
                await UpdateCoworkingSpaceBenefits(id, dto.Benefits.ToList());
                Console.WriteLine("🔍 DEBUG: Benefits updated");
            }

            if (dto.SafetyElements?.Any() == true)
            {
                await UpdateCoworkingSpaceSafetyElements(id, dto.SafetyElements.ToList());
                Console.WriteLine("🔍 DEBUG: SafetyElements updated");
            }

            if (dto.SpecialFeatures?.Any() == true)
            {
                await UpdateCoworkingSpaceSpecialFeatures(id, dto.SpecialFeatures.ToList());
                Console.WriteLine("🔍 DEBUG: SpecialFeatures updated");
            }

            // **6. Actualizar áreas**
            if (dto.Areas?.Any() == true)
            {
                await UpdateCoworkingSpaceAreas(id, dto.Areas);
                Console.WriteLine("🔍 DEBUG: Areas updated");
            }

            // **7. Guardar cambios del espacio principal**
            await _unitOfWork.CoworkingSpaces.UpdateAsync(coworkingSpace);
            await _unitOfWork.SaveChangesAsync();

            Console.WriteLine("🔍 DEBUG: All changes saved successfully");

            // **8. Log de auditoría exitoso**
            await _unitOfWork.AuditLogs.LogAsync(new AuditLog
            {
                Action = "UpdateCoworkingSpace",
                UserId = hosterId,
                UserRole = userRole,
                Success = true,
                Description = $"Coworking space {id} updated successfully by user {hosterId}."
            });

            Console.WriteLine("🔍 DEBUG: UpdateAsync completed successfully");
        }

        /// <summary>
        /// Actualiza los servicios de un espacio de coworking usando SQL directo
        /// </summary>
        private async Task UpdateCoworkingSpaceServices(int coworkingSpaceId, List<int> serviceIds)
        {
            // Eliminar servicios existentes
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM CoworkingSpaceServiceOffered WHERE CoworkingSpacesId = {0}", 
                coworkingSpaceId);

            // Insertar nuevos servicios
            if (serviceIds.Any())
            {
                var insertValues = string.Join(",", serviceIds.Select(id => $"({coworkingSpaceId}, {id})"));
                await _context.Database.ExecuteSqlRawAsync(
                    $"INSERT INTO CoworkingSpaceServiceOffered (CoworkingSpacesId, ServicesId) VALUES {insertValues}");
            }
        }

        /// <summary>
        /// Actualiza los beneficios de un espacio de coworking usando SQL directo
        /// </summary>
        private async Task UpdateCoworkingSpaceBenefits(int coworkingSpaceId, List<int> benefitIds)
        {
            // Eliminar beneficios existentes
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM BenefitCoworkingSpace WHERE CoworkingSpacesId = {0}", 
                coworkingSpaceId);

            // Insertar nuevos beneficios
            if (benefitIds.Any())
            {
                var insertValues = string.Join(",", benefitIds.Select(id => $"({id}, {coworkingSpaceId})"));
                await _context.Database.ExecuteSqlRawAsync(
                    $"INSERT INTO BenefitCoworkingSpace (BenefitsId, CoworkingSpacesId) VALUES {insertValues}");
            }
        }

        /// <summary>
        /// Actualiza los elementos de seguridad de un espacio de coworking usando SQL directo
        /// </summary>
        private async Task UpdateCoworkingSpaceSafetyElements(int coworkingSpaceId, List<int> safetyElementIds)
        {
            // Eliminar elementos de seguridad existentes
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM CoworkingSpaceSafetyElement WHERE CoworkingSpacesId = {0}", 
                coworkingSpaceId);

            // Insertar nuevos elementos de seguridad
            if (safetyElementIds.Any())
            {
                var insertValues = string.Join(",", safetyElementIds.Select(id => $"({coworkingSpaceId}, {id})"));
                await _context.Database.ExecuteSqlRawAsync(
                    $"INSERT INTO CoworkingSpaceSafetyElement (CoworkingSpacesId, SafetyElementsId) VALUES {insertValues}");
            }
        }

        /// <summary>
        /// Actualiza las características especiales de un espacio de coworking usando SQL directo
        /// </summary>
        private async Task UpdateCoworkingSpaceSpecialFeatures(int coworkingSpaceId, List<int> specialFeatureIds)
        {
            // Eliminar características especiales existentes
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM CoworkingSpaceSpecialFeature WHERE CoworkingSpacesId = {0}", 
                coworkingSpaceId);

            // Insertar nuevas características especiales
            if (specialFeatureIds.Any())
            {
                var insertValues = string.Join(",", specialFeatureIds.Select(id => $"({coworkingSpaceId}, {id})"));
                await _context.Database.ExecuteSqlRawAsync(
                    $"INSERT INTO CoworkingSpaceSpecialFeature (CoworkingSpacesId, SpecialFeaturesId) VALUES {insertValues}");
            }
        }

        /// <summary>
        /// Actualiza las áreas de un espacio de coworking
        /// </summary>
        private async Task UpdateCoworkingSpaceAreas(int coworkingSpaceId, List<UpdateCoworkingAreaDTO> areas)
        {
            foreach (var areaDto in areas)
            {
                var existingArea = await _unitOfWork.CoworkingAreas.GetByIdAsync(areaDto.Id);
                if (existingArea != null && existingArea.CoworkingSpaceId == coworkingSpaceId)
                {
                    // Actualizar área existente
                    existingArea.Type = areaDto.Type;
                    existingArea.Description = areaDto.Description;
                    existingArea.Capacity = areaDto.Capacity;
                    existingArea.PricePerDay = areaDto.PricePerDay;
                    existingArea.Available = areaDto.Available;
                    
                    await _unitOfWork.CoworkingAreas.UpdateAsync(existingArea);
                }
            }
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
            return await _unitOfWork.CoworkingSpaces.GetFilteredLightweightAsync(null, null, userId);
        }

        public async Task<IEnumerable<CoworkingSpaceListItemDTO>> GetFilteredLightweightAsync(int? capacity, string? location, int? userId = null)
        {
            return await _unitOfWork.CoworkingSpaces.GetFilteredLightweightAsync(capacity, location, userId);
        }

        public async Task<IEnumerable<CoworkingSpaceListItemDTO>> GetAdvancedFilteredAsync(
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
            try
            {
                // Log para propósitos de depuración
                Console.WriteLine("GetAdvancedFilteredAsync: Implementando filtrado avanzado");
                Console.WriteLine($"Parámetros recibidos: capacity={capacity}, location={location}, date={date}");
                Console.WriteLine($"minPrice={minPrice}, maxPrice={maxPrice}");
                Console.WriteLine($"individualDesk={individualDesk}, privateOffice={privateOffice}, hybridSpace={hybridSpace}");
                Console.WriteLine($"services={string.Join(", ", services ?? new List<string>())}");
                Console.WriteLine($"benefits={string.Join(", ", benefits ?? new List<string>())}");

                // Asegurarnos de que las listas no sean null
                services = services ?? new List<string>();
                benefits = benefits ?? new List<string>();

                // Mapeo de tipos de área según el enum
                var areaTypes = new List<int>();
                if (individualDesk == true) areaTypes.Add((int)CoworkingAreaType.IndividualDesk); // 3
                if (privateOffice == true) areaTypes.Add((int)CoworkingAreaType.PrivateOffice);   // 2
                if (hybridSpace == true) areaTypes.Add((int)CoworkingAreaType.SharedDesks);       // 1

                // Construir la consulta base para espacios activos y aprobados
                var baseQuery = $@"
                    SELECT DISTINCT cs.Id 
                    FROM CoworkingSpaces cs
                    WHERE cs.IsActive = 1 AND cs.Status = 1";

                // Agregar filtro de capacidad si existe
                if (capacity.HasValue)
                {
                    baseQuery += $" AND cs.CapacityTotal >= {capacity.Value}";
                }

                // Agregar filtro de ubicación si existe
                if (!string.IsNullOrEmpty(location))
                {
                    baseQuery += $@" AND EXISTS (
                        SELECT 1 FROM Addresses a 
                        WHERE a.Id = cs.Id AND (
                            a.City LIKE '%{location.Replace("'", "''")}%' OR 
                            a.Province LIKE '%{location.Replace("'", "''")}%' OR 
                            a.Street LIKE '%{location.Replace("'", "''")}%'
                        )
                    )";
                }

                // Agregar filtro por tipo de área si se especificó
                if (areaTypes.Any())
                {
                    baseQuery += $@" AND EXISTS (
                        SELECT 1 FROM CoworkingAreas ca 
                        WHERE ca.CoworkingSpaceId = cs.Id 
                        AND ca.Available = 1 
                        AND ca.Type IN ({string.Join(",", areaTypes)})
                    )";
                }

                // Agregar filtro por precio si se especificó
                if (minPrice.HasValue || maxPrice.HasValue)
                {
                    baseQuery += $@" AND EXISTS (
                        SELECT 1 FROM CoworkingAreas ca 
                        WHERE ca.CoworkingSpaceId = cs.Id 
                        AND ca.Available = 1";

                    // Condiciones de precio
                    if (minPrice.HasValue)
                    {
                        baseQuery += $" AND ca.PricePerDay >= {minPrice.Value}";
                    }

                    if (maxPrice.HasValue)
                    {
                        baseQuery += $" AND ca.PricePerDay <= {maxPrice.Value}";
                    }

                    // Si hay filtro por tipo, relacionarlo con precio
                    if (areaTypes.Any())
                    {
                        baseQuery += $" AND ca.Type IN ({string.Join(",", areaTypes)})";
                    }

                    baseQuery += ")";
                }

                // Agregar filtro por servicios si se especificaron
                if (services.Any())
                {
                    foreach (var service in services.Where(s => !string.IsNullOrWhiteSpace(s)))
                    {
                        var serviceNameSanitized = service.ToLower().Replace("'", "''");
                        baseQuery += $@" AND EXISTS (
                            SELECT 1 FROM CoworkingSpaceServiceOffered csso
                            JOIN ServicesOffered so ON so.Id = csso.ServicesId
                            WHERE csso.CoworkingSpacesId = cs.Id
                            AND LOWER(so.Name) LIKE '%{serviceNameSanitized}%'
                        )";
                    }
                }

                // Agregar filtro por beneficios si se especificaron
                if (benefits.Any())
                {
                    foreach (var benefit in benefits.Where(b => !string.IsNullOrWhiteSpace(b)))
                    {
                        var benefitNameSanitized = benefit.ToLower().Replace("'", "''");
                        baseQuery += $@" AND EXISTS (
                            SELECT 1 FROM BenefitCoworkingSpace bcs
                            JOIN Benefits b ON b.Id = bcs.BenefitsId
                            WHERE bcs.CoworkingSpacesId = cs.Id
                            AND LOWER(b.Name) LIKE '%{benefitNameSanitized}%'
                        )";
                    }
                }

                // Enfoque completamente nuevo para filtrado por fecha
                if (date.HasValue)
                {
                    var dateStr = date.Value.ToString("yyyy-MM-dd");
                    Console.WriteLine($"Aplicando nuevo filtro de fecha para: {dateStr}");

                    // *** IMPORTANTE: Ignoramos el filtro por fecha en la consulta SQL inicial ***
                    // En su lugar, obtendremos TODOS los espacios que cumplen otros criterios
                    // y luego filtraremos por fecha después, en memoria
                }

                // Ejecutar la consulta y obtener los IDs
                Console.WriteLine($"Query SQL: {baseQuery}");

                // Agregar log para verificar las tablas y columnas disponibles
                Console.WriteLine("Verificando esquema de tablas...");
                try
                {
                    var tableInfo = await _context.Database.SqlQueryRaw<string>(
                        "SELECT name FROM sys.tables WHERE name LIKE '%Benefit%' OR name LIKE '%Service%'"
                    ).ToListAsync();

                    Console.WriteLine("Tablas encontradas: " + string.Join(", ", tableInfo));

                    // Verificar columnas en BenefitCoworkingSpace
                    var columnInfo = await _context.Database.SqlQueryRaw<string>(
                        "SELECT c.name FROM sys.columns c INNER JOIN sys.tables t ON c.object_id = t.object_id WHERE t.name = 'BenefitCoworkingSpace'"
                    ).ToListAsync();

                    Console.WriteLine("Columnas en BenefitCoworkingSpace: " + string.Join(", ", columnInfo));

                    // Si hay una fecha, verifiquemos las reservas para esa fecha
                    if (date.HasValue)
                    {
                        var dateStr = date.Value.ToString("yyyy-MM-dd");
                        try
                        {
                            // Consulta simplificada para depuración
                            var debugQuery = $@"
                                SELECT cs.Id, cs.Name, COUNT(ca.Id) as AreaCount 
                                FROM CoworkingSpaces cs
                                JOIN CoworkingAreas ca ON ca.CoworkingSpaceId = cs.Id 
                                WHERE ca.Available = 1
                                GROUP BY cs.Id, cs.Name";

                            var spacesWithAreas = await _context.Database.SqlQueryRaw<DebugSpaceInfo>(debugQuery).ToListAsync();
                            Console.WriteLine($"Espacios con áreas disponibles ({spacesWithAreas.Count}):");
                            foreach (var space in spacesWithAreas)
                            {
                                Console.WriteLine($"  {space.Name} (ID: {space.Id}): {space.AreaCount} áreas");
                            }

                            // Consulta para áreas reservadas en la fecha específica
                            var reservedQuery = $@"
                                SELECT ca.Id as AreaId, ca.CoworkingSpaceId, cs.Name as SpaceName
                                FROM ReservationDetails rd 
                                JOIN Reservations r ON rd.ReservationId = r.Id
                                JOIN CoworkingAreas ca ON rd.CoworkingAreaId = ca.Id
                                JOIN CoworkingSpaces cs ON ca.CoworkingSpaceId = cs.Id
                                WHERE r.Status != 2 
                                AND '{dateStr}' BETWEEN CONVERT(DATE, r.StartDate) AND CONVERT(DATE, r.EndDate)";

                            var reservedAreas = await _context.Database.SqlQueryRaw<ReservedAreaInfo>(reservedQuery).ToListAsync();
                            Console.WriteLine($"Áreas reservadas para fecha {dateStr} ({reservedAreas.Count}):");
                            foreach (var area in reservedAreas)
                            {
                                Console.WriteLine($"  Espacio {area.SpaceName} (ID: {area.CoworkingSpaceId}): Área {area.AreaId} reservada");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error en consulta de depuración: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al verificar esquema: " + ex.Message);
                }

                var spaceIds = await _context.Database.SqlQueryRaw<int>(baseQuery).ToListAsync();

                if (!spaceIds.Any())
                {
                    Console.WriteLine("No se encontraron espacios que cumplan con los criterios");
                    return new List<CoworkingSpaceListItemDTO>();
                }

                Console.WriteLine($"Encontrados {spaceIds.Count} espacios después de aplicar todos los filtros");

                // Obtener los datos completos de los espacios filtrados
                var query = _context.CoworkingSpaces
                    .AsNoTracking()
                    .Include(cs => cs.Address)
                    .Include(cs => cs.Areas)
                    .Include(cs => cs.Photos)
                    .Where(cs => spaceIds.Contains(cs.Id));

                var rawSpaces = await query.ToListAsync();

                // Depuración de los resultados
                foreach (var space in rawSpaces)
                {
                    Console.WriteLine($"Space {space.Id}: {space.Name}");

                    var areas = space.Areas.Where(a => a.Available).ToList();
                    Console.WriteLine($"  Areas: {areas.Count} disponibles");
                    foreach (var area in areas)
                    {
                        Console.WriteLine($"    - {area.Type}: ${area.PricePerDay}");
                    }
                }

                // Si hay un usuario autenticado, obtener sus espacios favoritos
                List<int> userFavorites = new List<int>();
                if (userId.HasValue)
                {
                    userFavorites = await _context.FavoriteCoworkingSpaces
                        .Where(f => f.UserId == userId.Value)
                        .Select(f => f.CoworkingSpaceId)
                        .ToListAsync();
                }

                // Mapear a DTOs
                var data = rawSpaces.Select(cs => new
                {
                    cs.Id,
                    cs.Name,
                    Address = cs.Address,
                    CoverPhotoUrl = cs.Photos.Where(p => p.IsCoverPhoto).Select(p => p.FilePath).FirstOrDefault(),
                    Rate = cs.Rate,
                    Areas = cs.Areas.Select(a => new { a.Type, a.Capacity, a.PricePerDay, a.Available, a.Id }).ToList(),
                    TotalCapacity = cs.CapacityTotal,
                    HasAreas = cs.Areas.Any(),
                    IsFavorite = userId.HasValue && userFavorites.Contains(cs.Id)
                }).ToList();

                // Crear la lista de resultados
                var result = data.Select(cs => new CoworkingSpaceListItemDTO
                {
                    Id = cs.Id,
                    Name = cs.Name,
                    Address = cs.Address != null ? new Domain.DTOs.AddressDTO
                    {
                        City = cs.Address.City,
                        Province = cs.Address.Province,
                        Street = cs.Address.Street,
                        Number = cs.Address.Number,
                        Country = cs.Address.Country,
                        ZipCode = cs.Address.ZipCode,
                        Latitude = cs.Address?.Latitude,
                        Longitude = cs.Address?.Longitude
                    } : null,
                    CoverPhotoUrl = cs.CoverPhotoUrl,
                    Rate = cs.Rate,
                    TotalCapacity = cs.TotalCapacity,
                    HasConfiguredAreas = cs.HasAreas,
                    PrivateOfficesCount = cs.HasAreas ? cs.Areas.Count(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available) : 0,
                    IndividualDesksCount = cs.HasAreas ? cs.Areas.Count(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available) : 0,
                    SharedDesksCount = cs.HasAreas ? cs.Areas.Count(a => a.Type == CoworkingAreaType.SharedDesks && a.Available) : 0,

                    MinPrivateOfficePrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available)
                        ? cs.Areas.Where(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available).Min(a => a.PricePerDay)
                        : null,
                    MaxPrivateOfficePrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available)
                        ? cs.Areas.Where(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available).Max(a => a.PricePerDay)
                        : null,

                    MinIndividualDeskPrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available)
                        ? cs.Areas.Where(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available).Min(a => a.PricePerDay)
                        : null,
                    MaxIndividualDeskPrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available)
                        ? cs.Areas.Where(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available).Max(a => a.PricePerDay)
                        : null,

                    SharedDeskPrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.SharedDesks && a.Available)
                        ? cs.Areas.Where(a => a.Type == CoworkingAreaType.SharedDesks && a.Available).Min(a => a.PricePerDay)
                        : null,

                    IsFavorite = cs.IsFavorite
                }).ToList();

                // Si se filtró por fecha, ahora aplicamos el filtro en memoria
                if (date.HasValue)
                {
                    var dateStr = date.Value.ToString("yyyy-MM-dd");
                    var dateValue = date.Value.Date;

                    Console.WriteLine($"Aplicando filtro de fecha para: {dateStr}");

                    // Enfoque simplificado: primero obtener todas las áreas reservadas para la fecha
                    var reservedAreaIds = await _context.Database.SqlQueryRaw<int>($@"
                        SELECT rd.CoworkingAreaId 
                        FROM ReservationDetails rd 
                        JOIN Reservations r ON rd.ReservationId = r.Id
                        WHERE r.Status != {(int)ReservationStatus.Cancelled}
                        AND CONVERT(DATE, '{dateStr}') BETWEEN CONVERT(DATE, r.StartDate) AND CONVERT(DATE, r.EndDate)
                    ").ToListAsync();

                    Console.WriteLine($"Áreas reservadas para {dateStr}: {reservedAreaIds.Count}");

                    // Filtrar espacios que tengan al menos un área disponible para la fecha
                    var filteredResult = new List<CoworkingSpaceListItemDTO>();

                    foreach (var space in result)
                    {
                        // Obtener todas las áreas disponibles de este espacio
                        var availableAreas = await _context.Database.SqlQueryRaw<int>($@"
                            SELECT ca.Id 
                            FROM CoworkingAreas ca
                            WHERE ca.CoworkingSpaceId = {space.Id}
                            AND ca.Available = 1
                            AND ca.Id NOT IN ({(reservedAreaIds.Any() ? string.Join(",", reservedAreaIds) : "0")})
                        ").ToListAsync();

                        if (availableAreas.Any())
                        {
                            Console.WriteLine($"Espacio {space.Id} ({space.Name}) tiene {availableAreas.Count} áreas disponibles en {dateStr}");
                            filteredResult.Add(space);
                        }
                        else
                        {
                            Console.WriteLine($"Espacio {space.Id} ({space.Name}) NO tiene áreas disponibles en {dateStr}");
                        }
                    }

                    Console.WriteLine($"Total espacios disponibles en {dateStr}: {filteredResult.Count} de {result.Count}");
                    return filteredResult;
                }

                // Si no había filtro de fecha, devolvemos todos los resultados
                return result;
            }
            catch (Exception ex)
            {
                // Log detallado de la excepción para depuración
                Console.WriteLine($"ERROR en GetAdvancedFilteredAsync: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                }

                throw; // Relanzar la excepción para que se maneje en el controlador
            }
        }

        public async Task<IEnumerable<CoworkingSpace>> GetByHosterAsync(int hosterId)
        {
            return await _unitOfWork.CoworkingSpaces
                .GetQueryable()
                .Where(cs => cs.HosterId == hosterId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<CoworkingSpaceListItemDTO>> GetMyCoworkingsAsync(int hosterId)
        {
            try
            {
                // Obtener todos los coworking spaces del hoster con sus relaciones
                var spaces = await _context.CoworkingSpaces
                    .AsNoTracking()
                    .Include(cs => cs.Address)
                    .Include(cs => cs.Areas)
                    .Include(cs => cs.Photos)
                    .Where(cs => cs.HosterId == hosterId)
                    .ToListAsync();

                // Obtener espacios favoritos del usuario (aunque sea el mismo hoster, por consistencia)
                var userFavorites = await _context.FavoriteCoworkingSpaces
                    .Where(f => f.UserId == hosterId)
                    .Select(f => f.CoworkingSpaceId)
                    .ToListAsync();

                // Mapear a DTOs usando la misma lógica que GetAllLightweightAsync
                var result = spaces.Select(cs => new CoworkingSpaceListItemDTO
                {
                    Id = cs.Id,
                    Name = cs.Name,
                    Address = cs.Address != null ? new Domain.DTOs.AddressDTO
                    {
                        City = cs.Address.City,
                        Province = cs.Address.Province,
                        Street = cs.Address.Street,
                        Number = cs.Address.Number,
                        Country = cs.Address.Country,
                        ZipCode = cs.Address.ZipCode,
                        Latitude = cs.Address?.Latitude,
                        Longitude = cs.Address?.Longitude
                    } : null,
                    CoverPhotoUrl = cs.Photos.Where(p => p.IsCoverPhoto).Select(p => p.FilePath).FirstOrDefault(),
                    Rate = cs.Rate,
                    TotalCapacity = cs.CapacityTotal,
                    HasConfiguredAreas = cs.Areas.Any(),
                    PrivateOfficesCount = cs.Areas.Count(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available),
                    IndividualDesksCount = cs.Areas.Count(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available),
                    SharedDesksCount = cs.Areas.Count(a => a.Type == CoworkingAreaType.SharedDesks && a.Available),

                    MinPrivateOfficePrice = cs.Areas.Any(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available)
                        ? cs.Areas.Where(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available).Min(a => a.PricePerDay)
                        : null,
                    MaxPrivateOfficePrice = cs.Areas.Any(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available)
                        ? cs.Areas.Where(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available).Max(a => a.PricePerDay)
                        : null,

                    MinIndividualDeskPrice = cs.Areas.Any(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available)
                        ? cs.Areas.Where(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available).Min(a => a.PricePerDay)
                        : null,
                    MaxIndividualDeskPrice = cs.Areas.Any(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available)
                        ? cs.Areas.Where(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available).Max(a => a.PricePerDay)
                        : null,

                    SharedDeskPrice = cs.Areas.Any(a => a.Type == CoworkingAreaType.SharedDesks && a.Available)
                        ? cs.Areas.Where(a => a.Type == CoworkingAreaType.SharedDesks && a.Available).Min(a => a.PricePerDay)
                        : null,

                    IsFavorite = userFavorites.Contains(cs.Id)
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetMyCoworkingsAsync: {ex.Message}");
                throw;
            }
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