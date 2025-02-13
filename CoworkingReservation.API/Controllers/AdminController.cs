using CoworkingReservation.Application.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoworkingReservation.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly CoworkingApprovalJob _approvalJob;

        public AdminController(CoworkingApprovalJob approvalJob)
        {
            _approvalJob = approvalJob;
        }

        [HttpPost("run-approval-job")]
        public async Task<IActionResult> RunApprovalJob()
        {
            await _approvalJob.Run();
            return Ok(Responses.Response.Success("Aprobación automática ejecutada."));
        }
    }
}
