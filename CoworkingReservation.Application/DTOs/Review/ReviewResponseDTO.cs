using System;

namespace CoworkingReservation.Application.DTOs.Review
{
    /// <summary>
    /// DTO para la respuesta de un review de coworking space.
    /// </summary>
    public class ReviewResponseDTO
    {
        /// <summary>
        /// Identificador único del review.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del usuario que hizo el review.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// ID del espacio de coworking.
        /// </summary>
        public int CoworkingSpaceId { get; set; }

        /// <summary>
        /// ID de la reserva asociada.
        /// </summary>
        public int ReservationId { get; set; }

        /// <summary>
        /// Nombre del usuario que hizo el review.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Nombre del espacio de coworking.
        /// </summary>
        public string CoworkingSpaceName { get; set; }

        /// <summary>
        /// Calificación del 1 al 5.
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// Comentario del review.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Fecha de creación del review.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
} 