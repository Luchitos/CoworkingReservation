using System;
using System.Collections.Generic;

namespace CoworkingReservation.API.Models
{
    public class ReservationResponseDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int CoworkingSpaceId { get; set; }
        public string CoworkingSpaceName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ReservationDetailDTO> Details { get; set; }
        public List<ReservationAreaDTO> Areas { get; set; } = new List<ReservationAreaDTO>();
    }

    public class ReservationAreaDTO
    {
        public int Id { get; set; }
        public int CoworkingAreaId { get; set; }
        public string AreaName { get; set; }
        public string AreaType { get; set; }
        public decimal PricePerDay { get; set; }
    }

        public class ReservationDetailDTO
    {
        public int Id { get; set; }
        public int CoworkingAreaId { get; set; }
        public string AreaType { get; set; }
        public decimal PricePerDay { get; set; }
    }
} 