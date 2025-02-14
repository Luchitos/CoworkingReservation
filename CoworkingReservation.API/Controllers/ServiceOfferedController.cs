using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CoworkingReservation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceOfferedController : ControllerBase
    {
        private readonly IServiceOfferedService _serviceOfferedService;

        public ServiceOfferedController(IServiceOfferedService serviceOfferedService)
        {
            _serviceOfferedService = serviceOfferedService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceOffered>>> GetAll()
        {
            var services = await _serviceOfferedService.GetAllAsync();
            return Ok(services);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceOffered>> Create([FromBody] ServiceOffered service)
        {
            var createdService = await _serviceOfferedService.CreateAsync(service);
            return CreatedAtAction(nameof(GetAll), new { id = createdService.Id }, createdService);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _serviceOfferedService.DeleteAsync(id);
            return NoContent();
        }
    }
}