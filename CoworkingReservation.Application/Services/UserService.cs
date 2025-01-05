using CoworkingReservation.Application.DTOs.User;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using System.Security.Cryptography;
using System.Text;

namespace CoworkingReservation.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserPhoto> _photoRepository;

        public UserService(IRepository<User> userRepository, IRepository<UserPhoto> photoRepository)
        {
            _userRepository = userRepository;
            _photoRepository = photoRepository;
        }

        public async Task<IEnumerable<User>> GetAllAsync() => await _userRepository.GetAllAsync();

        public async Task<User> GetByIdAsync(int id) => await _userRepository.GetByIdAsync(id);

        public async Task<User> RegisterAsync(UserRegisterDTO userDto)
        {
            // Validar datos de entrada
            if (string.IsNullOrEmpty(userDto.Name) || string.IsNullOrEmpty(userDto.Email) || string.IsNullOrEmpty(userDto.Password))
                throw new ArgumentException("Name, email, and password are required.");

            if (string.IsNullOrEmpty(userDto.Cuit))
                throw new ArgumentException("CUIT is required.");

            // Verificar si el CUIT o correo ya existen
            var existingUser = (await _userRepository.GetAllAsync())
                .FirstOrDefault(u => u.Email == userDto.Email || u.Cuit == userDto.Cuit);

            if (existingUser != null)
                throw new InvalidOperationException("A user with this email or CUIT already exists.");

            // Crear el nuevo usuario sin foto aún
            var newUser = new User
            {
                Name = userDto.Name,
                Lastname = userDto.Lastname,
                UserName = userDto.UserName,
                Cuit = userDto.Cuit,
                Email = userDto.Email,
                PasswordHash = HashPassword(userDto.Password),
            };

            await _userRepository.AddAsync(newUser);

            // Manejar la foto de perfil después de que el usuario ha sido creado
            if (userDto.ProfilePhoto != null)
            {
                using var memoryStream = new MemoryStream();
                await userDto.ProfilePhoto.CopyToAsync(memoryStream);

                var photo = new UserPhoto
                {
                    FileName = userDto.ProfilePhoto.FileName,
                    MimeType = userDto.ProfilePhoto.ContentType,
                    FilePath = Convert.ToBase64String(memoryStream.ToArray()),
                    UserId = newUser.Id // Asociar la foto al usuario recién creado
                };

                await _photoRepository.AddAsync(photo);

                // Actualizar el usuario con la referencia de la foto
                newUser.PhotoId = photo.Id;
                await _userRepository.UpdateAsync(newUser);
            }

            return newUser;
        }

        public async Task<User> AuthenticateAsync(string email, string password)
        {
            var user = (await _userRepository.GetAllAsync())
                .FirstOrDefault(u => u.Email == email);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                return null; // Retornar null si no coincide
            }

            return user;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            var inputHash = HashPassword(inputPassword);
            return inputHash == storedHash;
        }
    }
}

