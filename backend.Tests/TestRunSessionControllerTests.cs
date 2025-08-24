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
    public class TestRunSessionControllerTests
    {
        [Fact]
        public async Task GetAll_ServiceThrows_Returns500()
        {
            var mock = new Mock<ITestRunSessionService>();
            mock.Setup(s => s.GetAllAsync()).ThrowsAsync(new Exception("fail"));
            var controller = new TestRunSessionController(mock.Object);
            var result = await controller.GetAll();
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task GetById_ServiceThrows_Returns500()
        {
            var mock = new Mock<ITestRunSessionService>();
            mock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ThrowsAsync(new Exception("fail"));
            var controller = new TestRunSessionController(mock.Object);
            var result = await controller.GetById(1);
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task Create_ServiceThrows_Returns500()
        {
            var mock = new Mock<ITestRunSessionService>();
            mock.Setup(s => s.CreateAsync(It.IsAny<TestRunSessionDto>())).ThrowsAsync(new Exception("fail"));
            var controller = new TestRunSessionController(mock.Object);
            var dto = new TestRunSessionDto { Id = 1 };
            var result = await controller.Create(dto);
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task Update_ServiceThrows_Returns500()
        {
            var mock = new Mock<ITestRunSessionService>();
            mock.Setup(s => s.UpdateAsync(It.IsAny<TestRunSessionDto>())).ThrowsAsync(new Exception("fail"));
            var controller = new TestRunSessionController(mock.Object);
            var dto = new TestRunSessionDto { Id = 1 };
            var result = await controller.Update(1, dto);
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task Delete_ServiceThrows_Returns500()
        {
            var mock = new Mock<ITestRunSessionService>();
            mock.Setup(s => s.DeleteAsync(It.IsAny<int>())).ThrowsAsync(new Exception("fail"));
            var controller = new TestRunSessionController(mock.Object);
            var result = await controller.Delete(1);
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }
        [Fact]
        public async Task GetAll_WithSessions_ReturnsOkWithSessions()
        {
            var mockService = new Mock<ITestRunSessionService>();
            var controller = new TestRunSessionController(mockService.Object);
            var sessions = new List<TestRunSessionDto> { new TestRunSessionDto { Id = 1, Name = "Session 1" }, new TestRunSessionDto { Id = 2, Name = "Session 2" } };
            mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(sessions);
            var result = await controller.GetAll();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSessions = Assert.IsType<List<TestRunSessionDto>>(okResult.Value);
            Assert.Equal(2, returnedSessions.Count);
        }

        [Fact]
        public async Task GetById_ExistingId_ReturnsOkWithSession()
        {
            var mockService = new Mock<ITestRunSessionService>();
            var controller = new TestRunSessionController(mockService.Object);
            var session = new TestRunSessionDto { Id = 1, Name = "Test Session" };
            mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(session);
            var result = await controller.GetById(1);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSession = Assert.IsType<TestRunSessionDto>(okResult.Value);
            Assert.Equal(1, returnedSession.Id);
        }

        [Fact]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            var mockService = new Mock<ITestRunSessionService>();
            var controller = new TestRunSessionController(mockService.Object);
            mockService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((TestRunSessionDto)null);
            var result = await controller.GetById(999);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_InvalidModelState_ReturnsBadRequest()
        {
            var mockService = new Mock<ITestRunSessionService>();
            var controller = new TestRunSessionController(mockService.Object);
            controller.ModelState.AddModelError("Name", "Required");
            var dto = new TestRunSessionDto();
            var result = await controller.Create(dto);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_ServiceReturnsNull_ReturnsInternalServerError()
        {
            var mockService = new Mock<ITestRunSessionService>();
            mockService.Setup(s => s.CreateAsync(It.IsAny<TestRunSessionDto>())).ReturnsAsync((TestRunSessionDto)null);
            var controller = new TestRunSessionController(mockService.Object);
            var dto = new TestRunSessionDto { Id = 1 };
            var result = await controller.Create(dto);
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenSuccessful()
        {
            var mockService = new Mock<ITestRunSessionService>();
            var created = new TestRunSessionDto { Id = 10 };
            mockService.Setup(s => s.CreateAsync(It.IsAny<TestRunSessionDto>())).ReturnsAsync(created);
            var controller = new TestRunSessionController(mockService.Object);
            var dto = new TestRunSessionDto { Id = 10 };
            var result = await controller.Create(dto);
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returned = Assert.IsType<TestRunSessionDto>(createdResult.Value);
            Assert.Equal(10, returned.Id);
        }

        [Fact]
        public async Task Update_IdMismatch_ReturnsBadRequest()
        {
            var mockService = new Mock<ITestRunSessionService>();
            var controller = new TestRunSessionController(mockService.Object);
            var dto = new TestRunSessionDto { Id = 2 };
            var result = await controller.Update(1, dto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Update_InvalidModelState_ReturnsBadRequest()
        {
            var mockService = new Mock<ITestRunSessionService>();
            var controller = new TestRunSessionController(mockService.Object);
            controller.ModelState.AddModelError("desc", "error");
            var dto = new TestRunSessionDto { Id = 1 };
            var result = await controller.Update(1, dto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Update_ServiceReturnsFalse_ReturnsNotFound()
        {
            var mockService = new Mock<ITestRunSessionService>();
            mockService.Setup(s => s.UpdateAsync(It.IsAny<TestRunSessionDto>())).ReturnsAsync(false);
            var controller = new TestRunSessionController(mockService.Object);
            var dto = new TestRunSessionDto { Id = 1 };
            var result = await controller.Update(1, dto);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Update_ServiceReturnsTrue_ReturnsNoContent()
        {
            var mockService = new Mock<ITestRunSessionService>();
            mockService.Setup(s => s.UpdateAsync(It.IsAny<TestRunSessionDto>())).ReturnsAsync(true);
            var controller = new TestRunSessionController(mockService.Object);
            var dto = new TestRunSessionDto { Id = 1 };
            var result = await controller.Update(1, dto);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ServiceReturnsFalse_ReturnsNotFound()
        {
            var mockService = new Mock<ITestRunSessionService>();
            mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(false);
            var controller = new TestRunSessionController(mockService.Object);
            var result = await controller.Delete(1);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Delete_ServiceReturnsTrue_ReturnsNoContent()
        {
            var mockService = new Mock<ITestRunSessionService>();
            mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);
            var controller = new TestRunSessionController(mockService.Object);
            var result = await controller.Delete(1);
            Assert.IsType<NoContentResult>(result);
        }

        #region StartTestRunSession Tests

        [Fact]
        public async Task StartTestRunSession_ServiceThrows_Returns500()
        {
            var mock = new Mock<ITestRunSessionService>();
            mock.Setup(s => s.StartTestRunSessionAsync(It.IsAny<TestRunSessionDto>())).ThrowsAsync(new Exception("fail"));
            var controller = new TestRunSessionController(mock.Object);
            var dto = new TestRunSessionDto { Id = 1 };
            var result = await controller.StartTestRunSession(dto);
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task StartTestRunSession_InvalidModelState_ReturnsBadRequest()
        {
            var mockService = new Mock<ITestRunSessionService>();
            var controller = new TestRunSessionController(mockService.Object);
            controller.ModelState.AddModelError("Name", "Required");
            var dto = new TestRunSessionDto();
            var result = await controller.StartTestRunSession(dto);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task StartTestRunSession_ServiceReturnsNull_ReturnsInternalServerError()
        {
            var mockService = new Mock<ITestRunSessionService>();
            mockService.Setup(s => s.StartTestRunSessionAsync(It.IsAny<TestRunSessionDto>())).ReturnsAsync((TestRunSessionDto)null);
            var controller = new TestRunSessionController(mockService.Object);
            var dto = new TestRunSessionDto { Id = 1 };
            var result = await controller.StartTestRunSession(dto);
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task StartTestRunSession_ReturnsCreatedAtAction_WhenSuccessful()
        {
            var mockService = new Mock<ITestRunSessionService>();
            var started = new TestRunSessionDto { Id = 10, Name = "Started Session" };
            mockService.Setup(s => s.StartTestRunSessionAsync(It.IsAny<TestRunSessionDto>())).ReturnsAsync(started);
            var controller = new TestRunSessionController(mockService.Object);
            var dto = new TestRunSessionDto { Id = 10 };
            var result = await controller.StartTestRunSession(dto);
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returned = Assert.IsType<TestRunSessionDto>(createdResult.Value);
            Assert.Equal(10, returned.Id);
            Assert.Equal("GetById", createdResult.ActionName);
        }

        #endregion

        #region CompleteTestRunSession Tests

        [Fact]
        public async Task CompleteTestRunSession_ServiceThrows_Returns500()
        {
            var mock = new Mock<ITestRunSessionService>();
            mock.Setup(s => s.CompleteTestRunSessionAsync(It.IsAny<int>())).ThrowsAsync(new Exception("fail"));
            var controller = new TestRunSessionController(mock.Object);
            var result = await controller.CompleteTestRunSession(1);
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task CompleteTestRunSession_ServiceReturnsFalse_ReturnsNotFound()
        {
            var mockService = new Mock<ITestRunSessionService>();
            mockService.Setup(s => s.CompleteTestRunSessionAsync(1)).ReturnsAsync(false);
            var controller = new TestRunSessionController(mockService.Object);
            var result = await controller.CompleteTestRunSession(1);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var message = notFoundResult.Value;
            Assert.NotNull(message);
        }

        [Fact]
        public async Task CompleteTestRunSession_ServiceReturnsTrue_ReturnsNoContent()
        {
            var mockService = new Mock<ITestRunSessionService>();
            mockService.Setup(s => s.CompleteTestRunSessionAsync(1)).ReturnsAsync(true);
            var controller = new TestRunSessionController(mockService.Object);
            var result = await controller.CompleteTestRunSession(1);
            Assert.IsType<NoContentResult>(result);
        }

        #endregion

        #region AbortTestRunSession Tests

        [Fact]
        public async Task AbortTestRunSession_ServiceThrows_Returns500()
        {
            var mock = new Mock<ITestRunSessionService>();
            mock.Setup(s => s.AbortTestRunSessionAsync(It.IsAny<int>())).ThrowsAsync(new Exception("fail"));
            var controller = new TestRunSessionController(mock.Object);
            var result = await controller.AbortTestRunSession(1);
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task AbortTestRunSession_ServiceReturnsFalse_ReturnsNotFound()
        {
            var mockService = new Mock<ITestRunSessionService>();
            mockService.Setup(s => s.AbortTestRunSessionAsync(1)).ReturnsAsync(false);
            var controller = new TestRunSessionController(mockService.Object);
            var result = await controller.AbortTestRunSession(1);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var message = notFoundResult.Value;
            Assert.NotNull(message);
        }

        [Fact]
        public async Task AbortTestRunSession_ServiceReturnsTrue_ReturnsNoContent()
        {
            var mockService = new Mock<ITestRunSessionService>();
            mockService.Setup(s => s.AbortTestRunSessionAsync(1)).ReturnsAsync(true);
            var controller = new TestRunSessionController(mockService.Object);
            var result = await controller.AbortTestRunSession(1);
            Assert.IsType<NoContentResult>(result);
        }

        #endregion

        #region GetActiveSessions Tests

        [Fact]
        public async Task GetActiveSessions_ServiceThrows_Returns500()
        {
            var mock = new Mock<ITestRunSessionService>();
            mock.Setup(s => s.GetActiveSessionsAsync()).ThrowsAsync(new Exception("fail"));
            var controller = new TestRunSessionController(mock.Object);
            var result = await controller.GetActiveSessions();
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task GetActiveSessions_WithActiveSessions_ReturnsOkWithSessions()
        {
            var mockService = new Mock<ITestRunSessionService>();
            var controller = new TestRunSessionController(mockService.Object);
            var sessions = new List<TestRunSessionDto> 
            { 
                new TestRunSessionDto { Id = 1, Name = "Active Session 1" }, 
                new TestRunSessionDto { Id = 2, Name = "Active Session 2" } 
            };
            mockService.Setup(s => s.GetActiveSessionsAsync()).ReturnsAsync(sessions);
            var result = await controller.GetActiveSessions();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSessions = Assert.IsType<List<TestRunSessionDto>>(okResult.Value);
            Assert.Equal(2, returnedSessions.Count);
        }

        [Fact]
        public async Task GetActiveSessions_EmptyList_ReturnsOkWithEmptyList()
        {
            var mockService = new Mock<ITestRunSessionService>();
            var controller = new TestRunSessionController(mockService.Object);
            var sessions = new List<TestRunSessionDto>();
            mockService.Setup(s => s.GetActiveSessionsAsync()).ReturnsAsync(sessions);
            var result = await controller.GetActiveSessions();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSessions = Assert.IsType<List<TestRunSessionDto>>(okResult.Value);
            Assert.Empty(returnedSessions);
        }

        #endregion
    }
}