using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using CoworkingReservation.Domain.Enums;

namespace CoworkingReservation.Domain.Entities
{
    public class CoworkingSpace
    {
        public int Id { get; set; } // Identificador único
        public string Name { get; set; } // Nombre del espacio
        public string Description { get; set; } // Descripción del espacio
        public int Capacity { get; set; } // Capacidad máxima
        public decimal PricePerDay { get; set; } // Precio por día
        public bool IsActive { get; set; } = true; // Indica si está activo

        public CoworkingStatus Status { get; set; } = CoworkingStatus.Pending;
        [ForeignKey("Hoster")]
        public int HosterId { get; set; }
        public virtual User Hoster { get; set; }


        // Relaciones
        public Address Address { get; set; } = new Address();

        public virtual ICollection<CoworkingSpacePhoto> Photos { get; set; } = new List<CoworkingSpacePhoto>();

        public List<Reservation> Reservations { get; set; } = new List<Reservation>();
        public List<Review> Reviews { get; set; } = new List<Review>();

        // Relación con Favoritos
        public ICollection<FavoriteCoworkingSpace> FavoritedByUsers { get; set; } = new List<FavoriteCoworkingSpace>();

        public CoworkingSpace()
        {
            Photos = new List<CoworkingSpacePhoto>();
            Reservations = new List<Reservation>();
            Reviews = new List<Review>();
            FavoritedByUsers = new List<FavoriteCoworkingSpace>();
        }
    }
}
