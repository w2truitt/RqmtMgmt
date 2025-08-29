using System.Diagnostics;

namespace backend.Middleware
{
    /// <summary>
    /// Middleware to log API response times for performance monitoring
    /// </summary>
    public class ResponseTimeLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResponseTimeLoggingMiddleware> _logger;

        public ResponseTimeLoggingMiddleware(RequestDelegate next, ILogger<ResponseTimeLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            
            await _next(context);
            
            stopwatch.Stop();
            
            // Log API calls that take longer than 1 second
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("SLOW API: {Method} {Path} took {ElapsedMs}ms", 
                    context.Request.Method, 
                    context.Request.Path, 
                    stopwatch.ElapsedMilliseconds);
            }
            else if (context.Request.Path.StartsWithSegments("/api"))
            {
                _logger.LogInformation("API: {Method} {Path} took {ElapsedMs}ms", 
                    context.Request.Method, 
                    context.Request.Path, 
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}