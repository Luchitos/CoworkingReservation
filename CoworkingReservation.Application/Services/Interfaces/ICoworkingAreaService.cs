using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Application.Services.Interfaces
{
    /// <summary>
    /// Servicio para la gestión de áreas dentro de un espacio de coworking.
    /// </summary>
    public interface ICoworkingAreaService
    {
        #region Métodos de Creación y Modificación

        /// <summary>
        /// Crea una nueva área de coworking asociada a un espacio de coworking.
        /// </summary>
        /// <param name="areaDto">DTO con los datos de la nueva área.</param>
        /// <param name="coworkingSpaceId">ID del espacio de coworking.</param>
        /// <param name="hosterId">ID del hoster que crea el área.</param>
        /// <returns>El área de coworking creada.</returns>
        Task<CoworkingArea> CreateAsync(CreateCoworkingAreaDTO areaDto, int coworkingSpaceId, int hosterId);
        Task AddAreasToCoworkingAsync(IEnumerable<CoworkingAreaDTO> areaDtos, int coworkingSpaceId, int hosterId);
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

        #endregion

        #region Métodos de Consulta

        /// <summary>
        /// Obtiene todas las áreas de coworking de un espacio específico.
        /// </summary>
        /// <param name="coworkingSpaceId">ID del espacio de coworking.</param>
        /// <returns>Lista de áreas de coworking.</returns>
        Task<IEnumerable<CoworkingArea>> GetByCoworkingSpaceIdAsync(int coworkingSpaceId);

        /// <summary>
        /// Obtiene todas las áreas de coworking registradas en el sistema.
        /// </summary>
        /// <returns>Lista de todas las áreas de coworking.</returns>
        Task<IEnumerable<CoworkingArea>> GetAllAsync();

        /// <summary>
        /// Obtiene un área de coworking por su ID.
        /// </summary>
        /// <param name="id">ID del área de coworking.</param>
        /// <returns>El área de coworking encontrada o `null` si no existe.</returns>
        Task<CoworkingArea?> GetByIdAsync(int id);

        #endregion

        #region Validaciones y Disponibilidad

        /// <summary>
        /// Verifica si un área de coworking con el ID especificado existe.
        /// </summary>
        /// <param name="id">ID del área de coworking.</param>
        /// <returns>True si existe, False en caso contrario.</returns>
        Task<bool> ExistsAsync(int id);

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

        #endregion
    }
}