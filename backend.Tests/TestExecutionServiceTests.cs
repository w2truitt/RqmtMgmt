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
    public class TestExecutionServiceTests
    {
        private static RqmtMgmtDbContext GetDbContext(string testName)
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestExecutionServiceTestDb_{testName}_{System.Guid.NewGuid()}")
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        [Fact]
        public async Task ExecuteTestCaseAsync_ValidExecution_ReturnsExecutionDto()
        {
            // Arrange
            using var db = GetDbContext(nameof(ExecuteTestCaseAsync_ValidExecution_ReturnsExecutionDto));
            await SeedTestData(db);
            var service = new TestExecutionService(db);

            var executionDto = new TestCaseExecutionDto
            {
                TestRunSessionId = 1,
                TestCaseId = 1,
                OverallResult = TestResult.Passed,
                ExecutedBy = 1,
                Notes = "Test executed successfully",
                DefectId = null
            };

            // Act
            var result = await service.ExecuteTestCaseAsync(executionDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal(executionDto.TestRunSessionId, result.TestRunSessionId);
            Assert.Equal(executionDto.TestCaseId, result.TestCaseId);
            Assert.Equal(executionDto.OverallResult, result.OverallResult);
            Assert.Equal(executionDto.ExecutedBy, result.ExecutedBy);
            Assert.Equal(executionDto.Notes, result.Notes);
            Assert.NotNull(result.ExecutedAt);
        }

        [Fact]
        public async Task ExecuteTestCaseAsync_WithStepExecutions_CreatesStepExecutions()
        {
            // Arrange
            using var db = GetDbContext(nameof(ExecuteTestCaseAsync_WithStepExecutions_CreatesStepExecutions));
            await SeedTestData(db);
            var service = new TestExecutionService(db);

            var executionDto = new TestCaseExecutionDto
            {
                TestRunSessionId = 1,
                TestCaseId = 1,
                OverallResult = TestResult.Passed,
                ExecutedBy = 1,
                TestStepExecutions = new List<TestStepExecutionDto>
                {
                    new TestStepExecutionDto
                    {
                        TestStepId = 1,
                        StepOrder = 1,
                        Result = TestResult.Passed,
                        ActualResult = "Step completed successfully"
                    }
                }
            };

            // Act
            var result = await service.ExecuteTestCaseAsync(executionDto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.TestStepExecutions);
            Assert.Single(result.TestStepExecutions);
            
            var stepExecution = result.TestStepExecutions.First();
            Assert.Equal(1, stepExecution.TestStepId);
            Assert.Equal(1, stepExecution.StepOrder);
            Assert.Equal(TestResult.Passed, stepExecution.Result);
            Assert.Equal("Step completed successfully", stepExecution.ActualResult);
        }

        [Fact]
        public async Task UpdateTestCaseExecutionAsync_ExistingExecution_ReturnsTrue()
        {
            // Arrange
            using var db = GetDbContext(nameof(UpdateTestCaseExecutionAsync_ExistingExecution_ReturnsTrue));
            await SeedTestData(db);
            var service = new TestExecutionService(db);

            // Create an execution first
            var execution = new TestCaseExecution
            {
                TestRunSessionId = 1,
                TestCaseId = 1,
                OverallResult = TestResult.NotRun,
                ExecutedAt = DateTime.UtcNow,
                ExecutedBy = 1
            };
            db.TestCaseExecutions.Add(execution);
            await db.SaveChangesAsync();

            var updateDto = new TestCaseExecutionDto
            {
                Id = execution.Id,
                TestRunSessionId = execution.TestRunSessionId,
                TestCaseId = execution.TestCaseId,
                OverallResult = TestResult.Passed,
                ExecutedAt = DateTime.UtcNow,
                ExecutedBy = 1,
                Notes = "Updated notes",
                DefectId = "DEF-001"
            };

            // Act
            var result = await service.UpdateTestCaseExecutionAsync(updateDto);

            // Assert
            Assert.True(result);
            
            var updatedExecution = await db.TestCaseExecutions.FindAsync(execution.Id);
            Assert.NotNull(updatedExecution);
            Assert.Equal(TestResult.Passed, updatedExecution.OverallResult);
            Assert.Equal("Updated notes", updatedExecution.Notes);
            Assert.Equal("DEF-001", updatedExecution.DefectId);
        }

        [Fact]
        public async Task UpdateTestCaseExecutionAsync_NonExistingExecution_ReturnsFalse()
        {
            // Arrange
            using var db = GetDbContext(nameof(UpdateTestCaseExecutionAsync_NonExistingExecution_ReturnsFalse));
            var service = new TestExecutionService(db);

            var updateDto = new TestCaseExecutionDto
            {
                Id = 999, // Non-existing ID
                TestRunSessionId = 1,
                TestCaseId = 1,
                OverallResult = TestResult.Passed
            };

            // Act
            var result = await service.UpdateTestCaseExecutionAsync(updateDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateStepResultAsync_NewStepExecution_CreatesNewExecution()
        {
            // Arrange
            using var db = GetDbContext(nameof(UpdateStepResultAsync_NewStepExecution_CreatesNewExecution));
            await SeedTestData(db);
            var service = new TestExecutionService(db);

            // Create a test case execution first
            var testCaseExecution = new TestCaseExecution
            {
                TestRunSessionId = 1,
                TestCaseId = 1,
                OverallResult = TestResult.NotRun,
                ExecutedAt = DateTime.UtcNow,
                ExecutedBy = 1
            };
            db.TestCaseExecutions.Add(testCaseExecution);
            await db.SaveChangesAsync();

            var stepExecutionDto = new TestStepExecutionDto
            {
                Id = 0, // New execution
                TestCaseExecutionId = testCaseExecution.Id,
                TestStepId = 1,
                StepOrder = 1,
                Result = TestResult.Passed,
                ActualResult = "Step completed successfully",
                Notes = "Step notes"
            };

            // Act
            var result = await service.UpdateStepResultAsync(stepExecutionDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal(stepExecutionDto.TestCaseExecutionId, result.TestCaseExecutionId);
            Assert.Equal(stepExecutionDto.TestStepId, result.TestStepId);
            Assert.Equal(stepExecutionDto.Result, result.Result);
            Assert.Equal(stepExecutionDto.ActualResult, result.ActualResult);
        }

        [Fact]
        public async Task UpdateStepResultAsync_ExistingStepExecution_UpdatesExecution()
        {
            // Arrange
            using var db = GetDbContext(nameof(UpdateStepResultAsync_ExistingStepExecution_UpdatesExecution));
            await SeedTestData(db);
            var service = new TestExecutionService(db);

            // Create a test case execution and step execution
            var testCaseExecution = new TestCaseExecution
            {
                TestRunSessionId = 1,
                TestCaseId = 1,
                OverallResult = TestResult.NotRun,
                ExecutedAt = DateTime.UtcNow,
                ExecutedBy = 1
            };
            db.TestCaseExecutions.Add(testCaseExecution);
            await db.SaveChangesAsync();

            var stepExecution = new TestStepExecution
            {
                TestCaseExecutionId = testCaseExecution.Id,
                TestStepId = 1,
                StepOrder = 1,
                Result = TestResult.NotRun,
                ExecutedAt = DateTime.UtcNow
            };
            db.TestStepExecutions.Add(stepExecution);
            await db.SaveChangesAsync();

            var updateDto = new TestStepExecutionDto
            {
                Id = stepExecution.Id,
                TestCaseExecutionId = testCaseExecution.Id,
                TestStepId = 1,
                StepOrder = 1,
                Result = TestResult.Passed,
                ActualResult = "Updated result",
                Notes = "Updated notes"
            };

            // Act
            var result = await service.UpdateStepResultAsync(updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateDto.Id, result.Id);
            Assert.Equal(TestResult.Passed, result.Result);
            Assert.Equal("Updated result", result.ActualResult);
            Assert.Equal("Updated notes", result.Notes);
        }

        [Fact]
        public async Task GetExecutionsForSessionAsync_ValidSessionId_ReturnsExecutions()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetExecutionsForSessionAsync_ValidSessionId_ReturnsExecutions));
            await SeedTestData(db);
            var service = new TestExecutionService(db);

            // Create test executions
            var executions = new List<TestCaseExecution>
            {
                new TestCaseExecution
                {
                    TestRunSessionId = 1,
                    TestCaseId = 1,
                    OverallResult = TestResult.Passed,
                    ExecutedAt = DateTime.UtcNow,
                    ExecutedBy = 1
                },
                new TestCaseExecution
                {
                    TestRunSessionId = 1,
                    TestCaseId = 2,
                    OverallResult = TestResult.Failed,
                    ExecutedAt = DateTime.UtcNow,
                    ExecutedBy = 1
                }
            };
            db.TestCaseExecutions.AddRange(executions);
            await db.SaveChangesAsync();

            // Act
            var result = await service.GetExecutionsForSessionAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, e => Assert.Equal(1, e.TestRunSessionId));
        }

        [Fact]
        public async Task GetStepExecutionsForCaseAsync_ValidCaseId_ReturnsStepExecutions()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetStepExecutionsForCaseAsync_ValidCaseId_ReturnsStepExecutions));
            await SeedTestData(db);
            var service = new TestExecutionService(db);

            // Create test case execution and step executions
            var testCaseExecution = new TestCaseExecution
            {
                TestRunSessionId = 1,
                TestCaseId = 1,
                OverallResult = TestResult.Passed,
                ExecutedAt = DateTime.UtcNow,
                ExecutedBy = 1
            };
            db.TestCaseExecutions.Add(testCaseExecution);
            await db.SaveChangesAsync();

            var stepExecutions = new List<TestStepExecution>
            {
                new TestStepExecution
                {
                    TestCaseExecutionId = testCaseExecution.Id,
                    TestStepId = 1,
                    StepOrder = 1,
                    Result = TestResult.Passed,
                    ExecutedAt = DateTime.UtcNow
                },
                new TestStepExecution
                {
                    TestCaseExecutionId = testCaseExecution.Id,
                    TestStepId = 2,
                    StepOrder = 2,
                    Result = TestResult.Failed,
                    ExecutedAt = DateTime.UtcNow
                }
            };
            db.TestStepExecutions.AddRange(stepExecutions);
            await db.SaveChangesAsync();

            // Act
            var result = await service.GetStepExecutionsForCaseAsync(testCaseExecution.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, se => Assert.Equal(testCaseExecution.Id, se.TestCaseExecutionId));
        }

        [Fact]
        public async Task GetExecutionStatsAsync_EmptyDatabase_ReturnsZeroedStatsAndNullDate()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetExecutionStatsAsync_EmptyDatabase_ReturnsZeroedStatsAndNullDate));
            var service = new TestExecutionService(db);

            // Act
            var stats = await service.GetExecutionStatsAsync();

            // Assert
            Assert.NotNull(stats);
            Assert.Null(stats.LastExecutionDate);
            Assert.Equal(0, stats.TotalTestRuns);
            Assert.Equal(0, stats.ActiveTestRuns);
            Assert.Equal(0, stats.CompletedTestRuns);
            Assert.Equal(0, stats.TotalTestCaseExecutions);
            Assert.Equal(0, stats.PassedExecutions);
            Assert.Equal(0, stats.FailedExecutions);
            Assert.Equal(0, stats.BlockedExecutions);
            Assert.Equal(0, stats.NotRunExecutions);
            Assert.Equal(0, stats.PassRate);
        }

        [Fact]
        public async Task GetExecutionStatsAsync_MixedDataScenario_ReturnsCorrectlyAggregatedStats()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetExecutionStatsAsync_MixedDataScenario_ReturnsCorrectlyAggregatedStats));
            var latestDate = DateTime.UtcNow;
            
            // Create test data with mixed statuses and results
            await SeedTestDataForStatistics(db, latestDate);
            var service = new TestExecutionService(db);

            // Act
            var stats = await service.GetExecutionStatsAsync();

            // Assert
            Assert.NotNull(stats);
            Assert.Equal(latestDate, stats.LastExecutionDate);
            
            // Test run statistics: 2 completed, 1 in progress, 1 aborted
            Assert.Equal(4, stats.TotalTestRuns);
            Assert.Equal(1, stats.ActiveTestRuns); // InProgress
            Assert.Equal(2, stats.CompletedTestRuns); // Completed

            // Test case execution statistics: 3 passed, 2 failed, 1 blocked, 1 not run
            Assert.Equal(7, stats.TotalTestCaseExecutions); // All executions including NotRun
            Assert.Equal(3, stats.PassedExecutions);
            Assert.Equal(2, stats.FailedExecutions);
            Assert.Equal(1, stats.BlockedExecutions);
            Assert.Equal(1, stats.NotRunExecutions);
            
            // Pass rate: 3 passed out of 7 total executions = 42.86%
            Assert.Equal(42.86, stats.PassRate, 2);
        }

        [Fact]
        public async Task GetExecutionStatsAsync_NoExecutionsHaveExecutionDate_HandlesGracefully()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetExecutionStatsAsync_NoExecutionsHaveExecutionDate_HandlesGracefully));
            await SeedTestData(db);
            
            // Add executions with null ExecutedAt dates
            var execution = new TestCaseExecution
            {
                TestRunSessionId = 1,
                TestCaseId = 1,
                OverallResult = TestResult.Passed,
                ExecutedAt = null, // This is key - no execution date
                ExecutedBy = 1
            };
            db.TestCaseExecutions.Add(execution);
            await db.SaveChangesAsync();
            
            var service = new TestExecutionService(db);

            // Act
            var stats = await service.GetExecutionStatsAsync();

            // Assert
            Assert.NotNull(stats);
            // This specifically tests that MaxAsync doesn't throw an InvalidOperationException
            Assert.Null(stats.LastExecutionDate);
            Assert.Equal(1, stats.PassedExecutions);
        }

        [Fact]
        public async Task GetExecutionStatsAsync_OnlyNotRunExecutions_HasZeroPassRate()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetExecutionStatsAsync_OnlyNotRunExecutions_HasZeroPassRate));
            await SeedTestData(db);
            
            // Add only NotRun executions
            var executions = new[]
            {
                new TestCaseExecution
                {
                    TestRunSessionId = 1,
                    TestCaseId = 1,
                    OverallResult = TestResult.NotRun,
                    ExecutedAt = DateTime.UtcNow,
                    ExecutedBy = 1
                },
                new TestCaseExecution
                {
                    TestRunSessionId = 1,
                    TestCaseId = 2,
                    OverallResult = TestResult.NotRun,
                    ExecutedAt = DateTime.UtcNow,
                    ExecutedBy = 1
                }
            };
            db.TestCaseExecutions.AddRange(executions);
            await db.SaveChangesAsync();
            
            var service = new TestExecutionService(db);

            // Act
            var stats = await service.GetExecutionStatsAsync();

            // Assert
            Assert.NotNull(stats);
            // This tests the divide-by-zero guard in the pass rate calculation
            Assert.Equal(2, stats.TotalTestCaseExecutions); // NotRun executions DO count toward total
            Assert.Equal(0, stats.PassRate);
            Assert.Equal(2, stats.NotRunExecutions);
        }

        [Fact]
        public async Task GetExecutionStatsForSessionAsync_SessionDoesNotExist_ReturnsZeroedStats()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetExecutionStatsForSessionAsync_SessionDoesNotExist_ReturnsZeroedStats));
            await SeedTestData(db); // Creates session with ID 1
            var service = new TestExecutionService(db);
            var nonExistentSessionId = 999;

            // Act
            var stats = await service.GetExecutionStatsForSessionAsync(nonExistentSessionId);

            // Assert
            Assert.NotNull(stats);
            Assert.Null(stats.LastExecutionDate);
            Assert.Equal(1, stats.TotalTestRuns); // Session-specific stats always show 1
            Assert.Equal(0, stats.ActiveTestRuns);
            Assert.Equal(0, stats.CompletedTestRuns);
            Assert.Equal(0, stats.TotalTestCaseExecutions);
            Assert.Equal(0, stats.PassedExecutions);
            Assert.Equal(0, stats.FailedExecutions);
            Assert.Equal(0, stats.BlockedExecutions);
            Assert.Equal(0, stats.NotRunExecutions);
            Assert.Equal(0, stats.PassRate);
        }

        [Fact]
        public async Task GetExecutionStatsForSessionAsync_SessionHasNoExecutions_ReturnsCorrectStats()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetExecutionStatsForSessionAsync_SessionHasNoExecutions_ReturnsCorrectStats));
            await SeedTestData(db);
            var service = new TestExecutionService(db);
            var sessionId = 1;

            // Act
            var stats = await service.GetExecutionStatsForSessionAsync(sessionId);

            // Assert
            Assert.NotNull(stats);
            Assert.Null(stats.LastExecutionDate);
            Assert.Equal(1, stats.TotalTestRuns); // Session-specific stats always show 1
            Assert.Equal(0, stats.ActiveTestRuns); // Session-specific stats don't show run stats
            Assert.Equal(0, stats.CompletedTestRuns);
            Assert.Equal(0, stats.TotalTestCaseExecutions);
            Assert.Equal(0, stats.PassedExecutions);
            Assert.Equal(0, stats.FailedExecutions);
            Assert.Equal(0, stats.BlockedExecutions);
            Assert.Equal(0, stats.NotRunExecutions);
            Assert.Equal(0, stats.PassRate);
        }

        [Fact]
        public async Task GetExecutionStatsForSessionAsync_ValidSessionWithMixedResults_ReturnsCorrectlyScopedStats()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetExecutionStatsForSessionAsync_ValidSessionWithMixedResults_ReturnsCorrectlyScopedStats));
            var targetSessionId = 2;
            var latestDateForSession = DateTime.UtcNow.AddHours(-5);

            // Create noise data (session 1)
            await SeedTestData(db);
            var noiseExecution = new TestCaseExecution
            {
                TestRunSessionId = 1,
                TestCaseId = 1,
                OverallResult = TestResult.Failed,
                ExecutedAt = DateTime.UtcNow,
                ExecutedBy = 1
            };
            db.TestCaseExecutions.Add(noiseExecution);

            // Create target session (session 2)
            var targetSession = new TestRunSession
            {
                Id = targetSessionId,
                Name = "Target Session",
                TestPlanId = 1,
                ExecutedBy = 1,
                StartedAt = DateTime.UtcNow,
                Status = TestRunStatus.InProgress
            };
            db.TestRunSessions.Add(targetSession);

            // Add executions for target session
            var targetExecutions = new[]
            {
                new TestCaseExecution
                {
                    TestRunSessionId = targetSessionId,
                    TestCaseId = 1,
                    OverallResult = TestResult.Passed,
                    ExecutedAt = latestDateForSession,
                    ExecutedBy = 1
                },
                new TestCaseExecution
                {
                    TestRunSessionId = targetSessionId,
                    TestCaseId = 2,
                    OverallResult = TestResult.Passed,
                    ExecutedAt = latestDateForSession.AddMinutes(-10),
                    ExecutedBy = 1
                },
                new TestCaseExecution
                {
                    TestRunSessionId = targetSessionId,
                    TestCaseId = 1,
                    OverallResult = TestResult.Blocked,
                    ExecutedAt = latestDateForSession.AddMinutes(-20),
                    ExecutedBy = 1
                }
            };
            db.TestCaseExecutions.AddRange(targetExecutions);
            await db.SaveChangesAsync();

            var service = new TestExecutionService(db);

            // Act
            var stats = await service.GetExecutionStatsForSessionAsync(targetSessionId);

            // Assert
            Assert.NotNull(stats);
            Assert.Equal(latestDateForSession, stats.LastExecutionDate);
            
            // Session stats always show 1 for session-specific queries
            Assert.Equal(1, stats.TotalTestRuns);
            Assert.Equal(0, stats.ActiveTestRuns);
            Assert.Equal(0, stats.CompletedTestRuns);

            // Execution stats should only include this session's executions
            Assert.Equal(3, stats.TotalTestCaseExecutions); // 2 passed + 1 blocked
            Assert.Equal(2, stats.PassedExecutions);
            Assert.Equal(0, stats.FailedExecutions); // Noise from session 1 should not appear
            Assert.Equal(1, stats.BlockedExecutions);
            Assert.Equal(0, stats.NotRunExecutions);
            
            // Pass rate: 2 passed out of 3 runnable executions = 66.67%
            Assert.Equal(66.67, stats.PassRate, 2);
        }

        [Fact]
        public async Task GetExecutionStatsForSessionAsync_SessionWithNullExecutionDates_HandlesGracefully()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetExecutionStatsForSessionAsync_SessionWithNullExecutionDates_HandlesGracefully));
            await SeedTestData(db);
            
            // Add executions with null ExecutedAt dates
            var executions = new[]
            {
                new TestCaseExecution
                {
                    TestRunSessionId = 1,
                    TestCaseId = 1,
                    OverallResult = TestResult.Passed,
                    ExecutedAt = null, // Null date
                    ExecutedBy = 1
                },
                new TestCaseExecution
                {
                    TestRunSessionId = 1,
                    TestCaseId = 2,
                    OverallResult = TestResult.Failed,
                    ExecutedAt = null, // Null date
                    ExecutedBy = 1
                }
            };
            db.TestCaseExecutions.AddRange(executions);
            await db.SaveChangesAsync();
            
            var service = new TestExecutionService(db);

            // Act
            var stats = await service.GetExecutionStatsForSessionAsync(1);

            // Assert
            Assert.NotNull(stats);
            // This specifically tests that MaxAsync doesn't throw when all dates are null
            Assert.Null(stats.LastExecutionDate);
            Assert.Equal(2, stats.TotalTestCaseExecutions);
            Assert.Equal(1, stats.PassedExecutions);
            Assert.Equal(1, stats.FailedExecutions);
            Assert.Equal(50.0, stats.PassRate, 2);
        }

        [Fact]
        public async Task GetExecutionStatsAsync_AllPossibleTestResults_CalculatesCorrectly()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetExecutionStatsAsync_AllPossibleTestResults_CalculatesCorrectly));
            await SeedTestData(db);
            
            // Add executions with all possible test results
            var executions = new[]
            {
                new TestCaseExecution { TestRunSessionId = 1, TestCaseId = 1, OverallResult = TestResult.Passed, ExecutedAt = DateTime.UtcNow, ExecutedBy = 1 },
                new TestCaseExecution { TestRunSessionId = 1, TestCaseId = 1, OverallResult = TestResult.Passed, ExecutedAt = DateTime.UtcNow, ExecutedBy = 1 },
                new TestCaseExecution { TestRunSessionId = 1, TestCaseId = 1, OverallResult = TestResult.Failed, ExecutedAt = DateTime.UtcNow, ExecutedBy = 1 },
                new TestCaseExecution { TestRunSessionId = 1, TestCaseId = 1, OverallResult = TestResult.Blocked, ExecutedAt = DateTime.UtcNow, ExecutedBy = 1 },
                new TestCaseExecution { TestRunSessionId = 1, TestCaseId = 1, OverallResult = TestResult.NotRun, ExecutedAt = DateTime.UtcNow, ExecutedBy = 1 }
            };
            db.TestCaseExecutions.AddRange(executions);
            await db.SaveChangesAsync();
            
            var service = new TestExecutionService(db);

            // Act
            var stats = await service.GetExecutionStatsAsync();

            // Assert
            Assert.NotNull(stats);
            Assert.Equal(5, stats.TotalTestCaseExecutions); // All executions including NotRun
            Assert.Equal(2, stats.PassedExecutions);
            Assert.Equal(1, stats.FailedExecutions);
            Assert.Equal(1, stats.BlockedExecutions);
            Assert.Equal(1, stats.NotRunExecutions);
            
            // Pass rate: 2 passed out of 5 total executions = 40%
            Assert.Equal(40.0, stats.PassRate, 2);
        }

        [Fact]
        public async Task GetExecutionStatsAsync_AllTestRunStatuses_CountsCorrectly()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetExecutionStatsAsync_AllTestRunStatuses_CountsCorrectly));
            
            // Create sessions with all possible statuses
            var user = new User { Id = 1, UserName = "testuser", Email = "test@example.com" };
            var testPlan = new TestPlan { Id = 1, Name = "Test Plan", Description = "Description" };
            db.Users.Add(user);
            db.TestPlans.Add(testPlan);
            
            var sessions = new[]
            {
                new TestRunSession { Id = 1, Name = "Session 1", TestPlanId = 1, ExecutedBy = 1, StartedAt = DateTime.UtcNow, Status = TestRunStatus.InProgress },
                new TestRunSession { Id = 2, Name = "Session 2", TestPlanId = 1, ExecutedBy = 1, StartedAt = DateTime.UtcNow, Status = TestRunStatus.InProgress },
                new TestRunSession { Id = 3, Name = "Session 3", TestPlanId = 1, ExecutedBy = 1, StartedAt = DateTime.UtcNow, Status = TestRunStatus.Completed },
                new TestRunSession { Id = 4, Name = "Session 4", TestPlanId = 1, ExecutedBy = 1, StartedAt = DateTime.UtcNow, Status = TestRunStatus.Completed },
                new TestRunSession { Id = 5, Name = "Session 5", TestPlanId = 1, ExecutedBy = 1, StartedAt = DateTime.UtcNow, Status = TestRunStatus.Completed },
                new TestRunSession { Id = 6, Name = "Session 6", TestPlanId = 1, ExecutedBy = 1, StartedAt = DateTime.UtcNow, Status = TestRunStatus.Aborted },
                new TestRunSession { Id = 7, Name = "Session 7", TestPlanId = 1, ExecutedBy = 1, StartedAt = DateTime.UtcNow, Status = TestRunStatus.Paused }
            };
            db.TestRunSessions.AddRange(sessions);
            await db.SaveChangesAsync();
            
            var service = new TestExecutionService(db);

            // Act
            var stats = await service.GetExecutionStatsAsync();

            // Assert
            Assert.NotNull(stats);
            Assert.Equal(7, stats.TotalTestRuns);
            Assert.Equal(2, stats.ActiveTestRuns); // InProgress sessions
            Assert.Equal(3, stats.CompletedTestRuns); // Completed sessions
            // Note: Aborted and Paused sessions are counted in TotalTestRuns but not in Active or Completed
        }

        private static async Task SeedTestDataForStatistics(RqmtMgmtDbContext db, DateTime latestDate)
        {
            // Add test user
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com"
            };
            db.Users.Add(user);

            // Add test plan
            var testPlan = new TestPlan
            {
                Id = 1,
                Name = "Test Plan 1",
                Description = "Test plan description"
            };
            db.TestPlans.Add(testPlan);

            // Add test cases
            for (int i = 1; i <= 3; i++)
            {
                db.TestCases.Add(new TestCase
                {
                    Id = i,
                    Title = $"Test Case {i}",
                    Description = $"Test case {i} description"
                });
            }

            // Add test run sessions with mixed statuses
            var sessions = new[]
            {
                new TestRunSession { Id = 1, Name = "Session 1", TestPlanId = 1, ExecutedBy = 1, StartedAt = DateTime.UtcNow, Status = TestRunStatus.Completed },
                new TestRunSession { Id = 2, Name = "Session 2", TestPlanId = 1, ExecutedBy = 1, StartedAt = DateTime.UtcNow, Status = TestRunStatus.Completed },
                new TestRunSession { Id = 3, Name = "Session 3", TestPlanId = 1, ExecutedBy = 1, StartedAt = DateTime.UtcNow, Status = TestRunStatus.InProgress },
                new TestRunSession { Id = 4, Name = "Session 4", TestPlanId = 1, ExecutedBy = 1, StartedAt = DateTime.UtcNow, Status = TestRunStatus.Aborted }
            };
            db.TestRunSessions.AddRange(sessions);

            // Add test case executions with mixed results
            var executions = new[]
            {
                // Session 1 - mixed results
                new TestCaseExecution { TestRunSessionId = 1, TestCaseId = 1, OverallResult = TestResult.Passed, ExecutedAt = latestDate.AddDays(-1), ExecutedBy = 1 },
                new TestCaseExecution { TestRunSessionId = 1, TestCaseId = 2, OverallResult = TestResult.Passed, ExecutedAt = latestDate, ExecutedBy = 1 },
                new TestCaseExecution { TestRunSessionId = 1, TestCaseId = 3, OverallResult = TestResult.Failed, ExecutedAt = latestDate.AddHours(-1), ExecutedBy = 1 },
                
                // Session 2 - more mixed results
                new TestCaseExecution { TestRunSessionId = 2, TestCaseId = 1, OverallResult = TestResult.Passed, ExecutedAt = latestDate.AddDays(-2), ExecutedBy = 1 },
                new TestCaseExecution { TestRunSessionId = 2, TestCaseId = 2, OverallResult = TestResult.Blocked, ExecutedAt = latestDate.AddDays(-2), ExecutedBy = 1 },
                new TestCaseExecution { TestRunSessionId = 2, TestCaseId = 3, OverallResult = TestResult.Failed, ExecutedAt = latestDate.AddDays(-2), ExecutedBy = 1 },
                
                // Session 3 - has NotRun
                new TestCaseExecution { TestRunSessionId = 3, TestCaseId = 1, OverallResult = TestResult.NotRun, ExecutedAt = null, ExecutedBy = 1 }
            };
            db.TestCaseExecutions.AddRange(executions);

            await db.SaveChangesAsync();
        }

        private static async Task SeedTestData(RqmtMgmtDbContext db)
        {
            // Add test user
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com"
            };
            db.Users.Add(user);

            // Add test plan
            var testPlan = new TestPlan
            {
                Id = 1,
                Name = "Test Plan 1",
                Description = "Test plan description"
            };
            db.TestPlans.Add(testPlan);

            // Add test run session
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

            // Add test cases
            var testCase1 = new TestCase
            {
                Id = 1,
                Title = "Test Case 1",
                Description = "Test case description"
            };
            var testCase2 = new TestCase
            {
                Id = 2,
                Title = "Test Case 2",
                Description = "Test case 2 description"
            };
            db.TestCases.Add(testCase1);
            db.TestCases.Add(testCase2);

            // Add test steps
            var testStep1 = new TestStep
            {
                Id = 1,
                TestCaseId = 1,
                Description = "Step 1 description",
                ExpectedResult = "Step 1 expected result"
            };
            var testStep2 = new TestStep
            {
                Id = 2,
                TestCaseId = 1,
                Description = "Step 2 description",
                ExpectedResult = "Step 2 expected result"
            };
            db.TestSteps.Add(testStep1);
            db.TestSteps.Add(testStep2);

            await db.SaveChangesAsync();
        }
    }
}