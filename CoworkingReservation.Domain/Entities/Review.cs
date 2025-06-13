namespace CoworkingReservation.Domain.Entities
{
    public class Review
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int CoworkingSpaceId { get; set; }
        public CoworkingSpace CoworkingSpace { get; set; }

        public int ReservationId { get; set; } // 🆕 Relación obligatoria con la reserva
        public Reservation Reservation { get; set; }

        public int Rating { get; set; } // 1-5 estrellas
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
