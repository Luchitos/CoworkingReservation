using CoworkingReservation.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    /// <summary>
    /// DTO para la creación de un área dentro de un espacio de coworking.
    /// </summary>
    public class CreateCoworkingAreaDTO
    {
        #region Properties

        /// <summary>
        /// Tipo de área dentro del coworking. (Ej: Escritorio Compartido, Oficina Privada, etc.).
        /// </summary>
        [Required]
        public CoworkingAreaType Type { get; set; }

        /// <summary>
        /// Descripción detallada del área.
        /// </summary>
        [Required]
        [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string Description { get; set; }

        /// <summary>
        /// Capacidad máxima del área.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La capacidad debe ser al menos 1.")]
        public int Capacity { get; set; }

        /// <summary>
        /// Precio por día de uso del área.
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal PricePerDay { get; set; }

        #endregion
    }
}
