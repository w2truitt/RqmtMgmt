using backend.Configuration;

/// <summary>
/// Main program class for the Requirements Management System backend API.
/// Configures services, middleware, authentication, and database seeding for the application.
/// Refactored to use extension methods for better separation of concerns and reduced complexity.
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point for the application.
    /// Configures and starts the web application with all necessary services and middleware.
    /// </summary>
    /// <param name="args">Command line arguments passed to the application.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure all services using extension methods to reduce complexity
        builder.Services
            .AddDatabaseServices(builder.Configuration, builder.Environment)
            .AddAuthenticationServices(builder.Configuration)
            .AddCorsServices()
            .AddSwaggerServices()
            .AddApplicationServices()
            .AddControllerServices()
            .AddProxyServices();

        var app = builder.Build();

        // Initialize database with retry logic for Docker environments
        await DatabaseInitializer.InitializeWithRetryAsync(app);

        // Configure middleware pipeline and endpoints
        app.ConfigureMiddlewarePipeline()
           .ConfigureEndpoints();

        await app.RunAsync();
    }
}