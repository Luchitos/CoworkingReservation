using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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

        // Relaciones
        public int AddressId { get; set; } // Relación con dirección
        public Address Address { get; set; }
        public List<Reservation> Reservations { get; set; } = new List<Reservation>();
        public List<Review> Reviews { get; set; } = new List<Review>();
    }
}

