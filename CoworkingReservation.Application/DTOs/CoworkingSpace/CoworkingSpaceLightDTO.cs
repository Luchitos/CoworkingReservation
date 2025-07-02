using CoworkingReservation.Application.DTOs.Address;
using CoworkingReservation.Application.DTOs.Photo;

namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    public class CoworkingSpaceLightDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Street { get; set; }
        public string Number { get; set; }
        public string ContentType { get; set; }
        public PhotoResponseDTO? CoverPhoto { get; set; }
        public float Rate { get; set; }
        public AddressDTO Address { get; set; }
    }
}
