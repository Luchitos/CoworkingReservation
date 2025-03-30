using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Domain.Entities
{
    public class CoworkingArea
    {
        public int Id { get; set; }

        public CoworkingAreaType Type { get; set; } // Ahora es un enum en lugar de un string

        public string Description { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerDay { get; set; }

        [ForeignKey("CoworkingSpace")]
        public int CoworkingSpaceId { get; set; }
        public virtual CoworkingSpace CoworkingSpace { get; set; }

        // Relación con disponibilidad diaria
        public virtual ICollection<CoworkingAvailability> Availabilities { get; set; } = new List<CoworkingAvailability>();
    }
}
