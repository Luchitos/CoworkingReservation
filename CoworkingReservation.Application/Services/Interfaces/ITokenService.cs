using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CoworkingReservation.Application.Services.Interfaces
{
    /// <summary>
    /// Servicio para la gestión de tokens JWT.
    /// </summary>
    public interface ITokenService
    {
        #region Generación de Token

        /// <summary>
        /// Genera un token JWT para un usuario autenticado.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="email">Correo electrónico del usuario.</param>
        /// <param name="role">Rol del usuario.</param>
        /// <returns>Token JWT generado.</returns>
        string GenerateToken(int userId, string email, string role);

        #endregion

        #region Validación de Token

        /// <summary>
        /// Valida un token JWT y extrae sus claims.
        /// </summary>
        /// <param name="token">Token JWT a validar.</param>
        /// <returns>Lista de claims extraídos del token.</returns>
        Task<IEnumerable<Claim>> ValidateTokenAsync(string token);

        #endregion
    }
}
