using CoworkingReservation.Application.DTOs.User;
using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para los servicios relacionados con usuarios.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Obtiene todos los usuarios registrados.
        /// </summary>
        Task<IEnumerable<User>> GetAllAsync();

        /// <summary>
        /// Obtiene un usuario por su ID.
        /// </summary>
        Task<User> GetByIdAsync(int id);

        /// <summary>
        /// Registra un nuevo usuario.
        /// </summary>
        Task<User> RegisterAsync(UserRegisterDTO userDto);

        /// <summary>
        /// Autentica un usuario por correo y contraseña.
        /// </summary>
        Task<User> AuthenticateAsync(string email, string password);

        /// <summary>
        /// Cambia el estado activo/inactivo del usuario.
        /// </summary>
        Task ToggleActiveStatusAsync(int userId);

        /// <summary>
        /// Solicita el cambio de rol a hoster.
        /// </summary>
        Task BecomeHosterAsync(int userId);

    }
}
