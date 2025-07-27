using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoworkingReservation.Application.DTOs.PaymentMethod;
using CoworkingReservation.Application.Services.Interfaces;
using System.Security.Claims;

namespace CoworkingReservation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentMethodController : ControllerBase
    {
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly ILogger<PaymentMethodController> _logger;

        public PaymentMethodController(
            IPaymentMethodService paymentMethodService,
            ILogger<PaymentMethodController> logger)
        {
            _paymentMethodService = paymentMethodService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetPaymentMethods()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var paymentMethods = await _paymentMethodService.GetPaymentMethodsByUserAsync(userId);
                return Ok(new { status = 200, data = paymentMethods, error = (string?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener métodos de pago para el usuario");
                return StatusCode(500, new { status = 500, data = (object?)null, error = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentMethod(int id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id, userId);
                
                if (paymentMethod == null)
                {
                    return NotFound(new { status = 404, data = (object?)null, error = "Método de pago no encontrado" });
                }

                return Ok(new { status = 200, data = paymentMethod, error = (string?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener método de pago con ID {Id}", id);
                return StatusCode(500, new { status = 500, data = (object?)null, error = "Error interno del servidor" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentMethod([FromBody] CreatePaymentMethodDTO createDto)
        {
            try
            {
                var userId = GetUserIdFromToken();

                var paymentMethod = await _paymentMethodService.CreatePaymentMethodAsync(userId, createDto);
                return CreatedAtAction(nameof(GetPaymentMethod), new { id = paymentMethod.Id }, 
                    new { status = 201, data = paymentMethod, error = (string?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear método de pago");
                return StatusCode(500, new { status = 500, data = (object?)null, error = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePaymentMethod(int id, [FromBody] UpdatePaymentMethodDTO updateDto)
        {
            try
            {
                var userId = GetUserIdFromToken();

                var paymentMethod = await _paymentMethodService.UpdatePaymentMethodAsync(id, userId, updateDto);
                
                if (paymentMethod == null)
                {
                    return NotFound(new { status = 404, data = (object?)null, error = "Método de pago no encontrado" });
                }

                return Ok(new { status = 200, data = paymentMethod, error = (string?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar método de pago con ID {Id}", id);
                return StatusCode(500, new { status = 500, data = (object?)null, error = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentMethod(int id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var result = await _paymentMethodService.DeletePaymentMethodAsync(id, userId);
                
                if (!result)
                {
                    return NotFound(new { status = 404, data = (object?)null, error = "Método de pago no encontrado" });
                }

                return Ok(new { status = 200, data = new { message = "Método de pago eliminado correctamente" }, error = (string?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar método de pago con ID {Id}", id);
                return StatusCode(500, new { status = 500, data = (object?)null, error = "Error interno del servidor" });
            }
        }

        [HttpPost("{id}/set-default")]
        public async Task<IActionResult> SetDefaultPaymentMethod(int id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var result = await _paymentMethodService.SetDefaultPaymentMethodAsync(id, userId);
                
                if (!result)
                {
                    return NotFound(new { status = 404, data = (object?)null, error = "Método de pago no encontrado" });
                }

                return Ok(new { status = 200, data = new { message = "Método de pago establecido como predeterminado" }, error = (string?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al establecer método de pago predeterminado con ID {Id}", id);
                return StatusCode(500, new { status = 500, data = (object?)null, error = "Error interno del servidor" });
            }
        }

        [HttpGet("default")]
        public async Task<IActionResult> GetDefaultPaymentMethod()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var paymentMethod = await _paymentMethodService.GetDefaultPaymentMethodAsync(userId);
                
                if (paymentMethod == null)
                {
                    return NotFound(new { status = 404, data = (object?)null, error = "No se encontró método de pago predeterminado" });
                }

                return Ok(new { status = 200, data = paymentMethod, error = (string?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener método de pago predeterminado");
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