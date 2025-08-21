using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using backend.Data;
using backend.Models;
using RqmtMgmtShared;
using System.Text.Json;

namespace backend.ApiTests
{
    public class TestAuthorizationPolicyProvider : IAuthorizationPolicyProvider
    {
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return Task.FromResult(new AuthorizationPolicyBuilder().RequireAssertion(_ => true).Build());
        }

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            return Task.FromResult<AuthorizationPolicy?>(new AuthorizationPolicyBuilder().RequireAssertion(_ => true).Build());
        }

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return Task.FromResult<AuthorizationPolicy?>(new AuthorizationPolicyBuilder().RequireAssertion(_ => true).Build());
        }
    }

    public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private static int _databaseCounter = 0;
        private readonly string _databaseName;
        
        public TestWebApplicationFactory()
        {
            _databaseName = $"TestDatabase_{Interlocked.Increment(ref _databaseCounter)}_{Guid.NewGuid():N}";
        }

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
                    options.UseInMemoryDatabase(_databaseName);
                    options.EnableSensitiveDataLogging();
                });

                // Replace authorization policy provider to bypass authentication in tests
                var authPolicyDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IAuthorizationPolicyProvider));
                if (authPolicyDescriptor != null)
                    services.Remove(authPolicyDescriptor);
                
                services.AddSingleton<IAuthorizationPolicyProvider, TestAuthorizationPolicyProvider>();

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
            // Since we're using a unique database per factory instance, no need to clear data
            // Just ensure we don't have any tracked entities
            context.ChangeTracker.Clear();

            // Add test users
            var testUser = new User
            {
                UserName = "testuser",
                Email = "test@example.com"
            };
            context.Users.Add(testUser);

            // Add test roles
            var adminRole = new Role
            {
                Name = "Admin"
            };
            context.Roles.Add(adminRole);

            // Save users and roles first to get their IDs
            context.SaveChanges();

            // Add test requirement
            var testRequirement = new Requirement
            {
                Type = RequirementType.CRS,
                Title = "Test Requirement",
                Description = "Test Description",
                Status = RequirementStatus.Draft,
                Version = 1,
                CreatedBy = testUser.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Requirements.Add(testRequirement);

            // Add test suite
            var testSuite = new TestSuite
            {
                Name = "Test Suite",
                Description = "Test Suite Description",
                CreatedBy = testUser.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.TestSuites.Add(testSuite);

            // Save requirements and suites to get their IDs
            context.SaveChanges();

            // Add test case
            var testCase = new TestCase
            {
                Title = "Test Case",
                Description = "Test Case Description",
                SuiteId = testSuite.Id,
                CreatedBy = testUser.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.TestCases.Add(testCase);

            // Add test plan
            var testPlan = new TestPlan
            {
                Name = "Test Plan",
                Description = "Test Plan Description",
                Type = TestPlanType.UserValidation,
                CreatedBy = testUser.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.TestPlans.Add(testPlan);

            context.SaveChanges();
            
            // Clear the change tracker after seeding to prevent conflicts
            context.ChangeTracker.Clear();
        }
    }
}