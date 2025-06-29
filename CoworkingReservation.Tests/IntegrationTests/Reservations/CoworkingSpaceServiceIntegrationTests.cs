using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Application.DTOs.Address;
using CoworkingReservation.Application.Services;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Infrastructure.Data;
using CoworkingReservation.Application.DTOs.CoworkingArea;
using Microsoft.AspNetCore.Http;
using CoworkingReservation.Application.Jobs;

namespace CoworkingReservation.Tests.Integration.CoworkingSpaces
{
    public class CoworkingSpaceServiceIntegrationTests
    {
        private ApplicationDbContext CreateDbContext()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new ApplicationDbContext(options);
            dbContext.Database.EnsureCreated();

            dbContext.Users.Add(new User
            {
                Id = 1,
                Name = "Hoster",
                Lastname = "Test",
                UserName = "Hoster",
                Role = "Client",
                PasswordHash = "hashedpassword",
                Email = "hoster@mail.com",
                Cuit = "2033"
            });
            dbContext.Users.Add(new User
            {
                Id = 2,
                Name = "Client",
                Lastname = "Test2",
                UserName = "Client",
                PasswordHash = "hashedpassword",
                Role = "Client",
                Email = "client@mail.com",
                Cuit = "2044"
            });

            dbContext.ServicesOffered.Add(new ServiceOffered
            {
                Id = 1,
                Name = "WiFi",
                Description = "Conexión WiFi de alta velocidad"
            });

            dbContext.Benefits.Add(new Benefit
            {
                Id = 1,
                Name = "Café",
                Description = "Café de cortesía"
            });

            dbContext.SafetyElements.Add(new SafetyElement
            {
                Id = 1,
                Name = "Cámaras",
                Description = "Cámaras de seguridad 24hs"
            });

            dbContext.SpecialFeatures.Add(new SpecialFeature
            {
                Id = 1,
                Name = "Vista",
                Description = "Vista panorámica a la ciudad"
            });

            dbContext.SaveChanges();

            return dbContext;
        }



        private CoworkingSpaceService BuildService(ApplicationDbContext dbContext, Mock<ICoworkingAreaService> areaServiceMock = null)
        {
            // Mocks de repositorios
            var userRepo = new Mock<IUserRepository>();
            var coworkingSpaceRepo = new Mock<ICoworkingSpaceRepository>();
            var reservationRepo = new Mock<IRepository<Reservation>>();
            var reviewRepo = new Mock<IRepository<Review>>();
            var addressRepo = new Mock<IAddressRepository>();
            var photoRepo = new Mock<IRepository<CoworkingSpacePhoto>>();
            var auditLogRepo = new Mock<IAuditLogRepository>();
            var userPhotoRepo = new Mock<IRepository<UserPhoto>>();
            var serviceRepo = new Mock<IRepository<ServiceOffered>>();
            var benefitRepo = new Mock<IRepository<Benefit>>();
            var safetyRepo = new Mock<IRepository<SafetyElement>>();
            var specialRepo = new Mock<IRepository<SpecialFeature>>();
            var coworkingAreaRepo = new Mock<ICoworkingAreaRepository>();
            var availabilityRepo = new Mock<ICoworkingAvailabilityRepository>();
            var favoriteRepo = new Mock<IFavoriteCoworkingSpaceRepository>();

            // Setup mocks para devolver datos reales del contexto
            userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                .Returns<int, string>(async (id, _) => await dbContext.Users.FindAsync(id));
            addressRepo.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Address, bool>>>()))
                .Returns<System.Linq.Expressions.Expression<Func<Address, bool>>>(async exp => dbContext.Addresses.Any(exp));
            coworkingSpaceRepo.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CoworkingSpace, bool>>>()))
                .Returns<System.Linq.Expressions.Expression<Func<CoworkingSpace, bool>>>(async exp => dbContext.CoworkingSpaces.Any(exp));
            coworkingSpaceRepo.Setup(r => r.AddAsync(It.IsAny<CoworkingSpace>()))
                .Returns<CoworkingSpace>(async cs => { dbContext.CoworkingSpaces.Add(cs); await dbContext.SaveChangesAsync(); });
            userRepo.Setup(r => r.UpdateAsync(It.IsAny<User>()))
                .Returns<User>(async u => { dbContext.Users.Update(u); await dbContext.SaveChangesAsync(); });
            addressRepo.Setup(r => r.AddAsync(It.IsAny<Address>()))
                .Returns<Address>(async a => { dbContext.Addresses.Add(a); await dbContext.SaveChangesAsync(); });
            benefitRepo.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Benefit, bool>>>()))
                .Returns<System.Linq.Expressions.Expression<Func<Benefit, bool>>>(async exp => dbContext.Benefits.Where(exp).ToList());
            serviceRepo.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ServiceOffered, bool>>>()))
                .Returns<System.Linq.Expressions.Expression<Func<ServiceOffered, bool>>>(async exp => dbContext.ServicesOffered.Where(exp).ToList());
            safetyRepo.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<SafetyElement, bool>>>()))
                .Returns<System.Linq.Expressions.Expression<Func<SafetyElement, bool>>>(async exp => dbContext.SafetyElements.Where(exp).ToList());
            specialRepo.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<SpecialFeature, bool>>>()))
                .Returns<System.Linq.Expressions.Expression<Func<SpecialFeature, bool>>>(async exp => dbContext.SpecialFeatures.Where(exp).ToList());
            coworkingAreaRepo.Setup(r => r.GetByCoworkingSpaceIdAsync(It.IsAny<int>()))
                .Returns<int>(async id => dbContext.CoworkingAreas.Where(a => a.CoworkingSpaceId == id).ToList());

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(u => u.Users).Returns(userRepo.Object);
            unitOfWork.Setup(u => u.CoworkingSpaces).Returns(coworkingSpaceRepo.Object);
            unitOfWork.Setup(u => u.Reservations).Returns(reservationRepo.Object);
            unitOfWork.Setup(u => u.Reviews).Returns(reviewRepo.Object);
            unitOfWork.Setup(u => u.Addresses).Returns(addressRepo.Object);
            unitOfWork.Setup(u => u.CoworkingSpacePhotos).Returns(photoRepo.Object);
            unitOfWork.Setup(u => u.AuditLogs).Returns(auditLogRepo.Object);
            unitOfWork.Setup(u => u.UserPhotos).Returns(userPhotoRepo.Object);
            unitOfWork.Setup(u => u.Services).Returns(serviceRepo.Object);
            unitOfWork.Setup(u => u.Benefits).Returns(benefitRepo.Object);
            unitOfWork.Setup(u => u.SafetyElements).Returns(safetyRepo.Object);
            unitOfWork.Setup(u => u.SpecialFeatures).Returns(specialRepo.Object);
            unitOfWork.Setup(u => u.CoworkingAreas).Returns(coworkingAreaRepo.Object);
            unitOfWork.Setup(u => u.CoworkingAvailabilities).Returns(availabilityRepo.Object);
            unitOfWork.Setup(u => u.FavoriteCoworkingSpaces).Returns(favoriteRepo.Object);
            unitOfWork.Setup(u => u.SaveChangesAsync()).Returns(async () => await dbContext.SaveChangesAsync());
            unitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<System.Data.IsolationLevel>()))
                .ReturnsAsync(new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>().Object);

            // FIX: Usar el DTO correcto (CoworkingReservation.Application.DTOs.CoworkingSpace.CoworkingAreaDTO)
            // TODO Revisar los dto en el servicio porque se estan mezclando
            var areaService = areaServiceMock ?? new Mock<ICoworkingAreaService>();
            areaService.Setup(a => a.AddAreasToCoworkingAsync(It.IsAny<List<CoworkingReservation.Application.DTOs.CoworkingSpace.CoworkingAreaDTO>>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Approval job

            var approvalJobMock = new Mock<ICoworkingApprovalJob>();
            approvalJobMock.Setup(j => j.Run()).Returns(Task.CompletedTask);

            // Image upload
            var imageUploadMock = new Mock<IImageUploadService>();
            imageUploadMock.Setup(i => i.UploadCoworkingSpaceImageAsync(It.IsAny<IFormFile>(), It.IsAny<int>()))
                .ReturnsAsync("http://imageurl/fake.jpg");

            return new CoworkingSpaceService(
                unitOfWork.Object,
                approvalJobMock.Object,
                areaService.Object,
                imageUploadMock.Object,
                dbContext
            );
        }

        [Fact]
        public async Task CreateAsync_HappyPath_CreatesSpace()
        {
            var db = CreateDbContext();
            var service = BuildService(db);

            var dto = new CreateCoworkingSpaceDTO
            {
                Title = "Espacio Test",
                Description = "Cow lindo",
                CapacityTotal = 15,
                Rate = 4.5f, // <--- FLOAT
                Address = new CoworkingReservation.Domain.DTOs.AddressDTO
                {
                    City = "CABA",
                    Country = "AR",
                    Number = "10",
                    Province = "CABA",
                    Street = "Falsa",
                    ZipCode = "1100"
                },
                Services = "[1]",
                Benefits = "[1]",
                SafetyElements = "[1]",
                SpeacialFeatures = "[1]",
                AreasJson = "[{\"Type\":1,\"Description\":\"Area 1\",\"Capacity\":6,\"PricePerDay\":1234.5,\"Available\":true}]", // decimal OK
                Photos = new List<IFormFile>()
            };

            var created = await service.CreateAsync(dto, 1);
            Assert.NotNull(created);
            Assert.Equal("Espacio Test", created.Name);
            Assert.Equal(15, created.CapacityTotal);
            Assert.Equal(CoworkingStatus.Pending, created.Status);

            var user = await db.Users.FindAsync(1);
            Assert.Equal("Hoster", user.Role);
        }

        [Fact]
        public async Task CreateAsync_DuplicatePending_Throws()
        {
            var db = CreateDbContext();
            var service = BuildService(db);

            var dto = new CreateCoworkingSpaceDTO
            {
                Title = "Pendiente1",
                Description = "Test",
                CapacityTotal = 10,
                Rate = 3.3f,
                Address = new CoworkingReservation.Domain.DTOs.AddressDTO { City = "BsAs", Country = "AR", Number = "12", Province = "CABA", Street = "A", ZipCode = "123" },
                Services = "[1]",
                Benefits = "[1]",
                SafetyElements = "[1]",
                SpeacialFeatures = "[1]",
                AreasJson = "[]",
                Photos = new List<IFormFile>()
            };

            await service.CreateAsync(dto, 1);

            var dto2 = new CreateCoworkingSpaceDTO
            {
                Title = "Pendiente2",
                Description = "Test2",
                CapacityTotal = 9,
                Rate = 2.0f,
                Address = new CoworkingReservation.Domain.DTOs.AddressDTO { City = "Otra", Country = "AR", Number = "13", Province = "CABA", Street = "B", ZipCode = "456" },
                Services = "[1]",
                Benefits = "[1]",
                SafetyElements = "[1]",
                SpeacialFeatures = "[1]",
                AreasJson = "[]",
                Photos = new List<IFormFile>()
            };
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto2, 1));
        }

        [Fact]
        public async Task CreateAsync_DuplicateAddress_Throws()
        {
            var db = CreateDbContext();
            var service = BuildService(db);

            var dto = new CreateCoworkingSpaceDTO
            {
                Title = "CowUno",
                Description = "desc",
                CapacityTotal = 10,
                Rate = 3.0f,
                Address = new CoworkingReservation.Domain.DTOs.AddressDTO { City = "X", Country = "AR", Number = "22", Province = "CABA", Street = "Igual", ZipCode = "2000" },
                Services = "[1]",
                Benefits = "[1]",
                SafetyElements = "[1]",
                SpeacialFeatures = "[1]",
                AreasJson = "[]",
                Photos = new List<IFormFile>()
            };
            await service.CreateAsync(dto, 1);

            var dto2 = new CreateCoworkingSpaceDTO
            {
                Title = "CowDos",
                Description = "otro",
                CapacityTotal = 8,
                Rate = 2.0f,
                Address = new CoworkingReservation.Domain.DTOs.AddressDTO { City = "X", Country = "AR", Number = "22", Province = "CABA", Street = "Igual", ZipCode = "2000" },
                Services = "[1]",
                Benefits = "[1]",
                SafetyElements = "[1]",
                SpeacialFeatures = "[1]",
                AreasJson = "[]",
                Photos = new List<IFormFile>()
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto2, 2));
        }

        [Fact]
        public async Task CreateAsync_UserNotExists_Throws()
        {
            var db = CreateDbContext();
            var service = BuildService(db);

            var dto = new CreateCoworkingSpaceDTO
            {
                Title = "NoUser",
                Description = "fail",
                CapacityTotal = 10,
                Rate = 2.0f,
                Address = new CoworkingReservation.Domain.DTOs.AddressDTO { City = "Nada", Country = "AR", Number = "33", Province = "CABA", Street = "No", ZipCode = "100" },
                Services = "[1]",
                Benefits = "[1]",
                SafetyElements = "[1]",
                SpeacialFeatures = "[1]",
                AreasJson = "[]",
                Photos = new List<IFormFile>()
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateAsync(dto, 404));
        }

        [Fact]
        public async Task DeleteAsync_DeletesSpace()
        {
            var db = CreateDbContext();
            var service = BuildService(db);

            var dto = new CreateCoworkingSpaceDTO
            {
                Title = "ToDelete",
                Description = "desc",
                CapacityTotal = 10,
                Rate = 3.0f,
                Address = new CoworkingReservation.Domain.DTOs.AddressDTO { City = "X", Country = "AR", Number = "90", Province = "CABA", Street = "B", ZipCode = "8000" },
                Services = "[1]",
                Benefits = "[1]",
                SafetyElements = "[1]",
                SpeacialFeatures = "[1]",
                AreasJson = "[]",
                Photos = new List<IFormFile>()
            };
            var space = await service.CreateAsync(dto, 1);

            await service.DeleteAsync(space.Id, 1);
            var deleted = await db.CoworkingSpaces.FindAsync(space.Id);
            Assert.Null(deleted);
        }
    }
}
