using CoworkingReservation.Application.DTOs.Transaction;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using Microsoft.Extensions.Logging;
using CoworkingReservation.Infrastructure.UnitOfWork;

namespace CoworkingReservation.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            ITransactionRepository transactionRepository,
            IPaymentMethodRepository paymentMethodRepository,
            IUnitOfWork unitOfWork,
            ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<TransactionDTO>> GetTransactionsByUserIdAsync(int userId)
        {
            try
            {
                var transactions = await _transactionRepository.GetTransactionsByUserAsync(userId);
                return transactions.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacciones del usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<TransactionDTO?> GetTransactionByIdAsync(int id, int userId)
        {
            try
            {
                var transaction = await _transactionRepository.GetByIdAsync(id);
                if (transaction == null || transaction.UserId != userId)
                    return null;

                return MapToDTO(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacción {TransactionId} para usuario {UserId}", id, userId);
                throw;
            }
        }

        public async Task<TransactionDTO> CreateTransactionAsync(CreateTransactionDTO createDto)
        {
            try
            {
                // Verificar que el método de pago pertenece al usuario
                var paymentMethod = await _paymentMethodRepository.GetByIdAsync(createDto.PaymentMethodId);
                if (paymentMethod == null || paymentMethod.UserId != createDto.UserId)
                    throw new UnauthorizedAccessException("Método de pago no válido para este usuario.");

                var transaction = new Transaction
                {
                    UserId = createDto.UserId,
                    PaymentMethodId = createDto.PaymentMethodId,
                    ReservationId = createDto.ReservationId,
                    Amount = createDto.Amount,
                    Currency = createDto.Currency ?? "ARS",
                    Status = "pending",
                    Description = createDto.Description,
                    CreatedAt = DateTime.UtcNow,
                    ExternalTransactionId = createDto.ExternalTransactionId,
                    Notes = createDto.Notes
                };

                await _transactionRepository.AddAsync(transaction);
                await _unitOfWork.SaveChangesAsync();

                return MapToDTO(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear transacción para usuario {UserId}", createDto.UserId);
                throw;
            }
        }

        public async Task<TransactionDTO?> UpdateTransactionStatusAsync(int id, UpdateTransactionStatusDTO updateDto)
        {
            try
            {
                var transaction = await _transactionRepository.GetByIdAsync(id);
                if (transaction == null)
                    return null;

                transaction.Status = updateDto.Status;
                transaction.UpdatedAt = DateTime.UtcNow;
                
                if (updateDto.Status == "completed")
                {
                    transaction.CompletedAt = DateTime.UtcNow;
                }

                await _transactionRepository.UpdateAsync(transaction);
                await _unitOfWork.SaveChangesAsync();

                return MapToDTO(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado de transacción {TransactionId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            try
            {
                var transaction = await _transactionRepository.GetByIdAsync(id);
                if (transaction == null)
                    return false;

                await _transactionRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar transacción {TransactionId}", id);
                throw;
            }
        }

        public async Task<List<TransactionDTO>> GetTransactionsByStatusAsync(int userId, string status)
        {
            try
            {
                var transactions = await _transactionRepository.GetTransactionsByStatusAsync(userId, status);
                return transactions.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacciones por estado {Status} para usuario {UserId}", status, userId);
                throw;
            }
        }

        public async Task<TransactionDTO?> GetTransactionByExternalIdAsync(string externalTransactionId)
        {
            try
            {
                var transaction = await _transactionRepository.GetTransactionByExternalIdAsync(externalTransactionId);
                return transaction != null ? MapToDTO(transaction) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacción por ID externo {ExternalTransactionId}", externalTransactionId);
                throw;
            }
        }

        public async Task<List<TransactionDTO>> GetTransactionsByPaymentMethodAsync(int paymentMethodId, int userId)
        {
            try
            {
                var transactions = await _transactionRepository.GetTransactionsByPaymentMethodAsync(paymentMethodId);
                // Filtrar por usuario para seguridad
                return transactions.Where(t => t.UserId == userId).Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacciones por método de pago {PaymentMethodId} para usuario {UserId}", paymentMethodId, userId);
                throw;
            }
        }

        public async Task<List<TransactionDTO>> GetTransactionsByReservationAsync(int reservationId, int userId)
        {
            try
            {
                var transactions = await _transactionRepository.GetTransactionsByReservationAsync(reservationId);
                // Filtrar por usuario para seguridad
                return transactions.Where(t => t.UserId == userId).Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacciones por reserva {ReservationId} para usuario {UserId}", reservationId, userId);
                throw;
            }
        }

        private static TransactionDTO MapToDTO(Transaction transaction)
        {
            return new TransactionDTO
            {
                Id = transaction.Id,
                UserId = transaction.UserId,
                PaymentMethodId = transaction.PaymentMethodId,
                ReservationId = transaction.ReservationId,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                Status = transaction.Status,
                Description = transaction.Description,
                CreatedAt = transaction.CreatedAt,
                UpdatedAt = transaction.UpdatedAt,
                CompletedAt = transaction.CompletedAt,
                ExternalTransactionId = transaction.ExternalTransactionId,
                Notes = transaction.Notes,
                PaymentMethodName = transaction.PaymentMethod?.Name ?? "N/A"
            };
        }
    }
} 