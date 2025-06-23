using System.ComponentModel.DataAnnotations;

namespace CoworkingReservation.Application.DTOs.Review
{
    public class CreateReviewRequest
    {
        /// <summary>
        /// ID de la reserva que se está reseñando
        /// </summary>
        [Required(ErrorMessage = "El ID de la reserva es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de la reserva debe ser mayor a 0")]
        public int ReservationId { get; set; }

        /// <summary>
        /// Calificación de 1 a 5 estrellas
        /// </summary>
        [Required(ErrorMessage = "La calificación es obligatoria")]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5 estrellas")]
        public int Rating { get; set; }

        /// <summary>
        /// Comentario de la reseña (opcional)
        /// </summary>
        [MaxLength(1000, ErrorMessage = "El comentario no puede exceder los 1000 caracteres")]
        public string? Comment { get; set; }
    }
}
