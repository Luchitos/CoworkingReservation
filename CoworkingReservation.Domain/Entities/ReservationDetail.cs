namespace CoworkingReservation.Domain.Entities
{
    public class ReservationDetail
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public int CoworkingAreaId { get; set; }
        public decimal PricePerDay { get; set; }
        
        // Relaciones
        public Reservation Reservation { get; set; }
        public CoworkingArea CoworkingArea { get; set; }
    }
} 