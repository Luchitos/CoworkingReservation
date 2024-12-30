namespace CoworkingReservation.Domain.Entities
{
    public class FavoriteCoworkingSpace
    {
        public int UserId { get; set; } // Identificador del usuario
        public User User { get; set; } // Relación con el usuario
        public int CoworkingSpaceId { get; set; } // Identificador del coworking space
        public CoworkingSpace CoworkingSpace { get; set; } // Relación con el coworking space
    }
}
