using System;
using System.Collections.Generic;
using CoworkingReservation.Domain.Enums;

namespace CoworkingReservation.Application.DTOs.Reservation
{
    public class ReservationResponseDTO
    {
        public int Id { get; set; }
        public string CoworkingSpaceName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ReservationStatus Status { get; set; }
        public decimal TotalPrice { get; set; }
        public PaymentMethodType PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ReservationDetailDTO> Details { get; set; }
    }

    public class ReservationDetailDTO
    {
        public int Id { get; set; }
        public int CoworkingAreaId { get; set; }
        public string AreaType { get; set; }
        public decimal PricePerDay { get; set; }
    }
} 