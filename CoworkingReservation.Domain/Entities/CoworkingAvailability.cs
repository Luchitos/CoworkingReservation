using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Domain.Entities
{
    public class CoworkingAvailability
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } // Día de la disponibilidad
        public int AvailableSpots { get; set; } // Espacios disponibles para ese día

        [ForeignKey("CoworkingArea")]
        public int CoworkingAreaId { get; set; }
        public virtual CoworkingArea CoworkingArea { get; set; }
    }
}
