using CoworkingReservation.Domain.Enums;

namespace CoworkingReservation.Domain.Entities
{
    public class Reservation
    {
        public int Id { get; set; } // Identificador único
        public int UserId { get; set; } // Relación con usuario
        public User User { get; set; }
        public int CoworkingSpaceId { get; set; } // Relación con espacio
        public CoworkingSpace CoworkingSpace { get; set; }
        public DateTime StartDate { get; set; } // Fecha de inicio
        public DateTime EndDate { get; set; } // Fecha de fin
        public decimal TotalPrice { get; set; } // Precio total
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending; // Estado de la reserva
    }

}
