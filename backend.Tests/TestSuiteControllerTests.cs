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
        public async Task GetAll_ReturnsOk_WithListOfTestSuites()
        {
            var suites = new List<TestSuite>
            {
                new TestSuite { Id = 1, Name = "Suite1", CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new TestSuite { Id = 2, Name = "Suite2", CreatedBy = 2, CreatedAt = DateTime.UtcNow }
            };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(suites);
            var result = await _controller.GetAll();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<TestSuiteDto>>(okResult.Value);
            Assert.Collection(value,
                item => Assert.Equal("Suite1", item.Name),
                item => Assert.Equal("Suite2", item.Name));
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            _mockService.Setup(s => s.GetByIdAsync(42)).ReturnsAsync((TestSuite)null);
            var result = await _controller.GetById(42);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreated_WithTestSuite()
        {
            var dto = new TestSuiteDto { Name = "Suite1" };
            var entity = new TestSuite { Id = 1, Name = "Suite1", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            _mockService.Setup(s => s.CreateAsync(It.IsAny<TestSuite>())).ReturnsAsync(entity);
            var result = await _controller.Create(dto);
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = Assert.IsAssignableFrom<TestSuiteDto>(created.Value);
            Assert.Equal("Suite1", value.Name);
        }

        [Fact]
        public async Task Update_ReturnsOk_WithTestSuite()
        {
            var dto = new TestSuiteDto { Id = 1, Name = "Updated" };
            var entity = new TestSuite { Id = 1, Name = "Updated", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockService.Setup(s => s.UpdateAsync(It.IsAny<TestSuite>())).ReturnsAsync(entity);
            var result = await _controller.Update(1, dto);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<TestSuiteDto>(ok.Value);
            Assert.Equal("Updated", value.Name);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenNotExists()
        {
            var dto = new TestSuiteDto { Id = 99, Name = "NotFound" };
            _mockService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((TestSuite)null);
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
