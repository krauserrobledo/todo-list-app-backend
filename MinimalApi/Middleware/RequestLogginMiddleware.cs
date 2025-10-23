namespace MinimalApi.Middleware
{
    /// <summary>
    /// Middleware to log details of incoming HTTP requests and their processing time.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLoggingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger instance used for logging requests.</param>
        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        /// <summary>
        /// Processes an HTTP request, logging its details and duration.
        /// </summary>
        /// <param name="context">The HTTP context for the request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("Starting request: {Method} {Path}",
                context.Request.Method, context.Request.Path);
            await _next(context);
            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("Completed request: {Method} {Path} - {StatusCode} in {Duration}ms",
                context.Request.Method, context.Request.Path,
                context.Response.StatusCode, duration.TotalMilliseconds);
        }
    }
}
