using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Events;
using CoworkingReservation.Domain.IRepository;
using Microsoft.Extensions.Logging;

namespace CoworkingReservation.Application.CoworkingSpaceReservedEventHandler
{
    /// <summary>
    /// Maneja el evento de reserva de un espacio de coworking.
    /// </summary>
    public class CoworkingSpaceReservedEventHandler
    {
        #region Dependencies

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CoworkingSpaceReservedEventHandler> _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor con inyección de dependencias.
        /// </summary>
        public CoworkingSpaceReservedEventHandler(IUnitOfWork unitOfWork, ILogger<CoworkingSpaceReservedEventHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// Maneja el evento de reserva, reduciendo la disponibilidad del área correspondiente.
        /// </summary>
        /// <param name="event">Evento de reserva.</param>
        public async Task Handle(CoworkingSpaceReservedEvent @event)
        {
            try
            {
                // Validar entrada
                if (@event == null)
                {
                    _logger.LogWarning("El evento recibido es nulo.");
                    return;
                }

                // Obtener disponibilidad del área
                var availability = await _unitOfWork.CoworkingAvailabilities
                    .FindAsync(@event.CoworkingAreaId, @event.ReservationDate);

                if (availability == null)
                {
                    _logger.LogWarning($"No se encontró disponibilidad para el área ID {@event.CoworkingAreaId} en la fecha {@event.ReservationDate}.");
                    return;
                }

                // Validar que haya cupo disponible
                if (availability.AvailableSpots <= 0)
                {
                    _logger.LogWarning($"No hay más cupos disponibles en el área ID {@event.CoworkingAreaId} para la fecha {@event.ReservationDate}.");
                    return;
                }

                // Reducir la disponibilidad
                availability.AvailableSpots -= 1;

                // Guardar cambios en la base de datos
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Se redujo la disponibilidad del área ID {@event.CoworkingAreaId} en la fecha {@event.ReservationDate}. Disponible: {availability.AvailableSpots}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando el evento CoworkingSpaceReservedEvent.");
            }
        }

        #endregion
    }
}
