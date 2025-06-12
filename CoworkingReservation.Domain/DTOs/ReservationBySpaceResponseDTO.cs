public class ReservationBySpaceResponseDTO
{
    public int CoworkingSpaceId { get; set; }
    public string CoworkingSpaceName { get; set; }

    public List<ReservationSummaryDTO> Reservations { get; set; } = new();
}

public class ReservationSummaryDTO
{
    public int ReservationId { get; set; }
    public string UserName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; }
    public List<string> AreaTypes { get; set; } = new();
}