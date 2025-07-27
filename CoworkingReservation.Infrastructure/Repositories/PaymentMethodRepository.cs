using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.Data;
using CoworkingReservation.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CoworkingReservation.Infrastructure.Repositories
{
    public class PaymentMethodRepository : Repository<PaymentMethod>, IPaymentMethodRepository
    {
        public PaymentMethodRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<PaymentMethod>> GetPaymentMethodsByUserAsync(int userId)
        {
            return await _context.PaymentMethods
                .Where(pm => pm.UserId == userId && pm.IsActive)
                .OrderByDescending(pm => pm.IsDefault)
                .ThenBy(pm => pm.CreatedAt)
                .ToListAsync();
        }

        public async Task<PaymentMethod?> GetDefaultPaymentMethodByUserAsync(int userId)
        {
            return await _context.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.UserId == userId && pm.IsDefault && pm.IsActive);
        }

        public async Task<bool> SetDefaultPaymentMethodAsync(int userId, int paymentMethodId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Desactivar otros métodos de pago por defecto del usuario
                await DeactivateOtherDefaultMethodsAsync(userId, paymentMethodId);

                // Establecer el nuevo método como predeterminado
                var paymentMethod = await _context.PaymentMethods
                    .FirstOrDefaultAsync(pm => pm.Id == paymentMethodId && pm.UserId == userId);

                if (paymentMethod != null)
                {
                    paymentMethod.IsDefault = true;
                    paymentMethod.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }

                return false;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeactivateOtherDefaultMethodsAsync(int userId, int excludePaymentMethodId)
        {
            var defaultMethods = await _context.PaymentMethods
                .Where(pm => pm.UserId == userId && pm.IsDefault && pm.Id != excludePaymentMethodId)
                .ToListAsync();

            foreach (var method in defaultMethods)
            {
                method.IsDefault = false;
                method.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByUserAndTypeAsync(int userId, string type)
        {
            return await _context.PaymentMethods
                .AnyAsync(pm => pm.UserId == userId && pm.Type == type && pm.IsActive);
        }
    }
} 