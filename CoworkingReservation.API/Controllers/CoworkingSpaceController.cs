using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using System.Threading.Tasks;

namespace CoworkingReservation.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de espacios de coworking.
    /// </summary>
    [ApiController]
    [Route("api/coworking-spaces")]
    public class CoworkingSpaceController : ControllerBase
    {
        #region Fields

        private readonly ICoworkingSpaceService _coworkingSpaceService;

        #endregion

        #region Constructor

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="CoworkingSpaceController"/>.
        /// </summary>
        /// <param name="coworkingSpaceService">Servicio de espacios de coworking.</param>
        public CoworkingSpaceController(ICoworkingSpaceService coworkingSpaceService)
        {
            _coworkingSpaceService = coworkingSpaceService ?? throw new ArgumentNullException(nameof(coworkingSpaceService));
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Obtiene un espacio de coworking por su ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var space = await _coworkingSpaceService.GetByIdAsync(id);
            if (space == null)
            {
                return NotFound(Responses.Response.Failure("Coworking space not found."));
            }
            return Ok(Responses.Response.Success(space));
        }

        /// <summary>
        /// Obtiene todos los espacios de coworking activos y aprobados.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var spaces = await _coworkingSpaceService.GetAllActiveSpacesAsync();
            return Ok(Responses.Response.Success(spaces));
        }

        /// <summary>
        /// Obtiene los coworkings filtrados por capacidad y/o ubicación.
        /// </summary>
        /// <param name="capacity">Capacidad exacta (opcional).</param>
        /// <param name="location">Ubicación (ciudad, provincia o calle) (opcional).</param>
        [HttpGet("filter")]
        public async Task<IActionResult> GetFiltered([FromQuery] int? capacity, [FromQuery] string? location)
        {
            var spaces = await _coworkingSpaceService.GetAllFilteredAsync(capacity, location);
            return Ok(Responses.Response.Success(spaces));
        }

        /// <summary>
        /// Obtiene todos los espacios de coworking de un hoster autenticado.
        /// </summary>
        [HttpGet("my-spaces")]
        [Authorize(Roles = "Hoster")]
        public async Task<IActionResult> GetMySpaces()
        {
            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var spaces = await _coworkingSpaceService.GetAllByHosterIdAsync(hosterId);
            return Ok(Responses.Response.Success(spaces));
        }

        /// <summary>
        /// Crea un nuevo espacio de coworking.
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] CreateCoworkingSpaceDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(Responses.Response.Failure("Invalid data."));
            }

            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var space = await _coworkingSpaceService.CreateAsync(dto, hosterId);
            return Ok(Responses.Response.Success("Coworking space created successfully."));
        }

        /// <summary>
        /// Actualiza un espacio de coworking.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Hoster")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCoworkingSpaceDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(Responses.Response.Failure("Invalid data."));
            }

            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            await _coworkingSpaceService.UpdateAsync(id, dto, hosterId, userRole);
            return Ok(Responses.Response.Success("Coworking space updated successfully."));
        }

        /// <summary>
        /// Cambia el estado activo/inactivo de un espacio de coworking.
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Hoster,Admin")]
        public async Task<IActionResult> ToggleActiveStatus(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                await _coworkingSpaceService.ToggleActiveStatusAsync(id, userId, userRole);
                return Ok(Responses.Response.Success("Coworking space status updated successfully."));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Responses.Response.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Responses.Response.Failure(ex.Message));
            }
        }

        /// <summary>
        /// Elimina un espacio de coworking.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Hoster")]
        public async Task<IActionResult> Delete(int id)
        {
            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _coworkingSpaceService.DeleteAsync(id, hosterId);
            return Ok(Responses.Response.Success("Coworking space deleted successfully."));
        }

        #endregion
    }
}
