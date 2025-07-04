using CoworkingReservation.Application.DTOs.User;
using CoworkingReservation.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    }
} 