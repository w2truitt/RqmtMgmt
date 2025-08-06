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
    public class EnhancedDashboardServiceTests
    {
        private static RqmtMgmtDbContext GetDbContext(string testName)
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: $"EnhancedDashboardServiceTestDb_{testName}_{System.Guid.NewGuid()}")
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        [Fact]
        public async Task GetDashboardStatsAsync_WithData_ReturnsDashboardStats()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetDashboardStatsAsync_WithData_ReturnsDashboardStats));
            await SeedTestData(db);
            var service = new EnhancedDashboardService(db);

            // Act
            var result = await service.GetDashboardStatsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Requirements);
            Assert.NotNull(result.TestManagement);
            Assert.NotNull(result.TestExecution);
            Assert.NotNull(result.RecentActivities);
        }

        [Fact]
        public async Task GetDashboardStatsAsync_EmptyDatabase_ReturnsEmptyStats()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetDashboardStatsAsync_EmptyDatabase_ReturnsEmptyStats));
            var service = new EnhancedDashboardService(db);

            // Act
            var result = await service.GetDashboardStatsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Requirements);
            Assert.Equal(0, result.Requirements.TotalRequirements);
            Assert.NotNull(result.TestManagement);
            Assert.Equal(0, result.TestManagement.TotalTestSuites);
            Assert.NotNull(result.TestExecution);
            Assert.Equal(0, result.TestExecution.TotalTestRuns);
            Assert.NotNull(result.RecentActivities);
            Assert.Empty(result.RecentActivities);
        }

        [Fact]
        public async Task GetRequirementStatsAsync_WithRequirements_ReturnsCorrectStats()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetRequirementStatsAsync_WithRequirements_ReturnsCorrectStats));
            await SeedRequirementData(db);
            var service = new EnhancedDashboardService(db);

            // Act
            var result = await service.GetRequirementStatsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalRequirements > 0);
            Assert.NotNull(result.ByStatus);
            Assert.NotNull(result.ByType);
        }

        [Fact]
        public async Task GetRequirementStatsAsync_EmptyDatabase_ReturnsZeroStats()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetRequirementStatsAsync_EmptyDatabase_ReturnsZeroStats));
            var service = new EnhancedDashboardService(db);

            // Act
            var result = await service.GetRequirementStatsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalRequirements);
            Assert.Equal(0, result.DraftRequirements);
            Assert.Equal(0, result.ApprovedRequirements);
            Assert.Equal(0, result.ImplementedRequirements);
            Assert.Equal(0, result.VerifiedRequirements);
            Assert.NotNull(result.ByStatus);
            Assert.NotNull(result.ByType);
        }

        [Fact]
        public async Task GetTestManagementStatsAsync_WithTestData_ReturnsCorrectStats()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetTestManagementStatsAsync_WithTestData_ReturnsCorrectStats));
            await SeedTestManagementData(db);
            var service = new EnhancedDashboardService(db);

            // Act
            var result = await service.GetTestManagementStatsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalTestSuites >= 0);
            Assert.True(result.TotalTestPlans >= 0);
            Assert.True(result.TotalTestCases >= 0);
            Assert.True(result.TestCasesWithSteps >= 0);
            Assert.True(result.TestCoveragePercentage >= 0);
        }

        [Fact]
        public async Task GetTestManagementStatsAsync_EmptyDatabase_ReturnsZeroStats()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetTestManagementStatsAsync_EmptyDatabase_ReturnsZeroStats));
            var service = new EnhancedDashboardService(db);

            // Act
            var result = await service.GetTestManagementStatsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalTestSuites);
            Assert.Equal(0, result.TotalTestPlans);
            Assert.Equal(0, result.TotalTestCases);
            Assert.Equal(0, result.TestCasesWithSteps);
            Assert.Equal(0, result.TestCoveragePercentage);
        }

        [Fact]
        public async Task GetTestExecutionStatsAsync_WithExecutionData_ReturnsCorrectStats()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetTestExecutionStatsAsync_WithExecutionData_ReturnsCorrectStats));
            await SeedTestExecutionData(db);
            var service = new EnhancedDashboardService(db);

            // Act
            var result = await service.GetTestExecutionStatsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalTestRuns >= 0);
            Assert.True(result.PassedExecutions >= 0);
            Assert.True(result.FailedExecutions >= 0);
            Assert.True(result.BlockedExecutions >= 0);
            Assert.True(result.NotRunExecutions >= 0);
            Assert.True(result.PassRate >= 0 && result.PassRate <= 100);
        }

        [Fact]
        public async Task GetTestExecutionStatsAsync_EmptyDatabase_ReturnsZeroStats()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetTestExecutionStatsAsync_EmptyDatabase_ReturnsZeroStats));
            var service = new EnhancedDashboardService(db);

            // Act
            var result = await service.GetTestExecutionStatsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalTestRuns);
            Assert.Equal(0, result.PassedExecutions);
            Assert.Equal(0, result.FailedExecutions);
            Assert.Equal(0, result.BlockedExecutions);
            Assert.Equal(0, result.NotRunExecutions);
            Assert.Equal(0, result.PassRate);
        }

        [Fact]
        public async Task GetRecentActivityAsync_WithActivities_ReturnsRecentActivities()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetRecentActivityAsync_WithActivities_ReturnsRecentActivities));
            await SeedActivityData(db);
            var service = new EnhancedDashboardService(db);

            // Act
            var result = await service.GetRecentActivityAsync(5);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count <= 5);
        }

        [Fact]
        public async Task GetRecentActivityAsync_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetRecentActivityAsync_EmptyDatabase_ReturnsEmptyList));
            var service = new EnhancedDashboardService(db);

            // Act
            var result = await service.GetRecentActivityAsync(5);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(10)]
        public async Task GetRecentActivityAsync_WithCount_ReturnsCorrectNumber(int count)
        {
            // Arrange
            using var db = GetDbContext($"{nameof(GetRecentActivityAsync_WithCount_ReturnsCorrectNumber)}_{count}");
            await SeedActivityData(db);
            var service = new EnhancedDashboardService(db);

            // Act
            var result = await service.GetRecentActivityAsync(count);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count <= count);
        }

        private static async Task SeedTestData(RqmtMgmtDbContext db)
        {
            await SeedRequirementData(db);
            await SeedTestManagementData(db);
            await SeedTestExecutionData(db);
            await SeedActivityData(db);
        }

        private static async Task SeedRequirementData(RqmtMgmtDbContext db)
        {
            var requirements = new List<Requirement>
            {
                new Requirement
                {
                    Id = 1,
                    Title = "Requirement 1",
                    Description = "Description 1",
                    Status = RequirementStatus.Draft,
                    Type = RequirementType.UserStory
                },
                new Requirement
                {
                    Id = 2,
                    Title = "Requirement 2",
                    Description = "Description 2",
                    Status = RequirementStatus.Approved,
                    Type = RequirementType.BusinessRule
                }
            };
            db.Requirements.AddRange(requirements);
            await db.SaveChangesAsync();
        }

        private static async Task SeedTestManagementData(RqmtMgmtDbContext db)
        {
            var testSuite = new TestSuite
            {
                Id = 1,
                Name = "Test Suite 1",
                Description = "Test suite description"
            };
            db.TestSuites.Add(testSuite);

            var testPlan = new TestPlan
            {
                Id = 1,
                Name = "Test Plan 1",
                Description = "Test plan description"
            };
            db.TestPlans.Add(testPlan);

            var testCase = new TestCase
            {
                Id = 1,
                Title = "Test Case 1",
                Description = "Test case description"
            };
            db.TestCases.Add(testCase);

            var testStep = new TestStep
            {
                Id = 1,
                TestCaseId = 1,
                Description = "Test step description",
                ExpectedResult = "Expected result"
            };
            db.TestSteps.Add(testStep);

            await db.SaveChangesAsync();
        }

        private static async Task SeedTestExecutionData(RqmtMgmtDbContext db)
        {
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com"
            };
            db.Users.Add(user);

            var testRunSession = new TestRunSession
            {
                Id = 1,
                Name = "Test Run Session 1",
                TestPlanId = 1,
                ExecutedBy = 1,
                StartedAt = DateTime.UtcNow,
                Status = TestRunStatus.InProgress
            };
            db.TestRunSessions.Add(testRunSession);

            var testCaseExecution = new TestCaseExecution
            {
                Id = 1,
                TestRunSessionId = 1,
                TestCaseId = 1,
                OverallResult = TestResult.Passed,
                ExecutedAt = DateTime.UtcNow,
                ExecutedBy = 1
            };
            db.TestCaseExecutions.Add(testCaseExecution);

            await db.SaveChangesAsync();
        }

        private static async Task SeedActivityData(RqmtMgmtDbContext db)
        {
            var activities = new List<AuditLog>
            {
                new AuditLog
                {
                    Id = 1,
                    Entity = "Requirement",
                    EntityId = 1,
                    Action = "Created",
                    UserId = 1,
                    Timestamp = DateTime.UtcNow.AddHours(-1),
                    Details = "Created new requirement"
                },
                new AuditLog
                {
                    Id = 2,
                    Entity = "TestCase",
                    EntityId = 1,
                    Action = "Updated",
                    UserId = 1,
                    Timestamp = DateTime.UtcNow.AddMinutes(-30),
                    Details = "Updated test case"
                }
            };
            db.AuditLogs.AddRange(activities);
            await db.SaveChangesAsync();
        }
    }
}