using CoworkingReservation.Application.DTOs.PaymentMethod;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using Microsoft.Extensions.Logging;

namespace CoworkingReservation.Application.Services
{
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentMethodService> _logger;

        public PaymentMethodService(
            IPaymentMethodRepository paymentMethodRepository,
            IUnitOfWork unitOfWork,
            ILogger<PaymentMethodService> logger)
        {
            _paymentMethodRepository = paymentMethodRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<PaymentMethodDTO>> GetPaymentMethodsByUserAsync(int userId)
        {
            try
            {
                var paymentMethods = await _paymentMethodRepository.GetPaymentMethodsByUserAsync(userId);
                
                return paymentMethods.Select(pm => new PaymentMethodDTO
                {
                    Id = pm.Id,
                    UserId = pm.UserId,
                    Type = pm.Type,
                    Name = pm.Name,
                    CardNumber = pm.CardNumber,
                    Last4 = pm.Last4,
                    ExpiryMonth = pm.ExpiryMonth,
                    ExpiryYear = pm.ExpiryYear,
                    Cvv = pm.Cvv,
                    BankName = pm.BankName,
                    AccountNumber = pm.AccountNumber,
                    WalletType = pm.WalletType,
                    IsDefault = pm.IsDefault,
                    IsActive = pm.IsActive,
                    CreatedAt = pm.CreatedAt,
                    UpdatedAt = pm.UpdatedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener métodos de pago del usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<PaymentMethodDTO?> GetPaymentMethodByIdAsync(int id, int userId)
        {
            try
            {
                var paymentMethod = await _paymentMethodRepository.GetByIdAsync(id);
                
                if (paymentMethod == null || paymentMethod.UserId != userId)
                    return null;

                return new PaymentMethodDTO
                {
                    Id = paymentMethod.Id,
                    UserId = paymentMethod.UserId,
                    Type = paymentMethod.Type,
                    Name = paymentMethod.Name,
                    CardNumber = paymentMethod.CardNumber,
                    Last4 = paymentMethod.Last4,
                    ExpiryMonth = paymentMethod.ExpiryMonth,
                    ExpiryYear = paymentMethod.ExpiryYear,
                    Cvv = paymentMethod.Cvv,
                    BankName = paymentMethod.BankName,
                    AccountNumber = paymentMethod.AccountNumber,
                    WalletType = paymentMethod.WalletType,
                    IsDefault = paymentMethod.IsDefault,
                    IsActive = paymentMethod.IsActive,
                    CreatedAt = paymentMethod.CreatedAt,
                    UpdatedAt = paymentMethod.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener método de pago {Id} del usuario {UserId}", id, userId);
                throw;
            }
        }

        public async Task<PaymentMethodDTO> CreatePaymentMethodAsync(int userId, CreatePaymentMethodDTO createDto)
        {
            try
            {
                // Si es método por defecto, desactivar otros
                if (createDto.IsDefault)
                {
                    await _paymentMethodRepository.DeactivateOtherDefaultMethodsAsync(userId, 0);
                }

                var paymentMethod = new PaymentMethod
                {
                    UserId = userId,
                    Type = createDto.Type,
                    Name = createDto.Name,
                    CardNumber = createDto.CardNumber,
                    Last4 = createDto.CardNumber?.Length >= 4 ? createDto.CardNumber[^4..] : null,
                    ExpiryMonth = createDto.ExpiryMonth,
                    ExpiryYear = createDto.ExpiryYear,
                    Cvv = createDto.Cvv,
                    BankName = createDto.BankName,
                    AccountNumber = createDto.AccountNumber,
                    WalletType = createDto.WalletType,
                    IsDefault = createDto.IsDefault,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _paymentMethodRepository.AddAsync(paymentMethod);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Método de pago creado para el usuario {UserId}", userId);

                return new PaymentMethodDTO
                {
                    Id = paymentMethod.Id,
                    UserId = paymentMethod.UserId,
                    Type = paymentMethod.Type,
                    Name = paymentMethod.Name,
                    CardNumber = paymentMethod.CardNumber,
                    Last4 = paymentMethod.Last4,
                    ExpiryMonth = paymentMethod.ExpiryMonth,
                    ExpiryYear = paymentMethod.ExpiryYear,
                    Cvv = paymentMethod.Cvv,
                    BankName = paymentMethod.BankName,
                    AccountNumber = paymentMethod.AccountNumber,
                    WalletType = paymentMethod.WalletType,
                    IsDefault = paymentMethod.IsDefault,
                    IsActive = paymentMethod.IsActive,
                    CreatedAt = paymentMethod.CreatedAt,
                    UpdatedAt = paymentMethod.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear método de pago para el usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<PaymentMethodDTO> UpdatePaymentMethodAsync(int id, int userId, UpdatePaymentMethodDTO updateDto)
        {
            try
            {
                var paymentMethod = await _paymentMethodRepository.GetByIdAsync(id);
                
                if (paymentMethod == null || paymentMethod.UserId != userId)
                    throw new InvalidOperationException("Método de pago no encontrado");

                // Si se está estableciendo como predeterminado, desactivar otros
                if (updateDto.IsDefault && !paymentMethod.IsDefault)
                {
                    await _paymentMethodRepository.DeactivateOtherDefaultMethodsAsync(userId, id);
                }

                paymentMethod.Type = updateDto.Type;
                paymentMethod.Name = updateDto.Name;
                paymentMethod.CardNumber = updateDto.CardNumber;
                paymentMethod.Last4 = updateDto.CardNumber?.Length >= 4 ? updateDto.CardNumber[^4..] : null;
                paymentMethod.ExpiryMonth = updateDto.ExpiryMonth;
                paymentMethod.ExpiryYear = updateDto.ExpiryYear;
                paymentMethod.Cvv = updateDto.Cvv;
                paymentMethod.BankName = updateDto.BankName;
                paymentMethod.AccountNumber = updateDto.AccountNumber;
                paymentMethod.WalletType = updateDto.WalletType;
                paymentMethod.IsDefault = updateDto.IsDefault;
                paymentMethod.UpdatedAt = DateTime.UtcNow;

                await _paymentMethodRepository.UpdateAsync(paymentMethod);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Método de pago {Id} actualizado para el usuario {UserId}", id, userId);

                return new PaymentMethodDTO
                {
                    Id = paymentMethod.Id,
                    UserId = paymentMethod.UserId,
                    Type = paymentMethod.Type,
                    Name = paymentMethod.Name,
                    CardNumber = paymentMethod.CardNumber,
                    Last4 = paymentMethod.Last4,
                    ExpiryMonth = paymentMethod.ExpiryMonth,
                    ExpiryYear = paymentMethod.ExpiryYear,
                    Cvv = paymentMethod.Cvv,
                    BankName = paymentMethod.BankName,
                    AccountNumber = paymentMethod.AccountNumber,
                    WalletType = paymentMethod.WalletType,
                    IsDefault = paymentMethod.IsDefault,
                    IsActive = paymentMethod.IsActive,
                    CreatedAt = paymentMethod.CreatedAt,
                    UpdatedAt = paymentMethod.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar método de pago {Id} del usuario {UserId}", id, userId);
                throw;
            }
        }

        public async Task<bool> DeletePaymentMethodAsync(int id, int userId)
        {
            try
            {
                var paymentMethod = await _paymentMethodRepository.GetByIdAsync(id);
                
                if (paymentMethod == null || paymentMethod.UserId != userId)
                    return false;

                await _paymentMethodRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Método de pago {Id} eliminado para el usuario {UserId}", id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar método de pago {Id} del usuario {UserId}", id, userId);
                throw;
            }
        }

        public async Task<bool> SetDefaultPaymentMethodAsync(int userId, int paymentMethodId)
        {
            try
            {
                var result = await _paymentMethodRepository.SetDefaultPaymentMethodAsync(userId, paymentMethodId);
                await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    _logger.LogInformation("Método de pago {PaymentMethodId} establecido como predeterminado para el usuario {UserId}", paymentMethodId, userId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al establecer método de pago predeterminado {PaymentMethodId} para el usuario {UserId}", paymentMethodId, userId);
                throw;
            }
        }

        public async Task<PaymentMethodDTO?> GetDefaultPaymentMethodAsync(int userId)
        {
            try
            {
                var paymentMethod = await _paymentMethodRepository.GetDefaultPaymentMethodByUserAsync(userId);
                
                if (paymentMethod == null)
                    return null;

                return new PaymentMethodDTO
                {
                    Id = paymentMethod.Id,
                    UserId = paymentMethod.UserId,
                    Type = paymentMethod.Type,
                    Name = paymentMethod.Name,
                    CardNumber = paymentMethod.CardNumber,
                    Last4 = paymentMethod.Last4,
                    ExpiryMonth = paymentMethod.ExpiryMonth,
                    ExpiryYear = paymentMethod.ExpiryYear,
                    Cvv = paymentMethod.Cvv,
                    BankName = paymentMethod.BankName,
                    AccountNumber = paymentMethod.AccountNumber,
                    WalletType = paymentMethod.WalletType,
                    IsDefault = paymentMethod.IsDefault,
                    IsActive = paymentMethod.IsActive,
                    CreatedAt = paymentMethod.CreatedAt,
                    UpdatedAt = paymentMethod.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener método de pago predeterminado del usuario {UserId}", userId);
                throw;
            }
        }
    }
} 