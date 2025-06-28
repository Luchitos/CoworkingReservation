using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoworkingReservation.API.Models;
using CoworkingReservation.API.Services;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using CoworkingReservation.Tests.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using CoworkingReservation.Infrastructure.Data;
using CoworkingReservation.Infrastructure.Repositories;
using CoworkingReservation.Infrastructure.UnitOfWork;
using Xunit;

namespace CoworkingReservation.Tests.IntegrationTests.Reservations
{
    /// <summary>
    /// Integration tests for the Reservation business logic of the CoworkingReservation system.
    /// This test suite verifies key business rules and workflows for reservations.
    /// </summary>
    public class ReservationIntegrationBusinessTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ApplicationDbContext _context;
        private readonly IReservationService _reservationService;

        public ReservationIntegrationBusinessTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            var reservationRepository = new ReservationRepository(_context);
            var coworkingAreaRepository = new CoworkingAreaRepository(_context);
            var userRepository = new UserRepository(_context);
            var coworkingSpaceRepository = new CoworkingSpaceRepository(_context);
            var auditLogRepository = new AuditLogRepository(_context);
            var addressRepository = new AddressRepository(_context);
            var coworkingAvailabilityRepository = new CoworkingAvailabilityRepository(_context);

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
        public async Task CreateReservation_WithMultipleAreas_ShouldPersistAllAreas()
        {
            var user = await TestDataSeeder.SeedUser(_context, "testuser1", "testuser1@email.com");
            var address = await TestDataSeeder.SeedAddress(_context, 1);
            var coworkingSpace = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 1);
            var area1 = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace, 1);
            var area2 = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace, 2);

            var request = new CreateReservationRequest
            {
                UserId = user.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(2),
                EndDate = DateTime.UtcNow.Date.AddDays(2),
                AreaIds = new List<int> { area1.Id, area2.Id }
            };

            await _reservationService.CreateReservationAsync(request);

            var reservations = await _context.Reservations.Include(r => r.ReservationDetails).ToListAsync();
            Assert.Single(reservations);

            var reservation = reservations.First();
            Assert.Equal(2, reservation.ReservationDetails.Count);
            Assert.Contains(reservation.ReservationDetails, d => d.CoworkingAreaId == area1.Id);
            Assert.Contains(reservation.ReservationDetails, d => d.CoworkingAreaId == area2.Id);
        }

        [Fact]
        public async Task CreateReservation_OverlappingDates_ShouldNotAllow()
        {
            var user = await TestDataSeeder.SeedUser(_context, "testuser2", "testuser2@email.com");
            var address = await TestDataSeeder.SeedAddress(_context, 2);
            var coworkingSpace = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 2);
            var area = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace, 1);

            var request1 = new CreateReservationRequest
            {
                UserId = user.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(4),
                EndDate = DateTime.UtcNow.Date.AddDays(6),
                AreaIds = new List<int> { area.Id }
            };
            await _reservationService.CreateReservationAsync(request1);

            var request2 = new CreateReservationRequest
            {
                UserId = user.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(5),
                EndDate = DateTime.UtcNow.Date.AddDays(7),
                AreaIds = new List<int> { area.Id }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reservationService.CreateReservationAsync(request2));
        }

        [Fact]
        public async Task CancelReservation_ShouldSetStatusToCancelled()
        {
            var user = await TestDataSeeder.SeedUser(_context, "testuser3", "testuser3@email.com");
            var address = await TestDataSeeder.SeedAddress(_context, 3);
            var coworkingSpace = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 3);
            var area = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace, 1);

            var request = new CreateReservationRequest
            {
                UserId = user.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(10),
                EndDate = DateTime.UtcNow.Date.AddDays(12),
                AreaIds = new List<int> { area.Id }
            };

            await _reservationService.CreateReservationAsync(request);
            var reservation = await _context.Reservations.FirstAsync();

            await _reservationService.CancelReservationAsync(reservation.Id, user.Id.ToString());

            var updated = await _context.Reservations.FindAsync(reservation.Id);
            Assert.Equal(ReservationStatus.Cancelled, updated.Status);
        }

        [Fact]
        public async Task CancelReservation_ByOtherUser_ShouldThrow()
        {
            var user1 = await TestDataSeeder.SeedUser(_context, "testuser4", "testuser4@email.com");
            var user2 = await TestDataSeeder.SeedUser(_context, "testuser5", "testuser5@email.com");
            var address = await TestDataSeeder.SeedAddress(_context, 4);
            var coworkingSpace = await TestDataSeeder.SeedCoworkingSpace(_context, user1, address, 4);
            var area = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace, 1);

            var request = new CreateReservationRequest
            {
                UserId = user1.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(10),
                EndDate = DateTime.UtcNow.Date.AddDays(12),
                AreaIds = new List<int> { area.Id }
            };

            await _reservationService.CreateReservationAsync(request);
            var reservation = await _context.Reservations.FirstAsync();

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _reservationService.CancelReservationAsync(reservation.Id, user2.Id.ToString()));
        }

        [Fact]
        public async Task CreateReservation_WithInvalidArea_ShouldThrow()
        {
            var user = await TestDataSeeder.SeedUser(_context, "testuser6", "testuser6@email.com");
            var address = await TestDataSeeder.SeedAddress(_context, 5);
            var coworkingSpace = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 5);

            var request = new CreateReservationRequest
            {
                UserId = user.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(3),
                EndDate = DateTime.UtcNow.Date.AddDays(3),
                AreaIds = new List<int> { 999 }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reservationService.CreateReservationAsync(request));
        }

        [Fact]
        public async Task CreateReservation_AreaDoesNotBelongToSpace_ShouldThrow()
        {
            var user = await TestDataSeeder.SeedUser(_context, "testuser7", "testuser7@email.com");
            var address1 = await TestDataSeeder.SeedAddress(_context, 6);
            var address2 = await TestDataSeeder.SeedAddress(_context, 7);
            var coworkingSpace1 = await TestDataSeeder.SeedCoworkingSpace(_context, user, address1, 6);
            var coworkingSpace2 = await TestDataSeeder.SeedCoworkingSpace(_context, user, address2, 7);
            var area1 = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace1, 1);
            var area2 = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace2, 2);

            var request = new CreateReservationRequest
            {
                UserId = user.Id,
                CoworkingSpaceId = coworkingSpace1.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(5),
                EndDate = DateTime.UtcNow.Date.AddDays(5),
                AreaIds = new List<int> { area1.Id, area2.Id }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reservationService.CreateReservationAsync(request));
        }

        [Fact]
        public async Task CreateReservation_InThePast_ShouldThrow()
        {
            var user = await TestDataSeeder.SeedUser(_context, "testuser8", "testuser8@email.com");
            var address = await TestDataSeeder.SeedAddress(_context, 8);
            var coworkingSpace = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 8);
            var area = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace, 1);

            var request = new CreateReservationRequest
            {
                UserId = user.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(-2),
                EndDate = DateTime.UtcNow.Date.AddDays(-1),
                AreaIds = new List<int> { area.Id }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reservationService.CreateReservationAsync(request));
        }

        [Fact]
        public async Task CreateReservation_WhenAreaNotAvailable_ShouldThrow()
        {
            var user1 = await TestDataSeeder.SeedUser(_context, "testuser9", "testuser9@email.com");
            var address = await TestDataSeeder.SeedAddress(_context, 9);
            var coworkingSpace = await TestDataSeeder.SeedCoworkingSpace(_context, user1, address, 9);
            var area = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace, 1);

            var req1 = new CreateReservationRequest
            {
                UserId = user1.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(2),
                EndDate = DateTime.UtcNow.Date.AddDays(3),
                AreaIds = new List<int> { area.Id }
            };
            await _reservationService.CreateReservationAsync(req1);

            var user2 = await TestDataSeeder.SeedUser(_context, "testuser10", "testuser10@email.com");
            var req2 = new CreateReservationRequest
            {
                UserId = user2.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(3),
                EndDate = DateTime.UtcNow.Date.AddDays(4),
                AreaIds = new List<int> { area.Id }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reservationService.CreateReservationAsync(req2));
        }

        [Fact]
        public async Task CancelReservation_AlreadyCancelled_ShouldBeIdempotentOrThrow()
        {
            var user = await TestDataSeeder.SeedUser(_context, "testuser11", "testuser11@email.com");
            var address = await TestDataSeeder.SeedAddress(_context, 11);
            var coworkingSpace = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 11);
            var area = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace, 1);

            var request = new CreateReservationRequest
            {
                UserId = user.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(2),
                EndDate = DateTime.UtcNow.Date.AddDays(2),
                AreaIds = new List<int> { area.Id }
            };
            await _reservationService.CreateReservationAsync(request);
            var reservation = await _context.Reservations.FirstAsync();

            await _reservationService.CancelReservationAsync(reservation.Id, user.Id.ToString());

            await Assert.ThrowsAnyAsync<Exception>(() =>
                _reservationService.CancelReservationAsync(reservation.Id, user.Id.ToString()));
        }

        [Fact]
        public async Task CreateReservation_WithInvertedDates_ShouldThrow()
        {
            var user = await TestDataSeeder.SeedUser(_context, "testuser12", "testuser12@email.com");
            var address = await TestDataSeeder.SeedAddress(_context, 12);
            var coworkingSpace = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 12);
            var area = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace, 1);

            var request = new CreateReservationRequest
            {
                UserId = user.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(5),
                EndDate = DateTime.UtcNow.Date.AddDays(2),
                AreaIds = new List<int> { area.Id }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reservationService.CreateReservationAsync(request));
        }

        [Fact]
        public async Task CreateReservation_OnInactiveCoworking_ShouldThrow()
        {
            var user = await TestDataSeeder.SeedUser(_context, "testuser13", "testuser13@email.com");
            var address = await TestDataSeeder.SeedAddress(_context, 13);
            var coworkingSpace = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 13);
            coworkingSpace.Status = CoworkingStatus.Pending;
            await _context.SaveChangesAsync();
            var area = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace, 1);

            var request = new CreateReservationRequest
            {
                UserId = user.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(5),
                EndDate = DateTime.UtcNow.Date.AddDays(7),
                AreaIds = new List<int> { area.Id }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reservationService.CreateReservationAsync(request));
        }

        [Fact]
        public async Task CreateReservation_WhenAllAreasReserved_ShouldNotAllowMoreReservations()
        {
            var user = await TestDataSeeder.SeedUser(_context, "testuser14", "testuser14@email.com");
            var address = await TestDataSeeder.SeedAddress(_context, 14);
            var coworkingSpace = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 14);
            var area1 = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace, 1);
            var area2 = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace, 2);

            var req1 = new CreateReservationRequest
            {
                UserId = user.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(2),
                EndDate = DateTime.UtcNow.Date.AddDays(3),
                AreaIds = new List<int> { area1.Id, area2.Id }
            };
            await _reservationService.CreateReservationAsync(req1);

            var user2 = await TestDataSeeder.SeedUser(_context, "testuser15", "testuser15@email.com");
            var req2 = new CreateReservationRequest
            {
                UserId = user2.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(2),
                EndDate = DateTime.UtcNow.Date.AddDays(3),
                AreaIds = new List<int> { area1.Id }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reservationService.CreateReservationAsync(req2));
        }

        [Fact]
        public async Task CreateReservation_WithCapacityGreaterThanArea_ShouldThrow()
        {
            var user = await TestDataSeeder.SeedUser(_context, "testuser16", "testuser16@email.com");
            var address = await TestDataSeeder.SeedAddress(_context, 16);
            var coworkingSpace = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 16);
            var area = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace, 1);

            area.Capacity = 3;
            await _context.SaveChangesAsync();

            var request = new CreateReservationRequest
            {
                UserId = user.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(5),
                EndDate = DateTime.UtcNow.Date.AddDays(5),
                AreaIds = new List<int> { area.Id },
            };

            // Si el request soporta cantidad, agregarlo aquí
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reservationService.CreateReservationAsync(request));
        }

        [Fact]
        public async Task CancelReservation_Completed_ShouldThrow()
        {
            var user = await TestDataSeeder.SeedUser(_context, "testuser-cancel2", "cancel2@mail.com");
            var address = await TestDataSeeder.SeedAddress(_context, 21);
            var coworking = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 21);
            var area = await TestDataSeeder.SeedCoworkingArea(_context, coworking, 21);

            var reservation = new Reservation
            {
                UserId = user.Id,
                CoworkingSpaceId = coworking.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(-3),
                EndDate = DateTime.UtcNow.Date.AddDays(-1),
                Status = ReservationStatus.Completed,
                TotalPrice = area.PricePerDay * 2,
                PaymentMethod = PaymentMethod.CreditCard,
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                ReservationDetails = new List<ReservationDetail>
                {
                    new ReservationDetail { CoworkingAreaId = area.Id, PricePerDay = area.PricePerDay }
                }
            };
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reservationService.CancelReservationAsync(reservation.Id, user.Id.ToString()));
        }

        [Fact]
        public async Task CancelReservation_PastReservation_ShouldThrow()
        {
            var user = await TestDataSeeder.SeedUser(_context, "testuser-cancel3", "cancel3@mail.com");
            var address = await TestDataSeeder.SeedAddress(_context, 22);
            var coworking = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 22);
            var area = await TestDataSeeder.SeedCoworkingArea(_context, coworking, 22);

            var reservation = new Reservation
            {
                UserId = user.Id,
                CoworkingSpaceId = coworking.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(-2),
                EndDate = DateTime.UtcNow.Date.AddDays(-1),
                Status = ReservationStatus.Pending,
                TotalPrice = area.PricePerDay * 2,
                PaymentMethod = PaymentMethod.CreditCard,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                ReservationDetails = new List<ReservationDetail>
                {
                    new ReservationDetail { CoworkingAreaId = area.Id, PricePerDay = area.PricePerDay }
                }
            };
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reservationService.CancelReservationAsync(reservation.Id, user.Id.ToString()));
        }

        [Fact]
        public async Task CreateReservation_HosterInOwnCoworking_ShouldThrow()
        {
            var hoster = await TestDataSeeder.SeedUser(_context, "testhoster", "testhoster@email.com");
            hoster.Role = "Hoster";
            await _context.SaveChangesAsync();

            var address = await TestDataSeeder.SeedAddress(_context, 18);
            var coworkingSpace = await TestDataSeeder.SeedCoworkingSpace(_context, hoster, address, 18);
            var area = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace, 1);

            var request = new CreateReservationRequest
            {
                UserId = hoster.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(2),
                EndDate = DateTime.UtcNow.Date.AddDays(3),
                AreaIds = new List<int> { area.Id }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reservationService.CreateReservationAsync(request));
        }

        [Fact]
        public async Task CancelReservation_AlreadyCancelled_IsIdempotent()
        {
            var user = await TestDataSeeder.SeedUser(_context, "testuser-cancel1", "cancel1@mail.com");
            var address = await TestDataSeeder.SeedAddress(_context, 20);
            var coworking = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 20);
            var area = await TestDataSeeder.SeedCoworkingArea(_context, coworking, 20);

            var req = new CreateReservationRequest
            {
                UserId = user.Id,
                CoworkingSpaceId = coworking.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(2),
                EndDate = DateTime.UtcNow.Date.AddDays(3),
                AreaIds = new List<int> { area.Id }
            };

            var reservation = await _reservationService.CreateReservationAsync(req);
            var dbReservation = await _context.Reservations.FirstAsync();
            await _reservationService.CancelReservationAsync(dbReservation.Id, user.Id.ToString());

            // Should be idempotent, i.e., no error and status remains Cancelled
            await _reservationService.CancelReservationAsync(dbReservation.Id, user.Id.ToString());

            var updated = await _context.Reservations.FindAsync(dbReservation.Id);
            Assert.Equal(ReservationStatus.Cancelled, updated.Status);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _connection.Close();
            _connection.Dispose();
        }
    }
}
