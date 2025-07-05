using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    public class MetadataDTO
    {
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public string Version { get; set; } = "1.1";
        public Dictionary<string, object> AppliedFilters { get; set; } = new();
    }
}
