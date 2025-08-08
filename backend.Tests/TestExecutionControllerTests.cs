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
    }
}