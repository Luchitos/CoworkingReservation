using CoworkingReservation.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoworkingReservation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoworkingSpaceController : Controller
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


    }
}
