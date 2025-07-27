using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Domain.IRepository
{
    public interface IPaymentMethodRepository : IRepository<PaymentMethod>
    {
        Task<List<PaymentMethod>> GetPaymentMethodsByUserAsync(int userId);
        Task<PaymentMethod?> GetDefaultPaymentMethodByUserAsync(int userId);
        Task<bool> SetDefaultPaymentMethodAsync(int userId, int paymentMethodId);
        Task<bool> DeactivateOtherDefaultMethodsAsync(int userId, int excludePaymentMethodId);
        Task<bool> ExistsByUserAndTypeAsync(int userId, string type);
    }
} 