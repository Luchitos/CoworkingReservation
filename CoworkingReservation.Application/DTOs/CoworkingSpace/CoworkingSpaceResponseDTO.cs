using CoworkingReservation.Domain.Enums;
using System.Collections.Generic;

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
        public int Capacity { get; set; }

        /// <summary>
        /// Precio por día del espacio de coworking.
        /// </summary>
        public decimal PricePerDay { get; set; }

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

    /// <summary>
    /// DTO para la respuesta de una foto del espacio de coworking.
    /// </summary>
    public class PhotoResponseDTO
    {
        #region Properties

        /// <summary>
        /// Nombre del archivo de la foto.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Indica si esta foto es la portada del espacio.
        /// </summary>
        public bool IsCoverPhoto { get; set; }

        /// <summary>
        /// Tipo de contenido de la imagen (MIME Type).
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Ruta o URL donde se almacena la foto.
        /// </summary>
        public string FilePath { get; set; }

        #endregion
    }

    /// <summary>
    /// DTO para la respuesta de un servicio ofrecido por un coworking.
    /// </summary>
    public class ServiceOfferedDTO
    {
        #region Properties

        /// <summary>
        /// Identificador del servicio.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del servicio ofrecido.
        /// </summary>
        public string Name { get; set; }

        #endregion
    }

    /// <summary>
    /// DTO para la respuesta de un beneficio ofrecido por un coworking.
    /// </summary>
    public class BenefitDTO
    {
        #region Properties

        /// <summary>
        /// Identificador del beneficio.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del beneficio.
        /// </summary>
        public string Name { get; set; }

        #endregion
    }

    /// <summary>
    /// DTO para representar una área dentro de un coworking.
    /// </summary>
    public class CoworkingAreaResponseDTO
    {
        #region Properties

        /// <summary>
        /// Identificador único del área.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Tipo de área de coworking (Ej: Escritorios Compartidos, Oficina Privada, etc.).
        /// </summary>
        public CoworkingAreaType Type { get; set; }

        /// <summary>
        /// Descripción detallada del área.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Capacidad del área.
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Precio por día de uso del área.
        /// </summary>
        public decimal PricePerDay { get; set; }

        #endregion
    }
}
