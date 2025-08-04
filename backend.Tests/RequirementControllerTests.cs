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
    public class RequirementControllerTests
    {
        private readonly Mock<IRequirementService> _mockService;
        private readonly RequirementController _controller;

        public RequirementControllerTests()
        {
            _mockService = new Mock<IRequirementService>();
            _controller = new RequirementController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfRequirements()
        {
            var requirements = new List<Requirement>
            {
                new Requirement { Id = 1, Type = RequirementType.CRS, Title = "Req1", Status = RequirementStatus.Draft, Version = 1, CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new Requirement { Id = 2, Type = RequirementType.PRS, Title = "Req2", Status = RequirementStatus.Approved, Version = 1, CreatedBy = 2, CreatedAt = DateTime.UtcNow }
            };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(requirements);
            var result = await _controller.GetAll();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<RequirementDto>>(okResult.Value);
            Assert.Collection(value,
                item => Assert.Equal("Req1", item.Title),
                item => Assert.Equal("Req2", item.Title));
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            _mockService.Setup(s => s.GetByIdAsync(42)).ReturnsAsync((Requirement)null);
            var result = await _controller.GetById(42);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreated_WithRequirement()
        {
            var dto = new RequirementDto { Title = "Req1", Type = "CRS", Status = "Draft" };
            var entity = new Requirement { Id = 1, Title = "Req1", Type = RequirementType.CRS, Status = RequirementStatus.Draft, Version = 1, CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            _mockService.Setup(s => s.CreateAsync(It.IsAny<Requirement>())).ReturnsAsync(entity);
            var result = await _controller.Create(dto);
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = Assert.IsAssignableFrom<RequirementDto>(created.Value);
            Assert.Equal("Req1", value.Title);
        }

        [Fact]
        public async Task Update_ReturnsOk_WithRequirement()
        {
            var dto = new RequirementDto { Id = 1, Title = "Updated", Type = "CRS", Status = "Draft" };
            var entity = new Requirement { Id = 1, Title = "Updated", Type = RequirementType.CRS, Status = RequirementStatus.Draft, Version = 1, CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockService.Setup(s => s.UpdateAsync(It.IsAny<Requirement>())).ReturnsAsync(entity);
            var result = await _controller.Update(1, dto);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<RequirementDto>(ok.Value);
            Assert.Equal("Updated", value.Title);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenNotExists()
        {
            var dto = new RequirementDto { Id = 99, Title = "NotFound", Type = "CRS", Status = "Draft" };
            _mockService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Requirement)null);
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
