using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Enums;

namespace CoworkingReservation.Application.DTOs.CoworkingArea
{
    /// <summary>
    /// DTO para representar una área dentro de un coworking.
    /// </summary>
    public class CoworkingAreaResponseDTO
    {
        #region Properties

        /// <summary>
        /// Identificador único del área.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Tipo de área de coworking (Ej: Escritorios Compartidos, Oficina Privada, etc.).
        /// </summary>
        public CoworkingAreaType Type { get; set; }

        /// <summary>
        /// Descripción detallada del área.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Capacidad del área.
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Precio por día de uso del área.
        /// </summary>
        public decimal PricePerDay { get; set; }

        #endregion
    }
}
