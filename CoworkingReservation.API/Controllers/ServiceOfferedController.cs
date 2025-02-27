using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoworkingReservation.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de servicios ofrecidos en coworkings.
    /// </summary>
    [ApiController]
    [Route("api/services-offered")]
    public class ServiceOfferedController : ControllerBase
    {
        #region Fields

        private readonly IServiceOfferedService _serviceOfferedService;

        #endregion

        #region Constructor

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="ServiceOfferedController"/>.
        /// </summary>
        /// <param name="serviceOfferedService">Servicio para la gestión de servicios ofrecidos.</param>
        public ServiceOfferedController(IServiceOfferedService serviceOfferedService)
        {
            _serviceOfferedService = serviceOfferedService ?? throw new ArgumentNullException(nameof(serviceOfferedService));
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Obtiene todos los servicios ofrecidos.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var services = await _serviceOfferedService.GetAllAsync();
            return Ok(Responses.Response.Success(services));
        }

        /// <summary>
        /// Crea un nuevo servicio ofrecido.
        /// </summary>
        /// <param name="service">Detalles del nuevo servicio.</param>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServiceOffered service)
        {
            if (service == null || string.IsNullOrWhiteSpace(service.Name))
            {
                return BadRequest(Responses.Response.Failure("Service name is required."));
            }

            var createdService = await _serviceOfferedService.CreateAsync(service);
            return CreatedAtAction(nameof(GetAll), new { id = createdService.Id }, Responses.Response.Success(createdService));
        }

        /// <summary>
        /// Elimina un servicio ofrecido por su ID.
        /// </summary>
        /// <param name="id">ID del servicio a eliminar.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _serviceOfferedService.DeleteAsync(id);
                return Ok(Responses.Response.Success("Service deleted successfully."));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(Responses.Response.Failure("Service not found."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Responses.Response.Failure(ex.Message));
            }
        }

        #endregion
    }
}
