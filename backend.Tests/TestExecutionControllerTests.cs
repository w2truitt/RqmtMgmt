using System;
using System.Threading.Tasks;
using backend.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RqmtMgmtShared;
using Xunit;

namespace backend.Tests
{
    public class TestExecutionControllerTests
    {
        [Fact]
        public async Task ExecuteTestCase_ValidExecution_ReturnsOkResult()
        {
            // Arrange
            var mockService = new Mock<ITestExecutionService>();
            var controller = new TestExecutionController(mockService.Object);

            var executionDto = new TestCaseExecutionDto
            {
                TestRunSessionId = 1,
                TestCaseId = 1,
                OverallResult = TestResult.Passed,
                ExecutedBy = 1
            };

            var expectedResult = new TestCaseExecutionDto
            {
                Id = 1,
                TestRunSessionId = 1,
                TestCaseId = 1,
                OverallResult = TestResult.Passed,
                ExecutedBy = 1,
                ExecutedAt = DateTime.UtcNow
            };

            mockService.Setup(s => s.ExecuteTestCaseAsync(It.IsAny<TestCaseExecutionDto>()))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await controller.ExecuteTestCase(executionDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedExecution = Assert.IsType<TestCaseExecutionDto>(okResult.Value);
            Assert.Equal(expectedResult.Id, returnedExecution.Id);
        }

        [Fact]
        public async Task ExecuteTestCase_ServiceReturnsNull_ReturnsInternalServerError()
        {
            // Arrange
            var mockService = new Mock<ITestExecutionService>();
            var controller = new TestExecutionController(mockService.Object);

            var executionDto = new TestCaseExecutionDto
            {
                TestRunSessionId = 1,
                TestCaseId = 1,
                OverallResult = TestResult.Passed
            };

            mockService.Setup(s => s.ExecuteTestCaseAsync(It.IsAny<TestCaseExecutionDto>()))
                      .ReturnsAsync((TestCaseExecutionDto)null);

            // Act
            var result = await controller.ExecuteTestCase(executionDto);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
        }
    }
}