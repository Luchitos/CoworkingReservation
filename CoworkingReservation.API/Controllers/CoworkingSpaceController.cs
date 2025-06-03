using CoworkingReservation.Application.DTOs.CoworkingSpace;
using System.Security.Claims;
using CoworkingReservation.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoworkingReservation.API.Responses;
using CoworkingReservation.Domain.DTOs;
using System.Collections.Generic;

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

        /// <summary>
        /// Obtiene los detalles completos de un espacio de coworking por su ID
        /// </summary>
        /// <param name="id">ID del espacio de coworking</param>
        /// <returns>Detalles completos del espacio de coworking incluyendo áreas, servicios y beneficios</returns>
        /// <response code="200">El espacio de coworking se encontró correctamente</response>
        /// <response code="404">El espacio de coworking no existe</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CoworkingReservation.API.Responses.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CoworkingReservation.API.Responses.Response), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CoworkingReservation.API.Responses.Response), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var space = await _coworkingSpaceService.GetByIdAsync(id);

                // Enriquecer la respuesta con información adicional
                return Ok(Responses.Response.Success(new
                {
                    Details = space,
                    Metadata = new
                    {
                        RequestedAt = DateTime.UtcNow,
                        Version = "1.1",
                        AvailableOperations = new[]
                        {
                            new { Name = "Reservar", Endpoint = $"/api/reservation/create/{id}" },
                            new { Name = "Ver Disponibilidad", Endpoint = $"/api/availability/{id}" }
                        }
                    }
                }));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Responses.Response.Failure(ex.Message, 404));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, Responses.Response.Failure($"Error al obtener el espacio de coworking: {ex.Message}", 500));
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] CreateCoworkingSpaceDTO dto)
        {
            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var space = await _coworkingSpaceService.CreateAsync(dto, hosterId);
            return Ok(Responses.Response.Success("Coworking space created successfully."));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Hoster")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCoworkingSpaceDTO dto)
        {
            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            await _coworkingSpaceService.UpdateAsync(id, dto, hosterId, userRole);
            return Ok(Responses.Response.Success("Coworking space updated successfully."));
        }

        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Hoster,Admin")]
        public async Task<IActionResult> ToggleActiveStatus(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                await _coworkingSpaceService.ToggleActiveStatusAsync(id, userId, userRole);
                return Ok(Responses.Response.Success("Coworking space status updated successfully."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Responses.Response.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Responses.Response.Failure(ex.Message));
            }
        }

        /// <summary>
        /// Get optimizado solo con datos mínimos (id, nombre, dirección, foto de portada)
        /// </summary>
        [HttpGet("light")]
        public async Task<IActionResult> GetAllLightweight()
        {
            // Obtener el ID del usuario actual si está autenticado
            int? userId = null;
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedUserId))
                {
                    userId = parsedUserId;
                }
            }

            var result = await _coworkingSpaceService.GetAllLightweightAsync(userId);
            return Ok(Responses.Response.Success(result));
        }

        /// <summary>
        /// Obtiene todos los coworkings con datos mínimos (nombre, dirección y foto de portada).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                // Obtener el ID del usuario actual si está autenticado
                int? userId = null;
                if (User.Identity.IsAuthenticated)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedUserId))
                    {
                        userId = parsedUserId;
                    }
                }

                var spaces = await _coworkingSpaceService.GetAllLightweightAsync(userId);

                // Log para depuración
                Console.WriteLine("---------- CONTROLLER DEBUG - GetAll ----------");
                foreach (var space in spaces)
                {
                    Console.WriteLine($"Controller GetAll: Space {space.Id} - HasConfiguredAreas: {space.HasConfiguredAreas}");
                    Console.WriteLine($"Controller GetAll: Space {space.Id} - PrivateOfficesCount: {space.PrivateOfficesCount}");
                    Console.WriteLine($"Controller GetAll: Space {space.Id} - MinPrivateOfficePrice: {space.MinPrivateOfficePrice}");

                    if (space.HasConfiguredAreas == true && space.PrivateOfficesCount == 0 &&
                        space.IndividualDesksCount == 0 && space.SharedDesksCount == 0)
                    {
                        Console.WriteLine($"WARNING: Space {space.Id} has HasConfiguredAreas=true but all counts are 0!");
                    }
                }
                Console.WriteLine("---------- END CONTROLLER DEBUG ----------");

                var response = new CoworkingSpaceListResponseDTO
                {
                    Spaces = spaces.ToList(),
                    Metadata = new Metadata
                    {
                        RequestedAt = DateTime.UtcNow,
                        Version = "1.1"
                    }
                };

                return Ok(Responses.Response.Success(response));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetAll: {ex.Message}");
                return StatusCode(500, Responses.Response.Failure($"Error al obtener todos los espacios: {ex.Message}", 500));
            }
        }

        /// <summary>
        /// 🔹 Obtiene coworkings filtrados por diversos criterios (optimizado).
        /// </summary>
        [HttpGet("filter")]
        public async Task<IActionResult> GetFiltered(
            [FromQuery] int? capacity,
            [FromQuery] string? location,
            [FromQuery] DateTime? date,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] bool? individualDesk,
            [FromQuery] bool? privateOffice,
            [FromQuery] bool? hybridSpace,
            [FromQuery] List<string> services,
            [FromQuery] List<string> benefits)
        {
            try
            {
                // Obtener el ID del usuario actual si está autenticado
                int? userId = null;
                if (User.Identity.IsAuthenticated)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedUserId))
                    {
                        userId = parsedUserId;
                    }
                }

                // Asegurarnos de que las listas no sean null
                services = services ?? new List<string>();
                benefits = benefits ?? new List<string>();

                // Registrar los filtros que se están aplicando para propósitos de depuración
                var filterParams = new Dictionary<string, object>();

                // Agregar parámetros básicos
                if (capacity.HasValue) filterParams["capacity"] = capacity.Value;
                if (!string.IsNullOrEmpty(location)) filterParams["location"] = location;
                if (date.HasValue) filterParams["date"] = date.Value;

                // Agregar parámetros de precio
                if (minPrice.HasValue) filterParams["minPrice"] = minPrice.Value;
                if (maxPrice.HasValue) filterParams["maxPrice"] = maxPrice.Value;

                // Agregar tipos de espacio
                if (individualDesk.HasValue && individualDesk.Value) filterParams["individualDesk"] = true;
                if (privateOffice.HasValue && privateOffice.Value) filterParams["privateOffice"] = true;
                if (hybridSpace.HasValue && hybridSpace.Value) filterParams["hybridSpace"] = true;

                // Agregar servicios y beneficios
                if (services.Any()) filterParams["services"] = string.Join(", ", services);
                if (benefits.Any()) filterParams["benefits"] = string.Join(", ", benefits);

                // Llamar al servicio existente con los parámetros básicos (por ahora)
                // En un futuro se deberá modificar el servicio para que acepte todos los parámetros
                IEnumerable<CoworkingSpaceListItemDTO> spaces;

                try
                {
                    // Imprimir en consola para depuración
                    Console.WriteLine("---------- CONTROLLER DEBUG - GetFiltered START ----------");
                    Console.WriteLine($"minPrice={minPrice}, maxPrice={maxPrice}");
                    Console.WriteLine($"individualDesk={individualDesk}, privateOffice={privateOffice}, hybridSpace={hybridSpace}");
                    Console.WriteLine($"services (count={services.Count}): {string.Join(", ", services)}");
                    Console.WriteLine($"benefits (count={benefits.Count}): {string.Join(", ", benefits)}");

                    // Intentar usar el nuevo método avanzado
                    spaces = await _coworkingSpaceService.GetAdvancedFilteredAsync(
                        capacity, location, date, minPrice, maxPrice,
                        individualDesk, privateOffice, hybridSpace,
                        services, benefits, userId);
                }
                catch (NotImplementedException)
                {
                    // Fallback al método antiguo si el nuevo no está implementado
                    Console.WriteLine("El método avanzado de filtrado no está implementado. Usando el método básico.");
                    spaces = await _coworkingSpaceService.GetFilteredLightweightAsync(capacity, location, userId);
                }

                // Log para depuración
                Console.WriteLine("---------- CONTROLLER DEBUG - GetFiltered RESULTS ----------");
                Console.WriteLine($"Filtros aplicados: {string.Join(", ", filterParams.Select(kv => $"{kv.Key}={kv.Value}"))}");
                Console.WriteLine($"minPrice={minPrice}, maxPrice={maxPrice}");
                Console.WriteLine($"individualDesk={individualDesk}, privateOffice={privateOffice}, hybridSpace={hybridSpace}");
                Console.WriteLine($"services={string.Join(", ", services ?? new List<string>())}");
                Console.WriteLine($"benefits={string.Join(", ", benefits ?? new List<string>())}");
                Console.WriteLine($"Número de espacios devueltos: {spaces.Count()}");

                foreach (var space in spaces)
                {
                    Console.WriteLine($"Controller: Space {space.Id} - HasConfiguredAreas: {space.HasConfiguredAreas}");
                    Console.WriteLine($"Controller: Space {space.Id} - PrivateOfficesCount: {space.PrivateOfficesCount}");
                    Console.WriteLine($"Controller: Space {space.Id} - MinPrivateOfficePrice: {space.MinPrivateOfficePrice}");

                    if (space.HasConfiguredAreas == true && space.PrivateOfficesCount == 0 &&
                        space.IndividualDesksCount == 0 && space.SharedDesksCount == 0)
                    {
                        Console.WriteLine($"WARNING: Space {space.Id} has HasConfiguredAreas=true but all counts are 0!");
                    }
                }
                Console.WriteLine("---------- END CONTROLLER DEBUG ----------");

                var response = new CoworkingSpaceListResponseDTO
                {
                    Spaces = spaces.ToList(),
                    Metadata = new Metadata
                    {
                        RequestedAt = DateTime.UtcNow,
                        Version = "1.1",
                        AppliedFilters = filterParams
                    }
                };

                return Ok(Responses.Response.Success(response));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetFiltered: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
                return StatusCode(500, Responses.Response.Failure($"Error al obtener espacios filtrados: {ex.Message}", 500));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Hoster")]
        public async Task<IActionResult> Delete(int id)
        {
            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _coworkingSpaceService.DeleteAsync(id, hosterId);
            return Ok(Responses.Response.Success("Coworking space deleted successfully."));
        }

        /// <summary>
        /// Obtiene todos los espacios de coworking creados por el hoster autenticado.
        /// </summary>
        /// <returns>Lista de coworkings del hoster.</returns>
        [HttpGet("my-coworkings")]
        [Authorize(Roles = "Hoster")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMyCoworkings()
        {
            var hosterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var coworkings = await _coworkingSpaceService.GetByHosterAsync(hosterId);
            return Ok(Responses.Response.Success(coworkings));
        }

    }
}