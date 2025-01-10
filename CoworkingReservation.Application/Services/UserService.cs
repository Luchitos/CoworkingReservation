using CoworkingReservation.Application.DTOs.User;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using System.Security.Cryptography;
using System.Text;

namespace CoworkingReservation.Application.Services
{
    /// <summary>
    /// Servicio para la gestión de usuarios.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _unitOfWork.Users.GetAllAsync();
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<User> RegisterAsync(UserRegisterDTO userDto)
        {
            // Validar datos de entrada
            if (string.IsNullOrEmpty(userDto.Name) || string.IsNullOrEmpty(userDto.Email) || string.IsNullOrEmpty(userDto.Password))
                throw new ArgumentException("Name, email, and password are required.");

            if (string.IsNullOrEmpty(userDto.Cuit))
                throw new ArgumentException("CUIT is required.");

            // Verificar si el CUIT o correo ya existen
            var existingUser = (await _unitOfWork.Users.GetAllAsync())
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

            await _unitOfWork.Users.AddAsync(newUser);

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

                await _unitOfWork.Users.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();

                // Actualizar el usuario con la referencia de la foto
                newUser.PhotoId = photo.Id;
                await _unitOfWork.Users.UpdateAsync(newUser);
            }

            await _unitOfWork.SaveChangesAsync(); // Guardar todos los cambios de manera transaccional
            return newUser;
        }

        public async Task<User> AuthenticateAsync(string email, string password)
        {
            var user = (await _unitOfWork.Users.GetAllAsync())
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

        public async Task ToggleActiveStatusAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("User not found.");

            user.IsActive = !user.IsActive;
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task BecomeHosterAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("User not found.");

            user.IsHosterRequestPending = true;
            await _unitOfWork.SaveChangesAsync();
        }
    }
}