public interface IReservationJobService
{
    Task CompleteExpiredReservationsAsync();
}
