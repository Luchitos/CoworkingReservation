using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Enums;

namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    /// <summary>
    /// DTO para definir áreas dentro de un coworking space.
    /// </summary>
    public class CoworkingAreaDTO
    {
        #region Properties

        /// <summary>
        /// Capacidad de la zona específica dentro del coworking.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "La capacidad debe ser mayor o igual a 1.")]
        public int Capacity { get; set; }

        /// <summary>
        /// Precio por día de la zona específica.
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal PricePerDay { get; set; }

        /// <summary>
        /// Descripción de la zona específica.
        /// </summary>

        public string? Description { get; set; }

        /// <summary>
        /// Tipo de área dentro del coworking (Oficina Privada, Escritorio Compartido, etc.).
        /// </summary>
        [Required(ErrorMessage = "El tipo de área es obligatorio.")]
        public CoworkingAreaType Type { get; set; }

        #endregion
    }
}

