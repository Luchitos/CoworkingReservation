using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoworkingReservation.Infrastructure.BackgroundJobs
{
    public class ReservationStatusBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public ReservationStatusBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var job = scope.ServiceProvider.GetRequiredService<ReservationStatusJob>();
                    await job.ProcessPendingReservationAsync();
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
