using CoworkingReservation.API.Extensions;
using CoworkingReservation.API.Models;
using CoworkingReservation.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CoworkingReservation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationRequest request)
        {
            try
            {
                // Obtener el ID del usuario del token JWT
                if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
                {
                    await Response.Failure("No se pudo identificar al usuario", 401);
                    return new EmptyResult();
                }

                // Asignar el ID del usuario a la solicitud
                request.UserId = userId;

                var reservation = await _reservationService.CreateReservationAsync(request);
                await Response.Success(reservation, 201);
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                await Response.Failure(ex.Message);
                return new EmptyResult();
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetReservation(int id)
        {
            try
            {
                var reservation = await _reservationService.GetReservationByIdAsync(id);
                if (reservation == null)
                {
                    await Response.Failure("Reservation not found", 404);
                    return new EmptyResult();
                }

                await Response.Success(reservation);
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                await Response.Failure(ex.Message);
                return new EmptyResult();
            }
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUserReservations()
        {
            try
            {
                // Obtener el ID del usuario del token JWT usando ClaimTypes.NameIdentifier
                if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
                {
                    await Response.Failure("No se pudo identificar al usuario", 401);
                    return new EmptyResult();
                }

                var reservations = await _reservationService.GetUserReservationsAsync(userId.ToString());
                await Response.Success(reservations);
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                await Response.Failure(ex.Message);
                return new EmptyResult();
            }
        }

        [HttpPost("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelReservation(int id)
        {
            try
            {
                // Obtener el ID del usuario del token JWT
                if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
                {
                    await Response.Failure("No se pudo identificar al usuario", 401);
                    return new EmptyResult();
                }

                await _reservationService.CancelReservationAsync(id, userId.ToString());
                await Response.Success(new { message = "Reserva cancelada correctamente" });
                return new EmptyResult();
            }
            catch (KeyNotFoundException ex)
            {
                await Response.Failure(ex.Message, 404);
                return new EmptyResult();
            }
            catch (UnauthorizedAccessException ex)
            {
                await Response.Failure(ex.Message, 403);
                return new EmptyResult();
            }
            catch (InvalidOperationException ex)
            {
                await Response.Failure(ex.Message, 400);
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                await Response.Failure(ex.Message, 500);
                return new EmptyResult();
            }
        }

        [HttpPost("check-availability")]
        public async Task<IActionResult> CheckAvailability([FromBody] CheckAvailabilityRequest request)
        {
            try
            {
                var availabilityResult = await _reservationService.CheckAvailabilityAsync(request);
                await Response.Success(availabilityResult);
                return new EmptyResult();
            }
            catch (InvalidOperationException ex)
            {
                // Errores de validación
                await Response.Failure(ex.Message, 400);
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                // Errores inesperados
                await Response.Failure($"Error al verificar disponibilidad: {ex.Message}", 500);
                return new EmptyResult();
            }
        }

        /// <summary>
        /// Obtiene las reservas agrupadas por espacio del hoster.
        /// </summary>
        /// <returns>Lista de reservas agrupadas por espacio.</returns>
        [HttpGet("hoster/by-space")]
        [Authorize(Roles = "Hoster")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReservationsByCoworking()
        {
            // var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _reservationService.GetReservationsByCoworkingAsync(10);
            return Ok(Responses.Response.Success(result));
        }
        
        /// <summary>
        /// Obtiene las reservas del usuario agrupadas por estado.
        /// </summary>
        /// <returns>Lista de reservas agrupadas por estado (Activas, Pasadas, Canceladas)</returns>
        /// <response code="200">Retorna las reservas agrupadas exitosamente</response>
        /// <response code="400">Si ocurre un error al procesar la solicitud</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        [Authorize]
        [HttpGet("grouped")]
        public async Task<IActionResult> GetGroupedReservations()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var grouped = await _reservationService.GetUserReservationsGroupedAsync(userId);
                return Ok(Responses.Response.Success(grouped));
            }
            catch (Exception ex)
            {
                return BadRequest(Responses.Response.Failure(ex.Message));
            }
        }


    }
}