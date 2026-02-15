using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECommerce.API.Common.Services
{
    public class JwtService(IConfiguration configuration) : IJwtService
    {
        public string GenerateToken(Guid sessionId)
        {
            var secret = configuration["Jwt:Secret"]!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: [new Claim(JwtRegisteredClaimNames.Sub, sessionId.ToString())],
                expires: GetExpiration(),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public Guid? ValidateToken(string token)
        {
            try
            {
                var secret = configuration["Jwt:Secret"]!;
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

                new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                var jwt = (JwtSecurityToken)validatedToken;
                return Guid.Parse(jwt.Subject);
            }
            catch
            {
                return null;
            }
        }

        public DateTime GetExpiration()
        {
            var days = int.Parse(configuration["Jwt:ExpirationDays"] ?? "30");
            return DateTime.UtcNow.AddDays(days);
        }
    }
}
