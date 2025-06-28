using CoworkingReservation.API.Models;
using CoworkingReservation.API.Services;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.Data;
using CoworkingReservation.Infrastructure.Repositories;
using CoworkingReservation.Infrastructure.UnitOfWork;
using CoworkingReservation.Tests.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using IReservationService = CoworkingReservation.API.Services.IReservationService;

namespace CoworkingReservation.Tests.IntegrationTests.Reservations
{
    public class ReservationIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ApplicationDbContext _context;
        private readonly IReservationService _reservationService;

        public ReservationIntegrationTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            // REPOSITORIOS ESPECÍFICOS (no los genéricos)
            var userRepository = new UserRepository(_context);
            var coworkingSpaceRepository = new CoworkingSpaceRepository(_context);
            var auditLogRepository = new AuditLogRepository(_context);
            var addressRepository = new AddressRepository(_context);
            var coworkingAreaRepository = new CoworkingAreaRepository(_context);
            var coworkingAvailabilityRepository = new CoworkingAvailabilityRepository(_context);
            var reservationRepository = new ReservationRepository(_context);

            var unitOfWork = new UnitOfWork(
                _context,
                userRepository,
                coworkingSpaceRepository,
                auditLogRepository,
                addressRepository,
                coworkingAreaRepository,
                coworkingAvailabilityRepository
            );

            _reservationService = new ReservationService(reservationRepository, coworkingAreaRepository, unitOfWork);
        }

        [Fact]
        public async Task CreateReservation_ShouldPersistInDatabase()
        {
            // Arrange
            var user = await TestDataSeeder.SeedUser(_context);
            var coworkingSpace = await TestDataSeeder.SeedCoworkingSpace(_context, user);
            var coworkingArea = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace);

            var request = new CreateReservationRequest
            {
                UserId = user.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                EndDate = DateTime.UtcNow.Date.AddDays(3),
                AreaIds = new List<int> { coworkingArea.Id }
            };

            // Act
            var result = await _reservationService.CreateReservationAsync(request);

            // Assert
            var reservationsInDb = await _context.Reservations.Include(r => r.ReservationDetails).ToListAsync();
            Assert.Single(reservationsInDb);

            var reservation = reservationsInDb.First();
            Assert.Equal(user.Id, reservation.UserId);
            Assert.Equal(coworkingSpace.Id, reservation.CoworkingSpaceId);
            Assert.Equal(request.StartDate, reservation.StartDate);
            Assert.Equal(request.EndDate, reservation.EndDate);
            Assert.Equal(request.AreaIds.First(), reservation.ReservationDetails.First().CoworkingAreaId);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _connection.Close();
            _connection.Dispose();
        }
    }
}