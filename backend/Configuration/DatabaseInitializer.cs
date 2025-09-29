using Microsoft.EntityFrameworkCore;
using backend.Data;

namespace backend.Configuration
{
    /// <summary>
    /// Handles database initialization with retry logic for Docker container startup scenarios.
    /// Provides robust database setup with proper error handling and environment-specific behavior.
    /// </summary>
    public static class DatabaseInitializer
    {
        private const int MaxRetries = 10;
        private const int DelayMs = 3000; // 3 seconds between retries

        /// <summary>
        /// Initializes the database with retry logic to handle Docker container startup timing issues.
        /// Applies migrations and seeds data based on the current environment.
        /// </summary>
        /// <param name="app">The web application instance.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task InitializeWithRetryAsync(WebApplication app)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    await InitializeDatabaseAsync(app, attempt);
                    return; // Success - exit retry loop
                }
                catch (Exception ex)
                {
                    await HandleInitializationError(app, ex, attempt);
                }
            }
        }

        /// <summary>
        /// Performs the actual database initialization logic.
        /// Tests connectivity, applies migrations, and seeds data based on environment.
        /// </summary>
        /// <param name="app">The web application instance.</param>
        /// <param name="attempt">Current attempt number for logging purposes.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task InitializeDatabaseAsync(WebApplication app, int attempt)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RqmtMgmtDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Database initialization attempt {Attempt}/{MaxRetries}", attempt, MaxRetries);

            // Test database connectivity first
            await context.Database.CanConnectAsync();
            logger.LogInformation("Database connection established successfully");

            // Apply migrations and seed data based on environment
            await InitializeByEnvironment(app, context, logger);
            
            logger.LogInformation("Database initialization completed successfully");
        }

        /// <summary>
        /// Initializes the database based on the current environment.
        /// Development: Applies migrations and seeds test data
        /// Testing: Creates in-memory database and seeds test data
        /// Production: Only applies migrations, no test data
        /// </summary>
        /// <param name="app">The web application instance.</param>
        /// <param name="context">The main database context.</param>
        /// <param name="logger">The logger instance.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task InitializeByEnvironment(
            WebApplication app, 
            RqmtMgmtDbContext context, 
            ILogger<Program> logger)
        {
            if (app.Environment.IsDevelopment())
            {
                // Apply main database migrations and seed development data
                if (!app.Environment.IsEnvironment("Testing"))
                {
                    await context.Database.MigrateAsync();
                }
                
                // Only seed if database is empty (no users exist)
                if (!await context.Users.AnyAsync())
                {
                    await backend.Data.DatabaseSeeder.SeedAsync(context, includeTestData: true);
                    logger.LogInformation("Database seeded with development data successfully");
                }
                else
                {
                    logger.LogInformation("Database already contains data, skipping seeding");
                }
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
        /// Logs warnings for retryable attempts and throws critical errors on final failure.
        /// </summary>
        /// <param name="app">The web application instance.</param>
        /// <param name="ex">The exception that occurred.</param>
        /// <param name="attempt">Current attempt number.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task HandleInitializationError(WebApplication app, Exception ex, int attempt)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            
            if (attempt == MaxRetries)
            {
                logger.LogCritical(ex, "Database initialization failed after {MaxRetries} attempts. Application will exit.", MaxRetries);
                throw ex; // Re-throw on final attempt
            }

            logger.LogWarning(ex, "Database initialization attempt {Attempt}/{MaxRetries} failed. Retrying in {DelayMs}ms...", 
                attempt, MaxRetries, DelayMs);
            
            await Task.Delay(DelayMs);
        }
    }
}