using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using backend.Models;
using backend.Configuration;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

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

        // Configure database contexts based on environment
        if (builder.Environment.IsEnvironment("Testing"))
        {
            // Use InMemory database for testing to avoid conflicts and ensure isolation
            var testDbName = $"TestDb_{Guid.NewGuid()}";
            builder.Services.AddDbContext<RqmtMgmtDbContext>(options =>
                options.UseInMemoryDatabase(testDbName));
        }
        else
        {
            // Use SQL Server for development and production environments
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<RqmtMgmtDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

        // Add JWT Bearer authentication for API protection
        var identityServerUrl = builder.Configuration["Authentication:Authority"] ?? "http://localhost:5002";
        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = identityServerUrl;
                options.Audience = builder.Configuration["Authentication:Audience"] ?? "rqmtapi";
                options.RequireHttpsMetadata = false; // Allow HTTP for development
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });

        // Configure forwarded headers for proxy scenarios
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | 
                                       Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        // Configure CORS policy to allow frontend connections from various development ports
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(
                    "http://localhost:80",  // Docker nginx proxy
                    "https://localhost:443",  // Docker nginx proxy HTTPS
                    "https://localhost:7160", 
                    "http://localhost:5239", 
                    "https://localhost:5001", 
                    "http://localhost:5000",
                    "http://localhost:5001",  // Frontend container
                    "http://frontend:5001",   // Internal container communication
                    "http://rqmtmgmt.local:80",  // E2E test HTTP access
                    "https://rqmtmgmt.local:443"  // E2E test HTTPS access
                )
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
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
            { 
                Title = "Requirements Management API", 
                Version = "v1",
                Description = "API for Requirements Management System with JWT Bearer authentication"
            });

            // Add JWT Bearer authentication to Swagger
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme."
            });

            // Add security requirement
            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

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
        
        var app = builder.Build();

        // Initialize database with retry logic for Docker environments
        await InitializeDatabaseWithRetryAsync(app);

        // Configure middleware pipeline based on environment
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Requirements Management API v1");
                c.OAuthClientId("rqmtmgmt-wasm");
                c.OAuthAppName("Requirements Management API");
                c.OAuthScopes("openid", "profile", "email", "rqmtapi");
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
    /// <param name="context">The main database context</param>
    /// <param name="logger">The logger instance</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private static async Task InitializeDatabaseByEnvironment(WebApplication app, RqmtMgmtDbContext context, ILogger<Program> logger)
    {
        if (app.Environment.IsDevelopment())
        {
            // Apply main database migrations and seed development data
            if (!app.Environment.IsEnvironment("Testing"))
            {
                await context.Database.MigrateAsync();
            }
            
            await backend.Data.DatabaseSeeder.SeedAsync(context, includeTestData: true);
            logger.LogInformation("Database seeded with development data successfully");
        }
        else if (app.Environment.IsEnvironment("Testing"))
        {
            // For testing, ensure databases are created (in-memory)
            await context.Database.EnsureCreatedAsync();
            
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
