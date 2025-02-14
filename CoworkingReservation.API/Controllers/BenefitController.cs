using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CoworkingReservation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BenefitController : ControllerBase
    {
        private readonly IBenefitService _benefitService;

        public BenefitController(IBenefitService benefitService)
        {
            _benefitService = benefitService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Benefit>>> GetAll()
        {
            var benefits = await _benefitService.GetAllAsync();
            return Ok(benefits);
        }

        [HttpPost]
        public async Task<ActionResult<Benefit>> Create([FromBody] Benefit benefit)
        {
            var createdBenefit = await _benefitService.CreateAsync(benefit);
            return CreatedAtAction(nameof(GetAll), new { id = createdBenefit.Id }, createdBenefit);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _benefitService.DeleteAsync(id);
            return NoContent();
        }
    }
}