using System.ComponentModel.DataAnnotations;
using CoworkingReservation.Domain.Enums;

namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    public class UpdateCoworkingAreaDTO
    {
        [Required]
        public CoworkingAreaType Type { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than zero.")]
        public int Capacity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal PricePerDay { get; set; }
    }
}
