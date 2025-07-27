using CoworkingReservation.Application.DTOs.PaymentMethod;

namespace CoworkingReservation.Application.Services.Interfaces
{
    public interface IPaymentMethodService
    {
        Task<List<PaymentMethodDTO>> GetPaymentMethodsByUserAsync(int userId);
        Task<PaymentMethodDTO?> GetPaymentMethodByIdAsync(int id, int userId);
        Task<PaymentMethodDTO> CreatePaymentMethodAsync(int userId, CreatePaymentMethodDTO createDto);
        Task<PaymentMethodDTO> UpdatePaymentMethodAsync(int id, int userId, UpdatePaymentMethodDTO updateDto);
        Task<bool> DeletePaymentMethodAsync(int id, int userId);
        Task<bool> SetDefaultPaymentMethodAsync(int userId, int paymentMethodId);
        Task<PaymentMethodDTO?> GetDefaultPaymentMethodAsync(int userId);
    }
} 