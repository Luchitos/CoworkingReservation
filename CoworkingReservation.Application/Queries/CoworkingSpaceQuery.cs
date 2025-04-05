using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Application.DTOs.CoworkingSpace;
using MediatR;

namespace CoworkingReservation.Application.Queries
{
    public class CoworkingSpaceQuery
    {
        public record GetCoworkingSpaceByIdQuery(int Id,CancellationToken CancellationToken) : IRequest<CoworkingSpaceResponseDTO>;

    }
}
