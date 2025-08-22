using Duende.IdentityServer;
using identityserver.Data;
using identityserver.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace identityserver;

/// <summary>
/// Extension methods for configuring services and pipeline for the Identity Server.
/// </summary>
internal static class HostingExtensions
{
    /// <summary>
    /// Configures all services required for the Identity Server.
    /// </summary>
    /// <param name="builder">The web application builder</param>
    /// <returns>The configured web application builder</returns>
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        // Configure database contexts
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        
        if (builder.Environment.IsEnvironment("Testing") || string.IsNullOrEmpty(connectionString))
        {
            // Use InMemory databases for testing or when no connection string
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase($"IdentityDb_{Guid.NewGuid()}"));
        }
        else
        {
            // Use SQL Server for development and production
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));
        }

        // Configure ASP.NET Core Identity
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Configure password requirements for development
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Configure IdentityServer
        var identityServerBuilder = builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // Configure issuer URI for Docker environment (port 80)
                if (builder.Environment.IsDevelopment())
                {
                    options.IssuerUri = "http://localhost";
                }
                
                // Disable automatic key management for development
                options.KeyManagement.Enabled = false;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryApiResources(Config.ApiResources)
            .AddInMemoryClients(Config.Clients)
            .AddAspNetIdentity<ApplicationUser>();

        // Add development signing credential
        if (builder.Environment.IsDevelopment())
        {
            identityServerBuilder.AddDeveloperSigningCredential();
        }
        else
        {
            // In production, you should use a proper certificate
            // identityServerBuilder.AddSigningCredential(cert);
            identityServerBuilder.AddDeveloperSigningCredential(); // Remove this in production
        }

        // Configure CORS for the frontend
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost", "http://localhost:5001", "https://localhost:5001")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // Add MVC for the UI
        builder.Services.AddRazorPages();

        return builder.Build();
    }

    /// <summary>
    /// Configures the HTTP request pipeline for the Identity Server.
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>The configured web application</returns>
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();

        app.UseRouting();
        app.UseCors("AllowFrontend");
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapRazorPages()
            .RequireAuthorization();

        return app;
    }
}