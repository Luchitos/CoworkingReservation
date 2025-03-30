using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Application.DTOs.Photo
{
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
}