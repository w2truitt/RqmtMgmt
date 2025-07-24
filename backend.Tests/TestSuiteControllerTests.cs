using System;
using backend.Controllers;
using backend.DTOs;
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
    /// Unit tests for <see cref="TestSuiteController"/>.
    /// </summary>
    public class TestSuiteControllerTests
    {
        private readonly Mock<ITestSuiteService> _mockService;
        private readonly TestSuiteController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestSuiteControllerTests"/> class.
        /// </summary>
        public TestSuiteControllerTests()
        {
            _mockService = new Mock<ITestSuiteService>();
            _controller = new TestSuiteController(_mockService.Object);
        }

        /// <summary>
        /// Verifies that GetAll returns OkResult with a list of test suites.
        /// </summary>
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

        /// <summary>
        /// Verifies that GetById returns NotFound when the test suite does not exist.
        /// </summary>
        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            _mockService.Setup(s => s.GetByIdAsync(42)).ReturnsAsync((TestSuite)null);
            var result = await _controller.GetById(42);
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
