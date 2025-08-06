using System;
using System.Collections.Generic;
using backend.Models;
using RqmtMgmtShared;
using Xunit;

namespace backend.Tests
{
    public class TestRunSessionTests
    {
        [Fact]
        public void TestRunSession_DefaultConstructor_InitializesProperties()
        {
            // Act
            var session = new TestRunSession();

            // Assert
            Assert.Equal(0, session.Id);
            Assert.Equal(string.Empty, session.Name);
            Assert.Null(session.Description);
            Assert.Equal(0, session.TestPlanId);
            Assert.Equal(0, session.ExecutedBy);
            Assert.Equal(default(DateTime), session.StartedAt);
            Assert.Null(session.CompletedAt);
            Assert.Equal(default(TestRunStatus), session.Status);
            Assert.Null(session.Environment);
            Assert.Null(session.BuildVersion);
            Assert.Null(session.TestPlan);
            Assert.Null(session.Executor);
            Assert.NotNull(session.TestCaseExecutions);
            Assert.Empty(session.TestCaseExecutions);
        }

        [Fact]
        public void TestRunSession_SetProperties_PropertiesSetCorrectly()
        {
            // Arrange
            var session = new TestRunSession();
            var startedAt = DateTime.UtcNow;
            var completedAt = DateTime.UtcNow.AddHours(2);

            // Act
            session.Id = 1;
            session.Name = "Test Run Session 1";
            session.Description = "Test run description";
            session.TestPlanId = 100;
            session.ExecutedBy = 200;
            session.StartedAt = startedAt;
            session.CompletedAt = completedAt;
            session.Status = TestRunStatus.InProgress;
            session.Environment = "Production";
            session.BuildVersion = "1.0.0";

            // Assert
            Assert.Equal(1, session.Id);
            Assert.Equal("Test Run Session 1", session.Name);
            Assert.Equal("Test run description", session.Description);
            Assert.Equal(100, session.TestPlanId);
            Assert.Equal(200, session.ExecutedBy);
            Assert.Equal(startedAt, session.StartedAt);
            Assert.Equal(completedAt, session.CompletedAt);
            Assert.Equal(TestRunStatus.InProgress, session.Status);
            Assert.Equal("Production", session.Environment);
            Assert.Equal("1.0.0", session.BuildVersion);
        }

        [Fact]
        public void TestRunSession_NavigationProperties_CanBeSet()
        {
            // Arrange
            var session = new TestRunSession();
            var testPlan = new TestPlan { Id = 100, Name = "Test Plan" };
            var executor = new User { Id = 200, UserName = "testuser", Email = "test@example.com" };
            var testCaseExecutions = new List<TestCaseExecution>
            {
                new TestCaseExecution { Id = 1 },
                new TestCaseExecution { Id = 2 }
            };

            // Act
            session.TestPlan = testPlan;
            session.Executor = executor;
            session.TestCaseExecutions = testCaseExecutions;

            // Assert
            Assert.Equal(testPlan, session.TestPlan);
            Assert.Equal(executor, session.Executor);
            Assert.Equal(testCaseExecutions, session.TestCaseExecutions);
            Assert.Equal(2, session.TestCaseExecutions.Count);
        }

        [Theory]
        [InlineData(TestRunStatus.Paused)]
        [InlineData(TestRunStatus.InProgress)]
        [InlineData(TestRunStatus.Completed)]
        [InlineData(TestRunStatus.Aborted)]
        public void TestRunSession_Status_AcceptsAllTestRunStatuses(TestRunStatus status)
        {
            // Arrange
            var session = new TestRunSession();

            // Act
            session.Status = status;

            // Assert
            Assert.Equal(status, session.Status);
        }

        [Fact]
        public void TestRunSession_TestCaseExecutions_CanAddItems()
        {
            // Arrange
            var session = new TestRunSession();
            var execution1 = new TestCaseExecution { Id = 1 };
            var execution2 = new TestCaseExecution { Id = 2 };

            // Act
            session.TestCaseExecutions.Add(execution1);
            session.TestCaseExecutions.Add(execution2);

            // Assert
            Assert.Equal(2, session.TestCaseExecutions.Count);
            Assert.Contains(execution1, session.TestCaseExecutions);
            Assert.Contains(execution2, session.TestCaseExecutions);
        }

        [Fact]
        public void TestRunSession_Name_DefaultsToEmptyString()
        {
            // Act
            var session = new TestRunSession();

            // Assert
            Assert.Equal(string.Empty, session.Name);
        }

        [Fact]
        public void TestRunSession_OptionalProperties_CanBeNull()
        {
            // Arrange
            var session = new TestRunSession();

            // Act & Assert
            Assert.Null(session.Description);
            Assert.Null(session.CompletedAt);
            Assert.Null(session.Environment);
            Assert.Null(session.BuildVersion);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Simple session name")]
        [InlineData("Complex session name with detailed information")]
        public void TestRunSession_Name_AcceptsVariousStrings(string name)
        {
            // Arrange
            var session = new TestRunSession();

            // Act
            session.Name = name;

            // Assert
            Assert.Equal(name, session.Name);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Development")]
        [InlineData("Staging")]
        [InlineData("Production")]
        public void TestRunSession_Environment_AcceptsVariousStrings(string environment)
        {
            // Arrange
            var session = new TestRunSession();

            // Act
            session.Environment = environment;

            // Assert
            Assert.Equal(environment, session.Environment);
        }

        [Theory]
        [InlineData("")]
        [InlineData("1.0.0")]
        [InlineData("2.1.3-beta")]
        [InlineData("v3.0.0-rc.1")]
        public void TestRunSession_BuildVersion_AcceptsVariousStrings(string buildVersion)
        {
            // Arrange
            var session = new TestRunSession();

            // Act
            session.BuildVersion = buildVersion;

            // Assert
            Assert.Equal(buildVersion, session.BuildVersion);
        }

        [Fact]
        public void TestRunSession_StartedAt_CanBeSetToSpecificDateTime()
        {
            // Arrange
            var session = new TestRunSession();
            var specificDateTime = new DateTime(2023, 12, 25, 10, 30, 45);

            // Act
            session.StartedAt = specificDateTime;

            // Assert
            Assert.Equal(specificDateTime, session.StartedAt);
        }

        [Fact]
        public void TestRunSession_CompletedAt_CanBeSetAndCleared()
        {
            // Arrange
            var session = new TestRunSession();
            var completedTime = DateTime.UtcNow;

            // Act & Assert - Set completion time
            session.CompletedAt = completedTime;
            Assert.Equal(completedTime, session.CompletedAt);

            // Act & Assert - Clear completion time
            session.CompletedAt = null;
            Assert.Null(session.CompletedAt);
        }
    }
}