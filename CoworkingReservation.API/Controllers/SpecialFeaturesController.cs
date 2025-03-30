using CoworkingReservation.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoworkingReservation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpecialFeaturesController : ControllerBase
    {
        private readonly ISpecialFeatureService _specialFeatureService;

        public SpecialFeaturesController(ISpecialFeatureService specialFeatureService)
        {
            _specialFeatureService = specialFeatureService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var features = await _specialFeatureService.GetAllAsync();
            return Ok(Responses.Response.Success(features));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var feature = await _specialFeatureService.GetByIdAsync(id);
            if (feature == null)
                return NotFound(Responses.Response.Failure("Special feature not found."));

            return Ok(Responses.Response.Success(feature));
        }
    }
}