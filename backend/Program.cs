using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Main program class for the Requirements Management System backend API.
/// Configures services, middleware, authentication, and database seeding for the application.
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

        // Add JWT Bearer authentication for OAuth2/OIDC integration
        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = builder.Configuration["Authentication:Authority"];
                options.Audience = builder.Configuration["Authentication:Audience"];
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });

        // Configure CORS policy to allow frontend connections from various development ports
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("https://localhost:7160", "http://localhost:5239", "https://localhost:5001", "http://localhost:5000")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // Add MVC controllers with JSON enum string conversion
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        // Register all business services for dependency injection
        builder.Services.AddScoped<RqmtMgmtShared.IRequirementService, backend.Services.RequirementService>();
        builder.Services.AddScoped<RqmtMgmtShared.ITestCaseService, backend.Services.TestCaseService>();
        builder.Services.AddScoped<RqmtMgmtShared.ITestPlanService, backend.Services.TestPlanService>();
        builder.Services.AddScoped<RqmtMgmtShared.ITestSuiteService, backend.Services.TestSuiteService>();
        builder.Services.AddScoped<RqmtMgmtShared.IUserService, backend.Services.UserService>();
        builder.Services.AddScoped<backend.Services.IRedlineService, backend.Services.RedlineService>();
        builder.Services.AddScoped<RqmtMgmtShared.IRequirementTestCaseLinkService, backend.Services.RequirementTestCaseLinkService>();
        builder.Services.AddScoped<RqmtMgmtShared.IRoleService, backend.Services.RoleService>();
        builder.Services.AddScoped<RqmtMgmtShared.IDashboardService, backend.Services.DashboardService>();
        builder.Services.AddScoped<RqmtMgmtShared.IEnhancedDashboardService, backend.Services.EnhancedDashboardService>();
        builder.Services.AddScoped<RqmtMgmtShared.ITestRunSessionService, backend.Services.TestRunSessionService>();
        builder.Services.AddScoped<RqmtMgmtShared.ITestExecutionService, backend.Services.TestExecutionService>();
        builder.Services.AddScoped<RqmtMgmtShared.IProjectService, backend.Services.ProjectService>();
        
        // Configure database context based on environment
        if (builder.Environment.IsEnvironment("Testing"))
        {
            // Use InMemory database for testing to avoid conflicts and ensure isolation
            builder.Services.AddDbContext<RqmtMgmtDbContext>(options =>
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
        }
        else
        {
            // Use SQL Server for development and production environments
            builder.Services.AddDbContext<RqmtMgmtDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        }

        var app = builder.Build();

        // Initialize database with retry logic for Docker environments
        await InitializeDatabaseWithRetryAsync(app);

        // Configure middleware pipeline based on environment
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseExceptionHandler("/error");
        }

        app.UseHttpsRedirection();

        // Enable CORS for frontend communication
        app.UseCors("AllowFrontend");

        // Enable authentication middleware
        app.UseAuthentication();

        // Custom impersonation middleware for development and testing
        app.Use(async (context, next) =>
        {
            // Look for a header 'X-User-Id' for user impersonation in development
            if (context.Request.Headers.TryGetValue("X-User-Id", out var userId))
            {
                context.Items["UserId"] = userId.ToString();
            }
            await next();
        });

        app.UseAuthorization();

        app.MapControllers();
        
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

        await app.RunAsync();
    }

    /// <summary>
    /// Initializes the database with retry logic to handle Docker container startup timing issues.
    /// </summary>
    /// <param name="app">The web application instance</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private static async Task InitializeDatabaseWithRetryAsync(WebApplication app)
    {
        const int maxRetries = 10;
        const int delayMs = 3000; // 3 seconds between retries

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await InitializeDatabaseAsync(app, attempt, maxRetries);
                return; // Success - exit retry loop
            }
            catch (Exception ex)
            {
                await HandleDatabaseInitializationError(app, ex, attempt, maxRetries, delayMs);
            }
        }
    }

    /// <summary>
    /// Performs the actual database initialization logic.
    /// </summary>
    /// <param name="app">The web application instance</param>
    /// <param name="attempt">Current attempt number</param>
    /// <param name="maxRetries">Maximum number of retries</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private static async Task InitializeDatabaseAsync(WebApplication app, int attempt, int maxRetries)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RqmtMgmtDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Database initialization attempt {Attempt}/{MaxRetries}", attempt, maxRetries);

        // Test database connectivity first
        await context.Database.CanConnectAsync();
        logger.LogInformation("Database connection established successfully");

        // Apply migrations and seed data based on environment
        await InitializeDatabaseByEnvironment(app, context, logger);
        
        logger.LogInformation("Database initialization completed successfully");
    }

    /// <summary>
    /// Initializes the database based on the current environment.
    /// </summary>
    /// <param name="app">The web application instance</param>
    /// <param name="context">The database context</param>
    /// <param name="logger">The logger instance</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private static async Task InitializeDatabaseByEnvironment(WebApplication app, RqmtMgmtDbContext context, ILogger<Program> logger)
    {
        if (app.Environment.IsDevelopment())
        {
            await backend.Data.DatabaseSeeder.SeedAsync(context, includeTestData: true);
            logger.LogInformation("Database seeded with development data successfully");
        }
        else if (app.Environment.IsEnvironment("Testing"))
        {
            await backend.Data.DatabaseSeeder.SeedAsync(context, includeTestData: true);
            logger.LogInformation("Database seeded with test data successfully");
        }
        else
        {
            // Production - only apply migrations, no test data
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");
        }
    }

    /// <summary>
    /// Handles errors during database initialization with appropriate logging and retry logic.
    /// </summary>
    /// <param name="app">The web application instance</param>
    /// <param name="ex">The exception that occurred</param>
    /// <param name="attempt">Current attempt number</param>
    /// <param name="maxRetries">Maximum number of retries</param>
    /// <param name="delayMs">Delay in milliseconds between retries</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private static async Task HandleDatabaseInitializationError(WebApplication app, Exception ex, int attempt, int maxRetries, int delayMs)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        if (attempt == maxRetries)
        {
            logger.LogCritical(ex, "Database initialization failed after {MaxRetries} attempts. Application will exit.", maxRetries);
            throw ex; // Re-throw on final attempt
        }

        logger.LogWarning(ex, "Database initialization attempt {Attempt}/{MaxRetries} failed. Retrying in {DelayMs}ms...", 
            attempt, maxRetries, delayMs);
        
        await Task.Delay(delayMs);
    }
}
