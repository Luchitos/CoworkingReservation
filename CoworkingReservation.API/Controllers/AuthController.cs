using CoworkingReservation.Application.Services;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Application.DTOs.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

                // Devolver respuesta exitosa con el token y la URL directa de la foto
                return Ok(Responses.Response.Success(new
                {
                    Token = token,
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
                return StatusCode(500, Responses.Response.Failure(ex.Message));
            }
        }

        /// <summary>
        /// Invalida el token en el frontend eliminándolo del almacenamiento local.
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public bool Logout()
        {
            return true;
        }

        /// <summary>
        /// Renueva el token JWT del usuario autenticado.
        /// </summary>
        /// <returns>Nuevo token JWT con 60 minutos de duración.</returns>
        [HttpPost("refresh")]
        [Authorize]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                // Obtener el userId del token actual
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var emailClaim = User.FindFirst(ClaimTypes.Email);
                var roleClaim = User.FindFirst(ClaimTypes.Role);

                if (userIdClaim == null || emailClaim == null || roleClaim == null)
                {
                    return Unauthorized(Responses.Response.Failure("Token inválido o incompleto."));
                }

                var userId = int.Parse(userIdClaim.Value);
                var email = emailClaim.Value;
                var role = roleClaim.Value;

                // Verificar que el usuario existe y está activo
                var user = await _userService.GetByIdAsync(userId);
                if (user == null || !user.IsActive)
                {
                    return Unauthorized(Responses.Response.Failure("Usuario no encontrado o inactivo."));
                }

                // Generar nuevo token
                var newToken = _tokenService.GenerateToken(userId, email, role);

                return Ok(Responses.Response.Success(new
                {
                    Token = newToken,
                    ExpiresIn = 60 * 60, // 60 minutos en segundos
                    Message = "Token renovado exitosamente"
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Responses.Response.Failure($"Error al renovar el token: {ex.Message}"));
            }
        }
    }
} 