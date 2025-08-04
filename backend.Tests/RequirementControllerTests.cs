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
    /// Unit tests for <see cref="RequirementController"/>.
    /// </summary>
    public class RequirementControllerTests
    {
        private readonly Mock<IRequirementService> _mockService;
        private readonly RequirementController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementControllerTests"/> class.
        /// </summary>
        public RequirementControllerTests()
        {
            _mockService = new Mock<IRequirementService>();
            _controller = new RequirementController(_mockService.Object);
        }

        /// <summary>
        /// Verifies that GetAll returns OkResult with a list of requirements.
        /// </summary>
        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfRequirements()
        {
            // Arrange
            var requirements = new List<Requirement>
            {
                new Requirement { Id = 1, Type = RequirementType.CRS, Title = "Req1", Status = RequirementStatus.Draft, Version = 1, CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new Requirement { Id = 2, Type = RequirementType.PRS, Title = "Req2", Status = RequirementStatus.Approved, Version = 1, CreatedBy = 2, CreatedAt = DateTime.UtcNow }
            };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(requirements);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<RequirementDto>>(okResult.Value);
            Assert.Collection(value,
                item => Assert.Equal("Req1", item.Title),
                item => Assert.Equal("Req2", item.Title));
        }

        /// <summary>
        /// Verifies that GetById returns NotFound when the requirement does not exist.
        /// </summary>
        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            _mockService.Setup(s => s.GetByIdAsync(42)).ReturnsAsync((Requirement)null);
            var result = await _controller.GetById(42);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        // Add more tests for Create, Update, Delete, etc.
    }
}
