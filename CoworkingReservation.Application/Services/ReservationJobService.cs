using CoworkingReservation.Domain.Enums;
using CoworkingReservation.Domain.IRepository;
using Microsoft.Extensions.Logging;

public class ReservationJobService : IReservationJobService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReservationJobService> _logger;

    public ReservationJobService(
        IReservationRepository reservationRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReservationJobService> logger)
    {
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task CompleteExpiredReservationsAsync()
    {
        var today = DateTime.Today;
        var expiredReservations = await _reservationRepository
            .GetReservationsAsync(r => r.Status == ReservationStatus.Confirmed && r.EndDate < today);

        if (!expiredReservations.Any())
        {
            _logger.LogInformation("✅ No se encontraron reservas expiradas para completar.");
            return;
        }

        foreach (var reservation in expiredReservations)
        {
            reservation.Status = ReservationStatus.Completed;
            reservation.UpdatedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation($"✅ {expiredReservations.Count()} reservas completadas automáticamente.");
    }
}
