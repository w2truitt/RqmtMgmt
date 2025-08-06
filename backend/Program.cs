using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using backend.Data;
using Microsoft.EntityFrameworkCore;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add authentication (JWT Bearer for OAuth2/OIDC)
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

        // Add CORS policy to allow frontend connection
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

        // Add services
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        // Register all services for DI
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
        
        // Register DbContext with connection string from configuration
        builder.Services.AddDbContext<RqmtMgmtDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        var app = builder.Build();

        // Seed the database in development
        if (app.Environment.IsDevelopment())
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RqmtMgmtDbContext>();
                await backend.Data.DatabaseSeeder.SeedAsync(context, includeTestData: true);
            }
        }

        // Seed the database in testing environment
        if (app.Environment.IsEnvironment("Testing"))
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RqmtMgmtDbContext>();
                await backend.Data.DatabaseSeeder.SeedAsync(context, includeTestData: true);
            }
        }

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

        // Enable CORS
        app.UseCors("AllowFrontend");

        // Enable authentication and authorization middleware
        app.UseAuthentication();

        // Impersonation Middleware
        app.Use(async (context, next) =>
        {
            // Look for a header 'X-User-Id' for impersonation
            if (context.Request.Headers.TryGetValue("X-User-Id", out var userId))
            {
                context.Items["UserId"] = userId.ToString();
            }
            await next();
        });

        app.UseAuthorization();

        app.MapControllers();
        app.Map("/error", (HttpContext context) =>
        {
            return Results.Problem("An unexpected error occurred. Please contact support if the issue persists.");
        });

        await app.RunAsync();
    }
}