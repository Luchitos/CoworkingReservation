using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Application.DTOs.Address;
using CoworkingReservation.Application.DTOs.Photo;
using CoworkingReservation.Application.DTOs.CoworkingArea;
using CoworkingReservation.Application.DTOs.Review;
using CoworkingReservation.Domain.Enums;

namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    /// <summary>
    /// DTO completo para edición de un espacio de coworking, incluyendo todas sus áreas y relaciones.
    /// </summary>
    public class CoworkingSpaceEditDTO
    {
        #region Coworking Space Basic Information

        /// <summary>
        /// ID del espacio de coworking.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del espacio de coworking.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción del espacio de coworking.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Capacidad total del espacio.
        /// </summary>
        public int CapacityTotal { get; set; }

        /// <summary>
        /// Estado del espacio (Activo, Inactivo, Pendiente, etc.).
        /// </summary>
        public CoworkingStatus Status { get; set; }

        /// <summary>
        /// Calificación promedio del espacio.
        /// </summary>
        public double Rate { get; set; }

        /// <summary>
        /// Número total de evaluaciones.
        /// </summary>
        public int EvaluationsCount { get; set; }

        /// <summary>
        /// ID del hoster propietario.
        /// </summary>
        public int? HosterId { get; set; }

        /// <summary>
        /// Indica si el espacio está activo.
        /// </summary>
        public bool IsActive { get; set; }

        #endregion

        #region Address Information

        /// <summary>
        /// Información de dirección del espacio.
        /// </summary>
        public AddressDTO Address { get; set; }

        #endregion

        #region Photos

        /// <summary>
        /// Lista de fotos del espacio de coworking.
        /// </summary>
        public List<PhotoResponseDTO> Photos { get; set; } = new List<PhotoResponseDTO>();

        #endregion

        #region Areas

        /// <summary>
        /// Lista de áreas de coworking del espacio.
        /// </summary>
        public List<CoworkingAreaResponseDTO> Areas { get; set; } = new List<CoworkingAreaResponseDTO>();

        #endregion

        #region Services and Benefits

        /// <summary>
        /// Lista de servicios ofrecidos por el espacio.
        /// </summary>
        public List<ServiceOfferedDTO> Services { get; set; } = new List<ServiceOfferedDTO>();

        /// <summary>
        /// Lista de beneficios ofrecidos por el espacio.
        /// </summary>
        public List<BenefitDTO> Benefits { get; set; } = new List<BenefitDTO>();

        #endregion

        #region Safety and Special Features

        /// <summary>
        /// Lista de elementos de seguridad del espacio.
        /// </summary>
        public List<CoworkingReservation.Application.DTOs.SafetyElementDTO.SafetyElementDTO> SafetyElements { get; set; } = new List<CoworkingReservation.Application.DTOs.SafetyElementDTO.SafetyElementDTO>();

        /// <summary>
        /// Lista de características especiales del espacio.
        /// </summary>
        public List<SpecialFeatureDTO> SpecialFeatures { get; set; } = new List<SpecialFeatureDTO>();

        /// <summary>
        /// Lista de reviews del espacio de coworking.
        /// </summary>
        public List<ReviewResponseDTO> Reviews { get; set; } = new List<ReviewResponseDTO>();

        #endregion

        #region Statistics and Metadata

        /// <summary>
        /// Número total de áreas configuradas.
        /// </summary>
        public int TotalAreas { get; set; }

        /// <summary>
        /// Número de áreas disponibles.
        /// </summary>
        public int AvailableAreas { get; set; }

        /// <summary>
        /// Capacidad total disponible.
        /// </summary>
        public int AvailableCapacity { get; set; }

        /// <summary>
        /// Fecha de creación del espacio.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Fecha de última actualización.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        #endregion
    }
} 