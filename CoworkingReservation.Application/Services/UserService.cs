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
        private readonly IImageUploadService _imageUploadService;


        public UserService(IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher, IImageUploadService imageUploadService)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _imageUploadService = imageUploadService;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _unitOfWork.Users.GetAllAsync();
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _unitOfWork.Users.GetByIdWithPhotoAsync(id);
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
                try 
                {
                    // Subir la imagen a ImgBB usando el nuevo método organizado por carpetas
                    string imageUrl = await _imageUploadService.UploadUserImageAsync(userDto.ProfilePhoto, newUser.Id);
                    
                    // Crear el registro de foto en nuestra base de datos
                    var photo = new UserPhoto
                    {
                        FileName = userDto.ProfilePhoto.FileName,
                        MimeType = userDto.ProfilePhoto.ContentType,
                        FilePath = imageUrl, // URL de ImgBB con organización de carpetas
                        UserId = newUser.Id
                    };

                    await _unitOfWork.UserPhotos.AddAsync(photo);
                    await _unitOfWork.SaveChangesAsync();

                    // Asociar la foto al usuario y guardar cambios
                    newUser.PhotoId = photo.Id;
                    await _unitOfWork.Users.UpdateAsync(newUser);
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Si hay un error al subir la foto, lo registramos pero no fallamos el registro del usuario
                    Console.WriteLine($"Error al subir foto de perfil: {ex.Message}");
                    // Aquí podríamos usar un ILogger adecuado
                }
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
        
        public async Task<User> UpdateProfileFieldAsync(int userId, string field, object value)
        {
            var user = await _unitOfWork.Users.GetByIdWithPhotoAsync(userId);
            if (user == null) throw new KeyNotFoundException("Usuario no encontrado.");

            // Actualizar el campo según el nombre proporcionado
            switch (field.ToLower())
            {
                case "phone":
                    user.Phone = value?.ToString();
                    break;
                case "cuit":
                    user.Cuit = value?.ToString();
                    break;
                case "address":
                    user.Address = value?.ToString();
                    break;
                case "photo":
                    if (value is PhotoDTO photoDto)
                    {
                        await UpdateUserPhoto(user, photoDto);
                    }
                    else
                    {
                        throw new ArgumentException("El valor para 'photo' debe ser un objeto PhotoDTO válido.");
                    }
                    break;
                default:
                    throw new ArgumentException($"Campo '{field}' no reconocido o no permitido para actualización.");
            }

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            return user;
        }
        
        public async Task<User> UpdateProfilePhotoAsync(int userId, PhotoDTO photoDto)
        {
            var user = await _unitOfWork.Users.GetByIdWithPhotoAsync(userId);
            if (user == null) throw new KeyNotFoundException("Usuario no encontrado.");
            
            await UpdateUserPhoto(user, photoDto);
            
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            return user;
        }
        
        public async Task<User> UpdatePhoneAsync(int userId, string phone)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("Usuario no encontrado.");
            
            user.Phone = phone;
            
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            return user;
        }
        
        public async Task<User> UpdateCuitAsync(int userId, string cuit)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("Usuario no encontrado.");
            
            user.Cuit = cuit;
            
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            return user;
        }
        
        public async Task<User> UpdateAddressAsync(int userId, string address)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("Usuario no encontrado.");
            
            user.Address = address;
            
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            return user;
        }
        
        private async Task UpdateUserPhoto(User user, PhotoDTO photoDto)
        {
            // Si el usuario ya tiene una foto
            if (user.PhotoId.HasValue && user.Photo != null)
            {
                // Actualizar la foto existente
                user.Photo.FilePath = photoDto.Url;
                user.Photo.FileName = photoDto.FileName;
                user.Photo.MimeType = photoDto.MimeType;
                
                await _unitOfWork.UserPhotos.UpdateAsync(user.Photo);
            }
            else
            {
                // Crear una nueva foto
                var photo = new UserPhoto
                {
                    FilePath = photoDto.Url,
                    FileName = photoDto.FileName,
                    MimeType = photoDto.MimeType,
                    UserId = user.Id
                };
                
                await _unitOfWork.UserPhotos.AddAsync(photo);
                await _unitOfWork.SaveChangesAsync(); // Guardar para obtener el ID
                
                // Actualizar la relación con el usuario
                user.PhotoId = photo.Id;
                user.Photo = photo;
            }
        }
    }
}