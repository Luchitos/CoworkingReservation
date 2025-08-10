using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Infrastructure.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CoworkingReservation.Application.Services
{
    /// <summary>
    /// Implementación del servicio de carga de imágenes usando ImgBB
    /// </summary>
    public class ImgBBImageUploadService : IImageUploadService
    {
        private readonly ImgBBSettings _imgBBSettings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ImgBBImageUploadService> _logger;
        private readonly Random _random = new Random();

        public ImgBBImageUploadService(
            IOptions<ImgBBSettings> imgBBSettings,
            HttpClient httpClient,
            ILogger<ImgBBImageUploadService> logger)
        {
            _imgBBSettings = imgBBSettings.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Sube una imagen a ImgBB y devuelve la URL
        /// </summary>
        public async Task<string> UploadImageAsync(IFormFile image)
        {
            try
            {
                // Convertir el archivo a bytes
                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                
                // Hacer la imagen única
                imageBytes = AddRandomMetadata(imageBytes);
                
                return await UploadImageAsync(imageBytes, image.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir imagen a ImgBB");
                throw;
            }
        }

        /// <summary>
        /// Sube una imagen como array de bytes a ImgBB y devuelve la URL
        /// </summary>
        public async Task<string> UploadImageAsync(byte[] imageBytes, string fileName)
        {
            // Hacer la imagen única
            imageBytes = AddRandomMetadata(imageBytes);
            
            return await UploadImageAsync(imageBytes, fileName, null, null);
        }

        /// <summary>
        /// Sube una imagen de perfil de usuario a ImgBB
        /// </summary>
        public async Task<string> UploadUserImageAsync(IFormFile image, int userId)
        {
            try
            {
                // Convertir el archivo a bytes
                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                
                // Hacer la imagen única
                imageBytes = AddRandomMetadata(imageBytes);
                
                return await UploadImageAsync(imageBytes, image.FileName, "users", userId.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir imagen de usuario a ImgBB");
                throw;
            }
        }

        /// <summary>
        /// Sube una imagen de espacio de coworking a ImgBB
        /// </summary>
        public async Task<string> UploadCoworkingSpaceImageAsync(IFormFile image, int spaceId)
        {
            try
            {
                // Convertir el archivo a bytes
                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                
                // Hacer la imagen única
                imageBytes = AddRandomMetadata(imageBytes);
                
                return await UploadImageAsync(imageBytes, image.FileName, "spaces", spaceId.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir imagen de espacio de coworking a ImgBB");
                throw;
            }
        }
        
        /// <summary>
        /// Añade metadatos aleatorios al final del array de bytes para hacer cada imagen única
        /// </summary>
        private byte[] AddRandomMetadata(byte[] imageBytes)
        {
            try
            {
                // Generar bytes aleatorios
                byte[] randomBytes = new byte[16];
                _random.NextBytes(randomBytes);
            
                // Métodos sin usar System.Drawing - más compatible pero afecta la imagen
                // Simplemente añadir bytes al final de la imagen, generando un hash de contenido diferente
                // Esto alterará levemente la imagen pero asegurará que ImgBB la trate como diferente
                byte[] resultBytes = new byte[imageBytes.Length + randomBytes.Length];
                Buffer.BlockCopy(imageBytes, 0, resultBytes, 0, imageBytes.Length);
                Buffer.BlockCopy(randomBytes, 0, resultBytes, imageBytes.Length, randomBytes.Length);
                
                return resultBytes;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudieron añadir metadatos aleatorios a la imagen. Usando la imagen original.");
                return imageBytes;
            }
        }

        /// <summary>
        /// Método principal para subir imágenes a ImgBB con organización lógica
        /// </summary>
        private async Task<string> UploadImageAsync(byte[] imageBytes, string fileName, string category, string itemId)
        {
            try
            {
                // Validar tamaño de imagen (máximo 32MB para ImgBB)
                if (imageBytes.Length > 32 * 1024 * 1024)
                {
                    throw new InvalidOperationException($"La imagen {fileName} excede el límite de 32MB de ImgBB");
                }

                // Convertir la imagen a base64
                var base64Image = Convert.ToBase64String(imageBytes);
                
                // Crear un nombre de archivo organizado si se proporcionaron datos de categoría
                string organizedName = fileName;
                if (!string.IsNullOrEmpty(category) && !string.IsNullOrEmpty(itemId))
                {
                    // Extraer la extensión
                    string extension = Path.GetExtension(fileName);
                    
                    // Generar un identificador único para el archivo
                    string uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
                    
                    // El nombre será diferente según la categoría
                    if (category == "users")
                    {
                        // Para usuarios: users/123/user_123_uniqueId.jpg
                        organizedName = $"{category}/{itemId}/user_{itemId}_{uniqueId}{extension}";
                    }
                    else if (category == "spaces")
                    {
                        // Para espacios: spaces/456/space_456_index_uniqueId.jpg
                        // itemId contiene el ID del espacio, incluimos un índice si está presente en el nombre original
                        
                        // Intentar extraer un índice del nombre original si existe (ej. "photo1.jpg" -> "1")
                        string indexPart = "0";
                        string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                        
                        // Buscar dígitos al final del nombre
                        string pattern = @"\d+$";
                        var match = System.Text.RegularExpressions.Regex.Match(nameWithoutExtension, pattern);
                        if (match.Success)
                        {
                            indexPart = match.Value;
                        }
                        
                        organizedName = $"{category}/{itemId}/space_{itemId}_{indexPart}_{uniqueId}{extension}";
                    }
                    else
                    {
                        // Para otros tipos, mantener el formato original con timestamp y uniqueId
                        string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                        organizedName = $"{category}/{itemId}/{nameWithoutExtension}_{DateTime.UtcNow.Ticks}_{uniqueId}{extension}";
                    }
                }
                
                // Crear el formulario para ImgBB
                var formContent = new MultipartFormDataContent
                {
                    { new StringContent(_imgBBSettings.ApiKey), "key" },
                    { new StringContent(base64Image), "image" },
                    { new StringContent(organizedName), "name" },
                    { new StringContent("1"), "explicit" }
                };
                
                // Si hay un tiempo de expiración configurado, agregarlo
                if (_imgBBSettings.ExpirationTime > 0)
                {
                    formContent.Add(new StringContent(_imgBBSettings.ExpirationTime.ToString()), "expiration");
                }
                
                // Realizar la solicitud a ImgBB con retry logic
                HttpResponseMessage response = null;
                int maxRetries = 3;
                int currentRetry = 0;
                
                while (currentRetry < maxRetries)
                {
                    try
                    {
                        _logger.LogInformation($"Intentando subir imagen {fileName} a ImgBB (intento {currentRetry + 1}/{maxRetries})");
                        
                        response = await _httpClient.PostAsync(_imgBBSettings.ApiUrl, formContent);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            break; // Éxito, salir del bucle
                        }
                        
                        // Si no es exitoso, verificar si es un error temporal
                        if (response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout ||
                            response.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
                            response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                        {
                            currentRetry++;
                            if (currentRetry < maxRetries)
                            {
                                _logger.LogWarning($"Timeout en intento {currentRetry} para {fileName}. Reintentando en {currentRetry * 2} segundos...");
                                await Task.Delay(currentRetry * 2000); // Backoff exponencial
                                continue;
                            }
                        }
                        
                        // Si no es un error temporal o se agotaron los reintentos, lanzar excepción
                        response.EnsureSuccessStatusCode();
                    }
                    catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                    {
                        currentRetry++;
                        if (currentRetry < maxRetries)
                        {
                            _logger.LogWarning($"Timeout en intento {currentRetry} para {fileName}. Reintentando en {currentRetry * 2} segundos...");
                            await Task.Delay(currentRetry * 2000);
                            continue;
                        }
                        throw new TimeoutException($"Timeout al subir imagen {fileName} después de {maxRetries} intentos", ex);
                    }
                }
                
                if (response == null || !response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error al subir imagen {fileName}. Status: {response?.StatusCode}");
                }
                
                // Parsear la respuesta
                var responseContent = await response.Content.ReadAsStringAsync();
                var imgBBResponse = JsonSerializer.Deserialize<ImgBBResponse>(responseContent, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (imgBBResponse?.Data == null)
                {
                    throw new Exception("La respuesta de ImgBB no contiene datos válidos");
                }
                
                _logger.LogInformation($"Imagen {fileName} subida exitosamente a ImgBB");
                
                // Devolver la URL de la imagen
                return imgBBResponse.Data.Url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir imagen {FileName} a ImgBB", fileName);
                throw;
            }
        }
        
        /// <summary>
        /// Clase para deserializar la respuesta de ImgBB
        /// </summary>
        private class ImgBBResponse
        {
            [JsonPropertyName("data")]
            public ImgBBData Data { get; set; }
            
            [JsonPropertyName("success")]
            public bool Success { get; set; }
            
            [JsonPropertyName("status")]
            public int Status { get; set; }
        }
        
        /// <summary>
        /// Datos de la respuesta de ImgBB
        /// </summary>
        private class ImgBBData
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }
            
            [JsonPropertyName("url")]
            public string Url { get; set; }
            
            [JsonPropertyName("display_url")]
            public string DisplayUrl { get; set; }
            
            [JsonPropertyName("delete_url")]
            public string DeleteUrl { get; set; }
        }
    }
} 