#nullable enable
using System;
using backend.Controllers;
using RqmtMgmtShared;
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
        public async Task GetAll_ReturnsOkResult_WithListOfTestPlans()
        {
            var testPlans = new List<TestPlanDto>
            {
                new TestPlanDto { Id = 1, Name = "Plan1", Type = "UserValidation", Description = "Desc1", CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new TestPlanDto { Id = 2, Name = "Plan2", Type = "SoftwareVerification", Description = "Desc2", CreatedBy = 1, CreatedAt = DateTime.UtcNow }
            };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(testPlans);
            
            var result = await _controller.GetAll();
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<List<TestPlanDto>>(okResult.Value);
            Assert.Equal(2, value.Count);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            _mockService.Setup(s => s.GetByIdAsync(42)).ReturnsAsync((TestPlanDto?)null);
            
            var result = await _controller.GetById(42);
            
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreated_WithTestPlan()
        {
            var dto = new TestPlanDto { Name = "NewPlan", Type = "UserValidation", Description = "New Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            var entity = new TestPlanDto { Id = 1, Name = "NewPlan", Type = "UserValidation", Description = "New Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            _mockService.Setup(s => s.CreateAsync(It.IsAny<TestPlanDto>())).ReturnsAsync(entity);
            
            var result = await _controller.Create(dto);
            
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = Assert.IsAssignableFrom<TestPlanDto>(created.Value);
            Assert.Equal("NewPlan", value.Name);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenSuccessful()
        {
            var dto = new TestPlanDto { Id = 1, Name = "Updated", Type = "UserValidation", Description = "Updated Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            _mockService.Setup(s => s.UpdateAsync(It.IsAny<TestPlanDto>())).ReturnsAsync(true);
            
            var result = await _controller.Update(1, dto);
            
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenNotExists()
        {
            var dto = new TestPlanDto { Id = 99, Name = "NotFound", Type = "UserValidation", Description = "Not Found Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            _mockService.Setup(s => s.UpdateAsync(It.IsAny<TestPlanDto>())).ReturnsAsync(false);
            
            var result = await _controller.Update(99, dto);
            
            Assert.IsType<NotFoundResult>(result);
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