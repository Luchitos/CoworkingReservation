using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using CoworkingReservation.Domain.IRepository;

namespace CoworkingReservation.Application.Jobs
{
    public class CoworkingApprovalJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CoworkingApprovalJob> _logger;

        public CoworkingApprovalJob(IServiceScopeFactory serviceScopeFactory, ILogger<CoworkingApprovalJob> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task Run()
        {
            _logger.LogInformation("🔄 Job de aprobación de coworkings iniciado...");

            using var scope = _serviceScopeFactory.CreateScope();  // 🔥 Crear un nuevo scope
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var pendingSpaces = await unitOfWork.CoworkingSpaces.GetAllAsync("Address,Photos");

            foreach (var space in pendingSpaces.Where(s => s.Status == CoworkingStatus.Pending))
            {
                if (IsValidForApproval(space))
                {
                    space.Status = CoworkingStatus.Approved;
                    _logger.LogInformation($"✅ Coworking '{space.Title}' aprobado automáticamente.");
                }
                else
                {
                    space.Status = CoworkingStatus.Rejected;
                    _logger.LogWarning($"❌ Coworking '{space.Title}' rechazado por no cumplir requisitos.");
                }

                await unitOfWork.CoworkingSpaces.UpdateAsync(space);
            }

            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("✅ Job de aprobación de coworkings finalizado.");
        }

        private bool IsValidForApproval(CoworkingSpace space)
        {
            return !string.IsNullOrWhiteSpace(space.Title)
                && space.PricePerDay > 0
                && space.Capacity > 0
                && space.Address != null
                && !string.IsNullOrWhiteSpace(space.Address.Street)
                && space.Photos.Count >= 2;
        }
    }
}
