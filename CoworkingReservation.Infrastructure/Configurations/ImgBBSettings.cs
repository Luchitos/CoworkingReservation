using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Infrastructure.Configurations
{
    /// <summary>
    /// Configuración para el servicio ImgBB
    /// </summary>
    public class ImgBBSettings
    {
        /// <summary>
        /// API Key para ImgBB
        /// Puedes obtener una gratis en https://api.imgbb.com/
        /// </summary>
        public string ApiKey { get; set; }
        
        /// <summary>
        /// URL base de la API de ImgBB
        /// </summary>
        public string ApiUrl { get; set; } = "https://api.imgbb.com/1/upload";
        
        /// <summary>
        /// Tiempo de expiración en segundos para las imágenes (0 = nunca)
        /// </summary>
        public int ExpirationTime { get; set; } = 0;
    }
} 