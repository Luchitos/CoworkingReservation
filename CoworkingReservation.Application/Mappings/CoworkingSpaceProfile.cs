using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Application.Mappings
{
    public class CoworkingSpaceProfile : Profile
    {
        public CoworkingSpaceProfile()
        {
            CreateMap<CoworkingSpace, CoworkingSpaceResponseDTO>();
            // Otros mappings que uses
        }
    }
}
