using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using CoworkingReservation.Domain.IRepository;
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

        public CoworkingAreaService(IUnitOfWork unitOfWork, ILogger<CoworkingAreaService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Crear Área

        /// <summary>
        /// Crea una nueva área dentro de un espacio de coworking.
        /// </summary>
        public async Task<CoworkingAreaDTO> CreateAsync(CreateCoworkingAreaDTO areaDto, int coworkingSpaceId, int hosterId)
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
            var result = new CoworkingAreaDTO
            {
                Id = area.Id,
                Type = (int)area.Type,
                Description = area.Description,
                Available = area.Available,
                Capacity = area.Capacity,
                PricePerDay = area.PricePerDay,
                CoworkingSpaceId = area.CoworkingSpaceId
            };

            return result;
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

            var areas = areaDtos.Select(dto =>
            {
                return new CoworkingArea
                {
                    Capacity = dto.Capacity,
                    Description = dto.Description,
                    PricePerDay = dto.PricePerDay,
                    Available = dto.Available,
                    Type = (CoworkingAreaType)dto.Type,
                    CoworkingSpaceId = coworkingSpaceId
                };
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
            area.Available = dto.available;
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
        public async Task<IEnumerable<CoworkingAreaDTO>> GetByCoworkingSpaceIdAsync(int coworkingSpaceId)
        {
            var areas = await _unitOfWork.CoworkingAreas
                .GetQueryable()
                .Where(ca => ca.CoworkingSpaceId == coworkingSpaceId && ca.Available)
                .AsNoTracking()
                .ToListAsync();
            return areas.Select(area => new CoworkingAreaDTO
            {
                Id = area.Id,
                Type = (int)area.Type,
                Description = area.Description,
                Capacity = area.Capacity,
                PricePerDay = area.PricePerDay,
                Available = area.Available,
                CoworkingSpaceId = area.CoworkingSpaceId
            });
        }

        /// <summary>
        /// Obtiene un área de coworking por su ID.
        /// </summary>
        public async Task<CoworkingAreaDTO> GetByIdAsync(int id)
        {
            var area = await _unitOfWork.CoworkingAreas.GetByIdAsync(id);
            if (area == null)
                throw new KeyNotFoundException("Coworking area not found.");

            return new CoworkingAreaDTO
            {
                Id = area.Id,
                Type = (int)area.Type,
                Description = area.Description,
                Capacity = area.Capacity,
                PricePerDay = area.PricePerDay,
                Available = area.Available,
                CoworkingSpaceId = area.CoworkingSpaceId
            };
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
        public async Task<IEnumerable<CoworkingAreaDTO>> GetAllAsync()
        {
            var areas = await _unitOfWork.CoworkingAreas.GetAllAsync();
            return areas.Select(area => new CoworkingAreaDTO
            {
                Id = area.Id,
                Type = (int)area.Type,
                Description = area.Description,
                Capacity = area.Capacity,
                PricePerDay = area.PricePerDay,
                Available = area.Available,
                CoworkingSpaceId = area.CoworkingSpaceId
            });
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
            int totalCapacity = await GetTotalCapacityByCoworkingSpaceIdAsync(coworkingSpaceId);
            return totalCapacity >= requiredCapacity;
        }

        #endregion
    }
}