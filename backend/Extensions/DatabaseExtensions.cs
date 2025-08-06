using backend.Data;

namespace backend.Extensions
{
    /// <summary>
    /// Extension methods for database initialization and seeding.
    /// </summary>
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Initializes the database with migrations and optional seeding.
        /// </summary>
        /// <param name="app">The web application</param>
        /// <param name="seedTestData">Whether to include test data for development</param>
        /// <returns>The web application for chaining</returns>
        public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app, bool seedTestData = false)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RqmtMgmtDbContext>();
            
            try
            {
                // Apply any pending migrations
                await context.Database.EnsureCreatedAsync();
                
                // Seed initial data
                await DatabaseSeeder.SeedAsync(context, seedTestData);
                
                app.Logger.LogInformation("Database initialized successfully");
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "An error occurred while initializing the database");
                throw;
            }

            return app;
        }
    }
}