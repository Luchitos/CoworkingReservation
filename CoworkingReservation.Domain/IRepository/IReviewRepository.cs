using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;

public interface IReviewRepository : IRepository<Review>
{
    Task<bool> ExistsByReservationIdAsync(int reservationId);

    Task<List<Review>> GetReviewsByCoworkingSpaceAsync(int coworkingSpaceId);

}
