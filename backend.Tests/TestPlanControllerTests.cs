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
    /// Unit tests for <see cref="TestPlanController"/>.
    /// </summary>
    public class TestPlanControllerTests
    {
        private readonly Mock<ITestPlanService> _mockService;
        private readonly TestPlanController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestPlanControllerTests"/> class.
        /// </summary>
        public TestPlanControllerTests()
        {
            _mockService = new Mock<ITestPlanService>();
            _controller = new TestPlanController(_mockService.Object);
        }

        /// <summary>
        /// Verifies that GetAll returns OkResult with a list of test plans.
        /// </summary>
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

        /// <summary>
        /// Verifies that GetById returns NotFound when the test plan does not exist.
        /// </summary>
        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            _mockService.Setup(s => s.GetByIdAsync(42)).ReturnsAsync((TestPlan)null);
            var result = await _controller.GetById(42);
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
