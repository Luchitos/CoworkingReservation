using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoworkingReservation.Infrastructure.Repositories
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Transaction>> GetTransactionsByUserAsync(int userId)
        {
            return await _context.Transactions
                .Include(t => t.PaymentMethod)
                .Include(t => t.Reservation)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetTransactionsByPaymentMethodAsync(int paymentMethodId)
        {
            return await _context.Transactions
                .Include(t => t.PaymentMethod)
                .Include(t => t.Reservation)
                .Where(t => t.PaymentMethodId == paymentMethodId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetTransactionsByReservationAsync(int reservationId)
        {
            return await _context.Transactions
                .Include(t => t.PaymentMethod)
                .Include(t => t.Reservation)
                .Where(t => t.ReservationId == reservationId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Transaction?> GetTransactionByExternalIdAsync(string externalTransactionId)
        {
            return await _context.Transactions
                .Include(t => t.PaymentMethod)
                .Include(t => t.Reservation)
                .FirstOrDefaultAsync(t => t.ExternalTransactionId == externalTransactionId);
        }

        public async Task<List<Transaction>> GetTransactionsByStatusAsync(int userId, string status)
        {
            return await _context.Transactions
                .Include(t => t.PaymentMethod)
                .Include(t => t.Reservation)
                .Where(t => t.UserId == userId && t.Status == status)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
} 