using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;
using backend.Data;
using backend.Services;
using RqmtMgmtShared;

namespace backend.Configuration
{
    /// <summary>
    /// Extension methods for IServiceCollection to organize service registration and configuration.
    /// Helps reduce complexity in Program.cs by separating concerns into focused methods.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures database contexts based on the current environment.
        /// Uses InMemory database for testing, SQL Server for development and production.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configuration">Application configuration containing connection strings.</param>
        /// <param name="environment">The current hosting environment.</param>
        /// <returns>The configured service collection for method chaining.</returns>
        public static IServiceCollection AddDatabaseServices(
            this IServiceCollection services, 
            IConfiguration configuration, 
            IWebHostEnvironment environment)
        {
            if (environment.IsEnvironment("Testing"))
            {
                // Use InMemory database for testing to avoid conflicts and ensure isolation
                var testDbName = $"TestDb_{Guid.NewGuid()}";
                services.AddDbContext<RqmtMgmtDbContext>(options =>
                    options.UseInMemoryDatabase(testDbName));
            }
            else
            {
                // Use SQL Server for development and production environments
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<RqmtMgmtDbContext>(options =>
                    options.UseSqlServer(connectionString));
            }

            return services;
        }

        /// <summary>
        /// Configures JWT Bearer authentication for API protection with IdentityServer integration.
        /// Sets up token validation parameters and event handlers for debugging.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configuration">Application configuration containing authentication settings.</param>
        /// <returns>The configured service collection for method chaining.</returns>
        public static IServiceCollection AddAuthenticationServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Disable default JWT claim mapping to preserve original claim names
            Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();
            System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // Add JWT Bearer authentication for API protection
            var identityServerUrl = configuration["Authentication:Authority"] ?? "https://rqmtmgmt.local";
            
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = identityServerUrl;
                    options.RequireHttpsMetadata = false; // Allow HTTP for development, but use HTTPS authority
                    
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.FromMinutes(5),

                        // Explicitly set the valid issuer to match IdentityServer
                        ValidIssuer = identityServerUrl,
                        
                        // Configure multiple valid audiences to handle different token formats
                        ValidAudiences = new[] { 
                            "rqmtapi", 
                            "rqmtmgmt-api", 
                            "rqmtmgmt.api" 
                        },
                        
                        // Configure claim mapping for proper user identity extraction
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                    
                    // Add event handlers for debugging token validation
                    options.Events = CreateJwtBearerEvents();
                });

            // Add authorization services (CRITICAL: Required for RequireAuthorization() to work)
            services.AddAuthorization();

            return services;
        }

        /// <summary>
        /// Configures CORS policy to allow frontend connections from various development ports.
        /// Supports Docker containers, local development, and E2E testing scenarios.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The configured service collection for method chaining.</returns>
        public static IServiceCollection AddCorsServices(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(
                        "http://localhost:80",        // Docker nginx proxy
                        "https://localhost:443",      // Docker nginx proxy HTTPS
                        "https://localhost:7160", 
                        "http://localhost:5239", 
                        "https://localhost:5001", 
                        "http://localhost:5000",
                        "http://localhost:5001",      // Frontend container
                        "http://frontend:5001",       // Internal container communication
                        "http://rqmtmgmt.local:80",   // E2E test HTTP access
                        "https://rqmtmgmt.local:443"  // E2E test HTTPS access
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            return services;
        }

        /// <summary>
        /// Configures Swagger/OpenAPI documentation with OAuth2 authentication support.
        /// Sets up IdentityServer integration for API testing through Swagger UI.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The configured service collection for method chaining.</returns>
        public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "Requirements Management API", 
                    Version = "v1",
                    Description = "API for Requirements Management System with OIDC authentication"
                });

                // Add OAuth2 authentication to Swagger
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri("https://rqmtmgmt.local/connect/authorize"),
                            TokenUrl = new Uri("https://rqmtmgmt.local/connect/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                ["openid"] = "OpenID Connect",
                                ["profile"] = "User profile",
                                ["rqmtmgmt.api"] = "Requirements Management API"
                            }
                        }
                    }
                });

                // Add security requirement
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            }
                        },
                        new string[] { "openid", "profile", "rqmtmgmt.api" }
                    }
                });
            });

            return services;
        }

        /// <summary>
        /// Registers all business services for dependency injection.
        /// Centralizes service registration to maintain consistency and avoid duplication.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The configured service collection for method chaining.</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register all business services for dependency injection
            services.AddScoped<RqmtMgmtShared.IRequirementService, RequirementService>();
            services.AddScoped<RqmtMgmtShared.ITestCaseService, TestCaseService>();
            services.AddScoped<RqmtMgmtShared.ITestPlanService, TestPlanService>();
            services.AddScoped<RqmtMgmtShared.ITestSuiteService, TestSuiteService>();
            services.AddScoped<RqmtMgmtShared.IUserService, UserService>();
            services.AddScoped<backend.Services.IRedlineService, RedlineService>();
            services.AddScoped<RqmtMgmtShared.IRequirementTestCaseLinkService, RequirementTestCaseLinkService>();
            services.AddScoped<RqmtMgmtShared.IRoleService, RoleService>();
            services.AddScoped<RqmtMgmtShared.IDashboardService, DashboardService>();
            services.AddScoped<RqmtMgmtShared.IEnhancedDashboardService, EnhancedDashboardService>();
            services.AddScoped<RqmtMgmtShared.ITestRunSessionService, TestRunSessionService>();
            services.AddScoped<RqmtMgmtShared.ITestExecutionService, TestExecutionService>();
            services.AddScoped<RqmtMgmtShared.IProjectService, ProjectService>();

            return services;
        }

        /// <summary>
        /// Configures MVC controllers with JSON serialization options.
        /// Sets up enum string conversion and API explorer for Swagger.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The configured service collection for method chaining.</returns>
        public static IServiceCollection AddControllerServices(this IServiceCollection services)
        {
            // Add MVC controllers with JSON enum string conversion
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });
            
            services.AddEndpointsApiExplorer();

            return services;
        }

        /// <summary>
        /// Configures forwarded headers for proxy scenarios (Docker, load balancers).
        /// Ensures proper handling of X-Forwarded-For and X-Forwarded-Proto headers.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The configured service collection for method chaining.</returns>
        public static IServiceCollection AddProxyServices(this IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            return services;
        }

        /// <summary>
        /// Creates JWT Bearer event handlers for debugging and logging token validation.
        /// Helps troubleshoot authentication issues in development and testing.
        /// </summary>
        /// <returns>Configured JWT Bearer events.</returns>
        private static JwtBearerEvents CreateJwtBearerEvents()
        {
            return new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
                    {
                        var token = authHeader.FirstOrDefault()?.Split(" ").Last();
                        logger.LogDebug("Authorization header found. Token present: {TokenPresent}", !string.IsNullOrEmpty(token));
                    }
                    else
                    {
                        logger.LogWarning("Authorization header is MISSING from the request.");
                    }
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    var claims = context.Principal?.Claims?.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>();
                    logger.LogDebug("Token validated successfully. Claims: {Claims}", string.Join(", ", claims));
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(context.Exception, "JWT authentication failed: {Error}", context.Exception.Message);
                    return Task.CompletedTask;
                }
            };
        }
    }
}