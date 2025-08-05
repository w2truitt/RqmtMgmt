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
    public class TestCaseControllerTests
    {
        private readonly Mock<ITestCaseService> _mockService;
        private readonly TestCaseController _controller;

        public TestCaseControllerTests()
        {
            _mockService = new Mock<ITestCaseService>();
            _controller = new TestCaseController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfTestCases()
        {
            var testCases = new List<TestCaseDto>
            {
                new TestCaseDto { Id = 1, Title = "Case1", Description = "Desc1", CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new TestCaseDto { Id = 2, Title = "Case2", Description = "Desc2", CreatedBy = 1, CreatedAt = DateTime.UtcNow }
            };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(testCases);
            
            var result = await _controller.GetAll();
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<List<TestCaseDto>>(okResult.Value);
            Assert.Equal(2, value.Count);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            _mockService.Setup(s => s.GetByIdAsync(42)).ReturnsAsync((TestCaseDto?)null);
            
            var result = await _controller.GetById(42);
            
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreated_WithTestCase()
        {
            var dto = new TestCaseDto { Title = "NewCase", Description = "New Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            var entity = new TestCaseDto { Id = 1, Title = "NewCase", Description = "New Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            _mockService.Setup(s => s.CreateAsync(It.IsAny<TestCaseDto>())).ReturnsAsync(entity);
            
            var result = await _controller.Create(dto);
            
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = Assert.IsAssignableFrom<TestCaseDto>(created.Value);
            Assert.Equal("NewCase", value.Title);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenSuccessful()
        {
            var dto = new TestCaseDto { Id = 1, Title = "Updated", Description = "Updated Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            _mockService.Setup(s => s.UpdateAsync(It.IsAny<TestCaseDto>())).ReturnsAsync(true);
            
            var result = await _controller.Update(1, dto);
            
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenNotExists()
        {
            var dto = new TestCaseDto { Id = 99, Title = "NotFound", Description = "Not Found Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            _mockService.Setup(s => s.UpdateAsync(It.IsAny<TestCaseDto>())).ReturnsAsync(false);
            
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