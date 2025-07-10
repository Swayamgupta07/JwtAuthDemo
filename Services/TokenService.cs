using JwtAuthDemo.Model.Entity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace JwtAuthDemo.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string CreateToken(AppUser user)
        {
            var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
                        };

            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];
            var expiryMinutes = _configuration["Jwt:ExpiryMinutes"];

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is not configured.");
            }

            if (string.IsNullOrEmpty(jwtIssuer))
            {
                throw new InvalidOperationException("JWT Issuer is not configured.");
            }

            if (string.IsNullOrEmpty(jwtAudience))
            {
                throw new InvalidOperationException("JWT Audience is not configured.");
            }

            if (string.IsNullOrEmpty(expiryMinutes) || !int.TryParse(expiryMinutes, out int expiryMinutesValue))
            {
                throw new InvalidOperationException("JWT ExpiryMinutes is not configured or invalid.");
            }

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(expiryMinutesValue),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
