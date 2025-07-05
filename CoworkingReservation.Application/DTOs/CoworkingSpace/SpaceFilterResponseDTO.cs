using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    public class SpaceFilterResponseDTO
    {
        public List<CoworkingSpaceListItemDTO> Spaces { get; set; }
        public MetadataDTO Metadata { get; set; }
    }
}
