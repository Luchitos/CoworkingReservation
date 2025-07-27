using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoworkingReservation.Application.DTOs.Transaction;
using CoworkingReservation.Application.Services.Interfaces;
using System.Security.Claims;

namespace CoworkingReservation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(
            ITransactionService transactionService,
            ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery] string? status = null)
        {
            try
            {
                var userId = GetUserIdFromToken();
                List<TransactionDTO> transactions;

                if (!string.IsNullOrEmpty(status))
                {
                    transactions = await _transactionService.GetTransactionsByStatusAsync(userId, status);
                }
                else
                {
                    transactions = await _transactionService.GetTransactionsByUserIdAsync(userId);
                }

                return Ok(new { status = 200, data = transactions, error = (string?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacciones para el usuario");
                return StatusCode(500, new { status = 500, data = (object?)null, error = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction(int id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var transaction = await _transactionService.GetTransactionByIdAsync(id, userId);
                
                if (transaction == null)
                {
                    return NotFound(new { status = 404, data = (object?)null, error = "Transacción no encontrada" });
                }

                return Ok(new { status = 200, data = transaction, error = (string?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacción con ID {Id}", id);
                return StatusCode(500, new { status = 500, data = (object?)null, error = "Error interno del servidor" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDTO createDto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                createDto.UserId = userId;

                var transaction = await _transactionService.CreateTransactionAsync(createDto);
                return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, 
                    new { status = 201, data = transaction, error = (string?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear transacción");
                return StatusCode(500, new { status = 500, data = (object?)null, error = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTransactionStatus(int id, [FromBody] UpdateTransactionStatusDTO statusDto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var transaction = await _transactionService.UpdateTransactionStatusAsync(id, statusDto);
                
                if (transaction == null)
                {
                    return NotFound(new { status = 404, data = (object?)null, error = "Transacción no encontrada" });
                }

                return Ok(new { status = 200, data = transaction, error = (string?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado de transacción con ID {Id}", id);
                return StatusCode(500, new { status = 500, data = (object?)null, error = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var result = await _transactionService.DeleteTransactionAsync(id);
                
                if (!result)
                {
                    return NotFound(new { status = 404, data = (object?)null, error = "Transacción no encontrada" });
                }

                return Ok(new { status = 200, data = new { message = "Transacción eliminada correctamente" }, error = (string?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar transacción con ID {Id}", id);
                return StatusCode(500, new { status = 500, data = (object?)null, error = "Error interno del servidor" });
            }
        }

        [HttpGet("external/{externalTransactionId}")]
        public async Task<IActionResult> GetTransactionByExternalId(string externalTransactionId)
        {
            try
            {
                var transaction = await _transactionService.GetTransactionByExternalIdAsync(externalTransactionId);
                
                if (transaction == null)
                {
                    return NotFound(new { status = 404, data = (object?)null, error = "Transacción no encontrada" });
                }

                return Ok(new { status = 200, data = transaction, error = (string?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacción por ID externo {ExternalTransactionId}", externalTransactionId);
                return StatusCode(500, new { status = 500, data = (object?)null, error = "Error interno del servidor" });
            }
        }

        [HttpGet("payment-method/{paymentMethodId}")]
        public async Task<IActionResult> GetTransactionsByPaymentMethod(int paymentMethodId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var transactions = await _transactionService.GetTransactionsByPaymentMethodAsync(paymentMethodId, userId);
                
                return Ok(new { status = 200, data = transactions, error = (string?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacciones por método de pago {PaymentMethodId}", paymentMethodId);
                return StatusCode(500, new { status = 500, data = (object?)null, error = "Error interno del servidor" });
            }
        }

        [HttpGet("reservation/{reservationId}")]
        public async Task<IActionResult> GetTransactionsByReservation(int reservationId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var transactions = await _transactionService.GetTransactionsByReservationAsync(reservationId, userId);
                
                return Ok(new { status = 200, data = transactions, error = (string?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacciones por reserva {ReservationId}", reservationId);
                return StatusCode(500, new { status = 500, data = (object?)null, error = "Error interno del servidor" });
            }
        }

        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Usuario no autenticado");
            }
            return userId;
        }
    }
} 