using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace CoworkingReservation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoworkingAreaController : ControllerBase
    {
        private readonly ICoworkingAreaService _coworkingAreaService;

        public CoworkingAreaController(ICoworkingAreaService coworkingAreaService)
        {
            _coworkingAreaService = coworkingAreaService;
        }

        /// <summary>
        /// Agregar un área de coworking a un espacio existente.
        /// </summary>
        [HttpPost("{coworkingSpaceId}")]
        [Authorize(Roles = "Hoster")]
        public async Task<IActionResult> Create(int coworkingSpaceId, [FromBody] CreateCoworkingAreaDTO dto)
        {
            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var area = await _coworkingAreaService.CreateAsync(dto, coworkingSpaceId, hosterId);
            return Ok(Responses.Response.Success(area));
        }

        /// <summary>
        /// Obtener todas las áreas de un coworking space.
        /// </summary>
        [HttpGet("{coworkingSpaceId}")]
        public async Task<IActionResult> GetByCoworkingSpace(int coworkingSpaceId)
        {
            var areas = await _coworkingAreaService.GetByCoworkingSpaceIdAsync(coworkingSpaceId);
            return Ok(Responses.Response.Success(areas));
        }

        /// <summary>
        /// Obtener un área específica por su ID.
        /// </summary>
        [HttpGet("detail/{areaId}")]
        public async Task<IActionResult> GetById(int areaId)
        {
            var area = await _coworkingAreaService.GetByIdAsync(areaId);
            return Ok(Responses.Response.Success(area));
        }

        /// <summary>
        /// Actualizar un área de coworking existente.
        /// </summary>
        [HttpPut("{areaId}")]
        [Authorize(Roles = "Hoster")]
        public async Task<IActionResult> Update(int areaId, [FromBody] UpdateCoworkingAreaDTO dto)
        {
            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _coworkingAreaService.UpdateAsync(areaId, dto, hosterId);
            return Ok(Responses.Response.Success("Coworking area updated successfully."));
        }

        /// <summary>
        /// Eliminar un área de coworking.
        /// </summary>
        [HttpDelete("{areaId}")]
        [Authorize(Roles = "Hoster")]
        public async Task<IActionResult> Delete(int areaId)
        {
            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _coworkingAreaService.DeleteAsync(areaId, hosterId);
            return Ok(Responses.Response.Success("Coworking area deleted successfully."));
        }

        [HttpGet("{coworkingSpaceId}/available-capacity")]
        public async Task<IActionResult> GetAvailableCapacity(int coworkingSpaceId)
        {
            var totalCapacity = await _coworkingAreaService.GetTotalCapacityByCoworkingSpaceIdAsync(coworkingSpaceId);
            return Ok(Responses.Response.Success(totalCapacity));
        }

    }
}