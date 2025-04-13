using CoworkingReservation.Application.DTOs.CoworkingSpace;
using System.Security.Claims;
using CoworkingReservation.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoworkingReservation.API.Responses;

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

        /// <summary>
        /// Obtiene los detalles completos de un espacio de coworking por su ID
        /// </summary>
        /// <param name="id">ID del espacio de coworking</param>
        /// <returns>Detalles completos del espacio de coworking incluyendo áreas, servicios y beneficios</returns>
        /// <response code="200">El espacio de coworking se encontró correctamente</response>
        /// <response code="404">El espacio de coworking no existe</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CoworkingReservation.API.Responses.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CoworkingReservation.API.Responses.Response), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CoworkingReservation.API.Responses.Response), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var space = await _coworkingSpaceService.GetByIdAsync(id);
                
                // Enriquecer la respuesta con información adicional
                return Ok(Responses.Response.Success(new
                {
                    Details = space,
                    Metadata = new
                    {
                        RequestedAt = DateTime.UtcNow,
                        Version = "1.1",
                        AvailableOperations = new[] 
                        {
                            new { Name = "Reservar", Endpoint = $"/api/reservation/create/{id}" },
                            new { Name = "Ver Disponibilidad", Endpoint = $"/api/availability/{id}" }
                        }
                    }
                }));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Responses.Response.Failure(ex.Message, 404));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, Responses.Response.Failure($"Error al obtener el espacio de coworking: {ex.Message}", 500));
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] CreateCoworkingSpaceDTO dto)
        {
            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var space = await _coworkingSpaceService.CreateAsync(dto, hosterId);
            return Ok(Responses.Response.Success("Coworking space created successfully."));
        }

        [HttpPut("{id}")]   
        [Authorize(Roles = "Hoster")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCoworkingSpaceDTO dto)
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

        /// <summary>
        /// Get optimizado solo con datos mínimos (id, nombre, dirección, foto de portada)
        /// </summary>
        [HttpGet("light")]
        public async Task<IActionResult> GetAllLightweight()
        {
            var result = await _coworkingSpaceService.GetAllLightweightAsync();
            return Ok(Responses.Response.Success(result));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Hoster")]
        public async Task<IActionResult> Delete(int id)
        {
            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _coworkingSpaceService.DeleteAsync(id, hosterId);
            return Ok(Responses.Response.Success("Coworking space deleted successfully."));
        }

        /// <summary>
        /// Obtiene todos los coworkings con datos mínimos (nombre, dirección y foto de portada).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var spaces = await _coworkingSpaceService.GetAllLightweightAsync();
            return Ok(Responses.Response.Success(spaces));
        }

        /// <summary>
        /// 🔹 Obtiene coworkings filtrados por capacidad y/o ubicación (optimizado).
        /// </summary>
        [HttpGet("filter")]
        public async Task<IActionResult> GetFiltered([FromQuery] int? capacity, [FromQuery] string? location)
        {
            var spaces = await _coworkingSpaceService.GetFilteredLightweightAsync(capacity, location);
            return Ok(Responses.Response.Success(spaces));
        }
    }
}