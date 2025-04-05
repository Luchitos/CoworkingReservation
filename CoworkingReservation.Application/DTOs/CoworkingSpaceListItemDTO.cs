using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Application.DTOs.Address;
using CoworkingReservation.Application.DTOs.Photo;

namespace CoworkingReservation.Application.DTOs
{
    public class CoworkingSpaceListItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public AddressDTO Address { get; set; }
        public PhotoResponseDTO? CoverPhoto { get; set; }
        public float Rate { get; set; }
        public decimal PricePerDay { get; set; }
    }
}
