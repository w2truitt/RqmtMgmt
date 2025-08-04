using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using backend.Data;
using Microsoft.EntityFrameworkCore;


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

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Register all services for DI
builder.Services.AddScoped<backend.Services.IRequirementService, backend.Services.RequirementService>();
builder.Services.AddScoped<backend.Services.ITestCaseService, backend.Services.TestCaseService>();
builder.Services.AddScoped<backend.Services.ITestPlanService, backend.Services.TestPlanService>();
builder.Services.AddScoped<backend.Services.ITestSuiteService, backend.Services.TestSuiteService>();
builder.Services.AddScoped<backend.Services.IUserService, backend.Services.UserService>();
builder.Services.AddScoped<backend.Services.IRedlineService, backend.Services.RedlineService>();
builder.Services.AddScoped<backend.Services.IRequirementTestCaseLinkService, backend.Services.RequirementTestCaseLinkService>();
builder.Services.AddScoped<backend.Services.IRoleService, backend.Services.RoleService>();
// Register DbContext with connection string from configuration
builder.Services.AddDbContext<RqmtMgmtDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add our DbContext and DI registrations later


var app = builder.Build();


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

app.Run();
