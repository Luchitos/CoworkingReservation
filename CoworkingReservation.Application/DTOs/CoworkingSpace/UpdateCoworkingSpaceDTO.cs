using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.DTOs;

namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    /// <summary>
    /// DTO para actualizar un espacio de coworking existente.
    /// </summary>
    public class UpdateCoworkingSpaceDTO
    {
        #region Properties

        /// <summary>
        /// Nombre del espacio de coworking.
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Name { get; set; }

        /// <summary>
        /// Descripción detallada del espacio de coworking.
        /// </summary>
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [MaxLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres.")]
        public string Description { get; set; }

        /// <summary>
        /// Capacidad total del espacio.
        /// </summary>
        [Required(ErrorMessage = "La capacidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La capacidad debe ser mayor a 0.")]
        public int CapacityTotal { get; set; }

        /// <summary>
        /// Dirección del espacio de coworking.
        /// </summary>
        [Required(ErrorMessage = "La dirección es obligatoria.")]
        public AddressDTO Address { get; set; }

        /// <summary>
        /// Lista de IDs de los servicios ofrecidos.
        /// </summary>
        public HashSet<int> ServiceIds { get; set; } = new HashSet<int>();

        /// <summary>
        /// Lista de IDs de los beneficios incluidos.
        /// </summary>
        public HashSet<int> Benefits { get; set; } = new HashSet<int>();

        /// <summary>
        /// Lista de IDs de los SafetyElements
        /// </summary>
        public HashSet<int> SafetyElements { get; set; } = new HashSet<int>();

        /// <summary>
        /// Lista de IDs de las características especiales.
        /// </summary>
        public HashSet<int> SpecialFeatures { get; set; } = new HashSet<int>();

        #endregion
    }
}
