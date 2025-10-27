using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;

namespace EventManagement.Application.Services
{
    public class JwtAuthService : IJwtAuthService
    {
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenDays;
        private readonly IUserRoleRepository _userRoleRepo;

        public JwtAuthService(IConfiguration configuration, IUserRoleRepository userRoleRepo)
        {
            _key = configuration["Jwt:Secret-Key"] ?? throw new ArgumentNullException("JWT Secret-Key not found");
            _issuer = configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("JWT Issuer not found");
            _audience = configuration["Jwt:Audience"] ?? throw new ArgumentNullException("JWT Audience not found");
            _accessTokenDays = int.TryParse(configuration["Jwt:AccessTokenDays"], out var d) ? d : 1;
            _userRoleRepo = userRoleRepo;
        }

        public string GenerateToken(User user)
        {
            var keyBytes = Encoding.ASCII.GetBytes(_key);

            var claims = new List<Claim>
            {
                new Claim("UserName", user.FullName ?? string.Empty),
                new Claim("Email", user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString())
            };

            // ⚡ lấy roles qua Repository (Application interface, impl ở Infrastructure)
            var roles = _userRoleRepo.GetRolesByUserIdAsync(user.UserId).Result;
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(_accessTokenDays),
                SigningCredentials = creds,
                Issuer = _issuer,
                Audience = _audience,
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        public string DecodePayloadToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token không được để trống", nameof(token));

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserName");
            if (usernameClaim == null)
                throw new InvalidOperationException("Không tìm thấy UserName trong token");

            return usernameClaim.Value;
        }

        public bool ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var keyBytes = Encoding.ASCII.GetBytes(_key);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
            };

            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out var _);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}