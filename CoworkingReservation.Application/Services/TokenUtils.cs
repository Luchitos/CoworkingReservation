using System.Security.Claims;

namespace CoworkingReservation.Application.Services
{
    /// <summary>
    /// Utilidad para extraer información del token JWT.
    /// </summary>
    public static class TokenUtils
    {
        public static int GetUserIdFromToken(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value);
        }

        public static string GetRoleFromToken(ClaimsPrincipal user)
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value;
        }
    }
}