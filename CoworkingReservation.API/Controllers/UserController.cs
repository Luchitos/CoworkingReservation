using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CoworkingReservation.Application.DTOs.User;
using CoworkingReservation.Application.Services;
using CoworkingReservation.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        private readonly ITokenService _tokenService;


        public UserController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;

        }


        [HttpGet("debug-token")]
        [Authorize]
        public IActionResult DebugToken()
        {
            var claims = new
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier), // Extrae el UserID
                email = User.FindFirstValue(ClaimTypes.Email), // Extrae el email
                role = User.FindFirstValue(ClaimTypes.Role), // Extrae el rol
                sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub), // Extrae el Subject (User ID)
                jwtId = User.FindFirstValue(JwtRegisteredClaimNames.Jti), // Extrae el JWT ID
                issuer = User.FindFirst(ClaimTypes.Name)?.Issuer, // Extrae el emisor
                audience = User.FindFirst(ClaimTypes.Name)?.Value, // Extrae la audiencia
                expiration = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value // Extrae la expiración
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
                var token = _tokenService.GenerateToken(user.Id, user.Email, user.Role);

                return Ok(Responses.Response.Success(new
                {
                    token = token,
                    User = new
                    {
                        user.Id,
                        user.Name,
                        user.Lastname,
                        user.Email,
                        user.Role,
                        ProfilePhotoUrl = user.Photo?.FilePath // URL directa a ImgBB
                    }
                }));
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
                return Ok(Responses.Response.Success("Role changed to Hoster successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(Responses.Response.Failure($"{ex.Message}"));
            }
        }
        [HttpPost("toggle-favorite")]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite([FromQuery] int coworkingSpaceId, [FromQuery] bool isFavorite)
        {
            try
            {
                var userId = GetUserIdFromToken();

                await _userService.ToggleFavoriteAsync(userId, coworkingSpaceId, isFavorite);

                return Ok(Responses.Response.Success("Favorite updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(Responses.Response.Failure(ex.Message));
            }
        }

        [HttpGet("favorites")]
        [Authorize]
        public async Task<IActionResult> GetFavorites()
        {
            var userId = GetUserIdFromToken();
            var data = await _userService.GetFavoriteSpacesResponseAsync(userId);

            var result = new
            {
                status = 200,
                data = data,
                error = (string)null
            };

            return Ok(result);
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
