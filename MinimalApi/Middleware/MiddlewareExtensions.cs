namespace MinimalApi.Middleware
{
    public static class MiddlewareExtensions 

        {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
        public static IApplicationBuilder UseRequestLogginMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
