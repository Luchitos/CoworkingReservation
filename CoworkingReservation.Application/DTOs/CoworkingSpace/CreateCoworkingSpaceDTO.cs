using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.DTOs;
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
        public AddressDTO Address { get; set; }
        public List<IFormFile>? Photos { get; set; } // Recibe archivos
        public List<int> ServiceIds { get; set; } = new List<int>();
        public List<int> BenefitIds { get; set; } = new List<int>();
        public CoworkingStatus Status { get; set; }
        

    }

}