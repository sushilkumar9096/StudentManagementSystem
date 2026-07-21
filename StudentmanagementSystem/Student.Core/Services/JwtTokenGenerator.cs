using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Student.Core.Entities;
using Student.Core.Interfaces;

namespace Student.Core.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (string token, DateTime expiration) GenerateToken(User user)
        {
            var secretKey = _configuration["Jwt:Key"] ?? "SuperSecretKeyForZestIndiaStudentManagementSystem2026!";
            var issuer = _configuration["Jwt:Issuer"] ?? "StudentApi";
            var audience = _configuration["Jwt:Audience"] ?? "StudentApiClients";
            var expiryMinutes = double.TryParse(_configuration["Jwt:ExpiryMinutes"], out var minutes) ? minutes : 120;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expiration = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiration,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);

            return (tokenHandler.WriteToken(securityToken), expiration);
        }
    }
}
