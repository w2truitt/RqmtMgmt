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
        public async Task GetAll_WithSessions_ReturnsOkWithSessions()
        {
            // Arrange
            var mockService = new Mock<ITestRunSessionService>();
            var controller = new TestRunSessionController(mockService.Object);

            var sessions = new List<TestRunSessionDto>
            {
                new TestRunSessionDto { Id = 1, Name = "Session 1" },
                new TestRunSessionDto { Id = 2, Name = "Session 2" }
            };

            mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(sessions);

            // Act
            var result = await controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSessions = Assert.IsType<List<TestRunSessionDto>>(okResult.Value);
            Assert.Equal(2, returnedSessions.Count);
        }

        [Fact]
        public async Task GetById_ExistingId_ReturnsOkWithSession()
        {
            // Arrange
            var mockService = new Mock<ITestRunSessionService>();
            var controller = new TestRunSessionController(mockService.Object);

            var session = new TestRunSessionDto { Id = 1, Name = "Test Session" };
            mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(session);

            // Act
            var result = await controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSession = Assert.IsType<TestRunSessionDto>(okResult.Value);
            Assert.Equal(1, returnedSession.Id);
        }

        [Fact]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var mockService = new Mock<ITestRunSessionService>();
            var controller = new TestRunSessionController(mockService.Object);

            mockService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((TestRunSessionDto)null);

            // Act
            var result = await controller.GetById(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}