using CoworkingReservation.Application.DTOs.Reservation;

public class UserReservationsGroupedDTO
{
    public IEnumerable<ReservationResponseDTO> PastReservations { get; set; }
    public IEnumerable<ReservationResponseDTO> CurrentAndFutureReservations { get; set; }
}
