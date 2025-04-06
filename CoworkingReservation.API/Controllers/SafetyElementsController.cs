using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CoworkingReservation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SafetyElementsController : ControllerBase
    {
        private readonly ISafetyElementService _safetyElementService;

        public SafetyElementsController(ISafetyElementService safetyElementService)
        {
            _safetyElementService = safetyElementService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var elements = await _safetyElementService.GetAllAsync();
            return Ok(Responses.Response.Success(elements));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var element = await _safetyElementService.GetByIdAsync(id);
            if (element == null)
                return NotFound(Responses.Response.Failure("Safety element not found."));

            return Ok(Responses.Response.Success(element));
        }
        
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SafetyElement safetyElement)
        {
            var createdElement = await _safetyElementService.CreateAsync(safetyElement);
            return CreatedAtAction(nameof(GetAll), new { id = createdElement.Id }, createdElement);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var element = await _safetyElementService.GetByIdAsync(id);
            if (element == null)
                return NotFound(Responses.Response.Failure("Safety element not found."));
                
            await _safetyElementService.DeleteAsync(id);
            return Ok(Responses.Response.Success("Safety element deleted successfully"));
        }
    }
}   