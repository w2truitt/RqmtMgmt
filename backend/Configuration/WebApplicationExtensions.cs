using Microsoft.AspNetCore.HttpOverrides;
using backend.Data;

namespace backend.Configuration
{
    /// <summary>
    /// Extension methods for WebApplication to organize middleware pipeline configuration.
    /// Helps reduce complexity in Program.cs by separating middleware concerns into focused methods.
    /// </summary>
    public static class WebApplicationExtensions
    {
        /// <summary>
        /// Configures the middleware pipeline based on the current environment.
        /// Sets up development vs production middleware with appropriate error handling.
        /// </summary>
        /// <param name="app">The web application to configure.</param>
        /// <returns>The configured web application for method chaining.</returns>
        public static WebApplication ConfigureMiddlewarePipeline(this WebApplication app)
        {
            // Configure middleware pipeline based on environment
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Requirements Management API v1");
                    c.OAuthClientId("swagger-ui");
                    c.OAuthAppName("Requirements Management API");
                    c.OAuthScopes("openid", "profile", "rqmtmgmt.api");
                    c.OAuthUsePkce();
                });
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            // Use forwarded headers for proxy scenarios
            app.UseForwardedHeaders();

            // Only use HTTPS redirection in production
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // Enable CORS for frontend communication
            app.UseCors("AllowFrontend");

            // Enable routing (required before authentication)
            app.UseRouting();

            // Enable authentication middleware (JWT Bearer tokens from Identity Server)
            app.UseAuthentication();

            // Custom impersonation middleware for development and testing
            app.UseUserImpersonation();

            app.UseAuthorization();

            return app;
        }

        /// <summary>
        /// Configures API endpoints including controllers and health checks.
        /// Sets up route mapping and health monitoring for Docker containers.
        /// </summary>
        /// <param name="app">The web application to configure.</param>
        /// <returns>The configured web application for method chaining.</returns>
        public static WebApplication ConfigureEndpoints(this WebApplication app)
        {
            app.MapControllers().RequireAuthorization();
            
            // Health check endpoint for Docker container monitoring
            app.MapGet("/health", async (RqmtMgmtDbContext context) =>
            {
                try
                {
                    // Test database connectivity
                    await context.Database.CanConnectAsync();
                    return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
                }
                catch (Exception ex)
                {
                    return Results.Problem(detail: ex.Message, statusCode: 503);
                }
            });
            
            // Global error handling endpoint
            app.Map("/error", (HttpContext context) =>
            {
                return Results.Problem("An unexpected error occurred. Please contact support if the issue persists.");
            });

            return app;
        }

        /// <summary>
        /// Adds custom impersonation middleware for development and testing scenarios.
        /// Allows user impersonation via X-User-Id header for testing purposes.
        /// </summary>
        /// <param name="app">The web application to configure.</param>
        /// <returns>The configured web application for method chaining.</returns>
        private static WebApplication UseUserImpersonation(this WebApplication app)
        {
            app.Use(async (context, next) =>
            {
                // Look for a header 'X-User-Id' for user impersonation in development
                if (context.Request.Headers.TryGetValue("X-User-Id", out var userId))
                {
                    context.Items["UserId"] = userId.ToString();
                }
                await next();
            });

            return app;
        }
    }
}