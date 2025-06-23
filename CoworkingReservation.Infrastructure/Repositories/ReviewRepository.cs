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
        try
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.CoworkingSpaceId == coworkingSpaceId 
                           && r.User != null 
                           && r.UserId > 0
                           && r.Rating >= 1 
                           && r.Rating <= 5)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // Log del error específico para debugging
            Console.WriteLine($"Error en GetReviewsByCoworkingSpaceAsync: {ex.Message}");
            Console.WriteLine($"CoworkingSpaceId: {coworkingSpaceId}");
            
            // Intentar una consulta más simple para identificar el problema
            var reviewCount = await _context.Reviews.CountAsync(r => r.CoworkingSpaceId == coworkingSpaceId);
            Console.WriteLine($"Total reviews para CoworkingSpace {coworkingSpaceId}: {reviewCount}");
            
            throw; // Re-lanzar la excepción para que el controlador la maneje
        }
    }

    public async Task<Review?> GetByUserAndReservationAsync(int userId, int reservationId)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.CoworkingSpace)
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ReservationId == reservationId);
    }

}
