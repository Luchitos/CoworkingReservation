using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Domain.DTOs
{
    public class AddressDTO
    {
        [Required]
        public string City { get; set; }

        [Required]
        public string Country { get; set; }

        public string? Apartment { get; set; }
        public string? Floor { get; set; }

        [Required]
        public string Number { get; set; }

        [Required]
        public string Province { get; set; }

        [Required]
        public string Street { get; set; }

        public string? StreetOne { get; set; }
        public string? StreetTwo { get; set; }

        [Required]
        public string ZipCode { get; set; }
    }
}
