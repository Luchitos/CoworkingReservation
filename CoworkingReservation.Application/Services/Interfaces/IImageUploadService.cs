using Microsoft.AspNetCore.Http;

namespace CoworkingReservation.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de carga de imágenes a servicios externos
    /// </summary>
    public interface IImageUploadService
    {
        /// <summary>
        /// Sube una imagen a ImgBB y devuelve la URL
        /// </summary>
        /// <param name="image">Archivo de imagen</param>
        /// <returns>URL de la imagen en ImgBB</returns>
        Task<string> UploadImageAsync(IFormFile image);
        
        /// <summary>
        /// Sube una imagen como array de bytes a ImgBB y devuelve la URL
        /// </summary>
        /// <param name="imageBytes">Bytes de la imagen</param>
        /// <param name="fileName">Nombre del archivo</param>
        /// <returns>URL de la imagen en ImgBB</returns>
        Task<string> UploadImageAsync(byte[] imageBytes, string fileName);

        /// <summary>
        /// Sube una imagen de perfil de usuario con organización por carpeta
        /// </summary>
        /// <param name="image">Archivo de imagen</param>
        /// <param name="userId">ID del usuario</param>
        /// <returns>URL de la imagen en ImgBB</returns>
        Task<string> UploadUserImageAsync(IFormFile image, int userId);

        /// <summary>
        /// Sube una imagen de espacio de coworking con organización por carpeta
        /// </summary>
        /// <param name="image">Archivo de imagen</param>
        /// <param name="spaceId">ID del espacio</param>
        /// <returns>URL de la imagen en ImgBB</returns>
        Task<string> UploadCoworkingSpaceImageAsync(IFormFile image, int spaceId);
    }
} 