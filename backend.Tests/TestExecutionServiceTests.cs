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