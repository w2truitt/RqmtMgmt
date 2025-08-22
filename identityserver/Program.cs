using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using identityserver.Data;
using identityserver.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace identityserver;

/// <summary>
/// Main program class for the Requirements Management System Identity Server.
/// Configures Duende IdentityServer with ASP.NET Core Identity for user management.
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point for the Identity Server application.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Log.Information("Starting up Identity Server");

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((ctx, lc) => lc
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(ctx.Configuration));

            var app = builder
                .ConfigureServices()
                .ConfigurePipeline();
            
            // Initialize the database
            await InitializeDatabaseAsync(app);
            
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception");
        }
        finally
        {
            Log.Information("Shut down complete");
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Initializes the database with test users.
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private static async Task InitializeDatabaseAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        
        try
        {
            // Initialize ASP.NET Identity database
            var identityContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Only migrate if using SQL Server
            if (identityContext.Database.IsSqlServer())
            {
                await identityContext.Database.MigrateAsync();
                Log.Information("Database migrations applied successfully");
            }
            else
            {
                // For in-memory database, ensure it's created
                await identityContext.Database.EnsureCreatedAsync();
                Log.Information("In-memory database created successfully");
            }
            
            // Seed test users
            await SeedUsersAsync(scope.ServiceProvider);
            
            Log.Information("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while initializing the database");
            // Don't throw - let the application start anyway for development
        }
    }

    /// <summary>
    /// Seeds test users into the ASP.NET Identity database.
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private static async Task SeedUsersAsync(IServiceProvider serviceProvider)
    {
        try
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Create roles
            var roles = new[] { "Administrator", "ProjectManager", "Developer", "Tester", "Viewer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Log.Information("Created role: {Role}", role);
                }
            }

            // Create test users
            var testUsers = new[]
            {
                new { Email = "admin@rqmtmgmt.local", Password = "Admin123!", Role = "Administrator", Name = "System Administrator" },
                new { Email = "pm@rqmtmgmt.local", Password = "PM123!", Role = "ProjectManager", Name = "Project Manager" },
                new { Email = "dev@rqmtmgmt.local", Password = "Dev123!", Role = "Developer", Name = "Developer" },
                new { Email = "tester@rqmtmgmt.local", Password = "Test123!", Role = "Tester", Name = "Quality Tester" },
                new { Email = "viewer@rqmtmgmt.local", Password = "View123!", Role = "Viewer", Name = "Requirements Viewer" }
            };

            foreach (var testUser in testUsers)
            {
                var existingUser = await userManager.FindByEmailAsync(testUser.Email);
                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = testUser.Email,
                        Email = testUser.Email,
                        EmailConfirmed = true,
                        Name = testUser.Name
                    };

                    var result = await userManager.CreateAsync(user, testUser.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, testUser.Role);
                        Log.Information("Created test user: {Email} with role: {Role}", testUser.Email, testUser.Role);
                    }
                    else
                    {
                        Log.Warning("Failed to create user {Email}: {Errors}", testUser.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while seeding users");
        }
    }
}