using System;
using System.Collections.Generic;
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
        public async Task ExecuteTestCase_ServiceThrows_Returns500()
        {
            var mock = new Mock<ITestExecutionService>();
            mock.Setup(s => s.ExecuteTestCaseAsync(It.IsAny<TestCaseExecutionDto>())).ThrowsAsync(new Exception("fail"));
            var controller = new TestExecutionController(mock.Object);
            var dto = new TestCaseExecutionDto { TestRunSessionId = 1, TestCaseId = 1, OverallResult = TestResult.Passed };
            var result = await controller.ExecuteTestCase(dto);
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task UpdateTestCaseExecution_ServiceThrows_Returns500()
        {
            var mock = new Mock<ITestExecutionService>();
            mock.Setup(s => s.UpdateTestCaseExecutionAsync(It.IsAny<TestCaseExecutionDto>())).ThrowsAsync(new Exception("fail"));
            var controller = new TestExecutionController(mock.Object);
            var dto = new TestCaseExecutionDto { Id = 1 };
            var result = await controller.UpdateTestCaseExecution(1, dto);
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task UpdateStepResult_ServiceThrows_Returns500()
        {
            var mock = new Mock<ITestExecutionService>();
            mock.Setup(s => s.UpdateStepResultAsync(It.IsAny<TestStepExecutionDto>())).ThrowsAsync(new Exception("fail"));
            var controller = new TestExecutionController(mock.Object);
            var dto = new TestStepExecutionDto { Id = 1 };
            var result = await controller.UpdateStepResult(dto);
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }
        [Fact]
        public async Task ExecuteTestCase_ValidExecution_ReturnsOkResult()
        {
            var mockService = new Mock<ITestExecutionService>();
            var controller = new TestExecutionController(mockService.Object);
            var executionDto = new TestCaseExecutionDto { TestRunSessionId = 1, TestCaseId = 1, OverallResult = TestResult.Passed, ExecutedBy = 1 };
            var expectedResult = new TestCaseExecutionDto { Id = 1, TestRunSessionId = 1, TestCaseId = 1, OverallResult = TestResult.Passed, ExecutedBy = 1, ExecutedAt = DateTime.UtcNow };
            mockService.Setup(s => s.ExecuteTestCaseAsync(It.IsAny<TestCaseExecutionDto>())).ReturnsAsync(expectedResult);
            var result = await controller.ExecuteTestCase(executionDto);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedExecution = Assert.IsType<TestCaseExecutionDto>(okResult.Value);
            Assert.Equal(expectedResult.Id, returnedExecution.Id);
        }

        [Fact]
        public async Task ExecuteTestCase_ServiceReturnsNull_ReturnsInternalServerError()
        {
            var mockService = new Mock<ITestExecutionService>();
            var controller = new TestExecutionController(mockService.Object);
            var executionDto = new TestCaseExecutionDto { TestRunSessionId = 1, TestCaseId = 1, OverallResult = TestResult.Passed };
            mockService.Setup(s => s.ExecuteTestCaseAsync(It.IsAny<TestCaseExecutionDto>())).ReturnsAsync((TestCaseExecutionDto)null);
            var result = await controller.ExecuteTestCase(executionDto);
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        [Fact]
        public async Task ExecuteTestCase_InvalidModelState_ReturnsBadRequest()
        {
            var mockService = new Mock<ITestExecutionService>();
            var controller = new TestExecutionController(mockService.Object);
            controller.ModelState.AddModelError("TestField", "Required");
            var executionDto = new TestCaseExecutionDto();
            var result = await controller.ExecuteTestCase(executionDto);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateTestCaseExecution_IdMismatch_ReturnsBadRequest()
        {
            var mockService = new Mock<ITestExecutionService>();
            var controller = new TestExecutionController(mockService.Object);
            var dto = new TestCaseExecutionDto { Id = 2 };
            var result = await controller.UpdateTestCaseExecution(1, dto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateTestCaseExecution_InvalidModelState_ReturnsBadRequest()
        {
            var mockService = new Mock<ITestExecutionService>();
            var controller = new TestExecutionController(mockService.Object);
            controller.ModelState.AddModelError("Field", "error");
            var dto = new TestCaseExecutionDto { Id = 1 };
            var result = await controller.UpdateTestCaseExecution(1, dto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateTestCaseExecution_NotFound_ReturnsNotFound()
        {
            var mockService = new Mock<ITestExecutionService>();
            mockService.Setup(s => s.UpdateTestCaseExecutionAsync(It.IsAny<TestCaseExecutionDto>())).ReturnsAsync(false);
            var controller = new TestExecutionController(mockService.Object);
            var dto = new TestCaseExecutionDto { Id = 1 };
            var result = await controller.UpdateTestCaseExecution(1, dto);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateTestCaseExecution_Success_ReturnsNoContent()
        {
            var mockService = new Mock<ITestExecutionService>();
            mockService.Setup(s => s.UpdateTestCaseExecutionAsync(It.IsAny<TestCaseExecutionDto>())).ReturnsAsync(true);
            var controller = new TestExecutionController(mockService.Object);
            var dto = new TestCaseExecutionDto { Id = 1 };
            var result = await controller.UpdateTestCaseExecution(1, dto);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateStepResult_InvalidModelState_ReturnsBadRequest()
        {
            var mockService = new Mock<ITestExecutionService>();
            var controller = new TestExecutionController(mockService.Object);
            controller.ModelState.AddModelError("step", "error");
            var dto = new TestStepExecutionDto();
            var result = await controller.UpdateStepResult(dto);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateStepResult_ReturnsOk_WhenFound()
        {
            var mockService = new Mock<ITestExecutionService>();
            mockService.Setup(s => s.UpdateStepResultAsync(It.IsAny<TestStepExecutionDto>())).ReturnsAsync(new TestStepExecutionDto { Id = 1 });
            var controller = new TestExecutionController(mockService.Object);
            var dto = new TestStepExecutionDto { Id = 1 };
            var result = await controller.UpdateStepResult(dto);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<TestStepExecutionDto>(ok.Value);
        }

        [Fact]
        public async Task UpdateStepResult_ReturnsInternalServerError_WhenNull()
        {
            var mockService = new Mock<ITestExecutionService>();
            mockService.Setup(s => s.UpdateStepResultAsync(It.IsAny<TestStepExecutionDto>())).ReturnsAsync((TestStepExecutionDto)null);
            var controller = new TestExecutionController(mockService.Object);
            var dto = new TestStepExecutionDto { Id = 1 };
            var result = await controller.UpdateStepResult(dto);
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        #region GetExecutionsForSession Tests

        [Fact]
        public async Task GetExecutionsForSession_ServiceThrows_Returns500()
        {
            var mock = new Mock<ITestExecutionService>();
            mock.Setup(s => s.GetExecutionsForSessionAsync(It.IsAny<int>())).ThrowsAsync(new Exception("fail"));
            var controller = new TestExecutionController(mock.Object);
            var result = await controller.GetExecutionsForSession(1);
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task GetExecutionsForSession_ReturnsOkWithExecutions()
        {
            var mockService = new Mock<ITestExecutionService>();
            var controller = new TestExecutionController(mockService.Object);
            var executions = new List<TestCaseExecutionDto> 
            { 
                new TestCaseExecutionDto { Id = 1, TestCaseId = 1, TestRunSessionId = 1 }, 
                new TestCaseExecutionDto { Id = 2, TestCaseId = 2, TestRunSessionId = 1 } 
            };
            mockService.Setup(s => s.GetExecutionsForSessionAsync(1)).ReturnsAsync(executions);
            var result = await controller.GetExecutionsForSession(1);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedExecutions = Assert.IsType<List<TestCaseExecutionDto>>(okResult.Value);
            Assert.Equal(2, returnedExecutions.Count);
        }

        [Fact]
        public async Task GetExecutionsForSession_EmptyList_ReturnsOkWithEmptyList()
        {
            var mockService = new Mock<ITestExecutionService>();
            var controller = new TestExecutionController(mockService.Object);
            var executions = new List<TestCaseExecutionDto>();
            mockService.Setup(s => s.GetExecutionsForSessionAsync(1)).ReturnsAsync(executions);
            var result = await controller.GetExecutionsForSession(1);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedExecutions = Assert.IsType<List<TestCaseExecutionDto>>(okResult.Value);
            Assert.Empty(returnedExecutions);
        }

        #endregion

        #region GetStepExecutionsForCase Tests

        [Fact]
        public async Task GetStepExecutionsForCase_ServiceThrows_Returns500()
        {
            var mock = new Mock<ITestExecutionService>();
            mock.Setup(s => s.GetStepExecutionsForCaseAsync(It.IsAny<int>())).ThrowsAsync(new Exception("fail"));
            var controller = new TestExecutionController(mock.Object);
            var result = await controller.GetStepExecutionsForCase(1);
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task GetStepExecutionsForCase_ReturnsOkWithStepExecutions()
        {
            var mockService = new Mock<ITestExecutionService>();
            var controller = new TestExecutionController(mockService.Object);
            var stepExecutions = new List<TestStepExecutionDto> 
            { 
                new TestStepExecutionDto { Id = 1, TestCaseExecutionId = 1 }, 
                new TestStepExecutionDto { Id = 2, TestCaseExecutionId = 1 } 
            };
            mockService.Setup(s => s.GetStepExecutionsForCaseAsync(1)).ReturnsAsync(stepExecutions);
            var result = await controller.GetStepExecutionsForCase(1);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedStepExecutions = Assert.IsType<List<TestStepExecutionDto>>(okResult.Value);
            Assert.Equal(2, returnedStepExecutions.Count);
        }

        [Fact]
        public async Task GetStepExecutionsForCase_EmptyList_ReturnsOkWithEmptyList()
        {
            var mockService = new Mock<ITestExecutionService>();
            var controller = new TestExecutionController(mockService.Object);
            var stepExecutions = new List<TestStepExecutionDto>();
            mockService.Setup(s => s.GetStepExecutionsForCaseAsync(1)).ReturnsAsync(stepExecutions);
            var result = await controller.GetStepExecutionsForCase(1);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedStepExecutions = Assert.IsType<List<TestStepExecutionDto>>(okResult.Value);
            Assert.Empty(returnedStepExecutions);
        }

        #endregion

        #region GetExecutionStats Tests

        [Fact]
        public async Task GetExecutionStats_ServiceThrows_Returns500()
        {
            var mock = new Mock<ITestExecutionService>();
            mock.Setup(s => s.GetExecutionStatsAsync()).ThrowsAsync(new Exception("fail"));
            var controller = new TestExecutionController(mock.Object);
            var result = await controller.GetExecutionStats();
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task GetExecutionStats_ReturnsOkWithStats()
        {
            var mockService = new Mock<ITestExecutionService>();
            var controller = new TestExecutionController(mockService.Object);
            var stats = new TestExecutionStatsDto { TotalTestCaseExecutions = 100, PassedExecutions = 85, FailedExecutions = 15 };
            mockService.Setup(s => s.GetExecutionStatsAsync()).ReturnsAsync(stats);
            var result = await controller.GetExecutionStats();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedStats = Assert.IsType<TestExecutionStatsDto>(okResult.Value);
            Assert.Equal(100, returnedStats.TotalTestCaseExecutions);
        }

        #endregion

        #region GetExecutionStatsForSession Tests

        [Fact]
        public async Task GetExecutionStatsForSession_ServiceThrows_Returns500()
        {
            var mock = new Mock<ITestExecutionService>();
            mock.Setup(s => s.GetExecutionStatsForSessionAsync(It.IsAny<int>())).ThrowsAsync(new Exception("fail"));
            var controller = new TestExecutionController(mock.Object);
            var result = await controller.GetExecutionStatsForSession(1);
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task GetExecutionStatsForSession_ReturnsOkWithStats()
        {
            var mockService = new Mock<ITestExecutionService>();
            var controller = new TestExecutionController(mockService.Object);
            var stats = new TestExecutionStatsDto { TotalTestCaseExecutions = 50, PassedExecutions = 40, FailedExecutions = 10 };
            mockService.Setup(s => s.GetExecutionStatsForSessionAsync(1)).ReturnsAsync(stats);
            var result = await controller.GetExecutionStatsForSession(1);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedStats = Assert.IsType<TestExecutionStatsDto>(okResult.Value);
            Assert.Equal(50, returnedStats.TotalTestCaseExecutions);
        }

        #endregion
    }
}