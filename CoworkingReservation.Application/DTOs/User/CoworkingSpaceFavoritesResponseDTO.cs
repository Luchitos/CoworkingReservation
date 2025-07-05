using System;
using CoworkingReservation.Application.DTOs.CoworkingSpace;

namespace CoworkingReservation.Application.DTOs.User
{
    public class CoworkingSpaceFavoritesResponseDTO
    {
        public List<CoworkingSpaceListItemDTO> Spaces { get; set; }
        public MetadataDTO Metadata { get; set; }
    }
}