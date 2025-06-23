using CoworkingReservation.Application.DTOs.Review;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.API.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using CoworkingReservation.API.Extensions;
using System.ComponentModel.DataAnnotations;

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
        /// Crea una reseña asociada a una reserva completada.
        /// </summary>
        /// <param name="request">Datos de la reseña a crear</param>
        /// <returns>Resultado de la creación de la reseña</returns>
        /// <response code="201">Reseña creada exitosamente</response>
        /// <response code="400">Error de validación o reserva no válida</response>
        /// <response code="401">Usuario no autenticado</response>
        /// <response code="403">Usuario no autorizado para esta reserva</response>
        /// <response code="404">Reserva no encontrada</response>
        /// <response code="422">Datos de validación incorrectos</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("create")]
        [Authorize]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
        {
            try
            {
                // Validar modelo
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return UnprocessableEntity(new { 
                        success = false, 
                        error = $"Datos de validación incorrectos: {string.Join(", ", errors)}", 
                        status = 422 
                    });
                }

                // Obtener ID del usuario autenticado
                if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId) || userId <= 0)
                {
                    return Unauthorized(new { 
                        success = false, 
                        error = "Usuario no autenticado correctamente.", 
                        status = 401 
                    });
                }

                // Validaciones de negocio específicas
                var validationResult = await _reviewService.ValidateReviewCreationAsync(userId, request.ReservationId);
                
                if (!validationResult.IsValid)
                {
                    return validationResult.StatusCode switch
                    {
                        404 => NotFound(new { success = false, error = validationResult.ErrorMessage, status = 404 }),
                        403 => StatusCode(403, new { success = false, error = validationResult.ErrorMessage, status = 403 }),
                        400 => BadRequest(new { success = false, error = validationResult.ErrorMessage, status = 400 }),
                        _ => BadRequest(new { success = false, error = validationResult.ErrorMessage, status = 400 })
                    };
                }

                // Crear la reseña
                var reviewId = await _reviewService.CreateReviewAsync(request, userId);
                
                // Devolver respuesta simple sin usar la clase Response personalizada
                return StatusCode(201, new { 
                    success = true,
                    data = new { 
                        id = reviewId, 
                        message = "Reseña creada exitosamente" 
                    },
                    status = 201
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, error = ex.Message, status = 403 });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, error = ex.Message, status = 404 });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, error = ex.Message, status = 400 });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, error = ex.Message, status = 400 });
            }
            catch (Exception ex)
            {
                // Log del error (aquí podrías usar ILogger)
                return StatusCode(500, new { 
                    success = false, 
                    error = "Error interno del servidor. Por favor, inténtelo más tarde.", 
                    status = 500 
                });
            }
        }

        /// <summary>
        /// Verifica si el usuario puede dejar una reseña para la reserva dada.
        /// </summary>
        /// <param name="reservationId">ID de la reserva a verificar</param>
        /// <returns>Resultado de elegibilidad para crear reseña</returns>
        /// <response code="200">Verificación exitosa</response>
        /// <response code="400">ID de reserva inválido</response>
        /// <response code="401">Usuario no autenticado</response>
        [HttpGet("eligibility")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CanReview([FromQuery] [Required] int reservationId)
        {
            try
            {
                if (reservationId <= 0)
                {
                    return BadRequest(new { 
                        success = false, 
                        error = "El ID de la reserva debe ser mayor a 0.", 
                        status = 400 
                    });
                }

                if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId) || userId <= 0)
                {
                    return Unauthorized(new { 
                        success = false, 
                        error = "Usuario no autenticado correctamente.", 
                        status = 401 
                    });
                }

                var validationResult = await _reviewService.ValidateReviewCreationAsync(userId, reservationId);
                
                return Ok(new { 
                    success = true,
                    data = new { 
                        canReview = validationResult.IsValid,
                        reason = validationResult.IsValid ? "Puede crear reseña" : validationResult.ErrorMessage
                    },
                    status = 200
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    error = "Error interno del servidor. Por favor, inténtelo más tarde.", 
                    status = 500 
                });
            }
        }

        /// <summary>
        /// Obtiene las reviews de un espacio de coworking con promedio y cantidad.
        /// </summary>
        /// <param name="id">ID del espacio de coworking</param>
        /// <returns>Lista de reseñas del espacio de coworking</returns>
        /// <response code="200">Reseñas obtenidas exitosamente</response>
        /// <response code="400">ID de coworking space inválido</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("by-coworking")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetReviewsByCoworkingSpace([FromQuery] [Required] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { 
                        success = false, 
                        error = "El ID del espacio de coworking debe ser mayor a 0.", 
                        status = 400 
                    });
                }

                var response = await _reviewService.GetReviewsByCoworkingSpaceAsync(id);
                return Ok(new { 
                    success = true,
                    data = response,
                    status = 200
                });
            }
            catch (Exception ex)
            {
                // Log del error para debugging
                Console.WriteLine($"Error en GetReviewsByCoworkingSpace: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                return StatusCode(500, new { 
                    success = false, 
                    error = "Error interno del servidor al obtener las reseñas.", 
                    status = 500 
                });
            }
        }



        /// <summary>
        /// Obtiene una reseña específica por usuario y reserva.
        /// </summary>
        /// <param name="reservationId">ID de la reserva</param>
        /// <returns>La reseña del usuario para la reserva especificada</returns>
        /// <response code="200">Reseña encontrada</response>
        /// <response code="400">ID de reserva inválido</response>
        /// <response code="401">Usuario no autenticado</response>
        /// <response code="403">Usuario no autorizado para esta reserva</response>
        /// <response code="404">Reserva no encontrada o reseña no existe</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("by-user-reservation")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetReviewByUserAndReservation([FromQuery] int reservationId)
        {
            try
            {
                if (reservationId <= 0)
                {
                    return BadRequest(new { 
                        success = false, 
                        error = "El ID de la reserva debe ser mayor a 0.", 
                        status = 400 
                    });
                }

                if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId) || userId <= 0)
                {
                    return Unauthorized(new { 
                        success = false, 
                        error = "Usuario no autenticado correctamente.", 
                        status = 401 
                    });
                }

                var review = await _reviewService.GetReviewByUserAndReservationAsync(userId, reservationId);
                
                if (review == null)
                {
                    return NotFound(new { 
                        success = false, 
                        error = "No existe una reseña para esta reserva.", 
                        status = 404 
                    });
                }

                return Ok(new { 
                    success = true,
                    data = review,
                    status = 200
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { 
                    success = false, 
                    error = ex.Message, 
                    status = 404 
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { 
                    success = false, 
                    error = ex.Message, 
                    status = 403 
                });
            }
            catch (Exception ex)
            {
                // Log del error para debugging
                Console.WriteLine($"Error en GetReviewByUserAndReservation: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                return StatusCode(500, new { 
                    success = false, 
                    error = "Error interno del servidor al obtener la reseña.", 
                    status = 500 
                });
            }
        }

    }
}
