using CoworkingReservation.Application.DTOs.CoworkingSpace;
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
        Task<User> AuthenticateAsync(string identifier, string password);

        /// <summary>
        /// Cambia el estado activo/inactivo del usuario.
        /// </summary>
        Task ToggleActiveStatusAsync(int userId);

        /// <summary>
        /// Solicita el cambio de rol a hoster.
        /// </summary>
        Task BecomeHosterAsync(int userId);

        /// <summary>
        /// Actualiza un campo específico del perfil de usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="field">Nombre del campo a actualizar</param>
        /// <param name="value">Nuevo valor para el campo</param>
        Task<User> UpdateProfileFieldAsync(int userId, string field, object value);

        /// <summary>
        /// Actualiza la foto de perfil del usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="photoDto">Datos de la foto</param>
        Task<User> UpdateProfilePhotoAsync(int userId, PhotoDTO photoDto);

        /// <summary>
        /// Actualiza el teléfono del usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="phone">Nuevo número de teléfono</param>
        Task<User> UpdatePhoneAsync(int userId, string phone);

        /// <summary>
        /// Actualiza el CUIT del usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cuit">Nuevo CUIT</param>
        Task<User> UpdateCuitAsync(int userId, string cuit);

        /// <summary>
        /// Actualiza la dirección del usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="address">Nueva dirección</param>
        Task<User> UpdateAddressAsync(int userId, string address);

        Task<IEnumerable<CoworkingSpaceListItemDTO>> GetFavoriteSpacesAsync(int userId);

        Task ToggleFavoriteAsync(int userId, int coworkingSpaceId, bool isFavorite);

        Task<CoworkingSpaceFavoritesResponseDTO> GetFavoriteSpacesResponseAsync(int userId);
    }
}
