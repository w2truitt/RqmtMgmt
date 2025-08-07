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
    public class TestRunSessionServiceTests
    {
        private static RqmtMgmtDbContext GetDbContext(string testName)
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestRunSessionServiceTestDb_{testName}_{System.Guid.NewGuid()}")
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        [Fact]
        public async Task GetAllAsync_WithSessions_ReturnsSessions()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetAllAsync_WithSessions_ReturnsSessions));
            await SeedTestData(db);
            var service = new TestRunSessionService(db);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
            Assert.All(result, session => Assert.NotNull(session.Name));
        }

        [Fact]
        public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetAllAsync_EmptyDatabase_ReturnsEmptyList));
            var service = new TestRunSessionService(db);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsSession()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetByIdAsync_ExistingId_ReturnsSession));
            await SeedTestData(db);
            var service = new TestRunSessionService(db);

            // Act
            var result = await service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Run Session 1", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetByIdAsync_NonExistingId_ReturnsNull));
            await SeedTestData(db);
            var service = new TestRunSessionService(db);

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_ValidSession_CreatesAndReturnsSession()
        {
            // Arrange
            using var db = GetDbContext(nameof(CreateAsync_ValidSession_CreatesAndReturnsSession));
            await SeedTestData(db);
            var service = new TestRunSessionService(db);

            var sessionDto = new TestRunSessionDto
            {
                Name = "New Test Run Session",
                Description = "New session description",
                TestPlanId = 1,
                ExecutedBy = 1,
                StartedAt = DateTime.UtcNow,
                Status = TestRunStatus.Paused,
                Environment = "Test",
                BuildVersion = "1.0.0"
            };

            // Act
            var result = await service.CreateAsync(sessionDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal(sessionDto.Name, result.Name);
            Assert.Equal(sessionDto.Description, result.Description);
            Assert.Equal(sessionDto.TestPlanId, result.TestPlanId);
            Assert.Equal(sessionDto.ExecutedBy, result.ExecutedBy);
            Assert.Equal(sessionDto.Status, result.Status);
            Assert.Equal(sessionDto.Environment, result.Environment);
            Assert.Equal(sessionDto.BuildVersion, result.BuildVersion);
        }

        [Fact]
        public async Task UpdateAsync_ExistingSession_ReturnsTrue()
        {
            // Arrange
            using var db = GetDbContext(nameof(UpdateAsync_ExistingSession_ReturnsTrue));
            await SeedTestData(db);
            var service = new TestRunSessionService(db);

            var updateDto = new TestRunSessionDto
            {
                Id = 1,
                Name = "Updated Session Name",
                Description = "Updated description",
                TestPlanId = 1,
                ExecutedBy = 1,
                StartedAt = DateTime.UtcNow,
                Status = TestRunStatus.Completed,
                Environment = "Production",
                BuildVersion = "2.0.0"
            };

            // Act
            var result = await service.UpdateAsync(updateDto);

            // Assert
            Assert.True(result);

            var updatedSession = await service.GetByIdAsync(1);
            Assert.NotNull(updatedSession);
            Assert.Equal("Updated Session Name", updatedSession.Name);
            Assert.Equal("Updated description", updatedSession.Description);
            Assert.Equal(TestRunStatus.Completed, updatedSession.Status);
            Assert.Equal("Production", updatedSession.Environment);
            Assert.Equal("2.0.0", updatedSession.BuildVersion);
        }

        [Fact]
        public async Task UpdateAsync_NonExistingSession_ReturnsFalse()
        {
            // Arrange
            using var db = GetDbContext(nameof(UpdateAsync_NonExistingSession_ReturnsFalse));
            var service = new TestRunSessionService(db);

            var updateDto = new TestRunSessionDto
            {
                Id = 999,
                Name = "Non-existing Session",
                TestPlanId = 1,
                ExecutedBy = 1,
                StartedAt = DateTime.UtcNow,
                Status = TestRunStatus.Paused
            };

            // Act
            var result = await service.UpdateAsync(updateDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ExistingSession_ReturnsTrue()
        {
            // Arrange
            using var db = GetDbContext(nameof(DeleteAsync_ExistingSession_ReturnsTrue));
            await SeedTestData(db);
            var service = new TestRunSessionService(db);

            // Act
            var result = await service.DeleteAsync(1);

            // Assert
            Assert.True(result);

            var deletedSession = await service.GetByIdAsync(1);
            Assert.Null(deletedSession);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingSession_ReturnsFalse()
        {
            // Arrange
            using var db = GetDbContext(nameof(DeleteAsync_NonExistingSession_ReturnsFalse));
            var service = new TestRunSessionService(db);

            // Act
            var result = await service.DeleteAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task StartTestRunSessionAsync_ValidSession_StartsSession()
        {
            // Arrange
            using var db = GetDbContext(nameof(StartTestRunSessionAsync_ValidSession_StartsSession));
            await SeedTestData(db);
            var service = new TestRunSessionService(db);

            var sessionDto = new TestRunSessionDto
            {
                Name = "Session to Start",
                TestPlanId = 1,
                ExecutedBy = 1,
                Status = TestRunStatus.Paused
            };

            // Act
            var result = await service.StartTestRunSessionAsync(sessionDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestRunStatus.InProgress, result.Status);
        }

        [Fact]
        public async Task CompleteTestRunSessionAsync_ExistingSession_CompletesSession()
        {
            // Arrange
            using var db = GetDbContext(nameof(CompleteTestRunSessionAsync_ExistingSession_CompletesSession));
            await SeedTestData(db);
            var service = new TestRunSessionService(db);

            // Act
            var result = await service.CompleteTestRunSessionAsync(1);

            // Assert
            Assert.True(result);

            var completedSession = await service.GetByIdAsync(1);
            Assert.NotNull(completedSession);
            Assert.Equal(TestRunStatus.Completed, completedSession.Status);
            Assert.NotNull(completedSession.CompletedAt);
        }

        [Fact]
        public async Task CompleteTestRunSessionAsync_NonExistingSession_ReturnsFalse()
        {
            // Arrange
            using var db = GetDbContext(nameof(CompleteTestRunSessionAsync_NonExistingSession_ReturnsFalse));
            var service = new TestRunSessionService(db);

            // Act
            var result = await service.CompleteTestRunSessionAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AbortTestRunSessionAsync_ExistingSession_AbortsSession()
        {
            // Arrange
            using var db = GetDbContext(nameof(AbortTestRunSessionAsync_ExistingSession_AbortsSession));
            await SeedTestData(db);
            var service = new TestRunSessionService(db);

            // Act
            var result = await service.AbortTestRunSessionAsync(1);

            // Assert
            Assert.True(result);

            var abortedSession = await service.GetByIdAsync(1);
            Assert.NotNull(abortedSession);
            Assert.Equal(TestRunStatus.Aborted, abortedSession.Status);
            Assert.NotNull(abortedSession.CompletedAt);
        }

        [Fact]
        public async Task AbortTestRunSessionAsync_NonExistingSession_ReturnsFalse()
        {
            // Arrange
            using var db = GetDbContext(nameof(AbortTestRunSessionAsync_NonExistingSession_ReturnsFalse));
            var service = new TestRunSessionService(db);

            // Act
            var result = await service.AbortTestRunSessionAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetActiveSessionsAsync_WithActiveSessions_ReturnsActiveSessions()
        {
            // Arrange
            using var db = GetDbContext(nameof(GetActiveSessionsAsync_WithActiveSessions_ReturnsActiveSessions));
            await SeedTestDataWithVariousStatuses(db);
            var service = new TestRunSessionService(db);

            // Act
            var result = await service.GetActiveSessionsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.All(result, session => 
                Assert.True(session.Status == TestRunStatus.InProgress || session.Status == TestRunStatus.Paused));
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
                Description = "Test run session description",
                TestPlanId = 1,
                ExecutedBy = 1,
                StartedAt = DateTime.UtcNow,
                Status = TestRunStatus.InProgress,
                Environment = "Test",
                BuildVersion = "1.0.0"
            };
            db.TestRunSessions.Add(testRunSession);

            await db.SaveChangesAsync();
        }

        private static async Task SeedTestDataWithVariousStatuses(RqmtMgmtDbContext db)
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

            // Add test run sessions with various statuses
            var sessions = new List<TestRunSession>
            {
                new TestRunSession
                {
                    Id = 1,
                    Name = "Active Session 1",
                    TestPlanId = 1,
                    ExecutedBy = 1,
                    StartedAt = DateTime.UtcNow,
                    Status = TestRunStatus.InProgress
                },
                new TestRunSession
                {
                    Id = 2,
                    Name = "Paused Session",
                    TestPlanId = 1,
                    ExecutedBy = 1,
                    StartedAt = DateTime.UtcNow,
                    Status = TestRunStatus.Paused
                },
                new TestRunSession
                {
                    Id = 3,
                    Name = "Completed Session",
                    TestPlanId = 1,
                    ExecutedBy = 1,
                    StartedAt = DateTime.UtcNow.AddHours(-2),
                    CompletedAt = DateTime.UtcNow,
                    Status = TestRunStatus.Completed
                }
            };
            db.TestRunSessions.AddRange(sessions);

            await db.SaveChangesAsync();
        }
    }
}