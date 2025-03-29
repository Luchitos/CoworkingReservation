using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Domain.DTOs
{
    public class CoworkingSpaceListItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public AddressDTO Address { get; set; }
        public PhotoResponseDTO? CoverPhoto { get; set; }
    }
}
