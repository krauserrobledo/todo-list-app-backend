using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Data.Abstractions;
using Data.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Data.Services
{

    /// <summary>
    /// Handles JWT token generation and validation.
    /// </summary>
    /// <param name="configuration"></param>
    public class TokenService(IConfiguration configuration) : ITokenService
    {
        // inject configuration to access JWT settings
        private readonly IConfiguration _configuration = configuration;

        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <remarks> The token includes claims such as user ID and email, and is signed using a symmetric security key.</remarks>
        /// <param name="user"> </param>
        /// <returns>Token value</returns>
        public string GenerateToken(ApplicationUser user)
        {

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define claims
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id)
            };

            // Create token
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Validates the specified JWT token and returns the associated ClaimsPrincipal if valid.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>ClaimsPrincipal if valid; otherwise, null.</returns>
        public ClaimsPrincipal? ValidateToken(string token)
        {

            // Get JWT settings
            var jwtSettings = _configuration.GetSection("JwtSettings");

            // Create security key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

            // Validate token
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {

                // Validate the token and retrieve claims
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                // Return the principal if validation is successful
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}