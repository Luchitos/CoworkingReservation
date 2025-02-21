using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Enums;

namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    public class CreateCoworkingAreaDTO
    {
        public CoworkingAreaType Type { get; set; } // Solo valores del enum
        public string Description { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerDay { get; set; }
    }
}
