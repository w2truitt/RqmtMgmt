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
    /// <summary>
    /// Unit tests for <see cref="TestCaseController"/>.
    /// </summary>
    public class TestCaseControllerTests
    {
        private readonly Mock<ITestCaseService> _mockService;
        private readonly TestCaseController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCaseControllerTests"/> class.
        /// </summary>
        public TestCaseControllerTests()
        {
            _mockService = new Mock<ITestCaseService>();
            _controller = new TestCaseController(_mockService.Object);
        }

        /// <summary>
        /// Verifies that GetAll returns OkResult with a list of test cases.
        /// </summary>
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

        /// <summary>
        /// Verifies that GetById returns NotFound when the test case does not exist.
        /// </summary>
        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            _mockService.Setup(s => s.GetByIdAsync(42)).ReturnsAsync((TestCase)null);
            var result = await _controller.GetById(42);
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
