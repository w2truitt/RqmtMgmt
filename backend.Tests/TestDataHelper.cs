using System;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using RqmtMgmtShared;

namespace backend.Tests
{
    /// <summary>
    /// Helper class for setting up test data with project segmentation
    /// </summary>
    public static class TestDataHelper
    {
        /// <summary>
        /// Creates a test database context with a unique name
        /// </summary>
        public static RqmtMgmtDbContext GetDbContext(string testName)
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{testName}_{Guid.NewGuid()}")
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        /// <summary>
        /// Sets up basic test data including a user and project
        /// </summary>
        public static async System.Threading.Tasks.Task<(User user, Project project)> SetupBasicTestDataAsync(RqmtMgmtDbContext db)
        {
            // Create a test user
            var user = new User
            {
                UserName = "testuser",
                Email = "test@example.com",
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            // Create a test project
            var project = new Project
            {
                Name = "Test Project",
                Code = "TEST",
                Description = "Test project for unit tests",
                Status = ProjectStatus.Active,
                OwnerId = user.Id,
                CreatedAt = DateTime.UtcNow
            };
            db.Projects.Add(project);
            await db.SaveChangesAsync();

            return (user, project);
        }

        /// <summary>
        /// Creates a test requirement with project association
        /// </summary>
        public static Requirement CreateTestRequirement(int projectId, int createdBy, string title = "Test Requirement")
        {
            return new Requirement
            {
                Title = title,
                Type = RequirementType.CRS,
                Status = RequirementStatus.Draft,
                ProjectId = projectId,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a test requirement DTO with project association
        /// </summary>
        public static RequirementDto CreateTestRequirementDto(int projectId, int createdBy, string title = "Test Requirement")
        {
            return new RequirementDto
            {
                Title = title,
                Type = RequirementType.CRS,
                Status = RequirementStatus.Draft,
                ProjectId = projectId,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a test suite with project association
        /// </summary>
        public static TestSuite CreateTestSuite(int projectId, int createdBy, string name = "Test Suite")
        {
            return new TestSuite
            {
                Name = name,
                Description = "Test suite description",
                ProjectId = projectId,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a test suite DTO with project association
        /// </summary>
        public static TestSuiteDto CreateTestSuiteDto(int projectId, int createdBy, string name = "Test Suite")
        {
            return new TestSuiteDto
            {
                Name = name,
                Description = "Test suite description",
                ProjectId = projectId,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a test plan with project association
        /// </summary>
        public static TestPlan CreateTestPlan(int projectId, int createdBy, string name = "Test Plan")
        {
            return new TestPlan
            {
                Name = name,
                Type = TestPlanType.SoftwareVerification,
                Description = "Test plan description",
                ProjectId = projectId,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a test plan DTO with project association
        /// </summary>
        public static TestPlanDto CreateTestPlanDto(int projectId, int createdBy, string name = "Test Plan")
        {
            return new TestPlanDto
            {
                Name = name,
                Type = "SoftwareVerification",
                Description = "Test plan description",
                ProjectId = projectId,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}