using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CoworkingReservation.Domain.Enums;

namespace CoworkingReservation.Application.DTOs.Reservation
{
    public class CreateReservationDTO
    {
        [Required]
        public int CoworkingSpaceId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public List<int> AreaIds { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }
    }
} 