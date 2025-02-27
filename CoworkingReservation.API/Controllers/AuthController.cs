using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Application.DTOs.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

namespace CoworkingReservation.API.Controllers
{
    /// <summary>
    /// Controlador para la autenticación de usuarios y gestión de sesiones.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        #region Fields

        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        #endregion

        #region Constructor

        /// <summary>
        /// Inicializa una nueva instancia del <see cref="AuthController"/>.
        /// </summary>
        /// <param name="userService">Servicio de usuarios para autenticación.</param>
        /// <param name="tokenService">Servicio de generación de tokens JWT.</param>
        public AuthController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Autentica a un usuario y devuelve un token JWT.
        /// </summary>
        /// <param name="request">Datos de login.</param>
        /// <returns>Token JWT si las credenciales son correctas.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(Responses.Response.Failure("Request cannot be null."));

                if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(Responses.Response.Failure("Identifier and password are required."));
                }

                var user = await _userService.AuthenticateAsync(request.Identifier, request.Password);
                if (user == null)
                {
                    return Unauthorized(Responses.Response.Failure("Invalid credentials."));
                }

                var token = _tokenService.GenerateToken(user.Id, user.Email, user.Role);

                return Ok(Responses.Response.Success(new
                {
                    Token = token,
                    User = new
                    {
                        user.Id,
                        user.Name,
                        user.Email,
                        user.Role
                    }
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Responses.Response.Failure($"Internal Server Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cierra sesión invalidando el token en el frontend.
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return Ok(Responses.Response.Success("User logged out successfully."));
        }

        #endregion
    }
}
