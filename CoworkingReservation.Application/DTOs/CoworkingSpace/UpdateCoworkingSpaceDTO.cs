using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    public class UpdateCoworkingSpaceDTO
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int Capacity { get; set; }

        [Required]
        public decimal PricePerDay { get; set; }

        [Required]
        public AddressDTO Address { get; set; }

        public List<int> ServiceIds { get; set; } = new List<int>();
        public List<int> BenefitIds { get; set; } = new List<int>();
    }
}
