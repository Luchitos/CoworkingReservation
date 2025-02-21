using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    public class CreateCoworkingSpaceDTO
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than zero.")]
        public int Capacity { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal PricePerDay { get; set; }

        [Required]
        public AddressDTO Address { get; set; } = new AddressDTO();
        public List<IFormFile>? Photos { get; set; } // Recibe archivos
        public List<int> ServiceIds { get; set; } = new List<int>();
        public List<int> BenefitIds { get; set; } = new List<int>();
        public List<CoworkingAreaDTO> Areas { get; set; } = new List<CoworkingAreaDTO>();
    }

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

    public class CoworkingAreaDTO
    {
        public int Capacity { get; set; }
        public decimal PricePerDay { get; set; }
        public string Description { get; set; }
        public CoworkingAreaType Type { get; set; }
    }
}