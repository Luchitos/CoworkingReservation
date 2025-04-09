using System;
using System.Text.Json.Serialization;

namespace CoworkingReservation.Application.DTOs.User
{
    /// <summary>
    /// DTO para representar la información de una foto de usuario
    /// </summary>
    public class PhotoDTO
    {
        /// <summary>
        /// ID de la foto
        /// </summary>
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        /// <summary>
        /// URL de la foto
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }

        /// <summary>
        /// Nombre del archivo
        /// </summary>
        [JsonPropertyName("fileName")]
        public string FileName { get; set; }

        /// <summary>
        /// Tipo MIME del archivo
        /// </summary>
        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; }
    }
} 