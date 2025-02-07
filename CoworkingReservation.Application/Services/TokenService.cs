using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CoworkingReservation.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;

        public TokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public string GenerateToken(int userId, string email, string role)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()), // Identificador único del usuario
                new Claim(JwtRegisteredClaimNames.Email, email),          // Email del usuario
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),  // ID del usuario con el esquema estándar
                new Claim(ClaimTypes.Name, email),                        // Nombre del usuario (usamos el email)
                new Claim(ClaimTypes.Role, role),                         // Rol del usuario
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ID único del JWT
                new Claim("userId", userId.ToString())                    // Reclamación personalizada con el UserID
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
