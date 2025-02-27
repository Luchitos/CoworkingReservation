using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using Microsoft.Extensions.Logging;

namespace CoworkingReservation.Application.Services
{
    /// <summary>
    /// Servicio para la gestión de beneficios en espacios de coworking.
    /// </summary>
    public class BenefitService : IBenefitService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BenefitService> _logger;

        public BenefitService(IUnitOfWork unitOfWork, ILogger<BenefitService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Obtener Beneficios

        /// <summary>
        /// Obtiene la lista de todos los beneficios disponibles.
        /// </summary>
        public async Task<IEnumerable<Benefit>> GetAllAsync()
        {
            _logger.LogInformation("Obteniendo todos los beneficios.");
            return await _unitOfWork.Benefits.GetAllAsync();
        }

        /// <summary>
        /// Obtiene un beneficio por su ID.
        /// </summary>
        /// <param name="id">ID del beneficio.</param>
        /// <returns>El beneficio encontrado o `null` si no existe.</returns>
        public async Task<Benefit> GetByIdAsync(int id)
        {
            _logger.LogInformation($"Buscando beneficio con ID {id}");
            return await _unitOfWork.Benefits.GetByIdAsync(id);
        }

        #endregion

        #region Crear Beneficio

        /// <summary>
        /// Crea un nuevo beneficio.
        /// </summary>
        /// <param name="benefit">Objeto `Benefit` con los datos.</param>
        /// <returns>El beneficio creado.</returns>
        public async Task<Benefit> CreateAsync(Benefit benefit)
        {
            if (benefit == null)
            {
                _logger.LogError("Intento de crear un beneficio con datos nulos.");
                throw new ArgumentNullException(nameof(benefit), "El beneficio no puede ser nulo.");
            }

            _logger.LogInformation($"Creando beneficio: {benefit.Name}");

            await _unitOfWork.Benefits.AddAsync(benefit);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Beneficio creado con ID {benefit.Id}");
            return benefit;
        }

        #endregion

        #region Eliminar Beneficio

        /// <summary>
        /// Elimina un beneficio por su ID.
        /// </summary>
        /// <param name="id">ID del beneficio a eliminar.</param>
        public async Task DeleteAsync(int id)
        {
            var benefit = await _unitOfWork.Benefits.GetByIdAsync(id);
            if (benefit == null)
            {
                _logger.LogWarning($"Intento de eliminar un beneficio inexistente (ID {id})");
                throw new KeyNotFoundException($"No se encontró un beneficio con el ID {id}.");
            }

            _logger.LogInformation($"Eliminando beneficio con ID {id}");
            await _unitOfWork.Benefits.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        #endregion
    }
}
