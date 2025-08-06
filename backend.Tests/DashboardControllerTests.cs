using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RqmtMgmtShared;
using Xunit;

namespace backend.Tests
{
    public class DashboardControllerTests
    {
        private readonly Mock<IDashboardService> _mockDashboardService;
        private readonly Mock<IEnhancedDashboardService> _mockEnhancedDashboardService;
        private readonly Mock<ILogger<DashboardController>> _mockLogger;
        private readonly DashboardController _controller;

        public DashboardControllerTests()
        {
            _mockDashboardService = new Mock<IDashboardService>();
            _mockEnhancedDashboardService = new Mock<IEnhancedDashboardService>();
            _mockLogger = new Mock<ILogger<DashboardController>>();
            _controller = new DashboardController(
                _mockDashboardService.Object, 
                _mockEnhancedDashboardService.Object, 
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetStatistics_ReturnsOkResult_WithDashboardStatistics()
        {
            // Arrange
            var expectedStats = new DashboardStatisticsDto
            {
                Requirements = new RequirementStatisticsDto { Total = 10, Approved = 5, Draft = 3, Implemented = 1, Verified = 1 },
                TestSuites = new TestSuiteStatisticsDto { Total = 5, Active = 4, Completed = 1 },
                TestCases = new TestCaseStatisticsDto { Total = 20, Passed = 15, Failed = 3, NotRun = 2 },
                TestPlans = new TestPlanStatisticsDto { Total = 3, ExecutionProgress = 75, CoveragePercentage = 80 }
            };
            _mockDashboardService.Setup(s => s.GetStatisticsAsync()).ReturnsAsync(expectedStats);

            // Act
            var result = await _controller.GetStatistics();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualStats = Assert.IsType<DashboardStatisticsDto>(okResult.Value);
            Assert.Equal(expectedStats.Requirements.Total, actualStats.Requirements.Total);
            Assert.Equal(expectedStats.TestSuites.Total, actualStats.TestSuites.Total);
            Assert.Equal(expectedStats.TestCases.Total, actualStats.TestCases.Total);
            Assert.Equal(expectedStats.TestPlans.Total, actualStats.TestPlans.Total);
        }

        [Fact]
        public async Task GetStatistics_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            _mockDashboardService.Setup(s => s.GetStatisticsAsync()).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetStatistics();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetEnhancedStatistics_ReturnsOkResult_WithEnhancedStatistics()
        {
            // Arrange
            var expectedStats = new DashboardStatsDto
            {
                Requirements = new RequirementStatsDto { TotalRequirements = 10 },
                TestManagement = new TestManagementStatsDto { TotalTestCases = 20, TotalTestSuites = 5, TotalTestPlans = 3 },
                TestExecution = new TestExecutionStatsDto { PassRate = 75.5 }
            };
            _mockEnhancedDashboardService.Setup(s => s.GetDashboardStatsAsync()).ReturnsAsync(expectedStats);

            // Act
            var result = await _controller.GetEnhancedStatistics();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualStats = Assert.IsType<DashboardStatsDto>(okResult.Value);
            Assert.Equal(expectedStats.Requirements.TotalRequirements, actualStats.Requirements.TotalRequirements);
            Assert.Equal(expectedStats.TestManagement.TotalTestCases, actualStats.TestManagement.TotalTestCases);
            Assert.Equal(expectedStats.TestExecution.PassRate, actualStats.TestExecution.PassRate);
        }

        [Fact]
        public async Task GetEnhancedStatistics_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            _mockEnhancedDashboardService.Setup(s => s.GetDashboardStatsAsync()).ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.GetEnhancedStatistics();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetRequirementStats_ReturnsOkResult_WithRequirementStats()
        {
            // Arrange
            var expectedStats = new RequirementStatsDto
            {
                TotalRequirements = 100,
                ByStatus = new Dictionary<RequirementStatus, int>
                {
                    { RequirementStatus.Draft, 30 },
                    { RequirementStatus.Approved, 50 },
                    { RequirementStatus.Implemented, 15 },
                    { RequirementStatus.Verified, 5 }
                }
            };
            _mockEnhancedDashboardService.Setup(s => s.GetRequirementStatsAsync()).ReturnsAsync(expectedStats);

            // Act
            var result = await _controller.GetRequirementStats();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualStats = Assert.IsType<RequirementStatsDto>(okResult.Value);
            Assert.Equal(expectedStats.TotalRequirements, actualStats.TotalRequirements);
            Assert.Equal(4, actualStats.ByStatus.Count);
        }

        [Fact]
        public async Task GetRequirementStats_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            _mockEnhancedDashboardService.Setup(s => s.GetRequirementStatsAsync()).ThrowsAsync(new Exception("Stats error"));

            // Act
            var result = await _controller.GetRequirementStats();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetTestManagementStats_ReturnsOkResult_WithTestManagementStats()
        {
            // Arrange
            var expectedStats = new TestManagementStatsDto
            {
                TotalTestSuites = 10,
                TotalTestCases = 50,
                TotalTestPlans = 5,
                TestCoveragePercentage = 80.0
            };
            _mockEnhancedDashboardService.Setup(s => s.GetTestManagementStatsAsync()).ReturnsAsync(expectedStats);

            // Act
            var result = await _controller.GetTestManagementStats();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualStats = Assert.IsType<TestManagementStatsDto>(okResult.Value);
            Assert.Equal(expectedStats.TotalTestSuites, actualStats.TotalTestSuites);
            Assert.Equal(expectedStats.TotalTestCases, actualStats.TotalTestCases);
            Assert.Equal(expectedStats.TestCoveragePercentage, actualStats.TestCoveragePercentage);
        }

        [Fact]
        public async Task GetTestManagementStats_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            _mockEnhancedDashboardService.Setup(s => s.GetTestManagementStatsAsync()).ThrowsAsync(new Exception("Management stats error"));

            // Act
            var result = await _controller.GetTestManagementStats();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetTestExecutionStats_ReturnsOkResult_WithTestExecutionStats()
        {
            // Arrange
            var expectedStats = new TestExecutionStatsDto
            {
                TotalTestCaseExecutions = 100,
                PassedExecutions = 80,
                FailedExecutions = 15,
                NotRunExecutions = 5,
                PassRate = 95.0
            };
            _mockEnhancedDashboardService.Setup(s => s.GetTestExecutionStatsAsync()).ReturnsAsync(expectedStats);

            // Act
            var result = await _controller.GetTestExecutionStats();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualStats = Assert.IsType<TestExecutionStatsDto>(okResult.Value);
            Assert.Equal(expectedStats.TotalTestCaseExecutions, actualStats.TotalTestCaseExecutions);
            Assert.Equal(expectedStats.PassedExecutions, actualStats.PassedExecutions);
            Assert.Equal(expectedStats.PassRate, actualStats.PassRate);
        }

        [Fact]
        public async Task GetTestExecutionStats_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            _mockEnhancedDashboardService.Setup(s => s.GetTestExecutionStatsAsync()).ThrowsAsync(new Exception("Execution stats error"));

            // Act
            var result = await _controller.GetTestExecutionStats();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetRecentActivity_ReturnsOkResult_WithDefaultCount()
        {
            // Arrange
            var expectedActivities = new List<RecentActivityDto>
            {
                new RecentActivityDto { Id = 1, Description = "Requirement created", EntityType = "Requirement", Action = "Created", UserId = 1, UserName = "testuser", CreatedAt = DateTime.UtcNow },
                new RecentActivityDto { Id = 2, Description = "Test case updated", EntityType = "TestCase", Action = "Updated", UserId = 2, UserName = "testuser2", CreatedAt = DateTime.UtcNow.AddMinutes(-10) }
            };
            _mockEnhancedDashboardService.Setup(s => s.GetRecentActivityAsync(5)).ReturnsAsync(expectedActivities);

            // Act
            var result = await _controller.GetRecentActivity();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualActivities = Assert.IsType<List<RecentActivityDto>>(okResult.Value);
            Assert.Equal(2, actualActivities.Count);
            Assert.Equal("Requirement created", actualActivities[0].Description);
        }

        [Fact]
        public async Task GetRecentActivity_ReturnsOkResult_WithCustomCount()
        {
            // Arrange
            var expectedActivities = new List<RecentActivityDto>
            {
                new RecentActivityDto { Id = 1, Description = "Activity 1", EntityType = "Requirement", Action = "Created", UserId = 1, UserName = "user1", CreatedAt = DateTime.UtcNow }
            };
            _mockEnhancedDashboardService.Setup(s => s.GetRecentActivityAsync(10)).ReturnsAsync(expectedActivities);

            // Act
            var result = await _controller.GetRecentActivity(10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualActivities = Assert.IsType<List<RecentActivityDto>>(okResult.Value);
            Assert.Single(actualActivities);
        }

        [Fact]
        public async Task GetRecentActivity_ReturnsBadRequest_WhenCountIsZero()
        {
            // Act
            var result = await _controller.GetRecentActivity(0);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = badRequestResult.Value;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task GetRecentActivity_ReturnsBadRequest_WhenCountIsNegative()
        {
            // Act
            var result = await _controller.GetRecentActivity(-1);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = badRequestResult.Value;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task GetRecentActivity_ReturnsBadRequest_WhenCountExceedsLimit()
        {
            // Act
            var result = await _controller.GetRecentActivity(51);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = badRequestResult.Value;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task GetRecentActivity_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            _mockEnhancedDashboardService.Setup(s => s.GetRecentActivityAsync(It.IsAny<int>())).ThrowsAsync(new Exception("Activity error"));

            // Act
            var result = await _controller.GetRecentActivity();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}