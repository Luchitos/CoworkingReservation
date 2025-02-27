using CoworkingReservation.Application.DTOs.User;
using CoworkingReservation.Application.Services;
using CoworkingReservation.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CoworkingReservation.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de usuarios y autenticación.
    /// </summary>
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        #region Fields

        private readonly IUserService _userService;

        #endregion

        #region Constructor

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="UserController"/>.
        /// </summary>
        /// <param name="userService">Servicio de usuarios.</param>
        public UserController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Devuelve los claims del token autenticado (para debugging).
        /// </summary>
        [HttpGet("debug-token")]
        [Authorize]
        public IActionResult DebugToken()
        {
            var claims = new
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                email = User.FindFirstValue(ClaimTypes.Email),
                role = User.FindFirstValue(ClaimTypes.Role),
                sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub),
                jwtId = User.FindFirstValue(JwtRegisteredClaimNames.Jti),
                issuer = User.FindFirst(ClaimTypes.Name)?.Issuer,
                audience = User.FindFirst(ClaimTypes.Name)?.Value,
                expiration = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value
            };

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
        [Authorize]
        public async Task<IActionResult> ToggleActiveStatus()
        {
            try
            {
                var userId = TokenUtils.GetUserIdFromToken(User);
                await _userService.ToggleActiveStatusAsync(userId);
                return Ok(Responses.Response.Success("User active status toggled successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(Responses.Response.Failure(ex.Message));
            }
        }

        /// <summary>
        /// Solicita el cambio de rol del usuario autenticado a hoster.
        /// </summary>
        [HttpPost("become-hoster")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> BecomeHoster()
        {
            try
            {
                var userId = TokenUtils.GetUserIdFromToken(User);
                await _userService.BecomeHosterAsync(userId);
                return Ok(Responses.Response.Success("Role changed to Hoster successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(Responses.Response.Failure(ex.Message));
            }
        }

        /// <summary>
        /// Obtiene los detalles del usuario autenticado.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyDetails()
        {
            var userId = TokenUtils.GetUserIdFromToken(User);
            var user = await _userService.GetByIdAsync(userId);
            return Ok(Responses.Response.Success(user));
        }

        #endregion
    }
}
