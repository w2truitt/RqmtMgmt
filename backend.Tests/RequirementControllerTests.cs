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
            var requirements = new List<RequirementDto>
            {
                new RequirementDto { Id = 1, Type = RequirementType.CRS, Title = "Req1", Status = RequirementStatus.Draft, Version = 1, CreatedBy = 1, CreatedAt = System.DateTime.UtcNow },
                new RequirementDto { Id = 2, Type = RequirementType.PRS, Title = "Req2", Status = RequirementStatus.Approved, Version = 1, CreatedBy = 2, CreatedAt = System.DateTime.UtcNow }
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
            _mockService.Setup(s => s.GetByIdAsync(42)).ReturnsAsync((RequirementDto)null);
            var result = await _controller.GetById(42);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreated_WithRequirement()
        {
            var dto = new RequirementDto { Title = "Req1", Type = RequirementType.CRS, Status = RequirementStatus.Draft };
            var entity = new RequirementDto { Id = 1, Title = "Req1", Type = RequirementType.CRS, Status = RequirementStatus.Draft, Version = 1, CreatedBy = 1, CreatedAt = System.DateTime.UtcNow };
            _mockService.Setup(s => s.CreateAsync(It.IsAny<RequirementDto>())).ReturnsAsync(entity);
            var result = await _controller.Create(dto);
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = Assert.IsAssignableFrom<RequirementDto>(created.Value);
            Assert.Equal("Req1", value.Title);
        }

        [Fact]
        public async Task Update_ReturnsOk_WithRequirement()
        {
            var dto = new RequirementDto { Id = 1, Title = "Updated", Type = RequirementType.CRS, Status = RequirementStatus.Draft };
            var entity = new RequirementDto { Id = 1, Title = "Updated", Type = RequirementType.CRS, Status = RequirementStatus.Draft, Version = 1, CreatedBy = 1, CreatedAt = System.DateTime.UtcNow };
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockService.Setup(s => s.UpdateAsync(It.IsAny<RequirementDto>())).ReturnsAsync(true);
            var result = await _controller.Update(1, dto);
            var noContent = Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenNotExists()
        {
            var dto = new RequirementDto { Id = 99, Title = "NotFound", Type = RequirementType.CRS, Status = RequirementStatus.Draft };
            _mockService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((RequirementDto)null);
            _mockService.Setup(s => s.UpdateAsync(It.IsAny<RequirementDto>())).ReturnsAsync(false);
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

        #region GetById Tests

        [Fact]
        public async Task GetById_ReturnsOk_WhenExists()
        {
            var requirement = new RequirementDto 
            { 
                Id = 1, 
                Title = "Test Requirement", 
                Type = RequirementType.CRS, 
                Status = RequirementStatus.Draft,
                Version = 1,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(requirement);
            var result = await _controller.GetById(1);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedRequirement = Assert.IsType<RequirementDto>(okResult.Value);
            Assert.Equal("Test Requirement", returnedRequirement.Title);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenServiceReturnsNull()
        {
            var dto = new RequirementDto { Title = "Req1", Type = RequirementType.CRS, Status = RequirementStatus.Draft };
            _mockService.Setup(s => s.CreateAsync(It.IsAny<RequirementDto>())).ReturnsAsync((RequirementDto)null);
            var result = await _controller.Create(dto);
            Assert.IsType<BadRequestResult>(result.Result);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenIdMismatch()
        {
            var dto = new RequirementDto { Id = 2, Title = "Updated", Type = RequirementType.CRS, Status = RequirementStatus.Draft };
            var result = await _controller.Update(1, dto);
            Assert.IsType<BadRequestResult>(result);
        }

        #endregion

        #region GetPaged Tests

        [Fact]
        public async Task GetPaged_ReturnsOkResult_WithPagedRequirements()
        {
            var requirements = new List<RequirementDto>
            {
                new RequirementDto { Id = 1, Title = "Req1", Type = RequirementType.CRS, Status = RequirementStatus.Draft },
                new RequirementDto { Id = 2, Title = "Req2", Type = RequirementType.PRS, Status = RequirementStatus.Approved }
            };

            var pagedResult = new PagedResult<RequirementDto>
            {
                Items = requirements,
                PageNumber = 1,
                PageSize = 20,
                TotalItems = 2
            };

            _mockService.Setup(s => s.GetPagedAsync(It.IsAny<PaginationParameters>())).ReturnsAsync(pagedResult);

            var result = await _controller.GetPaged();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResult = Assert.IsType<PagedResult<RequirementDto>>(okResult.Value);
            Assert.Equal(2, returnedResult.Items.Count);
            Assert.Equal(1, returnedResult.PageNumber);
            Assert.Equal(20, returnedResult.PageSize);
            Assert.Equal(2, returnedResult.TotalItems);
        }

        [Fact]
        public async Task GetPaged_WithSearchTerm_PassesCorrectParameters()
        {
            var pagedResult = new PagedResult<RequirementDto>
            {
                Items = new List<RequirementDto>(),
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 0
            };

            _mockService.Setup(s => s.GetPagedAsync(It.Is<PaginationParameters>(p => 
                p.SearchTerm == "test" && 
                p.PageNumber == 2 && 
                p.PageSize == 10 &&
                p.SortBy == "title" &&
                p.SortDescending == true
            ))).ReturnsAsync(pagedResult);

            var result = await _controller.GetPaged(
                pageNumber: 2,
                pageSize: 10, 
                searchTerm: "test",
                sortBy: "title",
                sortDescending: true
            );

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            _mockService.Verify(s => s.GetPagedAsync(It.Is<PaginationParameters>(p => 
                p.SearchTerm == "test" && 
                p.PageNumber == 2 && 
                p.PageSize == 10 &&
                p.SortBy == "title" &&
                p.SortDescending == true
            )), Times.Once);
        }

        [Fact]
        public async Task GetPaged_WithDefaultParameters_PassesDefaults()
        {
            var pagedResult = new PagedResult<RequirementDto>
            {
                Items = new List<RequirementDto>(),
                PageNumber = 1,
                PageSize = 20,
                TotalItems = 0
            };

            _mockService.Setup(s => s.GetPagedAsync(It.Is<PaginationParameters>(p => 
                p.PageNumber == 1 && 
                p.PageSize == 20 &&
                p.SearchTerm == null &&
                p.SortBy == null &&
                p.SortDescending == false
            ))).ReturnsAsync(pagedResult);

            var result = await _controller.GetPaged();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            _mockService.Verify(s => s.GetPagedAsync(It.Is<PaginationParameters>(p => 
                p.PageNumber == 1 && 
                p.PageSize == 20 &&
                p.SearchTerm == null &&
                p.SortBy == null &&
                p.SortDescending == false
            )), Times.Once);
        }

        #endregion
    }
}
