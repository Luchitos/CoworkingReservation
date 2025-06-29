using CoworkingReservation.Application.DTOs.User;
using CoworkingReservation.Application.Services;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.DTOs;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.Data;
using CoworkingReservation.Infrastructure.Repositories;
using CoworkingReservation.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CoworkingReservation.Tests.IntegrationTests.Users
{
    public class UserServiceIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UserService _userService;
        private readonly Mock<IImageUploadService> _imageUploadServiceMock;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserServiceIntegrationTests()
        {
            _context = DbContextHelper.CreateInMemoryContext();

            _imageUploadServiceMock = new Mock<IImageUploadService>();
            _imageUploadServiceMock
                .Setup(x => x.UploadUserImageAsync(It.IsAny<IFormFile>(), It.IsAny<int>()))
                .ReturnsAsync((IFormFile file, int userId) => $"https://fake.imgbb.com/userphotos/{userId}_{file?.FileName ?? "default"}.jpg");

            _passwordHasher = new PasswordHasher<User>();

            // Repositorios reales
            var userRepository = new UserRepository(_context);
            var coworkingSpaceRepository = new CoworkingSpaceRepository(_context);
            var auditLogRepository = new AuditLogRepository(_context);
            var addressRepository = new AddressRepository(_context);
            var coworkingAreaRepository = new CoworkingAreaRepository(_context);
            var coworkingAvailabilityRepository = new CoworkingAvailabilityRepository(_context);

            var unitOfWork = new Infrastructure.UnitOfWork.UnitOfWork(
                _context,
                userRepository,
                coworkingSpaceRepository,
                auditLogRepository,
                addressRepository,
                coworkingAreaRepository,
                coworkingAvailabilityRepository
            );

            _userService = new UserService(
                unitOfWork,
                _passwordHasher,
                _imageUploadServiceMock.Object
            );
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        private async Task CleanAndSeedBasicUser()
        {
            DbContextHelper.CleanDatabase(_context);
            await TestDataSeeder.SeedUser(_context, "basicuser", "basic@mail.com");
        }

        [Fact]
        public async Task RegisterAsync_Should_Create_User_Without_Photo()
        {
            DbContextHelper.CleanDatabase(_context);
            var dto = new UserRegisterDTO
            {
                Name = "Test",
                Lastname = "User",
                UserName = "testuser",
                Cuit = "20123456789",
                Email = "testuser@mail.com",
                Password = "Secret1234"
            };

            var result = await _userService.RegisterAsync(dto);

            var userDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            Assert.NotNull(userDb);
            Assert.Equal(dto.Name, userDb.Name);
            Assert.Equal(dto.Cuit, userDb.Cuit);
            Assert.True(_passwordHasher.VerifyHashedPassword(userDb, userDb.PasswordHash, dto.Password) == PasswordVerificationResult.Success);
            Assert.Null(userDb.PhotoId);
        }

        [Fact]
        public async Task RegisterAsync_Should_Create_User_With_Photo()
        {
            DbContextHelper.CleanDatabase(_context);
            var stream = new MemoryStream(new byte[100]);
            var photoMock = new FormFile(stream, 0, stream.Length, "photo", "profile.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var dto = new UserRegisterDTO
            {
                Name = "Photo",
                Lastname = "User",
                UserName = "photouser",
                Cuit = "20987654321",
                Email = "photouser@mail.com",
                Password = "PhotoPass123",
                ProfilePhoto = photoMock
            };

            var result = await _userService.RegisterAsync(dto);

            var userDb = await _context.Users.Include(u => u.Photo).FirstOrDefaultAsync(u => u.Email == dto.Email);

            Assert.NotNull(userDb);
            Assert.NotNull(userDb.PhotoId);
            Assert.NotNull(userDb.Photo);
            Assert.Equal("profile.jpg", userDb.Photo.FileName);
            Assert.Equal(userDb.PhotoId, userDb.Photo.Id);
        }

        [Fact]
        public async Task RegisterAsync_With_Existing_Email_Throws()
        {
            await CleanAndSeedBasicUser();
            var dto = new UserRegisterDTO
            {
                Name = "New",
                Lastname = "User",
                UserName = "otheruser",
                Cuit = "20999998888",
                Email = "basic@mail.com", // ya existe
                Password = "OtherPass"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.RegisterAsync(dto));
        }

        [Fact]
        public async Task AuthenticateAsync_Should_Return_User_When_Credentials_Are_Correct()
        {
            DbContextHelper.CleanDatabase(_context);
            var user = new User
            {
                Name = "Auth",
                Lastname = "Test",
                UserName = "authuser",
                Email = "auth@mail.com",
                Cuit = "20777778888"
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, "PasswordOk!");
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _userService.AuthenticateAsync("auth@mail.com", "PasswordOk!");

            Assert.NotNull(result);
            Assert.Equal("Auth", result.Name);
        }

        [Fact]
        public async Task AuthenticateAsync_Should_Return_Null_When_Wrong_Password()
        {
            DbContextHelper.CleanDatabase(_context);
            var user = new User
            {
                Name = "Auth",
                Lastname = "Test",
                UserName = "authuser",
                Email = "auth@mail.com",
                Cuit = "20777778888"
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, "PasswordOk!");
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _userService.AuthenticateAsync("auth@mail.com", "WrongPassword!");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Users()
        {
            DbContextHelper.CleanDatabase(_context);
            await TestDataSeeder.SeedUser(_context, "user1", "user1@mail.com");
            await TestDataSeeder.SeedUser(_context, "user2", "user2@mail.com");

            var all = await _userService.GetAllAsync();
            Assert.Equal(2, all.Count());
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_User_With_Photo()
        {
            DbContextHelper.CleanDatabase(_context);
            var user = await TestDataSeeder.SeedUser(_context, "userwithphoto", "withphoto@mail.com");
            var photo = await TestDataSeeder.SeedUserPhoto(_context, user, 99);

            var found = await _userService.GetByIdAsync(user.Id);

            Assert.NotNull(found);
            Assert.Equal(user.Email, found.Email);
            Assert.Equal(photo.Id, found.PhotoId);
        }

        [Fact]
        public async Task UpdateProfileFieldAsync_Should_Update_Phone()
        {
            DbContextHelper.CleanDatabase(_context);
            var user = await TestDataSeeder.SeedUser(_context, "updateuser", "update@mail.com");

            var result = await _userService.UpdateProfileFieldAsync(user.Id, "phone", "12345678");

            Assert.Equal("12345678", result.Phone);
        }

        [Fact]
        public async Task UpdateProfileFieldAsync_Should_Update_Cuit()
        {
            DbContextHelper.CleanDatabase(_context);
            var user = await TestDataSeeder.SeedUser(_context, "updatecuit", "updatecuit@mail.com");

            var result = await _userService.UpdateProfileFieldAsync(user.Id, "cuit", "20765432109");

            Assert.Equal("20765432109", result.Cuit);
        }

        [Fact]
        public async Task UpdateProfileFieldAsync_Should_Update_Address()
        {
            DbContextHelper.CleanDatabase(_context);
            var user = await TestDataSeeder.SeedUser(_context, "updateaddress", "updateaddress@mail.com");

            var result = await _userService.UpdateProfileFieldAsync(user.Id, "address", "New Address 123");

            Assert.Equal("New Address 123", result.Address);
        }

        [Fact]
        public async Task UpdateProfileFieldAsync_Should_Update_Photo()
        {
            DbContextHelper.CleanDatabase(_context);
            var user = await TestDataSeeder.SeedUser(_context, "updatephoto", "updatephoto@mail.com");

            var photoDto = new PhotoDTO
            {
                FileName = "nueva_foto.jpg",
                MimeType = "image/jpeg",
                Url = "https://fake.imgbb.com/userphotos/updatephoto_nueva_foto.jpg"
            };

            var result = await _userService.UpdateProfileFieldAsync(user.Id, "photo", photoDto);

            Assert.Equal(photoDto.Url, result.Photo.FilePath);
        }

        [Fact]
        public async Task ToggleActiveStatusAsync_Should_Toggle_Status()
        {
            DbContextHelper.CleanDatabase(_context);
            var user = await TestDataSeeder.SeedUser(_context, "activeuser", "activeuser@mail.com");

            Assert.True(user.IsActive);

            await _userService.ToggleActiveStatusAsync(user.Id);
            var updated = await _context.Users.FindAsync(user.Id);
            Assert.False(updated.IsActive);

            await _userService.ToggleActiveStatusAsync(user.Id);
            var reupdated = await _context.Users.FindAsync(user.Id);
            Assert.True(reupdated.IsActive);
        }

        [Fact]
        public async Task BecomeHosterAsync_Should_Set_Pending_And_Role()
        {
            DbContextHelper.CleanDatabase(_context);
            var user = await TestDataSeeder.SeedUser(_context, "becomehost", "becomehost@mail.com");
            Assert.False(user.IsHosterRequestPending);
            Assert.Equal("Client", user.Role);

            await _userService.BecomeHosterAsync(user.Id);

            var updated = await _context.Users.FindAsync(user.Id);
            Assert.True(updated.IsHosterRequestPending);
            Assert.Equal("Hoster", updated.Role);
        }

        [Fact]
        public async Task ToggleFavoriteAsync_Should_Add_And_Remove_Favorite()
        {
            DbContextHelper.CleanDatabase(_context);
            var user = await TestDataSeeder.SeedUser(_context, "favuser", "favuser@mail.com");
            var address = await TestDataSeeder.SeedAddress(_context, 17);
            var coworking = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 17);

            // Agregar favorito
            await _userService.ToggleFavoriteAsync(user.Id, coworking.Id, true);
            var fav = await _context.FavoriteCoworkingSpaces.FirstOrDefaultAsync(f => f.UserId == user.Id && f.CoworkingSpaceId == coworking.Id);
            Assert.NotNull(fav);

            // Quitar favorito
            await _userService.ToggleFavoriteAsync(user.Id, coworking.Id, false);
            var favRemoved = await _context.FavoriteCoworkingSpaces.FirstOrDefaultAsync(f => f.UserId == user.Id && f.CoworkingSpaceId == coworking.Id);
            Assert.Null(favRemoved);
        }

        [Fact]
        public async Task GetFavoriteSpacesAsync_Should_Return_Favorites()
        {
            DbContextHelper.CleanDatabase(_context);
            var user = await TestDataSeeder.SeedUser(_context, "favlistuser", "favlistuser@mail.com");
            var address = await TestDataSeeder.SeedAddress(_context, 25);
            var coworking = await TestDataSeeder.SeedCoworkingSpace(_context, user, address, 25);
            await TestDataSeeder.SeedFavoriteCoworkingSpace(_context, user, coworking);

            var favorites = await _userService.GetFavoriteSpacesAsync(user.Id);
            Assert.Single(favorites);
            Assert.Equal(coworking.Name, favorites.First().Name);
        }

        [Fact]
        public async Task UpdateProfileFieldAsync_Invalid_Field_Should_Throw()
        {
            DbContextHelper.CleanDatabase(_context);
            var user = await TestDataSeeder.SeedUser(_context, "invalidfield", "invalidfield@mail.com");

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.UpdateProfileFieldAsync(user.Id, "notarealfield", "algo"));
        }

        [Fact]
        public async Task ToggleActiveStatusAsync_User_Not_Found_Should_Throw()
        {
            DbContextHelper.CleanDatabase(_context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _userService.ToggleActiveStatusAsync(999999));
        }
    }
}
