using System;
using backend.Models;
using RqmtMgmtShared;
using Xunit;

namespace backend.Tests
{
    public class TestStepExecutionTests
    {
        [Fact]
        public void TestStepExecution_DefaultConstructor_InitializesProperties()
        {
            // Act
            var stepExecution = new TestStepExecution();

            // Assert
            Assert.Equal(0, stepExecution.Id);
            Assert.Equal(0, stepExecution.TestCaseExecutionId);
            Assert.Equal(0, stepExecution.TestStepId);
            Assert.Equal(0, stepExecution.StepOrder);
            Assert.Equal(default(TestResult), stepExecution.Result);
            Assert.Null(stepExecution.ActualResult);
            Assert.Null(stepExecution.Notes);
            Assert.Null(stepExecution.ExecutedAt);
            Assert.Null(stepExecution.TestCaseExecution);
            Assert.Null(stepExecution.TestStep);
        }

        [Fact]
        public void TestStepExecution_SetProperties_PropertiesSetCorrectly()
        {
            // Arrange
            var stepExecution = new TestStepExecution();
            var executedAt = DateTime.UtcNow;

            // Act
            stepExecution.Id = 1;
            stepExecution.TestCaseExecutionId = 100;
            stepExecution.TestStepId = 200;
            stepExecution.StepOrder = 1;
            stepExecution.Result = TestResult.Passed;
            stepExecution.ActualResult = "Login was successful";
            stepExecution.Notes = "Step executed without issues";
            stepExecution.ExecutedAt = executedAt;

            // Assert
            Assert.Equal(1, stepExecution.Id);
            Assert.Equal(100, stepExecution.TestCaseExecutionId);
            Assert.Equal(200, stepExecution.TestStepId);
            Assert.Equal(1, stepExecution.StepOrder);
            Assert.Equal(TestResult.Passed, stepExecution.Result);
            Assert.Equal("Login was successful", stepExecution.ActualResult);
            Assert.Equal("Step executed without issues", stepExecution.Notes);
            Assert.Equal(executedAt, stepExecution.ExecutedAt);
        }

        [Fact]
        public void TestStepExecution_NavigationProperties_CanBeSet()
        {
            // Arrange
            var stepExecution = new TestStepExecution();
            var testCaseExecution = new TestCaseExecution { Id = 100 };
            var testStep = new TestStep { Id = 200 };

            // Act
            stepExecution.TestCaseExecution = testCaseExecution;
            stepExecution.TestStep = testStep;

            // Assert
            Assert.Equal(testCaseExecution, stepExecution.TestCaseExecution);
            Assert.Equal(testStep, stepExecution.TestStep);
        }

        [Theory]
        [InlineData(TestResult.NotRun)]
        [InlineData(TestResult.Passed)]
        [InlineData(TestResult.Failed)]
        [InlineData(TestResult.Blocked)]
        public void TestStepExecution_Result_AcceptsAllTestResults(TestResult testResult)
        {
            // Arrange
            var stepExecution = new TestStepExecution();

            // Act
            stepExecution.Result = testResult;

            // Assert
            Assert.Equal(testResult, stepExecution.Result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(100)]
        public void TestStepExecution_StepOrder_AcceptsPositiveIntegers(int stepOrder)
        {
            // Arrange
            var stepExecution = new TestStepExecution();

            // Act
            stepExecution.StepOrder = stepOrder;

            // Assert
            Assert.Equal(stepOrder, stepExecution.StepOrder);
        }

        [Fact]
        public void TestStepExecution_OptionalProperties_CanBeNull()
        {
            // Arrange
            var stepExecution = new TestStepExecution();

            // Act & Assert
            Assert.Null(stepExecution.ActualResult);
            Assert.Null(stepExecution.Notes);
            Assert.Null(stepExecution.ExecutedAt);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Simple actual result")]
        [InlineData("Complex actual result with detailed observations")]
        [InlineData("Result with special characters: @#$%^&*()")]
        public void TestStepExecution_ActualResult_AcceptsVariousStrings(string actualResult)
        {
            // Arrange
            var stepExecution = new TestStepExecution();

            // Act
            stepExecution.ActualResult = actualResult;

            // Assert
            Assert.Equal(actualResult, stepExecution.ActualResult);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Simple notes")]
        [InlineData("Detailed notes about the step execution")]
        [InlineData("Notes with numbers: 123 and symbols: !@#")]
        public void TestStepExecution_Notes_AcceptsVariousStrings(string notes)
        {
            // Arrange
            var stepExecution = new TestStepExecution();

            // Act
            stepExecution.Notes = notes;

            // Assert
            Assert.Equal(notes, stepExecution.Notes);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(999)]
        [InlineData(int.MaxValue)]
        public void TestStepExecution_TestCaseExecutionId_AcceptsValidIds(int testCaseExecutionId)
        {
            // Arrange
            var stepExecution = new TestStepExecution();

            // Act
            stepExecution.TestCaseExecutionId = testCaseExecutionId;

            // Assert
            Assert.Equal(testCaseExecutionId, stepExecution.TestCaseExecutionId);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(999)]
        [InlineData(int.MaxValue)]
        public void TestStepExecution_TestStepId_AcceptsValidIds(int testStepId)
        {
            // Arrange
            var stepExecution = new TestStepExecution();

            // Act
            stepExecution.TestStepId = testStepId;

            // Assert
            Assert.Equal(testStepId, stepExecution.TestStepId);
        }

        [Fact]
        public void TestStepExecution_ExecutedAt_CanBeSetToSpecificDateTime()
        {
            // Arrange
            var stepExecution = new TestStepExecution();
            var specificDateTime = new DateTime(2023, 12, 25, 14, 30, 45);

            // Act
            stepExecution.ExecutedAt = specificDateTime;

            // Assert
            Assert.Equal(specificDateTime, stepExecution.ExecutedAt);
        }

        [Fact]
        public void TestStepExecution_ExecutedAt_CanBeSetAndCleared()
        {
            // Arrange
            var stepExecution = new TestStepExecution();
            var executedTime = DateTime.UtcNow;

            // Act & Assert - Set execution time
            stepExecution.ExecutedAt = executedTime;
            Assert.Equal(executedTime, stepExecution.ExecutedAt);

            // Act & Assert - Clear execution time
            stepExecution.ExecutedAt = null;
            Assert.Null(stepExecution.ExecutedAt);
        }

        [Fact]
        public void TestStepExecution_CanSetAllPropertiesAtOnce()
        {
            // Arrange
            var testCaseExecution = new TestCaseExecution { Id = 50 };
            var testStep = new TestStep { Id = 75 };
            var executedAt = DateTime.UtcNow;

            // Act
            var stepExecution = new TestStepExecution
            {
                Id = 25,
                TestCaseExecutionId = 50,
                TestStepId = 75,
                StepOrder = 3,
                Result = TestResult.Failed,
                ActualResult = "Error occurred during execution",
                Notes = "Need to investigate further",
                ExecutedAt = executedAt,
                TestCaseExecution = testCaseExecution,
                TestStep = testStep
            };

            // Assert
            Assert.Equal(25, stepExecution.Id);
            Assert.Equal(50, stepExecution.TestCaseExecutionId);
            Assert.Equal(75, stepExecution.TestStepId);
            Assert.Equal(3, stepExecution.StepOrder);
            Assert.Equal(TestResult.Failed, stepExecution.Result);
            Assert.Equal("Error occurred during execution", stepExecution.ActualResult);
            Assert.Equal("Need to investigate further", stepExecution.Notes);
            Assert.Equal(executedAt, stepExecution.ExecutedAt);
            Assert.Equal(testCaseExecution, stepExecution.TestCaseExecution);
            Assert.Equal(testStep, stepExecution.TestStep);
        }

        [Fact]
        public void TestStepExecution_NavigationProperties_CanBeSetToNull()
        {
            // Arrange
            var stepExecution = new TestStepExecution();
            var testCaseExecution = new TestCaseExecution { Id = 100 };
            var testStep = new TestStep { Id = 200 };

            // Act - Set then clear navigation properties
            stepExecution.TestCaseExecution = testCaseExecution;
            stepExecution.TestStep = testStep;
            stepExecution.TestCaseExecution = null;
            stepExecution.TestStep = null;

            // Assert
            Assert.Null(stepExecution.TestCaseExecution);
            Assert.Null(stepExecution.TestStep);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void TestStepExecution_StepOrder_AcceptsZeroAndNegativeNumbers(int stepOrder)
        {
            // Arrange
            var stepExecution = new TestStepExecution();

            // Act
            stepExecution.StepOrder = stepOrder;

            // Assert
            Assert.Equal(stepOrder, stepExecution.StepOrder);
        }
    }
}