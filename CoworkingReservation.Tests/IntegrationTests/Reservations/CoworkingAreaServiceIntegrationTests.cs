using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using CoworkingReservation.Infrastructure.Data;
using CoworkingReservation.Infrastructure.UnitOfWork;
using CoworkingReservation.Application.Services;
using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Domain.Enums;
using FluentAssertions;


namespace CoworkingReservation.Tests.Integration.CoworkingAreas
{
    public class CoworkingAreaServiceIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly CoworkingAreaService _service;

        private readonly int _hosterId = 1;
        private readonly int _nonHosterId = 2;
        private readonly int _coworkingId;

        public CoworkingAreaServiceIntegrationTests()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            _db = new ApplicationDbContext(options);
            _db.Database.EnsureCreated();

            // Seed users
            var hoster = new User
            {
                Id = _hosterId,
                Name = "Hoster",
                UserName = "hoster",
                Role = "Hoster",
                PasswordHash = "pass",
                Email = "hoster@test.com",
                Cuit = "20112233"
            };
            var nonHoster = new User
            {
                Id = _nonHosterId,
                Name = "NoHoster",
                UserName = "client",
                Role = "Client",
                PasswordHash = "pass",
                Email = "client@test.com",
                Cuit = "20224466"
            };

            _db.Users.AddRange(hoster, nonHoster);

            var coworking = new CoworkingSpace
            {
                Id = 100,
                Name = "Cow test",
                HosterId = _hosterId,
                CapacityTotal = 10,
                Status = CoworkingStatus.Approved
            };

            _db.CoworkingSpaces.Add(coworking);
            _db.SaveChanges();

            _coworkingId = coworking.Id;

            // Instancia real del UnitOfWork
            var unitOfWork = new UnitOfWork(_db);

            var logger = NullLogger<CoworkingAreaService>.Instance;

            _service = new CoworkingAreaService(unitOfWork, logger);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        private CreateCoworkingAreaDTO FakeAreaDto(int capacity = 3, bool available = true, CoworkingAreaType type = CoworkingAreaType.PrivateOffice)
        {
            return new CreateCoworkingAreaDTO
            {
                Description = "Area test",
                Capacity = capacity,
                PricePerDay = 100,
                Available = available,
                Type = type
            };
        }

        private UpdateCoworkingAreaDTO FakeUpdateDto(int capacity = 2, CoworkingAreaType type = CoworkingAreaType.IndividualDesk)
        {
            return new UpdateCoworkingAreaDTO
            {
                Description = "Actualizada",
                Capacity = capacity,
                PricePerDay = 150,
                available = true,
                Type = type
            };
        }

        [Fact]
        public async Task CreateAsync_HappyPath_CreatesArea()
        {
            var dto = FakeAreaDto();
            var area = await _service.CreateAsync(dto, _coworkingId, _hosterId);

            area.Should().NotBeNull();
            area.Id.Should().BeGreaterThan(0);
            area.Capacity.Should().Be(dto.Capacity);

            var dbArea = await _db.CoworkingAreas.FindAsync(area.Id);
            dbArea.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_CoworkingNotFound_Throws()
        {
            var dto = FakeAreaDto();
            Func<Task> act = () => _service.CreateAsync(dto, 9999, _hosterId);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task CreateAsync_NotHoster_Throws()
        {
            var dto = FakeAreaDto();
            Func<Task> act = () => _service.CreateAsync(dto, _coworkingId, _nonHosterId);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task CreateAsync_CapacityExceeded_Throws()
        {
            var dto1 = FakeAreaDto(9);
            var dto2 = FakeAreaDto(2);
            await _service.CreateAsync(dto1, _coworkingId, _hosterId);

            Func<Task> act = () => _service.CreateAsync(dto2, _coworkingId, _hosterId);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task AddAreasToCoworkingAsync_HappyPath_AddsAreas()
        {
            var areas = new List<CoworkingAreaDTO>
            {
                new CoworkingAreaDTO { Description="1", Capacity=3, PricePerDay=100, Available=true, Type = (int)CoworkingAreaType.PrivateOffice },
                new CoworkingAreaDTO { Description="2", Capacity=2, PricePerDay=80, Available=true, Type = (int)CoworkingAreaType.IndividualDesk }
            };
            await _service.AddAreasToCoworkingAsync(areas, _coworkingId, _hosterId);

            var dbAreas = _db.CoworkingAreas.Where(a => a.CoworkingSpaceId == _coworkingId).ToList();
            dbAreas.Count.Should().Be(2);
        }

        [Fact]
        public async Task AddAreasToCoworkingAsync_CapacityExceeded_Throws()
        {
            var areas = new List<CoworkingAreaDTO>
            {
                new CoworkingAreaDTO { Description="1", Capacity=6, PricePerDay=100, Available=true, Type = (int)CoworkingAreaType.PrivateOffice },
                new CoworkingAreaDTO { Description="2", Capacity=6, PricePerDay=80, Available=true, Type = (int)CoworkingAreaType.IndividualDesk }
            };
            Func<Task> act = () => _service.AddAreasToCoworkingAsync(areas, _coworkingId, _hosterId);
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task AddAreasToCoworkingAsync_NotHoster_Throws()
        {
            var areas = new List<CoworkingAreaDTO>
            {
                new CoworkingAreaDTO { Description="1", Capacity=2, PricePerDay=80, Available=true, Type = (int)CoworkingAreaType.IndividualDesk }
            };
            Func<Task> act = () => _service.AddAreasToCoworkingAsync(areas, _coworkingId, _nonHosterId);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task UpdateAsync_HappyPath_UpdatesArea()
        {
            var area = await _service.CreateAsync(FakeAreaDto(), _coworkingId, _hosterId);
            var dto = FakeUpdateDto(4);

            await _service.UpdateAsync(area.Id, dto, _hosterId);

            var dbArea = await _db.CoworkingAreas.FindAsync(area.Id);
            dbArea.Description.Should().Be(dto.Description);
            dbArea.Capacity.Should().Be(dto.Capacity);
            dbArea.PricePerDay.Should().Be(dto.PricePerDay);
            dbArea.Type.Should().Be(dto.Type);
        }

        [Fact]
        public async Task UpdateAsync_AreaNotFound_Throws()
        {
            var dto = FakeUpdateDto();
            Func<Task> act = () => _service.UpdateAsync(9999, dto, _hosterId);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UpdateAsync_NotHoster_Throws()
        {
            var area = await _service.CreateAsync(FakeAreaDto(), _coworkingId, _hosterId);
            var dto = FakeUpdateDto();
            Func<Task> act = () => _service.UpdateAsync(area.Id, dto, _nonHosterId);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task DeleteAsync_HappyPath_DeletesArea()
        {
            var area = await _service.CreateAsync(FakeAreaDto(), _coworkingId, _hosterId);
            await _service.DeleteAsync(area.Id, _hosterId);
            var dbArea = await _db.CoworkingAreas.FindAsync(area.Id);
            dbArea.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_AreaNotFound_Throws()
        {
            Func<Task> act = () => _service.DeleteAsync(9999, _hosterId);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task DeleteAsync_NotHoster_Throws()
        {
            var area = await _service.CreateAsync(FakeAreaDto(), _coworkingId, _hosterId);
            Func<Task> act = () => _service.DeleteAsync(area.Id, _nonHosterId);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task SetAvailabilityAsync_HappyPath_DisablesAndEnables()
        {
            var area = await _service.CreateAsync(FakeAreaDto(available: true), _coworkingId, _hosterId);
            // Disable
            await _service.SetAvailabilityAsync(area.Id, _hosterId, false);
            (await _db.CoworkingAreas.FindAsync(area.Id)).Available.Should().BeFalse();
            // Enable again
            await _service.SetAvailabilityAsync(area.Id, _hosterId, true);
            (await _db.CoworkingAreas.FindAsync(area.Id)).Available.Should().BeTrue();
        }

        [Fact]
        public async Task SetAvailabilityAsync_AreaNotFound_Throws()
        {
            Func<Task> act = () => _service.SetAvailabilityAsync(9999, _hosterId, true);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task SetAvailabilityAsync_NotHoster_Throws()
        {
            var area = await _service.CreateAsync(FakeAreaDto(available: true), _coworkingId, _hosterId);
            Func<Task> act = () => _service.SetAvailabilityAsync(area.Id, _nonHosterId, false);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task SetAvailabilityAsync_SameStatus_Throws()
        {
            var area = await _service.CreateAsync(FakeAreaDto(available: true), _coworkingId, _hosterId);
            Func<Task> act = () => _service.SetAvailabilityAsync(area.Id, _hosterId, true);
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task GetByCoworkingSpaceIdAsync_ReturnsOnlyAvailableAreas()
        {
            // Crea 2 disponibles y 1 no
            await _service.CreateAsync(FakeAreaDto(available: true), _coworkingId, _hosterId);
            var area2 = await _service.CreateAsync(FakeAreaDto(available: true), _coworkingId, _hosterId);
            var area3 = await _service.CreateAsync(FakeAreaDto(available: false), _coworkingId, _hosterId);

            var list = (await _service.GetByCoworkingSpaceIdAsync(_coworkingId)).ToList();
            list.Should().HaveCount(2);
            list.All(a => a.Available).Should().BeTrue();
        }
    }
}
