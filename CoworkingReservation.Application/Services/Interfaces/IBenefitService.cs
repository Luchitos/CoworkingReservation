using System.Collections.Generic;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Application.Services.Interfaces
{
    /// <summary>
    /// Define los métodos para la gestión de beneficios en espacios de coworking.
    /// </summary>
    public interface IBenefitService
    {
        #region Métodos de Consulta

        /// <summary>
        /// Obtiene todos los beneficios disponibles.
        /// </summary>
        /// <returns>Lista de beneficios.</returns>
        Task<IEnumerable<Benefit>> GetAllAsync();

        /// <summary>
        /// Obtiene un beneficio específico por su ID.
        /// </summary>
        /// <param name="id">ID del beneficio.</param>
        /// <returns>El beneficio si se encuentra, de lo contrario `null`.</returns>
        Task<Benefit?> GetByIdAsync(int id);

        #endregion

        #region Métodos de Gestión

        /// <summary>
        /// Crea un nuevo beneficio.
        /// </summary>
        /// <param name="benefit">Objeto con la información del beneficio.</param>
        /// <returns>El beneficio creado.</returns>
        Task<Benefit> CreateAsync(Benefit benefit);

        /// <summary>
        /// Elimina un beneficio por su ID.
        /// </summary>
        /// <param name="id">ID del beneficio a eliminar.</param>
        Task DeleteAsync(int id);

        #endregion
    }
}
