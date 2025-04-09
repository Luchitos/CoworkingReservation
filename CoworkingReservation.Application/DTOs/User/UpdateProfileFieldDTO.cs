using System;
using System.Text.Json.Serialization;

namespace CoworkingReservation.Application.DTOs.User
{
    /// <summary>
    /// DTO para actualizar un campo específico del perfil de usuario
    /// </summary>
    public class UpdateProfileFieldDTO
    {
        /// <summary>
        /// Nombre del campo a actualizar
        /// </summary>
        [JsonPropertyName("field")]
        public string Field { get; set; }

        /// <summary>
        /// Valor del campo (puede ser una cadena para campos simples)
        /// </summary>
        [JsonPropertyName("value")]
        public object Value { get; set; }
    }
} 