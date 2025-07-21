using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Application.DTOs.CoworkingArea;
using CoworkingReservation.Application.DTOs.Address;
using CoworkingReservation.Application.DTOs.Photo;
using CoworkingReservation.Application.DTOs.Review;
using CoworkingReservation.Application.DTOs.SafetyElementDTO;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoworkingReservation.Application.Services
{
    /// <summary>
    /// Servicio para la gestión de áreas dentro de espacios de coworking.
    /// </summary>
    public class CoworkingAreaService : ICoworkingAreaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CoworkingAreaService> _logger;
        private readonly ApplicationDbContext _context;

        public CoworkingAreaService(IUnitOfWork unitOfWork, ILogger<CoworkingAreaService> logger, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Crear Área

        /// <summary>
        /// Crea una nueva área dentro de un espacio de coworking.
        /// </summary>
        public async Task<CoworkingArea> CreateAsync(CreateCoworkingAreaDTO areaDto, int coworkingSpaceId, int hosterId)
        {
            var coworkingSpace = await _unitOfWork.CoworkingSpaces.GetByIdAsync(coworkingSpaceId);
            if (coworkingSpace == null)
                throw new KeyNotFoundException("Coworking space not found.");

            if (coworkingSpace.HosterId != hosterId)
                throw new UnauthorizedAccessException("You do not have permission to add an area.");

            var currentTotalCapacity = await GetTotalCapacityByCoworkingSpaceIdAsync(coworkingSpaceId);
            if (currentTotalCapacity + areaDto.Capacity > coworkingSpace.CapacityTotal)
                throw new InvalidOperationException("Total area capacity exceeds the coworking space's maximum capacity.");

            var area = new CoworkingArea
            {
                Type = areaDto.Type,
                Description = areaDto.Description,
                Available = areaDto.Available,
                Capacity = areaDto.Capacity,
                PricePerDay = areaDto.PricePerDay,
                CoworkingSpaceId = coworkingSpaceId
            };

            await _unitOfWork.CoworkingAreas.AddAsync(area);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Created coworking area {area.Id} in space {coworkingSpaceId}");

            return area;
        }
        public async Task AddAreasToCoworkingAsync(IEnumerable<CoworkingAreaDTO> areaDtos, int coworkingSpaceId, int hosterId)
        {
            if (areaDtos == null || !areaDtos.Any())
                return;

            int currentTotalCapacity = await GetTotalCapacityByCoworkingSpaceIdAsync(coworkingSpaceId);
            int totalNewCapacity = areaDtos.Sum(a => a.Capacity);

            var coworkingSpace = await _unitOfWork.CoworkingSpaces.GetByIdAsync(coworkingSpaceId);
            if (coworkingSpace == null)
                throw new KeyNotFoundException("Coworking space not found.");

            if (coworkingSpace.HosterId != hosterId)
                throw new UnauthorizedAccessException("You do not have permission to add areas to this space.");

            if (currentTotalCapacity + totalNewCapacity > coworkingSpace.CapacityTotal)
                throw new InvalidOperationException("Total capacity of areas exceeds coworking space capacity.");

            var areas = areaDtos.Select(dto => new CoworkingArea
            {
                Capacity = dto.Capacity,
                Description = dto.Description,
                PricePerDay = dto.PricePerDay,
                Available = dto.Available,
                Type = dto.Type,
                CoworkingSpaceId = coworkingSpaceId
            });

            await _unitOfWork.CoworkingAreas.AddRangeAsync(areas);
            await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Actualizar Área

        /// <summary>
        /// Actualiza un área de coworking existente.
        /// </summary>
        public async Task UpdateAsync(int id, UpdateCoworkingAreaDTO dto, int hosterId)
        {
            var area = await _unitOfWork.CoworkingAreas.GetByIdAsync(id);
            if (area == null)
                throw new KeyNotFoundException("Coworking area not found.");

            var coworkingSpace = await _unitOfWork.CoworkingSpaces.GetByIdAsync(area.CoworkingSpaceId);
            if (coworkingSpace.HosterId != hosterId)
                throw new UnauthorizedAccessException("You do not have permission to update this area.");

            area.Description = dto.Description;
            area.Capacity = dto.Capacity;
            area.PricePerDay = dto.PricePerDay;
            area.Available = dto.Available;
            area.Type = dto.Type;

            await _unitOfWork.CoworkingAreas.UpdateAsync(area);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Updated coworking area {id}");
        }

        #endregion

        #region Eliminar Área

        /// <summary>
        /// Elimina un área de coworking.
        /// </summary>
        public async Task DeleteAsync(int id, int hosterId)
        {
            var area = await _unitOfWork.CoworkingAreas.GetByIdAsync(id);
            if (area == null)
                throw new KeyNotFoundException("Coworking area not found.");

            var coworkingSpace = await _unitOfWork.CoworkingSpaces.GetByIdAsync(area.CoworkingSpaceId);
            if (coworkingSpace.HosterId != hosterId)
                throw new UnauthorizedAccessException("You do not have permission to delete this area.");

            await _unitOfWork.CoworkingAreas.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Deleted coworking area {id}");
        }
        
        /// <summary>
        /// Cambia el estado de disponibilidad de un área (activar/desactivar).
        /// </summary>
        public async Task SetAvailabilityAsync(int id, int hosterId, bool available)
        {
            var area = await _unitOfWork.CoworkingAreas.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Coworking area not found.");

            var coworkingSpace = await _unitOfWork.CoworkingSpaces.GetByIdAsync(area.CoworkingSpaceId);
            if (coworkingSpace.HosterId != hosterId)
                throw new UnauthorizedAccessException("You do not have permission to modify this area.");

            if (area.Available == available)
                throw new InvalidOperationException($"The area is already {(available ? "enabled" : "disabled")}.");

            area.Available = available;

            await _unitOfWork.CoworkingAreas.UpdateAsync(area);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Coworking area {id} was {(available ? "enabled" : "disabled")} by hoster {hosterId}");
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene todas las áreas de un espacio de coworking.
        /// </summary>
        public async Task<IEnumerable<CoworkingArea>> GetByCoworkingSpaceIdAsync(int coworkingSpaceId)
        {
            return await _unitOfWork.CoworkingAreas
                .GetQueryable()
                .Where(ca => ca.CoworkingSpaceId == coworkingSpaceId && ca.Available)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene un área de coworking por su ID.
        /// </summary>
        public async Task<CoworkingArea> GetByIdAsync(int id)
        {
            var area = await _unitOfWork.CoworkingAreas.GetByIdAsync(id);
            if (area == null)
                throw new KeyNotFoundException("Coworking area not found.");
            return area;
        }

        /// <summary>
        /// Verifica si un área existe.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _unitOfWork.CoworkingAreas.ExistsAsync(ca => ca.Id == id);
        }

        /// <summary>
        /// Obtiene todas las áreas de coworking.
        /// </summary>
        public async Task<IEnumerable<CoworkingArea>> GetAllAsync()
        {
            return await _unitOfWork.CoworkingAreas.GetAllAsync();
        }

        /// <summary>
        /// Obtiene la capacidad total de todas las áreas de un coworking space.
        /// </summary>
        public async Task<int> GetTotalCapacityByCoworkingSpaceIdAsync(int coworkingSpaceId)
        {
            return await _unitOfWork.CoworkingAreas
                .GetQueryable()
                .Where(ca => ca.CoworkingSpaceId == coworkingSpaceId)
                .AsNoTracking()
                .SumAsync(ca => ca.Capacity);
        }

        /// <summary>
        /// Verifica si un coworking tiene suficiente capacidad disponible.
        /// </summary>
        public async Task<bool> HasAvailableCapacity(int coworkingSpaceId, int requiredCapacity)
        {
            var totalCapacity = await GetTotalCapacityByCoworkingSpaceIdAsync(coworkingSpaceId);
            return totalCapacity >= requiredCapacity;
        }

        /// <summary>
        /// Obtiene información completa de un espacio de coworking para edición.
        /// </summary>
        public async Task<CoworkingSpaceEditDTO> GetCoworkingSpaceForEditAsync(int coworkingSpaceId)
        {
            try
            {
                // Obtener el coworking space básico con Address y Photos
                var coworkingSpace = await _context.CoworkingSpaces
                    .AsNoTracking()
                    .Include(cs => cs.Address)
                    .Include(cs => cs.Photos)
                    .Include(cs => cs.Areas)
                    .FirstOrDefaultAsync(cs => cs.Id == coworkingSpaceId);

                if (coworkingSpace == null)
                {
                    throw new KeyNotFoundException($"Coworking space with ID {coworkingSpaceId} not found.");
                }

                // Obtener servicios por separado usando consulta directa
                var services = await _context.ServicesOffered
                    .FromSqlRaw(@"
                        SELECT s.* FROM ServicesOffered s
                        INNER JOIN CoworkingSpaceServiceOffered css ON s.Id = css.ServicesId
                        WHERE css.CoworkingSpacesId = {0}", coworkingSpaceId)
                    .AsNoTracking()
                    .ToListAsync();

                // Obtener beneficios por separado
                var benefits = await _context.Benefits
                    .FromSqlRaw(@"
                        SELECT b.* FROM Benefits b
                        INNER JOIN BenefitCoworkingSpace bcs ON b.Id = bcs.BenefitsId
                        WHERE bcs.CoworkingSpacesId = {0}", coworkingSpaceId)
                    .AsNoTracking()
                    .ToListAsync();

                // Obtener elementos de seguridad por separado
                var safetyElements = await _context.SafetyElements
                    .FromSqlRaw(@"
                        SELECT se.* FROM SafetyElements se
                        INNER JOIN CoworkingSpaceSafetyElement csse ON se.Id = csse.SafetyElementsId
                        WHERE csse.CoworkingSpacesId = {0}", coworkingSpaceId)
                    .AsNoTracking()
                    .ToListAsync();

                // Obtener características especiales por separado
                var specialFeatures = await _context.SpecialFeatures
                    .FromSqlRaw(@"
                        SELECT sf.* FROM SpecialFeatures sf
                        INNER JOIN CoworkingSpaceSpecialFeature cssf ON sf.Id = cssf.SpecialFeaturesId
                        WHERE cssf.CoworkingSpacesId = {0}", coworkingSpaceId)
                    .AsNoTracking()
                    .ToListAsync();

                // Obtener reviews con información del usuario
                var reviews = await _context.Reviews
                    .Where(r => r.CoworkingSpaceId == coworkingSpaceId)
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

                // Mapear a DTO
                var result = new CoworkingSpaceEditDTO
                {
                    // Coworking Space Basic Information
                    Id = coworkingSpace.Id,
                    Name = coworkingSpace.Name,
                    Description = coworkingSpace.Description,
                    CapacityTotal = coworkingSpace.CapacityTotal,
                    IsActive = coworkingSpace.IsActive,
                    Rate = coworkingSpace.Rate,
                    Status = coworkingSpace.Status,
                    HosterId = coworkingSpace.HosterId,

                    // Address
                    Address = coworkingSpace.Address != null ? new AddressDTO
                    {
                        Street = coworkingSpace.Address.Street,
                        Number = coworkingSpace.Address.Number,
                        Apartment = coworkingSpace.Address.Apartment,
                        Floor = coworkingSpace.Address.Floor,
                        City = coworkingSpace.Address.City,
                        Province = coworkingSpace.Address.Province,
                        Country = coworkingSpace.Address.Country,
                        ZipCode = coworkingSpace.Address.ZipCode,
                        Latitude = coworkingSpace.Address.Latitude,
                        Longitude = coworkingSpace.Address.Longitude
                    } : new AddressDTO(),

                    // Photos
                    Photos = coworkingSpace.Photos?.Select(p => new PhotoResponseDTO
                    {
                        FileName = p.FileName,
                        IsCoverPhoto = p.IsCoverPhoto,
                        FilePath = p.FilePath
                    }).ToList() ?? new List<PhotoResponseDTO>(),

                    // Services
                    Services = services.Select(s => new ServiceOfferedDTO
                    {
                        Id = s.Id,
                        Name = s.Name
                    }).ToList(),

                    // Benefits
                    Benefits = benefits.Select(b => new BenefitDTO
                    {
                        Id = b.Id,
                        Name = b.Name
                    }).ToList(),

                    // Safety Elements
                    SafetyElements = safetyElements.Select(se => new CoworkingReservation.Application.DTOs.SafetyElementDTO.SafetyElementDTO
                    {
                        Id = se.Id,
                        Name = se.Name
                    }).ToList(),

                    // Special Features
                    SpecialFeatures = specialFeatures.Select(sf => new SpecialFeatureDTO
                    {
                        Id = sf.Id,
                        Name = sf.Name
                    }).ToList(),

                    // Reviews
                    Reviews = reviews,
                    EvaluationsCount = reviews.Count,

                    // Areas
                    Areas = coworkingSpace.Areas?.Select(area => new CoworkingAreaResponseDTO
                    {
                        Id = area.Id,
                        Type = area.Type,
                        Description = area.Description,
                        Capacity = area.Capacity,
                        PricePerDay = area.PricePerDay,
                        Available = area.Available
                    }).ToList() ?? new List<CoworkingAreaResponseDTO>()
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coworking space {CoworkingSpaceId} for edit", coworkingSpaceId);
                throw;
            }
        }

        #endregion
    }
}