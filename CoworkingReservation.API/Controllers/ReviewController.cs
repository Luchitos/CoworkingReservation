using CoworkingReservation.Application.DTOs.Review;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.API.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using CoworkingReservation.API.Extensions;

namespace CoworkingReservation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Crea una review asociada a una reserva completada.
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var canReview = await _reviewService.CanUserReviewAsync(userId, request.ReservationId);
            if (!canReview)
                return BadRequest(Response.Failure("No estás autorizado a dejar una reseña para esta reserva."));

            await _reviewService.CreateReviewAsync(request, userId);
            return Ok(Response.Success("Reseña creada con éxito."));
        }

        /// <summary>
        /// Verifica si el usuario puede dejar una reseña para la reserva dada.
        /// </summary>
        [HttpGet("eligibility")]
        [Authorize]
        public async Task<IActionResult> CanReview([FromQuery] int reservationId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var canReview = await _reviewService.CanUserReviewAsync(userId, reservationId);
            return Ok(Response.Success(canReview));
        }

        /// <summary>
        /// Obtiene las reviews de un espacio de coworking con promedio y cantidad.
        /// </summary>
        [HttpGet("by-coworking")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviewsByCoworkingSpace([FromQuery] int id)
        {
            var response = await _reviewService.GetReviewsByCoworkingSpaceAsync(id);
            return Ok(Response.Success(response));
        }

    }
}
