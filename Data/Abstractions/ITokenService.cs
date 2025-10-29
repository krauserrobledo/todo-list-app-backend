using System.Security.Claims;
using Data.Identity;
namespace Data.Abstractions
{
    /// <summary>
    /// Defines methods for generating and validating authentication tokens.
    /// </summary>
    /// <remarks>This interface provides functionality for creating secure tokens for a given user and
    /// validating tokens to extract claims. It is typically used in authentication and authorization
    /// workflows.</remarks>
    public interface ITokenService
    {
        string GenerateToken(ApplicationUser user);

        ClaimsPrincipal? ValidateToken(string token);
    }
}
