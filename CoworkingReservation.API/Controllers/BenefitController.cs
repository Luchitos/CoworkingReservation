using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoworkingReservation.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de beneficios en los espacios de coworking.
    /// </summary>
    [ApiController]
    [Route("api/benefits")]
    public class BenefitController : ControllerBase
    {
        #region Fields

        private readonly IBenefitService _benefitService;

        #endregion

        #region Constructor

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="BenefitController"/>.
        /// </summary>
        /// <param name="benefitService">Servicio de beneficios.</param>
        public BenefitController(IBenefitService benefitService)
        {
            _benefitService = benefitService ?? throw new ArgumentNullException(nameof(benefitService));
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Obtiene todos los beneficios disponibles.
        /// </summary>
        /// <returns>Lista de beneficios.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Benefit>>> GetAll()
        {
            var benefits = await _benefitService.GetAllAsync();
            return Ok(benefits);
        }

        /// <summary>
        /// Crea un nuevo beneficio.
        /// </summary>
        /// <param name="benefit">Datos del beneficio a crear.</param>
        /// <returns>El beneficio creado.</returns>
        [HttpPost]
        public async Task<ActionResult<Benefit>> Create([FromBody] Benefit benefit)
        {
            if (benefit == null || string.IsNullOrWhiteSpace(benefit.Name))
            {
                return BadRequest(Responses.Response.Failure("Benefit name is required."));
            }

            var createdBenefit = await _benefitService.CreateAsync(benefit);
            return CreatedAtAction(nameof(GetAll), new { id = createdBenefit.Id }, createdBenefit);
        }

        /// <summary>
        /// Elimina un beneficio por ID.
        /// </summary>
        /// <param name="id">ID del beneficio a eliminar.</param>
        /// <returns>HTTP 204 si se eliminó correctamente.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var benefit = await _benefitService.GetByIdAsync(id);
            if (benefit == null)
            {
                return NotFound(Responses.Response.Failure("Benefit not found."));
            }

            await _benefitService.DeleteAsync(id);
            return NoContent();
        }

        #endregion
    }
}
