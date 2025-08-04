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
        public async Task GetAll_ReturnsOk_WithListOfTestCases()
        {
            var cases = new List<TestCase>
            {
                new TestCase { Id = 1, Title = "TC1", CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new TestCase { Id = 2, Title = "TC2", CreatedBy = 2, CreatedAt = DateTime.UtcNow }
            };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(cases);
            var result = await _controller.GetAll();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<TestCaseDto>>(okResult.Value);
            Assert.Collection(value,
                item => Assert.Equal("TC1", item.Title),
                item => Assert.Equal("TC2", item.Title));
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            _mockService.Setup(s => s.GetByIdAsync(42)).ReturnsAsync((TestCase)null);
            var result = await _controller.GetById(42);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreated_WithTestCase()
        {
            var dto = new TestCaseDto { Title = "TC1" };
            var entity = new TestCase { Id = 1, Title = "TC1", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            _mockService.Setup(s => s.CreateAsync(It.IsAny<TestCase>())).ReturnsAsync(entity);
            var result = await _controller.Create(dto);
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = Assert.IsAssignableFrom<TestCaseDto>(created.Value);
            Assert.Equal("TC1", value.Title);
        }

        [Fact]
        public async Task Update_ReturnsOk_WithTestCase()
        {
            var dto = new TestCaseDto { Id = 1, Title = "Updated" };
            var entity = new TestCase { Id = 1, Title = "Updated", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockService.Setup(s => s.UpdateAsync(It.IsAny<TestCase>())).ReturnsAsync(entity);
            var result = await _controller.Update(1, dto);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<TestCaseDto>(ok.Value);
            Assert.Equal("Updated", value.Title);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenNotExists()
        {
            var dto = new TestCaseDto { Id = 99, Title = "NotFound" };
            _mockService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((TestCase)null);
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
