using System;
using System.Collections.Generic;
using backend.Models;
using RqmtMgmtShared;
using Xunit;

namespace backend.Tests
{
    public class TestCaseExecutionTests
    {
        [Fact]
        public void TestCaseExecution_DefaultConstructor_InitializesProperties()
        {
            // Act
            var execution = new TestCaseExecution();

            // Assert
            Assert.Equal(0, execution.Id);
            Assert.Equal(0, execution.TestRunSessionId);
            Assert.Equal(0, execution.TestCaseId);
            Assert.Equal(default(TestResult), execution.OverallResult);
            Assert.Null(execution.ExecutedAt);
            Assert.Null(execution.ExecutedBy);
            Assert.Null(execution.Notes);
            Assert.Null(execution.DefectId);
            Assert.Null(execution.TestRunSession);
            Assert.Null(execution.TestCase);
            Assert.Null(execution.Executor);
            Assert.NotNull(execution.TestStepExecutions);
            Assert.Empty(execution.TestStepExecutions);
        }

        [Fact]
        public void TestCaseExecution_SetProperties_PropertiesSetCorrectly()
        {
            // Arrange
            var execution = new TestCaseExecution();
            var executedAt = DateTime.UtcNow;

            // Act
            execution.Id = 1;
            execution.TestRunSessionId = 100;
            execution.TestCaseId = 200;
            execution.OverallResult = TestResult.Passed;
            execution.ExecutedAt = executedAt;
            execution.ExecutedBy = 300;
            execution.Notes = "Test execution notes";
            execution.DefectId = "DEF-001";

            // Assert
            Assert.Equal(1, execution.Id);
            Assert.Equal(100, execution.TestRunSessionId);
            Assert.Equal(200, execution.TestCaseId);
            Assert.Equal(TestResult.Passed, execution.OverallResult);
            Assert.Equal(executedAt, execution.ExecutedAt);
            Assert.Equal(300, execution.ExecutedBy);
            Assert.Equal("Test execution notes", execution.Notes);
            Assert.Equal("DEF-001", execution.DefectId);
        }

        [Fact]
        public void TestCaseExecution_NavigationProperties_CanBeSet()
        {
            // Arrange
            var execution = new TestCaseExecution();
            var testRunSession = new TestRunSession { Id = 100 };
            var testCase = new TestCase { Id = 200, Title = "Test Case" };
            var executor = new User { Id = 300, UserName = "testuser", Email = "test@example.com" };
            var stepExecutions = new List<TestStepExecution>
            {
                new TestStepExecution { Id = 1 },
                new TestStepExecution { Id = 2 }
            };

            // Act
            execution.TestRunSession = testRunSession;
            execution.TestCase = testCase;
            execution.Executor = executor;
            execution.TestStepExecutions = stepExecutions;

            // Assert
            Assert.Equal(testRunSession, execution.TestRunSession);
            Assert.Equal(testCase, execution.TestCase);
            Assert.Equal(executor, execution.Executor);
            Assert.Equal(stepExecutions, execution.TestStepExecutions);
            Assert.Equal(2, execution.TestStepExecutions.Count);
        }

        [Theory]
        [InlineData(TestResult.NotRun)]
        [InlineData(TestResult.Passed)]
        [InlineData(TestResult.Failed)]
        [InlineData(TestResult.Blocked)]
        public void TestCaseExecution_OverallResult_AcceptsAllTestResults(TestResult testResult)
        {
            // Arrange
            var execution = new TestCaseExecution();

            // Act
            execution.OverallResult = testResult;

            // Assert
            Assert.Equal(testResult, execution.OverallResult);
        }

        [Fact]
        public void TestCaseExecution_TestStepExecutions_CanAddItems()
        {
            // Arrange
            var execution = new TestCaseExecution();
            var stepExecution1 = new TestStepExecution { Id = 1 };
            var stepExecution2 = new TestStepExecution { Id = 2 };

            // Act
            execution.TestStepExecutions.Add(stepExecution1);
            execution.TestStepExecutions.Add(stepExecution2);

            // Assert
            Assert.Equal(2, execution.TestStepExecutions.Count);
            Assert.Contains(stepExecution1, execution.TestStepExecutions);
            Assert.Contains(stepExecution2, execution.TestStepExecutions);
        }

        [Fact]
        public void TestCaseExecution_OptionalProperties_CanBeNull()
        {
            // Arrange
            var execution = new TestCaseExecution();

            // Act & Assert
            Assert.Null(execution.ExecutedAt);
            Assert.Null(execution.ExecutedBy);
            Assert.Null(execution.Notes);
            Assert.Null(execution.DefectId);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Simple notes")]
        [InlineData("Complex notes with detailed information about the test execution")]
        public void TestCaseExecution_Notes_AcceptsVariousStrings(string notes)
        {
            // Arrange
            var execution = new TestCaseExecution();

            // Act
            execution.Notes = notes;

            // Assert
            Assert.Equal(notes, execution.Notes);
        }

        [Theory]
        [InlineData("")]
        [InlineData("BUG-001")]
        [InlineData("DEFECT-12345")]
        public void TestCaseExecution_DefectId_AcceptsVariousStrings(string defectId)
        {
            // Arrange
            var execution = new TestCaseExecution();

            // Act
            execution.DefectId = defectId;

            // Assert
            Assert.Equal(defectId, execution.DefectId);
        }
    }
}