using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoworkingReservation.API.Models
{
    public class CreateReservationRequest
    {
        [Required]
        public int CoworkingSpaceId { get; set; }
        
        /// <summary>
        /// Fecha de inicio de la reserva.
        /// Nota: Solo se considera la fecha, no la hora. Se normaliza a las 00:00:00.
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }
        
        /// <summary>
        /// Fecha de fin de la reserva.
        /// Nota: Solo se considera la fecha, no la hora. Se normaliza a las 00:00:00.
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }
        
        /// <summary>
        /// Lista de IDs de áreas a reservar.
        /// </summary>
        [Required]
        public List<int> AreaIds { get; set; } = new List<int>();
        
        /// <summary>
        /// ID del usuario que realiza la reserva. Se asigna automáticamente desde el token de autenticación.
        /// </summary>
        public int UserId { get; set; }
    }
} 