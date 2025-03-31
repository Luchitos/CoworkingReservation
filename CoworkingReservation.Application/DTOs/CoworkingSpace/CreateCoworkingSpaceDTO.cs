using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.DTOs;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    public class CreateCoworkingSpaceDTO
    {
        #region Properties

        /// <summary>
        /// Nombre del espacio de coworking.
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Name { get; set; }

        /// <summary>
        /// Descripción del espacio de coworking.
        /// </summary>
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [MaxLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres.")]
        public string Description { get; set; }

        /// <summary>
        /// Capacidad total del espacio.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "La capacidad debe ser al menos 1.")]
        public int Capacity { get; set; }

        /// <summary>
        /// Precio por día del coworking space.
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal PricePerDay { get; set; }
        public float Rate { get; set; }
        /// <summary>
        /// Dirección del coworking space.
        /// </summary>
        [Required(ErrorMessage = "La dirección es obligatoria.")]
        public AddressDTO Address { get; set; } = new AddressDTO();

        /// <summary>
        /// Lista de fotos asociadas al espacio.
        /// </summary>
        public List<IFormFile>? Photos { get; set; }

        /// <summary>
        /// Lista de IDs de los servicios ofrecidos.
        /// </summary>
        public List<int> ServiceIds { get; set; } = new List<int>();

        /// <summary>
        /// Lista de IDs de los beneficios adicionales.
        /// </summary>
        public List<int> BenefitIds { get; set; } = new List<int>();

        /// <summary>
        /// Lista de áreas dentro del coworking space.
        /// </summary>
        public List<CoworkingAreaDTO> Areas { get; set; } = new List<CoworkingAreaDTO>();

        /// <summary>
        /// Estado del coworking space (Pendiente, Aprobado, Rechazado).
        /// </summary>
        public CoworkingStatus Status { get; set; } = CoworkingStatus.Pending;

        #endregion
        public List<int> SafetyElementsIds { get; set; } = new List<int>();
        public List<int> SpeacialFeatureIds { get; set; } = new List<int>();


    }

}