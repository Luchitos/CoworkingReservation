using CoworkingReservation.Application.DTOs.Transaction;

namespace CoworkingReservation.Application.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<List<TransactionDTO>> GetTransactionsByUserIdAsync(int userId);
        Task<TransactionDTO?> GetTransactionByIdAsync(int id, int userId);
        Task<TransactionDTO> CreateTransactionAsync(CreateTransactionDTO createDto);
        Task<TransactionDTO?> UpdateTransactionStatusAsync(int id, UpdateTransactionStatusDTO updateDto);
        Task<bool> DeleteTransactionAsync(int id);
        Task<List<TransactionDTO>> GetTransactionsByStatusAsync(int userId, string status);
        Task<TransactionDTO?> GetTransactionByExternalIdAsync(string externalTransactionId);
        Task<List<TransactionDTO>> GetTransactionsByPaymentMethodAsync(int paymentMethodId, int userId);
        Task<List<TransactionDTO>> GetTransactionsByReservationAsync(int reservationId, int userId);
    }
} 