using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CoworkingReservation.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    /// <summary>
    /// DTO para la creación de un espacio de coworking.
    /// </summary>
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
    }

    /// <summary>
    /// DTO para manejar la dirección del coworking space.
    /// </summary>
    public class AddressDTO
    {
        #region Properties

        [Required(ErrorMessage = "La ciudad es obligatoria.")]
        public string City { get; set; }

        [Required(ErrorMessage = "El país es obligatorio.")]
        public string Country { get; set; }

        /// <summary>
        /// Número del domicilio.
        /// </summary>
        [Required(ErrorMessage = "El número es obligatorio.")]
        public string Number { get; set; }

        [Required(ErrorMessage = "La provincia es obligatoria.")]
        public string Province { get; set; }

        [Required(ErrorMessage = "La calle es obligatoria.")]
        public string Street { get; set; }

        [Required(ErrorMessage = "El código postal es obligatorio.")]
        public string ZipCode { get; set; }

        /// <summary>
        /// Piso del edificio (opcional).
        /// </summary>
        public string? Floor { get; set; }

        /// <summary>
        /// Apartamento del edificio (opcional).
        /// </summary>
        public string? Apartment { get; set; }

        /// <summary>
        /// Calle secundaria (opcional).
        /// </summary>
        public string? StreetOne { get; set; }

        /// <summary>
        /// Otra calle secundaria (opcional).
        /// </summary>
        public string? StreetTwo { get; set; }

        #endregion
    }

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
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string Description { get; set; }

        /// <summary>
        /// Tipo de área dentro del coworking (Oficina Privada, Escritorio Compartido, etc.).
        /// </summary>
        [Required(ErrorMessage = "El tipo de área es obligatorio.")]
        public CoworkingAreaType Type { get; set; }

        #endregion
    }
}
