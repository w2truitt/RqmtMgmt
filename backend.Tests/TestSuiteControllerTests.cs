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
    public class TestSuiteControllerTests
    {
        private readonly Mock<ITestSuiteService> _mockService;
        private readonly TestSuiteController _controller;

        public TestSuiteControllerTests()
        {
            _mockService = new Mock<ITestSuiteService>();
            _controller = new TestSuiteController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfTestSuites()
        {
            var testSuites = new List<TestSuiteDto>
            {
                new TestSuiteDto { Id = 1, Name = "Suite1", Description = "Desc1", CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new TestSuiteDto { Id = 2, Name = "Suite2", Description = "Desc2", CreatedBy = 1, CreatedAt = DateTime.UtcNow }
            };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(testSuites);
            
            var result = await _controller.GetAll();
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<List<TestSuiteDto>>(okResult.Value);
            Assert.Equal(2, value.Count);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            _mockService.Setup(s => s.GetByIdAsync(42)).ReturnsAsync((TestSuiteDto?)null);
            
            var result = await _controller.GetById(42);
            
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreated_WithTestSuite()
        {
            var dto = new TestSuiteDto { Name = "NewSuite", Description = "New Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            var entity = new TestSuiteDto { Id = 1, Name = "NewSuite", Description = "New Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            _mockService.Setup(s => s.CreateAsync(It.IsAny<TestSuiteDto>())).ReturnsAsync(entity);
            
            var result = await _controller.Create(dto);
            
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = Assert.IsAssignableFrom<TestSuiteDto>(created.Value);
            Assert.Equal("NewSuite", value.Name);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenSuccessful()
        {
            var dto = new TestSuiteDto { Id = 1, Name = "Updated", Description = "Updated Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            _mockService.Setup(s => s.UpdateAsync(It.IsAny<TestSuiteDto>())).ReturnsAsync(true);
            
            var result = await _controller.Update(1, dto);
            
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenNotExists()
        {
            var dto = new TestSuiteDto { Id = 99, Name = "NotFound", Description = "Not Found Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            _mockService.Setup(s => s.UpdateAsync(It.IsAny<TestSuiteDto>())).ReturnsAsync(false);
            
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