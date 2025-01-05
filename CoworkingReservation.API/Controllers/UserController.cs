using CoworkingReservation.API.Responses;
using CoworkingReservation.Application.DTOs.User;
using CoworkingReservation.Application.Services.Interfaces;
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

        public UserController(IUserService userService)
        {
            _userService = userService;
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
    }
}
