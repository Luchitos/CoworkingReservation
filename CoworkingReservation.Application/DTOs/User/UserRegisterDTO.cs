using Microsoft.AspNetCore.Http;

namespace CoworkingReservation.Application.DTOs.User
{
    /// <summary>
    /// DTO para el registro de un usuario.
    /// </summary>
    public class UserRegisterDTO
    {
        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        public string Lastname { get; set; }

        /// <summary>
        /// Nombre de usuario.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Cuit del usuario.
        /// </summary>
        public string Cuit { get; set; }

        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Foto de perfil opcional (formato base64 o archivo).
        /// </summary>
        public IFormFile? ProfilePhoto { get; set; }
    }
}
