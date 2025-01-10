using CoworkingReservation.Application.DTOs.User;
using CoworkingReservation.Application.Services;
using CoworkingReservation.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoworkingReservation.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de usuarios.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpGet("debug-token")]
        [Authorize]
        public IActionResult DebugToken()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(Responses.Response.Success(claims));
        }

        /// <summary>
        /// Registra un nuevo usuario.
        /// </summary>
        /// <param name="userDto">Datos del usuario.</param>
        /// <returns>Usuario creado.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] UserRegisterDTO userDto)
        {
            try
            {
                var user = await _userService.RegisterAsync(userDto);
                return Ok(Responses.Response.Success(user));
            }
            catch (Exception ex)
            {
                return BadRequest(Responses.Response.Failure(ex.Message));
            }
        }

        /// <summary>
        /// Habilita o deshabilita la cuenta del usuario autenticado.
        /// </summary>
        [HttpPatch("toggle-active")]
        [Authorize] // Solo usuarios autenticados
        public async Task<IActionResult> ToggleActiveStatus()
        {
            try
            {
                // Obtener el userId del token
                var userId = GetUserIdFromToken();

                await _userService.ToggleActiveStatusAsync(userId);
                return Ok(new { Message = "User active status toggled successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Solicita el cambio de rol del usuario autenticado a hoster.
        /// </summary>
        [HttpPost("become-hoster")]
        [Authorize(Roles = "Client")] // El rol "Client" debe existir en el token
        public async Task<IActionResult> BecomeHoster()
        {
            try
            {
                var userId = GetUserIdFromToken();
                await _userService.BecomeHosterAsync(userId);
                return Ok(new { Message = "Hoster request submitted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyDetails()
        {
            var userId = TokenUtils.GetUserIdFromToken(User);
            var user = await _userService.GetByIdAsync(userId);
            return Ok(Responses.Response.Success(user));
        }

        /// <summary>
        /// Extrae el UserId del token JWT.
        /// </summary>
        /// <returns>El UserId del usuario autenticado.</returns>
        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("Invalid token: UserId not found.");

            if (!int.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Invalid UserId format in token.");

            return userId;
        }

    }
}
