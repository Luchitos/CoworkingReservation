using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.DTOs;

namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    public class CoworkingSpaceSummaryDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public AddressDTO Address { get; set; }
        public PhotoResponseDTO? CoverPhoto { get; set; }
    }

}
