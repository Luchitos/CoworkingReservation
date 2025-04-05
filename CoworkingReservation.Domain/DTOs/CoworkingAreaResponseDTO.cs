using CoworkingReservation.Domain.Enums;

namespace CoworkingReservation.Domain.DTOs
{
    public class CoworkingAreaResponseDTO
    {
        public int Id { get; set; }
        public CoworkingAreaType Type { get; set; } // Ahora es un enum en lugar de un string
        public string? Description { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerDay { get; set; }
    }
}