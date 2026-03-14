using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services
{
    /// <summary>
    /// Generates signed HS256 JWT access tokens.
    /// Reads from appsettings.json → JwtSettings section.
    /// Requires NuGet: Microsoft.AspNetCore.Authentication.JwtBearer
    /// </summary>
    public class JwtTokenService : IJwtTokenService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiresInSeconds;

        public int ExpiresInSeconds => _expiresInSeconds;

        public JwtTokenService(IConfiguration configuration)
        {
            var section = configuration.GetSection("JwtSettings");
            _secretKey = section["SecretKey"]
                         ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");
            _issuer = section["Issuer"] ?? "CleanArchitectureDemo";
            _audience = section["Audience"] ?? "CleanArchitectureDemo";
            _expiresInSeconds = int.TryParse(section["ExpiresInSeconds"], out var exp) ? exp : 3600;
        }

        public string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddSeconds(_expiresInSeconds),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
