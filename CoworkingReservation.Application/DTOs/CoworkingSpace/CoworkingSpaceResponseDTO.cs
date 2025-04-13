using CoworkingReservation.Application.DTOs.Address;
using CoworkingReservation.Application.DTOs.CoworkingArea;
using CoworkingReservation.Application.DTOs.Photo;
using CoworkingReservation.Application.DTOs.SafetyElementDTO;

namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    /// <summary>
    /// DTO para la respuesta de un espacio de coworking.
    /// </summary>
    public class CoworkingSpaceResponseDTO
    {
        #region Properties

        /// <summary>
        /// Identificador único del espacio de coworking.
        /// </summary>
        public int Id { get; set; }

        public float Rate { get; set; }

        /// <summary>
        /// Nombre del espacio de coworking.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción detallada del espacio.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Capacidad total del espacio.
        /// </summary>
        public int CapacityTotal { get; set; }

        /// <summary>
        /// Indica si el espacio está activo o no.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Dirección del espacio de coworking.
        /// </summary>
        public AddressDTO Address { get; set; }

        /// <summary>
        /// Lista de URLs de fotos asociadas al espacio.
        /// </summary>
        public List<string> PhotoUrls { get; set; } = new List<string>();

        /// <summary>
        /// Lista de nombres de servicios ofrecidos en el espacio.
        /// </summary>
        public List<string> ServiceNames { get; set; } = new List<string>();

        /// <summary>
        /// Lista de nombres de beneficios adicionales disponibles.
        /// </summary>
        public List<string> BenefitNames { get; set; } = new List<string>();

        /// <summary>
        /// Lista de nombres de elementos de seguridad disponibles en el espacio.
        /// </summary>
        public List<string> SafetyElementNames { get; set; } = new List<string>();

        /// <summary>
        /// Lista de nombres de características especiales del espacio.
        /// </summary>
        public List<string> SpecialFeatureNames { get; set; } = new List<string>();

        /// <summary>
        /// Lista de áreas dentro del coworking.
        /// </summary>
        public List<CoworkingAreaResponseDTO> Areas { get; set; } = new List<CoworkingAreaResponseDTO>();

        #endregion
    }
    
}
