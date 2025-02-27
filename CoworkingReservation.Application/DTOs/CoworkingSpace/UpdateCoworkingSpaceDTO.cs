using System.ComponentModel.DataAnnotations;

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
        public int Capacity { get; set; }

        /// <summary>
        /// Precio por día del coworking.
        /// </summary>
        [Required(ErrorMessage = "El precio por día es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal PricePerDay { get; set; }

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
        public HashSet<int> BenefitIds { get; set; } = new HashSet<int>();

        #endregion
    }
}
