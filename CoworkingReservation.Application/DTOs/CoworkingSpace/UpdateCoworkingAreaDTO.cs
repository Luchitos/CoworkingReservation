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
    /// DTO para actualizar un área dentro de un espacio de coworking.
    /// </summary>
    public class UpdateCoworkingAreaDTO
    {
        #region Properties

        /// <summary>
        /// Tipo de área dentro del coworking (Escritorio Individual, Oficina Privada, etc.).
        /// </summary>
        [Required(ErrorMessage = "El tipo de área es obligatorio.")]
        public CoworkingAreaType Type { get; set; }

        /// <summary>
        /// Descripción detallada del área.
        /// </summary>
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string? Description { get; set; }

        /// <summary>
        /// Capacidad máxima del área.
        /// </summary>
        [Required(ErrorMessage = "La capacidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La capacidad debe ser mayor a 0.")]
        public int Capacity { get; set; }

        /// <summary>
        /// Precio por día del área de coworking.
        /// </summary>
        [Required(ErrorMessage = "El precio por día es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal PricePerDay { get; set; }

        /// <summary>
        /// ID de la área de coworking.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Indica si la área está disponible.
        /// </summary>
        public bool Available { get; set; } = true;
        #endregion
    }
}

