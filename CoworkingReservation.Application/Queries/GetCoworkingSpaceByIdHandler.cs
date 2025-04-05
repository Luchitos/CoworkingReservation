using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Domain.IRepository;
using Microsoft.EntityFrameworkCore;
using MediatR;
using static CoworkingReservation.Application.Queries.CoworkingSpaceQuery;

namespace CoworkingReservation.Application.Queries
{
    public class GetCoworkingSpaceByIdHandler : IRequestHandler<GetCoworkingSpaceByIdQuery, CoworkingSpaceResponseDTO>
    {
        private readonly ICoworkingSpaceRepository _repository;
        private readonly IMapper _mapper;

        public GetCoworkingSpaceByIdHandler(ICoworkingSpaceRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<CoworkingSpaceResponseDTO> Handle(GetCoworkingSpaceByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _repository
                .GetAll()
                .Include(x => x.Address)
                .Include(x => x.Photos)
                .Include(x => x.Services)
                .Include(x => x.Benefits)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null)
                throw new KeyNotFoundException($"Coworking space with ID {request.Id} not found.");

            return _mapper.Map<CoworkingSpaceResponseDTO>(entity);
        }
    }
}