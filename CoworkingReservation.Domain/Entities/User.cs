namespace CoworkingReservation.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        // Relaciones
        public List<Reservation> Reservations { get; set; } = new List<Reservation>();
        public List<Review> Reviews { get; set; } = new List<Review>();
    }
}
