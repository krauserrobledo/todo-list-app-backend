using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Data.Abstractions;
using Data.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Data.Services
{
<<<<<<< HEAD
<<<<<<< HEAD

=======
>>>>>>> f2584a8 (refactor: add comments and clean implementation)
=======

>>>>>>> 3b811eb (refactor: improve code organization)
    /// <summary>
    /// Handles JWT token generation and validation.
    /// </summary>
    /// <param name="configuration"></param>
    public class TokenService(IConfiguration configuration) : ITokenService
    {
<<<<<<< HEAD
<<<<<<< HEAD
        // inject configuration to access JWT settings
        private readonly IConfiguration _configuration = configuration;

=======
        private readonly IConfiguration _configuration = configuration;
>>>>>>> f2584a8 (refactor: add comments and clean implementation)
=======
        // inject configuration to access JWT settings
        private readonly IConfiguration _configuration = configuration;

>>>>>>> 3b811eb (refactor: improve code organization)
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
<<<<<<< HEAD
<<<<<<< HEAD

=======
>>>>>>> f2584a8 (refactor: add comments and clean implementation)
=======

>>>>>>> 3b811eb (refactor: improve code organization)
            // Define claims
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id)
            };
<<<<<<< HEAD
<<<<<<< HEAD

=======
>>>>>>> f2584a8 (refactor: add comments and clean implementation)
=======

>>>>>>> 3b811eb (refactor: improve code organization)
            // Create token
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"])),
                signingCredentials: credentials
            );
<<<<<<< HEAD
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
<<<<<<< HEAD

=======
>>>>>>> f2584a8 (refactor: add comments and clean implementation)
=======

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

>>>>>>> 3b811eb (refactor: improve code organization)
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
<<<<<<< HEAD
<<<<<<< HEAD

=======
>>>>>>> f2584a8 (refactor: add comments and clean implementation)
=======

>>>>>>> 3b811eb (refactor: improve code organization)
            // Validate token
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
<<<<<<< HEAD
<<<<<<< HEAD

=======
>>>>>>> f2584a8 (refactor: add comments and clean implementation)
=======

>>>>>>> 3b811eb (refactor: improve code organization)
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
<<<<<<< HEAD
<<<<<<< HEAD

=======
>>>>>>> f2584a8 (refactor: add comments and clean implementation)
=======

>>>>>>> 3b811eb (refactor: improve code organization)
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