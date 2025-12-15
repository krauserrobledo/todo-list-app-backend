using System.Security.Claims;

namespace MinimalApi.Extensions
{
    /// <summary>
    /// Extension methods for HttpContext
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Requires that the user is authenticated and returns the user ID
        /// </summary>
        /// <param name="context">A HttpContext instance</param>
        /// <returns>User ID</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if the user is not authenticated</exception>
        public static string RequireUserId(this HttpContext context)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User ID claim not found.");
            return userId;
        }
    }
}
