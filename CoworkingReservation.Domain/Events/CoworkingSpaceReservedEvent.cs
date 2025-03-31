using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Domain.Events
{
    public class CoworkingSpaceReservedEvent
    {
        public int CoworkingAreaId { get; }
        public DateTime ReservationDate { get; }

        public CoworkingSpaceReservedEvent(int coworkingAreaId, DateTime reservationDate)
        {
            CoworkingAreaId = coworkingAreaId;
            ReservationDate = reservationDate;
        }
    }
}
