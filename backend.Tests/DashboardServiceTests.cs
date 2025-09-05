using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using RqmtMgmtShared;
using Xunit;

namespace backend.Tests
{
    public class DashboardServiceTests
    {
        private static RqmtMgmtDbContext GetDbContext(string testName)
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: $"DashboardServiceTestDb_{testName}_{System.Guid.NewGuid()}")
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        [Fact]
        public async Task GetStatisticsAsync_ReturnsCorrectStatistics_WithSampleData()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetStatisticsAsync_ReturnsCorrectStatistics_WithSampleData));
            await SeedTestData(db);
            var service = new DashboardService(db);

            // Act
            var result = await service.GetStatisticsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Requirements.Total); // Total requirements
            Assert.Equal(2, result.Requirements.Approved); // Approved requirements
            Assert.Equal(1, result.Requirements.Draft); // Draft requirements
            Assert.Equal(1, result.Requirements.Implemented); // Implemented requirements
            Assert.Equal(0, result.Requirements.Verified); // Verified requirements

            Assert.Equal(2, result.TestSuites.Total); // Total test suites
            Assert.Equal(2, result.TestSuites.Active); // Active test suites (all are considered active)
            Assert.Equal(0, result.TestSuites.Completed); // Completed test suites (placeholder)

            Assert.Equal(3, result.TestCases.Total); // Total test cases
            Assert.Equal(0, result.TestCases.Passed); // Passed test cases (placeholder)
            Assert.Equal(0, result.TestCases.Failed); // Failed test cases (placeholder)
            Assert.Equal(3, result.TestCases.NotRun); // Not run test cases (all for now)

            Assert.Equal(2, result.TestPlans.Total); // Total test plans
            Assert.Equal(0, result.TestPlans.ExecutionProgress); // Execution progress (placeholder)
            Assert.Equal(0, result.TestPlans.CoveragePercentage); // Coverage percentage (placeholder)
        }

        [Fact]
        public async Task GetStatisticsAsync_ReturnsZeroStatistics_WhenNoData()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetStatisticsAsync_ReturnsZeroStatistics_WhenNoData));
            var service = new DashboardService(db);

            // Act
            var result = await service.GetStatisticsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Requirements.Total);
            Assert.Equal(0, result.Requirements.Approved);
            Assert.Equal(0, result.Requirements.Draft);
            Assert.Equal(0, result.Requirements.Implemented);
            Assert.Equal(0, result.Requirements.Verified);

            Assert.Equal(0, result.TestSuites.Total);
            Assert.Equal(0, result.TestSuites.Active);
            Assert.Equal(0, result.TestSuites.Completed);

            Assert.Equal(0, result.TestCases.Total);
            Assert.Equal(0, result.TestCases.Passed);
            Assert.Equal(0, result.TestCases.Failed);
            Assert.Equal(0, result.TestCases.NotRun);

            Assert.Equal(0, result.TestPlans.Total);
            Assert.Equal(0, result.TestPlans.ExecutionProgress);
            Assert.Equal(0, result.TestPlans.CoveragePercentage);
        }

        [Fact]
        public async Task GetRecentActivityAsync_ReturnsRecentActivities_WithDefaultCount()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetRecentActivityAsync_ReturnsRecentActivities_WithDefaultCount));
            await SeedTestDataWithUsers(db);
            var service = new DashboardService(db);

            // Act
            var result = await service.GetRecentActivityAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count <= 5); // Default count is 5
            Assert.All(result, activity => 
            {
                Assert.NotNull(activity.Description);
                Assert.NotNull(activity.EntityType);
                Assert.NotNull(activity.Action);
                Assert.True(activity.UserId > 0);
            });
        }

        [Fact]
        public async Task GetRecentActivityAsync_ReturnsRecentActivities_WithCustomCount()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetRecentActivityAsync_ReturnsRecentActivities_WithCustomCount));
            await SeedTestDataWithUsers(db);
            var service = new DashboardService(db);

            // Act
            var result = await service.GetRecentActivityAsync(3);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count <= 3);
            Assert.All(result, activity => 
            {
                Assert.NotNull(activity.Description);
                Assert.NotNull(activity.EntityType);
                Assert.NotNull(activity.Action);
                Assert.True(activity.UserId > 0);
            });
        }

        [Fact]
        public async Task GetRecentActivityAsync_ReturnsEmpty_WhenNoData()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetRecentActivityAsync_ReturnsEmpty_WhenNoData));
            var service = new DashboardService(db);

            // Act
            var result = await service.GetRecentActivityAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetRecentActivityAsync_OrdersActivitiesByDate()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetRecentActivityAsync_OrdersActivitiesByDate));
            await SeedTestDataWithUsersAndDates(db);
            var service = new DashboardService(db);

            // Act
            var result = await service.GetRecentActivityAsync(10);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count > 1);
            
            // Verify activities are ordered by CreatedAt descending (most recent first)
            for (int i = 0; i < result.Count - 1; i++)
            {
                Assert.True(result[i].CreatedAt >= result[i + 1].CreatedAt);
            }
        }

        private async Task SeedTestData(RqmtMgmtDbContext db)
        {
            // Add test requirements with different statuses
            db.Requirements.AddRange(
                new Requirement { Id = 1, Title = "Req1", Type = RequirementType.CRS, Status = RequirementStatus.Approved, CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new Requirement { Id = 2, Title = "Req2", Type = RequirementType.PRS, Status = RequirementStatus.Approved, CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new Requirement { Id = 3, Title = "Req3", Type = RequirementType.SRS, Status = RequirementStatus.Draft, CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new Requirement { Id = 4, Title = "Req4", Type = RequirementType.CRS, Status = RequirementStatus.Implemented, CreatedBy = 1, CreatedAt = DateTime.UtcNow }
            );

            // Add test suites
            db.TestSuites.AddRange(
                new TestSuite { Id = 1, Name = "Suite1", Description = "Test Suite 1", CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new TestSuite { Id = 2, Name = "Suite2", Description = "Test Suite 2", CreatedBy = 1, CreatedAt = DateTime.UtcNow }
            );

            // Add test cases
            db.TestCases.AddRange(
                new TestCase { Id = 1, Title = "TestCase1", Description = "Test Case 1", SuiteId = 1, CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new TestCase { Id = 2, Title = "TestCase2", Description = "Test Case 2", SuiteId = 1, CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new TestCase { Id = 3, Title = "TestCase3", Description = "Test Case 3", SuiteId = 2, CreatedBy = 1, CreatedAt = DateTime.UtcNow }
            );

            // Add test plans
            db.TestPlans.AddRange(
                new TestPlan { Id = 1, Name = "Plan1", Description = "Test Plan 1", CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new TestPlan { Id = 2, Name = "Plan2", Description = "Test Plan 2", CreatedBy = 1, CreatedAt = DateTime.UtcNow }
            );

            await db.SaveChangesAsync();
        }

        private async Task SeedTestDataWithUsers(RqmtMgmtDbContext db)
        {
            // Add test users
            db.Users.AddRange(
                new User { Id = 1, UserName = "testuser1", Email = "test1@example.com", CreatedAt = DateTime.UtcNow },
                new User { Id = 2, UserName = "testuser2", Email = "test2@example.com", CreatedAt = DateTime.UtcNow }
            );

            // Add test requirements
            db.Requirements.AddRange(
                new Requirement { Id = 1, Title = "Req1", Type = RequirementType.CRS, Status = RequirementStatus.Draft, CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new Requirement { Id = 2, Title = "Req2", Type = RequirementType.PRS, Status = RequirementStatus.Approved, CreatedBy = 2, CreatedAt = DateTime.UtcNow.AddMinutes(-10) }
            );

            // Add test cases
            db.TestCases.AddRange(
                new TestCase { Id = 1, Title = "TestCase1", Description = "Test Case 1", SuiteId = 1, CreatedBy = 1, CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
                new TestCase { Id = 2, Title = "TestCase2", Description = "Test Case 2", SuiteId = 1, CreatedBy = 2, CreatedAt = DateTime.UtcNow.AddMinutes(-15) }
            );

            // Add test suites
            db.TestSuites.AddRange(
                new TestSuite { Id = 1, Name = "Suite1", Description = "Test Suite 1", CreatedBy = 1, CreatedAt = DateTime.UtcNow.AddMinutes(-20) }
            );

            // Add test plans
            db.TestPlans.AddRange(
                new TestPlan { Id = 1, Name = "Plan1", Description = "Test Plan 1", CreatedBy = 2, CreatedAt = DateTime.UtcNow.AddMinutes(-25) }
            );

            await db.SaveChangesAsync();
        }

        private async Task SeedTestDataWithUsersAndDates(RqmtMgmtDbContext db)
        {
            var now = DateTime.UtcNow;

            // Add test users
            db.Users.AddRange(
                new User { Id = 1, UserName = "testuser1", Email = "test1@example.com", CreatedAt = now },
                new User { Id = 2, UserName = "testuser2", Email = "test2@example.com", CreatedAt = now }
            );

            // Add test requirements with different creation times
            db.Requirements.AddRange(
                new Requirement { Id = 1, Title = "Latest Req", Type = RequirementType.CRS, Status = RequirementStatus.Draft, CreatedBy = 1, CreatedAt = now }, // Most recent
                new Requirement { Id = 2, Title = "Middle Req", Type = RequirementType.PRS, Status = RequirementStatus.Approved, CreatedBy = 2, CreatedAt = now.AddMinutes(-30) }, // Middle
                new Requirement { Id = 3, Title = "Oldest Req", Type = RequirementType.SRS, Status = RequirementStatus.Draft, CreatedBy = 1, CreatedAt = now.AddHours(-2) } // Oldest
            );

            // Add test cases with different creation times
            db.TestCases.AddRange(
                new TestCase { Id = 1, Title = "Recent TestCase", Description = "Recent Test Case", SuiteId = 1, CreatedBy = 2, CreatedAt = now.AddMinutes(-10) },
                new TestCase { Id = 2, Title = "Old TestCase", Description = "Old Test Case", SuiteId = 1, CreatedBy = 1, CreatedAt = now.AddHours(-1) }
            );

            // Add test suite
            db.TestSuites.AddRange(
                new TestSuite { Id = 1, Name = "Test Suite", Description = "Test Suite", CreatedBy = 1, CreatedAt = now.AddMinutes(-45) }
            );

            await db.SaveChangesAsync();
        }

        [Fact]
        public async Task GetRequirementStatsAsync_ReturnsCorrectCounts_WithVariousStatuses()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetRequirementStatsAsync_ReturnsCorrectCounts_WithVariousStatuses));
            var now = DateTime.UtcNow;
            
            db.Requirements.AddRange(
                new Requirement { Id = 1, Title = "Draft Req 1", Type = RequirementType.SRS, Status = RequirementStatus.Draft, CreatedBy = 1, CreatedAt = now },
                new Requirement { Id = 2, Title = "Draft Req 2", Type = RequirementType.PRS, Status = RequirementStatus.Draft, CreatedBy = 1, CreatedAt = now },
                new Requirement { Id = 3, Title = "Approved Req", Type = RequirementType.SRS, Status = RequirementStatus.Approved, CreatedBy = 1, CreatedAt = now },
                new Requirement { Id = 4, Title = "Implemented Req", Type = RequirementType.PRS, Status = RequirementStatus.Implemented, CreatedBy = 1, CreatedAt = now },
                new Requirement { Id = 5, Title = "Verified Req", Type = RequirementType.SRS, Status = RequirementStatus.Verified, CreatedBy = 1, CreatedAt = now }
            );
            await db.SaveChangesAsync();
            
            var service = new DashboardService(db);

            // Act
            var result = await service.GetRequirementStatsAsync();

            // Assert
            Assert.Equal(5, result.TotalRequirements);
            Assert.Equal(2, result.DraftRequirements);
            Assert.Equal(1, result.ApprovedRequirements);
            Assert.Equal(1, result.ImplementedRequirements);
            Assert.Equal(1, result.VerifiedRequirements);
            
            Assert.Equal(2, result.ByStatus[RequirementStatus.Draft]);
            Assert.Equal(1, result.ByStatus[RequirementStatus.Approved]);
            Assert.Equal(1, result.ByStatus[RequirementStatus.Implemented]);
            Assert.Equal(1, result.ByStatus[RequirementStatus.Verified]);
            
            Assert.Equal(3, result.ByType[RequirementType.SRS]);
            Assert.Equal(2, result.ByType[RequirementType.PRS]);
        }

        [Fact]
        public async Task GetRequirementStatsAsync_ReturnsZeroStats_WithEmptyDatabase()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetRequirementStatsAsync_ReturnsZeroStats_WithEmptyDatabase));
            var service = new DashboardService(db);

            // Act
            var result = await service.GetRequirementStatsAsync();

            // Assert
            Assert.Equal(0, result.TotalRequirements);
            Assert.Equal(0, result.DraftRequirements);
            Assert.Equal(0, result.ApprovedRequirements);
            Assert.Equal(0, result.ImplementedRequirements);
            Assert.Equal(0, result.VerifiedRequirements);
            Assert.Empty(result.ByStatus);
            Assert.Empty(result.ByType);
        }

        [Fact]
        public async Task GetTestManagementStatsAsync_ReturnsCorrectCounts()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetTestManagementStatsAsync_ReturnsCorrectCounts));
            var now = DateTime.UtcNow;
            
            // Add test suites
            db.TestSuites.AddRange(
                new TestSuite { Id = 1, Name = "Suite 1", Description = "Test Suite 1", CreatedBy = 1, CreatedAt = now },
                new TestSuite { Id = 2, Name = "Suite 2", Description = "Test Suite 2", CreatedBy = 1, CreatedAt = now }
            );
            
            // Add test plans
            db.TestPlans.AddRange(
                new TestPlan { Id = 1, Name = "Plan 1", Description = "Test Plan 1", CreatedBy = 1, CreatedAt = now },
                new TestPlan { Id = 2, Name = "Plan 2", Description = "Test Plan 2", CreatedBy = 1, CreatedAt = now },
                new TestPlan { Id = 3, Name = "Plan 3", Description = "Test Plan 3", CreatedBy = 1, CreatedAt = now }
            );
            
            // Add test cases
            db.TestCases.AddRange(
                new TestCase { Id = 1, Title = "Case 1", Description = "Test Case 1", SuiteId = 1, CreatedBy = 1, CreatedAt = now },
                new TestCase { Id = 2, Title = "Case 2", Description = "Test Case 2", SuiteId = 1, CreatedBy = 1, CreatedAt = now },
                new TestCase { Id = 3, Title = "Case 3", Description = "Test Case 3", SuiteId = 2, CreatedBy = 1, CreatedAt = now },
                new TestCase { Id = 4, Title = "Case 4", Description = "Test Case 4", SuiteId = 2, CreatedBy = 1, CreatedAt = now }
            );
            
            // Add requirements for coverage calculation
            db.Requirements.AddRange(
                new Requirement { Id = 1, Title = "Req 1", Type = RequirementType.SRS, Status = RequirementStatus.Approved, CreatedBy = 1, CreatedAt = now },
                new Requirement { Id = 2, Title = "Req 2", Type = RequirementType.PRS, Status = RequirementStatus.Approved, CreatedBy = 1, CreatedAt = now }
            );
            
            // Add requirement-test case links for coverage
            db.RequirementTestCaseLinks.AddRange(
                new RequirementTestCaseLink { RequirementId = 1, TestCaseId = 1 },
                new RequirementTestCaseLink { RequirementId = 2, TestCaseId = 2 }
            );
            
            await db.SaveChangesAsync();
            var service = new DashboardService(db);

            // Act
            var result = await service.GetTestManagementStatsAsync();

            // Assert
            Assert.Equal(2, result.TotalTestSuites);
            Assert.Equal(3, result.TotalTestPlans);
            Assert.Equal(4, result.TotalTestCases);
            Assert.Equal(100.0, result.TestCoveragePercentage); // 2 requirements, 2 covered = 100%
        }

        [Fact]
        public async Task GetTestManagementStatsAsync_CalculatesCorrectCoverage_WithPartialCoverage()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetTestManagementStatsAsync_CalculatesCorrectCoverage_WithPartialCoverage));
            var now = DateTime.UtcNow;
            
            // Add requirements
            db.Requirements.AddRange(
                new Requirement { Id = 1, Title = "Req 1", Type = RequirementType.SRS, Status = RequirementStatus.Approved, CreatedBy = 1, CreatedAt = now },
                new Requirement { Id = 2, Title = "Req 2", Type = RequirementType.PRS, Status = RequirementStatus.Approved, CreatedBy = 1, CreatedAt = now },
                new Requirement { Id = 3, Title = "Req 3", Type = RequirementType.SRS, Status = RequirementStatus.Approved, CreatedBy = 1, CreatedAt = now },
                new Requirement { Id = 4, Title = "Req 4", Type = RequirementType.PRS, Status = RequirementStatus.Approved, CreatedBy = 1, CreatedAt = now }
            );
            
            // Add test cases
            db.TestCases.AddRange(
                new TestCase { Id = 1, Title = "Case 1", Description = "Test Case 1", CreatedBy = 1, CreatedAt = now },
                new TestCase { Id = 2, Title = "Case 2", Description = "Test Case 2", CreatedBy = 1, CreatedAt = now }
            );
            
            // Add partial coverage - only 2 out of 4 requirements covered
            db.RequirementTestCaseLinks.AddRange(
                new RequirementTestCaseLink { RequirementId = 1, TestCaseId = 1 },
                new RequirementTestCaseLink { RequirementId = 2, TestCaseId = 2 }
            );
            
            await db.SaveChangesAsync();
            var service = new DashboardService(db);

            // Act
            var result = await service.GetTestManagementStatsAsync();

            // Assert
            Assert.Equal(50.0, result.TestCoveragePercentage); // 2 out of 4 requirements covered = 50%
        }

        [Fact]
        public async Task GetTestManagementStatsAsync_ReturnsZeroCoverage_WithNoLinks()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetTestManagementStatsAsync_ReturnsZeroCoverage_WithNoLinks));
            var now = DateTime.UtcNow;
            
            // Add requirements but no test case links
            db.Requirements.AddRange(
                new Requirement { Id = 1, Title = "Req 1", Type = RequirementType.SRS, Status = RequirementStatus.Approved, CreatedBy = 1, CreatedAt = now },
                new Requirement { Id = 2, Title = "Req 2", Type = RequirementType.PRS, Status = RequirementStatus.Approved, CreatedBy = 1, CreatedAt = now }
            );
            
            await db.SaveChangesAsync();
            var service = new DashboardService(db);

            // Act
            var result = await service.GetTestManagementStatsAsync();

            // Assert
            Assert.Equal(0.0, result.TestCoveragePercentage);
        }

        [Fact]
        public async Task GetTestManagementStatsAsync_ReturnsZeroCoverage_WithNoRequirements()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetTestManagementStatsAsync_ReturnsZeroCoverage_WithNoRequirements));
            var service = new DashboardService(db);

            // Act
            var result = await service.GetTestManagementStatsAsync();

            // Assert
            Assert.Equal(0.0, result.TestCoveragePercentage);
            Assert.Equal(0, result.TotalTestSuites);
            Assert.Equal(0, result.TotalTestPlans);
            Assert.Equal(0, result.TotalTestCases);
        }
    }
}