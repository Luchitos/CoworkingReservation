using System.ComponentModel.DataAnnotations;

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

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
