using CoworkingReservation.Application.DTOs.User;
using CoworkingReservation.Application.Services;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CoworkingReservation.Tests.UnitTests.Application
{
    public class UserServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IImageUploadService> _imageUploadServiceMock;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _imageUploadServiceMock = new Mock<IImageUploadService>();
            _passwordHasher = new PasswordHasher<User>();

            // Configura UnitOfWork para devolver el mock de UserRepository
            _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

            _userService = new UserService(
                _unitOfWorkMock.Object,
                _passwordHasher,
                _imageUploadServiceMock.Object
            );
        }

        [Fact]
        public async Task RegisterAsync_Should_Throw_If_Required_Fields_Missing()
        {
            // Faltan campos requeridos
            var dto = new UserRegisterDTO
            {
                Name = "",
                Email = null,
                Password = "",
                Cuit = null
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _userService.RegisterAsync(dto));
            Assert.Contains("required", ex.Message);
        }

        [Fact]
        public async Task RegisterAsync_Should_Throw_If_User_Exists()
        {
            var dto = new UserRegisterDTO
            {
                Name = "Unit",
                Lastname = "Test",
                UserName = "unituser",
                Cuit = "20123456789",
                Email = "unit@test.com",
                Password = "Pass1234"
            };

            _userRepositoryMock.Setup(r => r.ExistsByEmailOrCuit(dto.Email, dto.Cuit))
                .ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.RegisterAsync(dto));
            Assert.Contains("already exists", ex.Message);
        }

        [Fact]
        public async Task RegisterAsync_Should_Add_And_Save_New_User()
        {
            var dto = new UserRegisterDTO
            {
                Name = "Unit",
                Lastname = "Test",
                UserName = "unituser",
                Cuit = "20123456789",
                Email = "unit@test.com",
                Password = "Pass1234"
            };

            // Simular que no existe previamente
            _userRepositoryMock.Setup(r => r.ExistsByEmailOrCuit(dto.Email, dto.Cuit))
                .ReturnsAsync(false);

            User addedUser = null;
            _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                .Callback<User>(u => addedUser = u)
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _userService.RegisterAsync(dto);

            // Chequeos
            Assert.NotNull(result);
            Assert.Equal(dto.Email, result.Email);
            Assert.Equal("Client", result.Role);
            Assert.True(_passwordHasher.VerifyHashedPassword(result, result.PasswordHash, dto.Password) == PasswordVerificationResult.Success);
            Assert.NotNull(addedUser); // Se pasó un User al repo
        }
    }
}
