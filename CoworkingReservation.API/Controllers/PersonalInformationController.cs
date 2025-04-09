using CoworkingReservation.Application.DTOs.User;
using CoworkingReservation.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CoworkingReservation.API.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class PersonalInformationController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IImageUploadService _imageUploadService;

        public PersonalInformationController(IUserService userService, IImageUploadService imageUploadService)
        {
            _userService = userService;
            _imageUploadService = imageUploadService;
        }

        /// <summary>
        /// Actualiza un campo específico del perfil del usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="updateDto">DTO con el campo y valor a actualizar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPatch("User/{userId}/update-profile")]
        public async Task<IActionResult> UpdateProfileField(int userId, [FromBody] UpdateProfileFieldDTO updateDto)
        {
            // Verificar que el usuario autenticado sea el mismo que se quiere actualizar
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentUserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                // Manejar el caso especial de fotos que se envían como archivos
                if (updateDto.Field.ToLower() == "photo" && updateDto.Value != null)
                {
                    return BadRequest(Responses.Response.Failure("Para actualizar la foto, use el endpoint POST /api/User/{userId}/update-photo"));
                }
                
                var user = await _userService.UpdateProfileFieldAsync(userId, updateDto.Field, updateDto.Value);
                return Ok(Responses.Response.Success("Información actualizada correctamente"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Responses.Response.Failure(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(Responses.Response.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Responses.Response.Failure($"Error al actualizar la información: {ex.Message}"));
            }
        }

        /// <summary>
        /// Actualiza la foto de perfil del usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="photoFile">Archivo de imagen</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("User/{userId}/update-photo")]
        public async Task<IActionResult> UpdateProfilePhoto(int userId, IFormFile photoFile)
        {
            // Verificar que el usuario autenticado sea el mismo que se quiere actualizar
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentUserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                if (photoFile == null || photoFile.Length == 0)
                {
                    return BadRequest(Responses.Response.Failure("No se ha proporcionado ningún archivo de imagen"));
                }

                // Obtener el usuario
                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(Responses.Response.Failure("Usuario no encontrado"));
                }

                // Subir la imagen a ImgBB (reemplaza la anterior si existe)
                string imageUrl = await _imageUploadService.UploadUserImageAsync(photoFile, userId);
                
                // Crear DTO con la información de la foto
                var photoDto = new PhotoDTO
                {
                    Url = imageUrl,
                    FileName = photoFile.FileName,
                    MimeType = photoFile.ContentType
                };
                
                // Actualizar la foto de perfil del usuario
                var updatedUser = await _userService.UpdateProfilePhotoAsync(userId, photoDto);
                
                return Ok(Responses.Response.Success("Foto de perfil actualizada correctamente"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Responses.Response.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Responses.Response.Failure($"Error al actualizar la foto de perfil: {ex.Message}"));
            }
        }

        /// <summary>
        /// Actualiza el teléfono del usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="phone">Nuevo número de teléfono</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPatch("User/{userId}/update-phone")]
        public async Task<IActionResult> UpdatePhone(int userId, [FromBody] string phone)
        {
            // Verificar que el usuario autenticado sea el mismo que se quiere actualizar
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentUserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                var user = await _userService.UpdatePhoneAsync(userId, phone);
                return Ok(Responses.Response.Success("Teléfono actualizado correctamente"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Responses.Response.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Responses.Response.Failure($"Error al actualizar el teléfono: {ex.Message}"));
            }
        }

        /// <summary>
        /// Actualiza el CUIT del usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cuit">Nuevo CUIT</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPatch("User/{userId}/update-cuit")]
        public async Task<IActionResult> UpdateCuit(int userId, [FromBody] string cuit)
        {
            // Verificar que el usuario autenticado sea el mismo que se quiere actualizar
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentUserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                var user = await _userService.UpdateCuitAsync(userId, cuit);
                return Ok(Responses.Response.Success("CUIT actualizado correctamente"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Responses.Response.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Responses.Response.Failure($"Error al actualizar el CUIT: {ex.Message}"));
            }
        }

        /// <summary>
        /// Actualiza la dirección del usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="address">Nueva dirección</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPatch("User/{userId}/update-address")]
        public async Task<IActionResult> UpdateAddress(int userId, [FromBody] string address)
        {
            // Verificar que el usuario autenticado sea el mismo que se quiere actualizar
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentUserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                var user = await _userService.UpdateAddressAsync(userId, address);
                return Ok(Responses.Response.Success("Dirección actualizada correctamente"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Responses.Response.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Responses.Response.Failure($"Error al actualizar la dirección: {ex.Message}"));
            }
        }
    }
} 