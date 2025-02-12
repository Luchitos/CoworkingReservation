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
            await _coworkingSpaceService.UpdateAsync(id, dto, hosterId);
            return Ok(Responses.Response.Success("Coworking space updated successfully."));
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