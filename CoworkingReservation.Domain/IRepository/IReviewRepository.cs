using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;

public interface IReviewRepository : IRepository<Review>
{
    Task<bool> ExistsByReservationIdAsync(int reservationId);

    Task<List<Review>> GetReviewsByCoworkingSpaceAsync(int coworkingSpaceId);

    Task<Review?> GetByUserAndReservationAsync(int userId, int reservationId);

    Task<List<Review>> GetReviewsByUserAsync(int userId);

}
