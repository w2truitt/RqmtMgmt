using Xunit;
using RqmtMgmtShared;
using backend.Services;
using System.Collections.Generic;
using System.Linq;

namespace backend.Tests.Services
{
    /// <summary>
    /// Unit tests for TestRunStatusCount helper class
    /// </summary>
    public class TestRunStatusCountTests
    {
        [Fact]
        public void TestRunStatusCount_DefaultConstructor_SetsDefaultValues()
        {
            // Arrange & Act
            var statusCount = new TestRunStatusCount();

            // Assert
            Assert.Equal(default(TestRunStatus), statusCount.Status);
            Assert.Equal(0, statusCount.Count);
        }

        [Fact]
        public void TestRunStatusCount_SetStatus_UpdatesStatusProperty()
        {
            // Arrange
            var statusCount = new TestRunStatusCount();
            var expectedStatus = TestRunStatus.InProgress;

            // Act
            statusCount.Status = expectedStatus;

            // Assert
            Assert.Equal(expectedStatus, statusCount.Status);
        }

        [Fact]
        public void TestRunStatusCount_SetCount_UpdatesCountProperty()
        {
            // Arrange
            var statusCount = new TestRunStatusCount();
            var expectedCount = 42;

            // Act
            statusCount.Count = expectedCount;

            // Assert
            Assert.Equal(expectedCount, statusCount.Count);
        }

        [Theory]
        [InlineData(TestRunStatus.InProgress, 5)]
        [InlineData(TestRunStatus.Completed, 10)]
        [InlineData(TestRunStatus.Aborted, 3)]
        [InlineData(TestRunStatus.Paused, 7)]
        public void TestRunStatusCount_SetProperties_WorksForAllStatusValues(TestRunStatus status, int count)
        {
            // Arrange
            var statusCount = new TestRunStatusCount();

            // Act
            statusCount.Status = status;
            statusCount.Count = count;

            // Assert
            Assert.Equal(status, statusCount.Status);
            Assert.Equal(count, statusCount.Count);
        }

        [Fact]
        public void TestRunStatusCount_SetNegativeCount_AllowsNegativeValues()
        {
            // Arrange
            var statusCount = new TestRunStatusCount();
            var negativeCount = -5;

            // Act
            statusCount.Count = negativeCount;

            // Assert
            Assert.Equal(negativeCount, statusCount.Count);
        }

        [Fact]
        public void TestRunStatusCount_MultipleAssignments_RetainsLastValue()
        {
            // Arrange
            var statusCount = new TestRunStatusCount();

            // Act
            statusCount.Status = TestRunStatus.InProgress;
            statusCount.Count = 1;
            statusCount.Status = TestRunStatus.Completed;
            statusCount.Count = 100;

            // Assert
            Assert.Equal(TestRunStatus.Completed, statusCount.Status);
            Assert.Equal(100, statusCount.Count);
        }
    }

    /// <summary>
    /// Unit tests for TestResultCount helper class
    /// </summary>
    public class TestResultCountTests
    {
        [Fact]
        public void TestResultCount_DefaultConstructor_SetsDefaultValues()
        {
            // Arrange & Act
            var resultCount = new TestResultCount();

            // Assert
            Assert.Equal(default(TestResult), resultCount.Result);
            Assert.Equal(0, resultCount.Count);
        }

        [Fact]
        public void TestResultCount_SetResult_UpdatesResultProperty()
        {
            // Arrange
            var resultCount = new TestResultCount();
            var expectedResult = TestResult.Passed;

            // Act
            resultCount.Result = expectedResult;

            // Assert
            Assert.Equal(expectedResult, resultCount.Result);
        }

        [Fact]
        public void TestResultCount_SetCount_UpdatesCountProperty()
        {
            // Arrange
            var resultCount = new TestResultCount();
            var expectedCount = 25;

            // Act
            resultCount.Count = expectedCount;

            // Assert
            Assert.Equal(expectedCount, resultCount.Count);
        }

        [Theory]
        [InlineData(TestResult.NotRun, 0)]
        [InlineData(TestResult.Passed, 15)]
        [InlineData(TestResult.Failed, 3)]
        [InlineData(TestResult.Blocked, 7)]
        public void TestResultCount_SetProperties_WorksForAllResultValues(TestResult result, int count)
        {
            // Arrange
            var resultCount = new TestResultCount();

            // Act
            resultCount.Result = result;
            resultCount.Count = count;

            // Assert
            Assert.Equal(result, resultCount.Result);
            Assert.Equal(count, resultCount.Count);
        }

        [Fact]
        public void TestResultCount_SetNegativeCount_AllowsNegativeValues()
        {
            // Arrange
            var resultCount = new TestResultCount();
            var negativeCount = -10;

            // Act
            resultCount.Count = negativeCount;

            // Assert
            Assert.Equal(negativeCount, resultCount.Count);
        }

        [Fact]
        public void TestResultCount_MultipleAssignments_RetainsLastValue()
        {
            // Arrange
            var resultCount = new TestResultCount();

            // Act
            resultCount.Result = TestResult.NotRun;
            resultCount.Count = 1;
            resultCount.Result = TestResult.Failed;
            resultCount.Count = 50;

            // Assert
            Assert.Equal(TestResult.Failed, resultCount.Result);
            Assert.Equal(50, resultCount.Count);
        }

        [Fact]
        public void TestResultCount_IndependentInstances_DoNotInterfereWithEachOther()
        {
            // Arrange
            var resultCount1 = new TestResultCount();
            var resultCount2 = new TestResultCount();

            // Act
            resultCount1.Result = TestResult.Passed;
            resultCount1.Count = 10;
            resultCount2.Result = TestResult.Failed;
            resultCount2.Count = 5;

            // Assert
            Assert.Equal(TestResult.Passed, resultCount1.Result);
            Assert.Equal(10, resultCount1.Count);
            Assert.Equal(TestResult.Failed, resultCount2.Result);
            Assert.Equal(5, resultCount2.Count);
        }
    }

    /// <summary>
    /// Integration tests for both helper classes working together
    /// </summary>
    public class TestExecutionHelperClassesIntegrationTests
    {
        [Fact]
        public void HelperClasses_CanBeUsedInCollections_WorkCorrectly()
        {
            // Arrange
            var statusCounts = new List<TestRunStatusCount>
            {
                new() { Status = TestRunStatus.InProgress, Count = 5 },
                new() { Status = TestRunStatus.Completed, Count = 10 },
                new() { Status = TestRunStatus.Aborted, Count = 2 }
            };

            var resultCounts = new List<TestResultCount>
            {
                new() { Result = TestResult.Passed, Count = 15 },
                new() { Result = TestResult.Failed, Count = 3 },
                new() { Result = TestResult.Blocked, Count = 1 }
            };

            // Act
            var totalStatusCount = statusCounts.Sum(s => s.Count);
            var totalResultCount = resultCounts.Sum(r => r.Count);
            var inProgressSessions = statusCounts.FirstOrDefault(s => s.Status == TestRunStatus.InProgress)?.Count ?? 0;
            var passedTests = resultCounts.FirstOrDefault(r => r.Result == TestResult.Passed)?.Count ?? 0;

            // Assert
            Assert.Equal(17, totalStatusCount);
            Assert.Equal(19, totalResultCount);
            Assert.Equal(5, inProgressSessions);
            Assert.Equal(15, passedTests);
        }

        [Fact]
        public void HelperClasses_CanBeUsedInLinqOperations_WorkCorrectly()
        {
            // Arrange
            var statusCounts = new List<TestRunStatusCount>
            {
                new() { Status = TestRunStatus.InProgress, Count = 5 },
                new() { Status = TestRunStatus.Completed, Count = 10 },
                new() { Status = TestRunStatus.Aborted, Count = 2 },
                new() { Status = TestRunStatus.Paused, Count = 3 }
            };

            // Act
            var activeSessions = statusCounts
                .Where(s => s.Status == TestRunStatus.InProgress || s.Status == TestRunStatus.Paused)
                .Sum(s => s.Count);

            var completedSessions = statusCounts
                .Where(s => s.Status == TestRunStatus.Completed)
                .Select(s => s.Count)
                .FirstOrDefault();

            // Assert
            Assert.Equal(8, activeSessions); // 5 InProgress + 3 Paused
            Assert.Equal(10, completedSessions);
        }

        [Fact]
        public void HelperClasses_EmptyCollections_HandleGracefully()
        {
            // Arrange
            var emptyStatusCounts = new List<TestRunStatusCount>();
            var emptyResultCounts = new List<TestResultCount>();

            // Act
            var totalStatusCount = emptyStatusCounts.Sum(s => s.Count);
            var totalResultCount = emptyResultCounts.Sum(r => r.Count);
            var inProgressCount = emptyStatusCounts.FirstOrDefault(s => s.Status == TestRunStatus.InProgress)?.Count ?? 0;

            // Assert
            Assert.Equal(0, totalStatusCount);
            Assert.Equal(0, totalResultCount);
            Assert.Equal(0, inProgressCount);
        }
    }
}