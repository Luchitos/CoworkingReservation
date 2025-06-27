using CoworkingReservation.Domain.Enums;
using CoworkingReservation.Domain.IRepository;
using Microsoft.EntityFrameworkCore;

public class ReservationStatusJob
{
    private readonly IReservationRepository _resarvationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReservationStatusJob(IReservationRepository reservationRepository, IUnitOfWork unitOfWork)
    {
        _resarvationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ProcessPendingReservationAsync()
    {
        var confirmationWindowMinutes = 5; //Cada cuanto tiempo se ejecuta el JOB
        Console.WriteLine($"[JOB] Procesando reservas pendientes a las {DateTime.UtcNow}");

        var pendingReservations = await _resarvationRepository.GetQueryable()
            .Where(r => r.Status == ReservationStatus.Pending &&
            r.CreatedAt <= DateTime.UtcNow.AddMinutes(-confirmationWindowMinutes))
            .ToListAsync();

        foreach (var reservation in pendingReservations)
        {
            reservation.Status = ReservationStatus.Confirmed;
            reservation.UpdatedAt = DateTime.UtcNow;
            await _resarvationRepository.UpdateAsync(reservation);
            // Opcional: loguear cuántas reservas encontró
            Console.WriteLine($"[JOB] Se encontraron {pendingReservations.Count} reservas pendientes para confirmar.");
        }

        await _unitOfWork.SaveChangesAsync();
    }
}