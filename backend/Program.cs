using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using backend.Data;
using Microsoft.EntityFrameworkCore;

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

        // Seed the database with initial data in development environment
        if (app.Environment.IsDevelopment())
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RqmtMgmtDbContext>();
                await backend.Data.DatabaseSeeder.SeedAsync(context, includeTestData: true);
            }
        }

        // Seed the database with test data in testing environment
        if (app.Environment.IsEnvironment("Testing"))
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RqmtMgmtDbContext>();
                await backend.Data.DatabaseSeeder.SeedAsync(context, includeTestData: true);
            }
        }

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
        
        // Global error handling endpoint
        app.Map("/error", (HttpContext context) =>
        {
            return Results.Problem("An unexpected error occurred. Please contact support if the issue persists.");
        });

        await app.RunAsync();
    }
}