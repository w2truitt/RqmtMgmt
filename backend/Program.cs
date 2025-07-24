using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add our DbContext and DI registrations later

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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

app.Run();
