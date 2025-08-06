using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using backend.Data;
using backend.Models;
using RqmtMgmtShared;
using System.Text.Json;

namespace backend.ApiTests
{
    public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<RqmtMgmtDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database for testing
                services.AddDbContext<RqmtMgmtDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                    options.EnableSensitiveDataLogging();
                });

                // Create the database and seed test data
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<RqmtMgmtDbContext>();
                    var logger = scopedServices.GetRequiredService<ILogger<TestWebApplicationFactory<TStartup>>>();

                    db.Database.EnsureCreated();

                    try
                    {
                        SeedTestData(db);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred seeding the database with test messages. Error: {Message}", ex.Message);
                    }
                }
            });

            builder.UseEnvironment("Testing");
        }

        private static void SeedTestData(RqmtMgmtDbContext context)
        {
            // Clear any existing data
            context.Users.RemoveRange(context.Users);
            context.Roles.RemoveRange(context.Roles);
            context.Requirements.RemoveRange(context.Requirements);
            context.TestCases.RemoveRange(context.TestCases);
            context.TestSuites.RemoveRange(context.TestSuites);
            context.TestPlans.RemoveRange(context.TestPlans);
            context.SaveChanges();

            // Add test users
            var testUser = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com"
            };
            context.Users.Add(testUser);

            // Add test roles
            var adminRole = new Role
            {
                Id = 1,
                Name = "Admin"
            };
            context.Roles.Add(adminRole);

            // Add test requirement
            var testRequirement = new Requirement
            {
                Id = 1,
                Type = RequirementType.CRS,
                Title = "Test Requirement",
                Description = "Test Description",
                Status = RequirementStatus.Draft,
                Version = 1,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            context.Requirements.Add(testRequirement);

            // Add test suite
            var testSuite = new TestSuite
            {
                Id = 1,
                Name = "Test Suite",
                Description = "Test Suite Description",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            context.TestSuites.Add(testSuite);

            // Add test case
            var testCase = new TestCase
            {
                Id = 1,
                Title = "Test Case",
                Description = "Test Case Description",
                SuiteId = 1,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            context.TestCases.Add(testCase);

            // Add test plan
            var testPlan = new TestPlan
            {
                Id = 1,
                Name = "Test Plan",
                Description = "Test Plan Description",
                Type = TestPlanType.UserValidation,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            context.TestPlans.Add(testPlan);

            context.SaveChanges();
        }
    }
}