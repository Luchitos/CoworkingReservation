using System.ComponentModel.DataAnnotations;
using CoworkingReservation.Application.DTOs.Address;
using CoworkingReservation.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    public class CreateCoworkingSpaceDTO
    {
        #region Properties

        /// <summary>
        /// Nombre del espacio de coworking.
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Title { get; set; }

        /// <summary>
        /// Descripción del espacio de coworking.
        /// </summary>
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [MaxLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres.")]
        public string Description { get; set; }

        /// <summary>
        /// Capacidad total del espacio.
        /// </summary>
        public int CapacityTotal { get; set; }

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
        /// String JSON de los servicios enviado desde el formulario
        /// </summary>
        [FromForm(Name = "Services")]
        public string Services { get; set; }

        /// <summary>
        /// String JSON de los beneficios enviado desde el formulario
        /// </summary>
        [FromForm(Name = "Benefits")]
        public string Benefits { get; set; }

        /// <summary>
        /// String JSON de los elementos de seguridad enviado desde el formulario
        /// </summary>
        [FromForm(Name = "SafetyElements")]
        public string SafetyElements { get; set; }

        /// <summary>
        /// String JSON de las características especiales enviado desde el formulario
        /// </summary>
        [FromForm(Name = "SpeacialFeatures")]
        public string SpeacialFeatures { get; set; }

        /// <summary>
        /// String JSON de las áreas enviado desde el formulario
        /// </summary>
        [FromForm(Name = "AreasJson")]
        public string AreasJson { get; set; }

        /// <summary>
        /// Estado del coworking space (Pendiente, Aprobado, Rechazado).
        /// </summary>
        public CoworkingStatus Status { get; set; } = CoworkingStatus.Pending;

        #endregion
    }
}