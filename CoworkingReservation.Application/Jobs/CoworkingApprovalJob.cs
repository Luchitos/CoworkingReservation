using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using CoworkingReservation.Domain.IRepository;
using Microsoft.Extensions.Logging;

namespace CoworkingReservation.Application.Jobs
{
    public class CoworkingApprovalJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CoworkingApprovalJob> _logger;

        public CoworkingApprovalJob(IUnitOfWork unitOfWork, ILogger<CoworkingApprovalJob> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Run()
        {
            _logger.LogInformation("🔄 Job de aprobación de coworkings iniciado...");

            var pendingSpaces = await _unitOfWork.CoworkingSpaces.GetAllAsync("Address,Photos");

            foreach (var space in pendingSpaces.Where(s => s.Status == CoworkingStatus.Pending))
            {
                if (IsValidForApproval(space))
                {
                    space.Status = CoworkingStatus.Approved;
                    _logger.LogInformation($"✅ Coworking '{space.Name}' aprobado automáticamente.");
                }
                else
                {
                    space.Status = CoworkingStatus.Rejected;
                    _logger.LogWarning($"❌ Coworking '{space.Name}' rechazado por no cumplir requisitos.");
                }

                await _unitOfWork.CoworkingSpaces.UpdateAsync(space);
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("✅ Job de aprobación de coworkings finalizado.");
        }

        private bool IsValidForApproval(CoworkingSpace space)
        {
            return !string.IsNullOrWhiteSpace(space.Name)
                && space.PricePerDay > 0
                && space.Capacity > 0
                && space.Address != null
                && !string.IsNullOrWhiteSpace(space.Address.Street)
                && space.Photos.Count >= 2;
        }
    }
}
