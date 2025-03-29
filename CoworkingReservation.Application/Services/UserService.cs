using CoworkingReservation.Application.DTOs.User;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;
using Microsoft.AspNetCore.Identity;
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
        private readonly IPasswordHasher<User> _passwordHasher;


        public UserService(IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
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

            // **Verificar si el usuario ya existe usando un método más eficiente**
            if (await _unitOfWork.Users.ExistsByEmailOrCuit(userDto.Email, userDto.Cuit))
                throw new InvalidOperationException("A user with this email or CUIT already exists.");
           
            // Crear el usuario
            var newUser = new User
            {
                Name = userDto.Name,
                Lastname = userDto.Lastname,
                UserName = userDto.UserName,
                Cuit = userDto.Cuit,
                Email = userDto.Email,
                Role = "Client",
            };

            // **Corregido: Usar la instancia real en lugar de null**
            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, userDto.Password);
            
            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.SaveChangesAsync(); // Guardar usuario antes de manejar la foto

            // Manejar la foto de perfil solo si está presente
            if (userDto.ProfilePhoto != null)
            {
                using var memoryStream = new MemoryStream();
                await userDto.ProfilePhoto.CopyToAsync(memoryStream);

                var photo = new UserPhoto
                {
                    FileName = userDto.ProfilePhoto.FileName,
                    MimeType = userDto.ProfilePhoto.ContentType,
                    FilePath = Convert.ToBase64String(memoryStream.ToArray()),
                    UserId = newUser.Id // Asignar después de guardar el usuario
                };

                await _unitOfWork.UserPhotos.AddAsync(photo);
                await _unitOfWork.SaveChangesAsync();

                // Asociar la foto al usuario y guardar cambios
                newUser.PhotoId = photo.Id;
                await _unitOfWork.Users.UpdateAsync(newUser);
                await _unitOfWork.SaveChangesAsync();
            }

            return newUser;
        }

        public async Task<User?> AuthenticateAsync(string identifier, string password)
        {
            var user = await _unitOfWork.Users.GetByIdentifierWithPhotoAsync(identifier);

            if (user == null)
            {
                return null; // El usuario no existe
            }

            // Verificar la contraseña
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success ? user : null;
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
            // Actualizar rol del usuario
            user.Role = "Hoster";
            await _unitOfWork.SaveChangesAsync();
        }
    }
}