using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Domain.DTOs
{
    public class CoworkingSpaceLightDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Street { get; set; }
        public string Number { get; set; }
        public string ContentType { get; set; }
        public PhotoResponseDTO? CoverPhoto { get; set; }
        public float Rate { get; set; }
        public AddressDTO Address { get; set; }
    }
}
