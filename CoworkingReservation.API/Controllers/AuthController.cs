using CoworkingReservation.Application.Services;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Application.DTOs.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CoworkingReservation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public AuthController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }


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
                // Validar entrada
                if (string.IsNullOrEmpty(request.Identifier) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(Responses.Response.Failure("Identifier and password are required."));
                }

                // Autenticar usuario
                var user = await _userService.AuthenticateAsync(request.Identifier, request.Password);
                if (user == null)
                {
                    return Unauthorized(Responses.Response.Failure("Invalid credentials."));
                }

                // Generar token
                var token = _tokenService.GenerateToken(user.Id, user.Email, user.Role);

                // Devolver respuesta exitosa con el token
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
                return StatusCode(500, Responses.Response.Failure(ex.Message));
            }
        }

        /// <summary>
        /// Invalida el token en el frontend eliminándolo del almacenamiento local.
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return Ok(Responses.Response.Success("User logged out successfully."));
        }
    }
}
