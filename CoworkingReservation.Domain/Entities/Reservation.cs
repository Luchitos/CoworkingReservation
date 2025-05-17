using System;
using System.Collections.Generic;
using CoworkingReservation.Domain.Enums;

namespace CoworkingReservation.Domain.Entities
{
    public class Reservation
    {
        public int Id { get; set; } // Identificador único
        public int UserId { get; set; } // Relación con usuario
        public int CoworkingSpaceId { get; set; } // Relación con espacio
        public DateTime StartDate { get; set; } // Fecha de inicio
        public DateTime EndDate { get; set; } // Fecha de fin
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending; // Estado de la reserva
        public decimal TotalPrice { get; set; } // Precio total
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Relaciones
        public User User { get; set; }
        public CoworkingSpace CoworkingSpace { get; set; }
        public ICollection<ReservationDetail> ReservationDetails { get; set; }
    }
}
