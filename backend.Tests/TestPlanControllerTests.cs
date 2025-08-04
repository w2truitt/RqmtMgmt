using System;
using backend.Controllers;
using RqmtMgmtShared;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace backend.Tests
{
    public class TestPlanControllerTests
    {
        private readonly Mock<ITestPlanService> _mockService;
        private readonly TestPlanController _controller;

        public TestPlanControllerTests()
        {
            _mockService = new Mock<ITestPlanService>();
            _controller = new TestPlanController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithListOfTestPlans()
        {
            var plans = new List<TestPlan>
            {
                new TestPlan { Id = 1, Name = "Plan1", Type = TestPlanType.UserValidation, CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new TestPlan { Id = 2, Name = "Plan2", Type = TestPlanType.SoftwareVerification, CreatedBy = 2, CreatedAt = DateTime.UtcNow }
            };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(plans);
            var result = await _controller.GetAll();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<TestPlanDto>>(okResult.Value);
            Assert.Collection(value,
                item => Assert.Equal("Plan1", item.Name),
                item => Assert.Equal("Plan2", item.Name));
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            _mockService.Setup(s => s.GetByIdAsync(42)).ReturnsAsync((TestPlan)null);
            var result = await _controller.GetById(42);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreated_WithTestPlan()
        {
            var dto = new TestPlanDto { Name = "Plan1", Type = "UserValidation" };
            var entity = new TestPlan { Id = 1, Name = "Plan1", Type = TestPlanType.UserValidation, CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            _mockService.Setup(s => s.CreateAsync(It.IsAny<TestPlan>())).ReturnsAsync(entity);
            var result = await _controller.Create(dto);
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = Assert.IsAssignableFrom<TestPlanDto>(created.Value);
            Assert.Equal("Plan1", value.Name);
        }

        [Fact]
        public async Task Update_ReturnsOk_WithTestPlan()
        {
            var dto = new TestPlanDto { Id = 1, Name = "Updated", Type = "UserValidation" };
            var entity = new TestPlan { Id = 1, Name = "Updated", Type = TestPlanType.UserValidation, CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockService.Setup(s => s.UpdateAsync(It.IsAny<TestPlan>())).ReturnsAsync(entity);
            var result = await _controller.Update(1, dto);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<TestPlanDto>(ok.Value);
            Assert.Equal("Updated", value.Name);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenNotExists()
        {
            var dto = new TestPlanDto { Id = 99, Name = "NotFound", Type = "UserValidation" };
            _mockService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((TestPlan)null);
            var result = await _controller.Update(99, dto);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenDeleted()
        {
            _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);
            var result = await _controller.Delete(1);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenNotExists()
        {
            _mockService.Setup(s => s.DeleteAsync(99)).ReturnsAsync(false);
            var result = await _controller.Delete(99);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
