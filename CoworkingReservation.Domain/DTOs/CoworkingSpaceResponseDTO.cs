using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Domain.DTOs
{
    public class CoworkingSpaceResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerDay { get; set; }
        public bool IsActive { get; set; }
        public AddressDTO Address { get; set; }
        public List<PhotoResponseDTO> Photos { get; set; }
        public float Rate { get; set; }
    }

}
