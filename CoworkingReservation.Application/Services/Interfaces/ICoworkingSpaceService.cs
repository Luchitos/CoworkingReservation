using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoworkingReservation.Application.Services.Interfaces
{
    /// <summary>
    /// Servicio para la gestión de espacios de coworking.
    /// </summary>
    public interface ICoworkingSpaceService
    {
        #region Métodos de Consulta

        /// <summary>
        /// Obtiene todos los espacios de coworking activos y aprobados.
        /// </summary>
        /// <returns>Lista de espacios disponibles.</returns>
        Task<IEnumerable<CoworkingSpaceResponseDTO>> GetAllActiveSpacesAsync();

        /// <summary>
        /// Obtiene un espacio de coworking por su ID.
        /// </summary>
        /// <param name="id">ID del espacio de coworking.</param>
        /// <returns>DTO con los datos del espacio.</returns>
        Task<CoworkingSpaceResponseDTO> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene los espacios de coworking filtrados por capacidad y/o ubicación.
        /// </summary>
        /// <param name="capacity">Capacidad mínima requerida (opcional).</param>
        /// <param name="location">Ubicación (ciudad, provincia o calle) (opcional).</param>
        /// <returns>Lista de espacios filtrados.</returns>
        Task<IEnumerable<CoworkingSpaceResponseDTO>> GetAllFilteredAsync(int? capacity, string? location);

        /// <summary>
        /// Obtiene todos los espacios de coworking de un hoster autenticado.
        /// </summary>
        /// <param name="hosterId">ID del hoster.</param>
        /// <returns>Lista de espacios administrados por el hoster.</returns>
        Task<IEnumerable<CoworkingSpaceResponseDTO>> GetAllByHosterIdAsync(int hosterId);

        #endregion

        #region Métodos de Creación y Modificación

        /// <summary>
        /// Crea un nuevo espacio de coworking.
        /// </summary>
        /// <param name="spaceDto">DTO con los datos del espacio.</param>
        /// <param name="hosterId">ID del hoster que lo crea.</param>
        /// <returns>Espacio de coworking creado.</returns>
        Task<CoworkingSpace> CreateAsync(CreateCoworkingSpaceDTO spaceDto, int hosterId);

        /// <summary>
        /// Actualiza un espacio de coworking existente.
        /// </summary>
        /// <param name="id">ID del espacio de coworking.</param>
        /// <param name="dto">DTO con los datos actualizados.</param>
        /// <param name="hosterId">ID del hoster que realiza la actualización.</param>
        /// <param name="userRole">Rol del usuario autenticado.</param>
        Task UpdateAsync(int id, UpdateCoworkingSpaceDTO dto, int hosterId, string userRole);

        #endregion

        #region Métodos de Administración y Control

        /// <summary>
        /// Elimina un espacio de coworking (solo permitido para hosters y administradores).
        /// </summary>
        /// <param name="id">ID del espacio a eliminar.</param>
        /// <param name="hosterId">ID del hoster que solicita la eliminación.</param>
        Task DeleteAsync(int id, int hosterId);

        /// <summary>
        /// Activa o desactiva un espacio de coworking.
        /// </summary>
        /// <param name="coworkingSpaceId">ID del espacio de coworking.</param>
        /// <param name="userId">ID del usuario que realiza la acción.</param>
        /// <param name="userRole">Rol del usuario autenticado.</param>
        Task ToggleActiveStatusAsync(int coworkingSpaceId, int userId, string userRole);

        #endregion
    }
}
