using CoworkingReservation.Application.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CoworkingReservation.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión administrativa del sistema.
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        #region Fields

        private readonly CoworkingApprovalJob _approvalJob;

        #endregion

        #region Constructor

        /// <summary>
        /// Inicializa una nueva instancia del <see cref="AdminController"/>.
        /// </summary>
        /// <param name="approvalJob">Job de aprobación automática de espacios de coworking.</param>
        public AdminController(CoworkingApprovalJob approvalJob)
        {
            _approvalJob = approvalJob ?? throw new ArgumentNullException(nameof(approvalJob));
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Ejecuta manualmente el job de aprobación de espacios de coworking.
        /// </summary>
        /// <returns>Un mensaje de éxito si el proceso se ejecuta correctamente.</returns>
        [HttpPost("run-approval-job")]
        public async Task<IActionResult> RunApprovalJob()
        {
            await _approvalJob.Run();
            return Ok(Responses.Response.Success("Aprobación automática ejecutada."));
        }

        #endregion
    }
}
