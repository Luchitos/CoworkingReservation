using CoworkingReservation.API.Services;
using CoworkingReservation.API.Models;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.UnitOfWork;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CoworkingReservation.Tests.UnitTests.Services
{
    public class ReservationServiceTests
    {
        [Fact]
        public async Task CreateReservationAsync_ShouldThrowException_WhenStartDateIsAfterEndDate()
        {
            // Arrange
            var reservationRepositoryMock = new Mock<IReservationRepository>();
            var areaRepositoryMock = new Mock<ICoworkingAreaRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var service = new ReservationService(reservationRepositoryMock.Object, areaRepositoryMock.Object, unitOfWorkMock.Object);

            var request = new CreateReservationRequest
            {
                StartDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow.AddDays(3),
                AreaIds = new List<int> { 1 },
                CoworkingSpaceId = 1,
                UserId = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateReservationAsync(request));
        }
    }
}
