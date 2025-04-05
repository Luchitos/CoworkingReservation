using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Domain.DTOs
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
        /// Lista de fotos asociadas al espacio.
        /// </summary>
        public List<PhotoResponseDTO> Photos { get; set; } = new List<PhotoResponseDTO>();

        /// <summary>
        /// Lista de servicios ofrecidos en el espacio.
        /// </summary>
        public List<ServiceOfferedDTO> Services { get; set; } = new List<ServiceOfferedDTO>();

        /// <summary>
        /// Lista de beneficios adicionales disponibles.
        /// </summary>
        public List<BenefitDTO> Benefits { get; set; } = new List<BenefitDTO>();

        /// <summary>
        /// Lista de áreas dentro del coworking.
        /// </summary>
        public List<CoworkingAreaResponseDTO> Areas { get; set; } = new List<CoworkingAreaResponseDTO>();

        #endregion
    }

}

