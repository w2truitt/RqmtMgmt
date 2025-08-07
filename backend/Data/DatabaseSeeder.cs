using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using RqmtMgmtShared;

namespace backend.Data
{
    /// <summary>
    /// Database seeding utility for initial data setup and development/testing scenarios.
    /// </summary>
    public static class DatabaseSeeder
    {
        /// <summary>
        /// Seeds the database with initial data including roles, admin user, and sample data.
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="includeTestData">Whether to include sample test data</param>
        public static async Task SeedAsync(RqmtMgmtDbContext context, bool includeTestData = false)
        {
            // Apply any pending migrations only for relational databases (not InMemory)
            if (context.Database.IsRelational())
            {
                await context.Database.MigrateAsync();
            }
            else
            {
                // For InMemory databases, ensure the database is created
                await context.Database.EnsureCreatedAsync();
            }

            // Seed Roles
            if (!await context.Roles.AnyAsync())
            {
                var roles = new[]
                {
                    new Role { Name = "Administrator" },
                    new Role { Name = "Product Owner" },
                    new Role { Name = "Engineer" },
                    new Role { Name = "Quality Assurance" },
                    new Role { Name = "Viewer" }
                };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            // Seed Admin User
            if (!await context.Users.AnyAsync())
            {
                var adminUser = new User
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    CreatedAt = DateTime.UtcNow
                };

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();

                // Assign admin role
                var adminRole = await context.Roles.FirstAsync(r => r.Name == "Administrator");
                var userRole = new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                };

                await context.UserRoles.AddAsync(userRole);
                await context.SaveChangesAsync();
            }

            // Seed sample data for development/testing
            if (includeTestData && !await context.Requirements.AnyAsync())
            {
                await SeedTestDataAsync(context);
            }
        }

        /// <summary>
        /// Seeds sample test data for development and testing purposes.
        /// </summary>
        private static async Task SeedTestDataAsync(RqmtMgmtDbContext context)
        {
            var adminUser = await context.Users.FirstAsync();

            // Sample Requirements
            var customerReq = new Requirement
            {
                Type = RequirementType.CRS,
                Title = "User Authentication System",
                Description = "The system shall provide secure user authentication using OAuth 2.0",
                Status = RequirementStatus.Approved,
                Version = 1,
                CreatedBy = adminUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            var productReq = new Requirement
            {
                Type = RequirementType.PRS,
                Title = "Login Page Design",
                Description = "The login page shall have a modern, responsive design",
                Status = RequirementStatus.Draft,
                Version = 1,
                CreatedBy = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                Parent = customerReq
            };

            var softwareReq = new Requirement
            {
                Type = RequirementType.SRS,
                Title = "JWT Token Implementation",
                Description = "Implement JWT token-based authentication with 24-hour expiry",
                Status = RequirementStatus.Implemented,
                Version = 1,
                CreatedBy = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                Parent = productReq
            };

            await context.Requirements.AddRangeAsync(customerReq, productReq, softwareReq);
            await context.SaveChangesAsync();

            // Sample Test Suite
            var testSuite = new TestSuite
            {
                Name = "Authentication Test Suite",
                Description = "Tests for user authentication functionality",
                CreatedBy = adminUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            await context.TestSuites.AddAsync(testSuite);
            await context.SaveChangesAsync();

            // Sample Test Cases
            var loginTestCase = new TestCase
            {
                Title = "Valid User Login",
                Description = "Test successful login with valid credentials",
                SuiteId = testSuite.Id,
                CreatedBy = adminUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            var invalidLoginTestCase = new TestCase
            {
                Title = "Invalid User Login",
                Description = "Test login failure with invalid credentials",
                SuiteId = testSuite.Id,
                CreatedBy = adminUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            await context.TestCases.AddRangeAsync(loginTestCase, invalidLoginTestCase);
            await context.SaveChangesAsync();

            // Sample Test Steps
            var loginSteps = new[]
            {
                new TestStep
                {
                    TestCaseId = loginTestCase.Id,
                    Description = "Navigate to login page",
                    ExpectedResult = "Login page is displayed"
                },
                new TestStep
                {
                    TestCaseId = loginTestCase.Id,
                    Description = "Enter valid username and password",
                    ExpectedResult = "Credentials are accepted"
                },
                new TestStep
                {
                    TestCaseId = loginTestCase.Id,
                    Description = "Click login button",
                    ExpectedResult = "User is redirected to dashboard"
                }
            };

            await context.TestSteps.AddRangeAsync(loginSteps);
            await context.SaveChangesAsync();

            // Sample Test Plan
            var testPlan = new TestPlan
            {
                Name = "Authentication Test Plan",
                Description = "Test plan for authentication features",
                Type = TestPlanType.UserValidation,
                CreatedBy = adminUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            await context.TestPlans.AddAsync(testPlan);
            await context.SaveChangesAsync();

            // Link test cases to test plan
            var testPlanLinks = new[]
            {
                new TestPlanTestCase { TestPlanId = testPlan.Id, TestCaseId = loginTestCase.Id },
                new TestPlanTestCase { TestPlanId = testPlan.Id, TestCaseId = invalidLoginTestCase.Id }
            };

            await context.TestPlanTestCases.AddRangeAsync(testPlanLinks);
            await context.SaveChangesAsync();

            // Link requirements to test cases
            var reqTestLinks = new[]
            {
                new RequirementTestCaseLink { RequirementId = customerReq.Id, TestCaseId = loginTestCase.Id },
                new RequirementTestCaseLink { RequirementId = productReq.Id, TestCaseId = loginTestCase.Id },
                new RequirementTestCaseLink { RequirementId = softwareReq.Id, TestCaseId = invalidLoginTestCase.Id }
            };

            await context.RequirementTestCaseLinks.AddRangeAsync(reqTestLinks);
            await context.SaveChangesAsync();

            // Sample Test Run
            var testRun = new TestRun
            {
                TestCaseId = loginTestCase.Id,
                TestPlanId = testPlan.Id,
                Result = TestResult.Passed,
                RunBy = adminUser.Id,
                RunAt = DateTime.UtcNow,
                Notes = "Test passed successfully on first attempt"
            };

            await context.TestRuns.AddAsync(testRun);
            await context.SaveChangesAsync();

            // Sample Audit Log
            var auditLog = new AuditLog
            {
                UserId = adminUser.Id,
                Action = "CREATE",
                Entity = "Requirement",
                EntityId = customerReq.Id,
                Details = "Created customer requirement for user authentication",
                Timestamp = DateTime.UtcNow
            };

            await context.AuditLogs.AddAsync(auditLog);
            await context.SaveChangesAsync();
        }
    }
}