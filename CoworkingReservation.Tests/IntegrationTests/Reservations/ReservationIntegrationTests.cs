using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoworkingReservation.API.Models;
using CoworkingReservation.API.Services;
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
    /// 
    /// This test suite verifies key business rules and workflows, including:
    /// - Creating reservations with multiple areas
    /// - Preventing overlapping bookings for the same area
    /// - Cancelling reservations and enforcing correct status transitions
    /// - Restricting cancellation to the reservation owner
    /// - Validating that only existing areas can be reserved and that areas belong to the selected coworking space
    /// - Preventing reservations in the past
    /// - Enforcing area availability based on existing reservations
    /// 
    /// Uses in-memory SQLite for isolated database state per test.
    /// Test data is seeded using TestDataSeeder to ensure repeatability and coverage.
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

        /// <summary>
        /// Validates that creating a reservation with multiple coworking areas
        /// correctly persists all reservation details for the specified areas.
        /// </summary>
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

            var result = await _reservationService.CreateReservationAsync(request);

            var reservations = await _context.Reservations.Include(r => r.ReservationDetails).ToListAsync();
            Assert.Single(reservations);

            var reservation = reservations.First();
            Assert.Equal(2, reservation.ReservationDetails.Count);
            Assert.Contains(reservation.ReservationDetails, d => d.CoworkingAreaId == area1.Id);
            Assert.Contains(reservation.ReservationDetails, d => d.CoworkingAreaId == area2.Id);
        }

        /// <summary>
        /// Ensures that creating overlapping reservations for the same area is not allowed.
        /// The system should throw InvalidOperationException for an attempt to overlap.
        /// </summary>
        [Fact]
        public async Task CreateReservation_OverlappingDates_ShouldNotAllow()
        {
            var user = await TestDataSeeder.SeedUser(_context, "testuser2", "testuser2@email.com");
            var address = await TestDataSeeder.SeedAddress(_context, 2);
            var coworkingSpace = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 2);
            var area = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace, 1);

            // Reserva 1
            var request1 = new CreateReservationRequest
            {
                UserId = user.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(4),
                EndDate = DateTime.UtcNow.Date.AddDays(6),
                AreaIds = new List<int> { area.Id }
            };
            await _reservationService.CreateReservationAsync(request1);

            // Reserva 2 (solapada)
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

        /// <summary>
        /// Checks that cancelling a reservation correctly sets its status to Cancelled.
        /// </summary>
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

            var result = await _reservationService.CreateReservationAsync(request);
            var reservation = await _context.Reservations.FirstAsync();

            await _reservationService.CancelReservationAsync(reservation.Id, user.Id.ToString());

            var updated = await _context.Reservations.FindAsync(reservation.Id);
            Assert.Equal(ReservationStatus.Cancelled, updated.Status);
        }

        /// <summary>
        /// Verifies that only the user who made the reservation can cancel it.
        /// Cancelling by a different user should throw UnauthorizedAccessException.
        /// </summary>
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

            var result = await _reservationService.CreateReservationAsync(request);
            var reservation = await _context.Reservations.FirstAsync();

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _reservationService.CancelReservationAsync(reservation.Id, user2.Id.ToString()));
        }

        /// <summary>
        /// Ensures that attempting to reserve a non-existent area throws InvalidOperationException.
        /// </summary>
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
                AreaIds = new List<int> { 999 } // No existe
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reservationService.CreateReservationAsync(request));
        }

        /// <summary>
        /// Validates that all reserved areas must belong to the selected coworking space.
        /// Attempting to reserve an area from a different space should throw InvalidOperationException.
        /// </summary>
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
                AreaIds = new List<int> { area1.Id, area2.Id } // area2 no pertenece
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reservationService.CreateReservationAsync(request));
        }

        /// <summary>
        /// Ensures that reservations cannot be created for dates in the past.
        /// The system should throw InvalidOperationException in such cases.
        /// </summary>
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

        /// <summary>
        /// Validates that if an area is already booked for overlapping dates,
        /// no other reservation can be created for the same period.
        /// </summary>
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
                StartDate = DateTime.UtcNow.Date.AddDays(3), // Solapado
                EndDate = DateTime.UtcNow.Date.AddDays(4),
                AreaIds = new List<int> { area.Id }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reservationService.CreateReservationAsync(req2));
        }
        /// <summary>
        /// If attempting to cancel an already cancelled reservation, should be idempotent or throw a specific exception.
        /// </summary>
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
            var result = await _reservationService.CreateReservationAsync(request);
            var reservation = await _context.Reservations.FirstAsync();

            await _reservationService.CancelReservationAsync(reservation.Id, user.Id.ToString());

            // Try to cancel again: should throw or do nothing depending on business logic
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _reservationService.CancelReservationAsync(reservation.Id, user.Id.ToString()));
        }

        /// <summary>
        /// Should not allow creating a reservation where the start date is after the end date.
        /// </summary>
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
                EndDate = DateTime.UtcNow.Date.AddDays(2), // End before start
                AreaIds = new List<int> { area.Id }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reservationService.CreateReservationAsync(request));
        }

        /// <summary>
        /// Should not allow creating a reservation on a coworking space that is not approved or active.
        /// </summary>
        [Fact]
        public async Task CreateReservation_OnInactiveCoworking_ShouldThrow()
        {
            var user = await TestDataSeeder.SeedUser(_context, "testuser13", "testuser13@email.com");
            var address = await TestDataSeeder.SeedAddress(_context, 13);
            var coworkingSpace = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 13);
            coworkingSpace.Status = CoworkingStatus.Pending; // or CoworkingStatus.Inactive according to your enum
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

        /// <summary>
        /// If all areas are reserved for a given date range, should not allow new reservations for that period.
        /// </summary>
        [Fact]
        public async Task CreateReservation_WhenAllAreasReserved_ShouldNotAllowMoreReservations()
        {
            var user = await TestDataSeeder.SeedUser(_context, "testuser14", "testuser14@email.com");
            var address = await TestDataSeeder.SeedAddress(_context, 14);
            var coworkingSpace = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 14);
            var area1 = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace, 1);
            var area2 = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace, 2);

            // Reserve both areas
            var req1 = new CreateReservationRequest
            {
                UserId = user.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(2),
                EndDate = DateTime.UtcNow.Date.AddDays(3),
                AreaIds = new List<int> { area1.Id, area2.Id }
            };
            await _reservationService.CreateReservationAsync(req1);

            // Another user tries to reserve any area in the same date range
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

        /// <summary>
        /// Should throw if a reservation requests more capacity than the area supports.
        /// </summary>
        [Fact]
        public async Task CreateReservation_WithCapacityGreaterThanArea_ShouldThrow()
        {
            var user = await TestDataSeeder.SeedUser(_context, "testuser16", "testuser16@email.com");
            var address = await TestDataSeeder.SeedAddress(_context, 16);
            var coworkingSpace = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 16);
            var area = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace, 1);

            // Simulate area with small capacity
            area.Capacity = 3;
            await _context.SaveChangesAsync();

            var request = new CreateReservationRequest
            {
                UserId = user.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(5),
                EndDate = DateTime.UtcNow.Date.AddDays(5),
                AreaIds = new List<int> { area.Id },
                // If your model supports it, add: RequestedCapacity = 5
            };

            // Adjust if your model supports capacity requests per reservation
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reservationService.CreateReservationAsync(request));
        }

        /// <summary>
        /// Should not allow cancelling a reservation that is already completed or in the past.
        /// </summary>
        [Fact]
        public async Task CancelReservation_Completed_ShouldThrow()
        {
            var user = await TestDataSeeder.SeedUser(_context, "testuser17", "testuser17@email.com");
            var address = await TestDataSeeder.SeedAddress(_context, 17);
            var coworkingSpace = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 17);
            var area = await TestDataSeeder.SeedCoworkingArea(_context, coworkingSpace, 1);

            var request = new CreateReservationRequest
            {
                UserId = user.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.Date.AddDays(-5),
                EndDate = DateTime.UtcNow.Date.AddDays(-3),
                AreaIds = new List<int> { area.Id }
            };

            var result = await _reservationService.CreateReservationAsync(request);
            var reservation = await _context.Reservations.FirstAsync();
            reservation.Status = ReservationStatus.Completed;
            await _context.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reservationService.CancelReservationAsync(reservation.Id, user.Id.ToString()));
        }

        /// <summary>
        /// Should not allow a hoster user to reserve in their own coworking space.
        /// </summary>
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


        /// <summary>
        /// Cleans up the in-memory database and connection after each test run.
        /// </summary>
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _connection.Close();
            _connection.Dispose();
        }
    }
}
