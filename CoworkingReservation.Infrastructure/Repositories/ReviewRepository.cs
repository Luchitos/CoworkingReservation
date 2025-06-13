using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Infrastructure.Data;
using CoworkingReservation.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context) : base(context) { }

    public async Task<bool> ExistsByReservationIdAsync(int reservationId)
    {
        return await _context.Reviews.AnyAsync(r => r.ReservationId == reservationId);
    }

    public async Task<List<Review>> GetReviewsByCoworkingSpaceAsync(int coworkingSpaceId)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.CoworkingSpaceId == coworkingSpaceId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

}
