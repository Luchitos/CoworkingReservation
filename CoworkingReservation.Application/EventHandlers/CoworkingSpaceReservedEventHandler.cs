using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Events;
using CoworkingReservation.Domain.IRepository;

namespace CoworkingReservation.Application.EventHandlers
{
    public class CoworkingSpaceReservedEventHandler
    {
        private readonly IUnitOfWork _unitOfWork;

        public CoworkingSpaceReservedEventHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CoworkingSpaceReservedEvent @event)
        {
            var availability = await _unitOfWork.CoworkingAvailabilities
                .FindAsync(@event.CoworkingAreaId, @event.ReservationDate);

            if (availability != null && availability.AvailableSpots > 0)
            {
                availability.AvailableSpots -= 1;
                await _unitOfWork.SaveChangesAsync();
            }
        }

    }
}
