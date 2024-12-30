using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Domain.Entities
{
    public class Address
    {
        public int Id { get; set; } // Identificador único
        public string City { get; set; }
        public string Country { get; set; }
        public string? Apartment { get; set; } // Opcional
        public string? Floor { get; set; } // Opcional
        public string Number { get; set; }
        public string Province { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }

        // Relación
        public List<CoworkingSpace> CoworkingSpaces { get; set; } = new List<CoworkingSpace>();
    }
}
