using CoworkingReservation.Application.DTOs.CoworkingSpace;
using System.Security.Claims;
using CoworkingReservation.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoworkingReservation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoworkingSpaceController : ControllerBase
    {
        private readonly ICoworkingSpaceService _coworkingSpaceService;

        public CoworkingSpaceController(ICoworkingSpaceService coworkingSpaceService)
        {
            _coworkingSpaceService = coworkingSpaceService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var space = await _coworkingSpaceService.GetByIdAsync(id);
            return Ok(Responses.Response.Success(space));
        }

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
        /// <returns>Lista de espacios aprobados y activos.</returns>
        [HttpGet("filter")]
        public async Task<IActionResult> GetFiltered([FromQuery] int? capacity, [FromQuery] string? location)
        {
            var spaces = await _coworkingSpaceService.GetAllFilteredAsync(capacity, location);
            return Ok(Responses.Response.Success(spaces));
        }

        [HttpPost]
        [Authorize(Roles = "Hoster")]
        public async Task<IActionResult> Create([FromForm] CreateCoworkingSpaceDTO dto)
        {
            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var space = await _coworkingSpaceService.CreateAsync(dto, hosterId);
            return Ok(Responses.Response.Success("Coworking space created successfully."));
        }

        [HttpPut("{id}")]   
        [Authorize(Roles = "Hoster")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCoworkingSpaceDTO dto)
        {
            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            await _coworkingSpaceService.UpdateAsync(id, dto, hosterId, userRole);
            return Ok(Responses.Response.Success("Coworking space updated successfully."));
        }

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
            catch (UnauthorizedAccessException ex)
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


        [HttpDelete("{id}")]
        [Authorize(Roles = "Hoster")]
        public async Task<IActionResult> Delete(int id)
        {
            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _coworkingSpaceService.DeleteAsync(id, hosterId);
            return Ok(Responses.Response.Success("Coworking space deleted successfully."));
        }
    }
}