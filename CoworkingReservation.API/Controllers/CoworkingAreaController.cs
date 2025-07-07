using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace CoworkingReservation.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de áreas de coworking dentro de un espacio.
    /// </summary>
    [ApiController]
    [Route("api/coworking-areas")]
    public class CoworkingAreaController : ControllerBase
    {
        #region Fields

        private readonly ICoworkingAreaService _coworkingAreaService;

        #endregion

        #region Constructor

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="CoworkingAreaController"/>.
        /// </summary>
        /// <param name="coworkingAreaService">Servicio de áreas de coworking.</param>
        public CoworkingAreaController(ICoworkingAreaService coworkingAreaService)
        {
            _coworkingAreaService = coworkingAreaService ?? throw new ArgumentNullException(nameof(coworkingAreaService));
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Agregar un área de coworking a un espacio existente.
        /// </summary>
        /// <param name="coworkingSpaceId">ID del espacio de coworking.</param>
        /// <param name="dto">Datos del área a agregar.</param>
        /// <returns>El área creada.</returns>
        [HttpPost("{coworkingSpaceId}")]
        [Authorize(Roles = "Hoster")]
        public async Task<IActionResult> Create(int coworkingSpaceId, [FromBody] CreateCoworkingAreaDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(Responses.Response.Failure("Invalid data."));
            }

            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var area = await _coworkingAreaService.CreateAsync(dto, coworkingSpaceId, hosterId);
            return Ok(Responses.Response.Success(area));
        }

        /// <summary>
        /// Obtener información completa de un espacio de coworking para edición.
        /// </summary>
        /// <param name="coworkingSpaceId">ID del coworking space.</param>
        /// <returns>Información completa del espacio incluyendo áreas, servicios, beneficios, etc.</returns>
        [HttpGet("{coworkingSpaceId}")]
        public async Task<IActionResult> GetByCoworkingSpace(int coworkingSpaceId)
        {
            try
            {
                var coworkingSpaceInfo = await _coworkingAreaService.GetCoworkingSpaceForEditAsync(coworkingSpaceId);
                return Ok(Responses.Response.Success(coworkingSpaceInfo));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Responses.Response.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Responses.Response.Failure($"Error al obtener información del espacio de coworking: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtener un área específica por su ID.
        /// </summary>
        /// <param name="areaId">ID del área de coworking.</param>
        /// <returns>Datos del área.</returns>
        [HttpGet("detail/{areaId}")]
        public async Task<IActionResult> GetById(int areaId)
        {
            var area = await _coworkingAreaService.GetByIdAsync(areaId);
            if (area == null)
            {
                return NotFound(Responses.Response.Failure("Coworking area not found."));
            }
            return Ok(Responses.Response.Success(area));
        }

        /// <summary>
        /// Actualizar un área de coworking existente.
        /// </summary>
        /// <param name="areaId">ID del área a actualizar.</param>
        /// <param name="dto">Datos actualizados del área.</param>
        /// <returns>Mensaje de éxito.</returns>
        [HttpPut("{areaId}")]
        [Authorize(Roles = "Hoster")]
        public async Task<IActionResult> Update(int areaId, [FromBody] UpdateCoworkingAreaDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(Responses.Response.Failure("Invalid data."));
            }

            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _coworkingAreaService.UpdateAsync(areaId, dto, hosterId);
            return Ok(Responses.Response.Success("Coworking area updated successfully."));
        }

        /// <summary>
        /// Eliminar un área de coworking.
        /// </summary>
        /// <param name="areaId">ID del área a eliminar.</param>
        /// <returns>Mensaje de éxito.</returns>
        [HttpDelete("{areaId}")]
        [Authorize(Roles = "Hoster")]
        public async Task<IActionResult> Delete(int areaId)
        {
            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var area = await _coworkingAreaService.GetByIdAsync(areaId);

            if (area == null)
            {
                return NotFound(Responses.Response.Failure("Coworking area not found."));
            }

            await _coworkingAreaService.DeleteAsync(areaId, hosterId);
            return Ok(Responses.Response.Success("Coworking area deleted successfully."));
        }

        /// <summary>
        /// Cambia el estado de disponibilidad de un área de coworking.
        /// </summary>
        /// <param name="areaId">ID del área a modificar.</param>
        /// <param name="available">Nuevo estado (true = habilitar, false = deshabilitar).</param>
        /// <returns>Mensaje de éxito.</returns>
        [HttpPatch("{areaId}/availability")]
        [Authorize(Roles = "Hoster")]
        public async Task<IActionResult> SetAvailability(int areaId, [FromQuery] bool available)
        {
            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _coworkingAreaService.SetAvailabilityAsync(areaId, hosterId, available);

            var message = available
                ? "Área habilitada correctamente."
                : "Área deshabilitada correctamente.";

            return Ok(Responses.Response.Success(message));
        }


        /// <summary>
        /// Obtiene la capacidad total disponible de un espacio de coworking.
        /// </summary>
        /// <param name="coworkingSpaceId">ID del coworking space.</param>
        /// <returns>Capacidad total disponible.</returns>
        [HttpGet("{coworkingSpaceId}/available-capacity")]
        public async Task<IActionResult> GetAvailableCapacity(int coworkingSpaceId)
        {
            var totalCapacity = await _coworkingAreaService.GetTotalCapacityByCoworkingSpaceIdAsync(coworkingSpaceId);
            return Ok(Responses.Response.Success(totalCapacity));
        }

        #endregion
    }
}
