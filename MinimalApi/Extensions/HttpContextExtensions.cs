using System.Security.Claims;

namespace MinimalApi.Extensions
{
    public static class HttpContextExtensions
    {
        public static string RequireUserId(this HttpContext context)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User ID claim not found.");
            return userId;
        }
    }
}
