using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Domain.IRepository
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<List<Transaction>> GetTransactionsByUserAsync(int userId);
        Task<List<Transaction>> GetTransactionsByPaymentMethodAsync(int paymentMethodId);
        Task<List<Transaction>> GetTransactionsByReservationAsync(int reservationId);
        Task<Transaction?> GetTransactionByExternalIdAsync(string externalTransactionId);
        Task<List<Transaction>> GetTransactionsByStatusAsync(int userId, string status);
    }
} 