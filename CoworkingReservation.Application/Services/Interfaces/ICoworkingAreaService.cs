using System.Collections.Generic;
using System.Threading.Tasks;
using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Application.Services.Interfaces
{
    public interface ICoworkingAreaService
    {
        /// <summary>
        /// Crea una nueva área de coworking asociada a un espacio de coworking.
        /// </summary>
        /// <param name="areaDto">DTO con los datos de la nueva área.</param>
        /// <param name="coworkingSpaceId">ID del espacio de coworking.</param>
        /// <returns>El área de coworking creada.</returns>
        Task<CoworkingArea> CreateAsync(CreateCoworkingAreaDTO areaDto, int coworkingSpaceId, int hosterId);

        /// <summary>
        /// Actualiza un área de coworking existente.
        /// </summary>
        /// <param name="id">ID del área de coworking.</param>
        /// <param name="dto">DTO con los datos actualizados.</param>
        /// <param name="hosterId">ID del hoster que realiza la actualización.</param>
        Task UpdateAsync(int id, UpdateCoworkingAreaDTO dto, int hosterId);

        /// <summary>
        /// Elimina un área de coworking.
        /// </summary>
        /// <param name="id">ID del área de coworking.</param>
        /// <param name="hosterId">ID del hoster que realiza la eliminación.</param>
        Task DeleteAsync(int id, int hosterId);

        /// <summary>
        /// Obtiene todas las áreas de coworking de un espacio específico.
        /// </summary>
        /// <param name="coworkingSpaceId">ID del espacio de coworking.</param>
        /// <returns>Lista de áreas de coworking.</returns>
        Task<IEnumerable<CoworkingArea>> GetByCoworkingSpaceIdAsync(int coworkingSpaceId);

        /// <summary>
        /// Obtiene un área de coworking por su ID.
        /// </summary>
        /// <param name="id">ID del área de coworking.</param>
        /// <returns>El área de coworking encontrada.</returns>
        Task<CoworkingArea> GetByIdAsync(int id);

        /// <summary>
        /// Verifica si existe un área de coworking con el ID especificado.
        /// </summary>
        /// <param name="id">ID del área de coworking.</param>
        /// <returns>True si existe, False en caso contrario.</returns>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Obtiene todas las áreas de coworking registradas.
        /// </summary>
        /// <returns>Lista de todas las áreas de coworking.</returns>
        Task<IEnumerable<CoworkingArea>> GetAllAsync();

        /// <summary>
        /// Obtiene la capacidad total de todas las áreas de un espacio de coworking.
        /// </summary>
        /// <param name="coworkingSpaceId">ID del espacio de coworking.</param>
        /// <returns>Capacidad total del espacio.</returns>
        Task<int> GetTotalCapacityByCoworkingSpaceIdAsync(int coworkingSpaceId);

        /// <summary>
        /// Verifica si un espacio de coworking tiene suficiente capacidad disponible.
        /// </summary>
        /// <param name="coworkingSpaceId">ID del espacio de coworking.</param>
        /// <param name="requiredCapacity">Capacidad requerida.</param>
        /// <returns>True si hay capacidad disponible, False en caso contrario.</returns>
        Task<bool> HasAvailableCapacity(int coworkingSpaceId, int requiredCapacity);
    }
}
