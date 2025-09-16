using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using backend.Controllers;
using RqmtMgmtShared;
using Xunit;

namespace backend.Tests
{
    /// <summary>
    /// Unit tests for the new document-related endpoints in RequirementController.
    /// </summary>
    public class RequirementControllerDocumentTests
    {
        private readonly Mock<IRequirementService> _mockService;
        private readonly RequirementController _controller;

        public RequirementControllerDocumentTests()
        {
            _mockService = new Mock<IRequirementService>();
            _controller = new RequirementController(_mockService.Object);
        }

        [Fact]
        public async Task GetByDocumentId_ReturnsOkResult_WithDocumentRequirements()
        {
            // Arrange
            var requirements = new List<RequirementDto>
            {
                new RequirementDto { Id = 1, Title = "Req 1", Type = RequirementType.CRD, DocumentId = 1, CreatedBy = 1, ProjectId = 1, ProjectCode = "TEST", ProjectName = "Test", FullRequirementId = "TEST-REQ-001" },
                new RequirementDto { Id = 2, Title = "Req 2", Type = RequirementType.CRD, DocumentId = 1, CreatedBy = 1, ProjectId = 1, ProjectCode = "TEST", ProjectName = "Test", FullRequirementId = "TEST-REQ-002" }
            };
            _mockService.Setup(s => s.GetByDocumentIdAsync(1)).ReturnsAsync(requirements);

            // Act
            var result = await _controller.GetByDocumentId(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedReqs = Assert.IsAssignableFrom<IEnumerable<RequirementDto>>(okResult.Value);
            Assert.Equal(2, returnedReqs.Count());
            Assert.All(returnedReqs, req => Assert.Equal(1, req.DocumentId));
        }

        [Fact]
        public async Task GetBySectionId_ReturnsOkResult_WithSectionRequirements()
        {
            // Arrange
            var requirements = new List<RequirementDto>
            {
                new RequirementDto { Id = 1, Title = "Section Req", Type = RequirementType.PRD, SectionId = 5, CreatedBy = 1, ProjectId = 1, ProjectCode = "TEST", ProjectName = "Test", FullRequirementId = "TEST-REQ-001" }
            };
            _mockService.Setup(s => s.GetBySectionIdAsync(5)).ReturnsAsync(requirements);

            // Act
            var result = await _controller.GetBySectionId(5);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedReqs = Assert.IsAssignableFrom<IEnumerable<RequirementDto>>(okResult.Value);
            Assert.Single(returnedReqs);
            Assert.Equal(5, returnedReqs.First().SectionId);
        }

        [Fact]
        public async Task GetPagedByDocumentId_ReturnsOkResult_WithPagedDocumentRequirements()
        {
            // Arrange
            var pagedResult = new PagedResult<RequirementDto>
            {
                Items = new List<RequirementDto>
                {
                    new RequirementDto { Id = 1, Title = "Paged Req", Type = RequirementType.SRS, DocumentId = 2, CreatedBy = 1, ProjectId = 1, ProjectCode = "TEST", ProjectName = "Test", FullRequirementId = "TEST-REQ-001" }
                },
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 1
            };
            _mockService.Setup(s => s.GetPagedByDocumentIdAsync(2, It.IsAny<PaginationParameters>())).ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetPagedByDocumentId(2, 1, 10, "test", "title", false);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResult = Assert.IsType<PagedResult<RequirementDto>>(okResult.Value);
            Assert.Single(returnedResult.Items);
            Assert.Equal(2, returnedResult.Items.First().DocumentId);
        }

        [Fact]
        public async Task GetPagedByDocumentId_PassesCorrectParameters()
        {
            // Arrange
            var pagedResult = new PagedResult<RequirementDto>
            {
                Items = new List<RequirementDto>(),
                PageNumber = 2,
                PageSize = 5,
                TotalItems = 0
            };

            PaginationParameters? capturedParams = null;
            _mockService.Setup(s => s.GetPagedByDocumentIdAsync(1, It.IsAny<PaginationParameters>()))
                .Callback<int, PaginationParameters>((docId, p) => capturedParams = p)
                .ReturnsAsync(pagedResult);

            // Act
            await _controller.GetPagedByDocumentId(1, 2, 5, "search term", "status", true);

            // Assert
            Assert.NotNull(capturedParams);
            Assert.Equal(2, capturedParams.PageNumber);
            Assert.Equal(5, capturedParams.PageSize);
            Assert.Equal("search term", capturedParams.SearchTerm);
            Assert.Equal("status", capturedParams.SortBy);
            Assert.True(capturedParams.SortDescending);
        }

        [Fact]
        public async Task GetPagedBySectionId_ReturnsOkResult_WithPagedSectionRequirements()
        {
            // Arrange
            var pagedResult = new PagedResult<RequirementDto>
            {
                Items = new List<RequirementDto>
                {
                    new RequirementDto { Id = 1, Title = "Section Paged Req", Type = RequirementType.CRD, SectionId = 3, CreatedBy = 1, ProjectId = 1, ProjectCode = "TEST", ProjectName = "Test", FullRequirementId = "TEST-REQ-001" }
                },
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 1
            };
            _mockService.Setup(s => s.GetPagedBySectionIdAsync(3, It.IsAny<PaginationParameters>())).ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetPagedBySectionId(3, 1, 10, "test", "title", false);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResult = Assert.IsType<PagedResult<RequirementDto>>(okResult.Value);
            Assert.Single(returnedResult.Items);
            Assert.Equal(3, returnedResult.Items.First().SectionId);
        }

        [Fact]
        public async Task GetPagedBySectionId_PassesCorrectParameters()
        {
            // Arrange
            var pagedResult = new PagedResult<RequirementDto>
            {
                Items = new List<RequirementDto>(),
                PageNumber = 3,
                PageSize = 20,
                TotalItems = 0
            };

            PaginationParameters? capturedParams = null;
            _mockService.Setup(s => s.GetPagedBySectionIdAsync(5, It.IsAny<PaginationParameters>()))
                .Callback<int, PaginationParameters>((sectionId, p) => capturedParams = p)
                .ReturnsAsync(pagedResult);

            // Act
            await _controller.GetPagedBySectionId(5, 3, 20, "filter", "type", false);

            // Assert
            Assert.NotNull(capturedParams);
            Assert.Equal(3, capturedParams.PageNumber);
            Assert.Equal(20, capturedParams.PageSize);
            Assert.Equal("filter", capturedParams.SearchTerm);
            Assert.Equal("type", capturedParams.SortBy);
            Assert.False(capturedParams.SortDescending);
        }

        [Fact]
        public async Task GetPagedByDocumentId_ValidatesPageParameters()
        {
            // Arrange
            var pagedResult = new PagedResult<RequirementDto>
            {
                Items = new List<RequirementDto>(),
                PageNumber = 1,
                PageSize = 1,
                TotalItems = 0
            };

            PaginationParameters? capturedParams = null;
            _mockService.Setup(s => s.GetPagedByDocumentIdAsync(1, It.IsAny<PaginationParameters>()))
                .Callback<int, PaginationParameters>((docId, p) => capturedParams = p)
                .ReturnsAsync(pagedResult);

            // Act - Test boundary conditions
            await _controller.GetPagedByDocumentId(1, 0, 200, null, null, false); // Invalid page/size

            // Assert - Should be corrected to valid values
            Assert.NotNull(capturedParams);
            Assert.Equal(1, capturedParams.PageNumber); // Corrected from 0
            Assert.Equal(100, capturedParams.PageSize); // Corrected from 200 (max is 100)
        }

        [Fact]
        public async Task GetVersions_ReturnsOkResult_WithRequirementVersions()
        {
            // Arrange
            var versions = new List<RequirementVersionDto>
            {
                new RequirementVersionDto { Id = 1, RequirementId = 1, Version = 1, Title = "Version 1", Type = RequirementType.CRD, Status = RequirementStatus.Draft, ModifiedBy = 1, ModifiedAt = DateTime.UtcNow },
                new RequirementVersionDto { Id = 2, RequirementId = 1, Version = 2, Title = "Version 2", Type = RequirementType.CRD, Status = RequirementStatus.Approved, ModifiedBy = 1, ModifiedAt = DateTime.UtcNow }
            };
            _mockService.Setup(s => s.GetVersionsAsync(1)).ReturnsAsync(versions);

            // Act
            var result = await _controller.GetVersions(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedVersions = Assert.IsAssignableFrom<IEnumerable<RequirementVersionDto>>(okResult.Value);
            Assert.Equal(2, returnedVersions.Count());
        }

        [Fact]
        public async Task GetByDocumentId_ReturnsEmptyList_WhenNoRequirements()
        {
            // Arrange
            _mockService.Setup(s => s.GetByDocumentIdAsync(999)).ReturnsAsync(new List<RequirementDto>());

            // Act
            var result = await _controller.GetByDocumentId(999);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedReqs = Assert.IsAssignableFrom<IEnumerable<RequirementDto>>(okResult.Value);
            Assert.Empty(returnedReqs);
        }

        [Fact]
        public async Task GetBySectionId_ReturnsEmptyList_WhenNoRequirements()
        {
            // Arrange
            _mockService.Setup(s => s.GetBySectionIdAsync(999)).ReturnsAsync(new List<RequirementDto>());

            // Act
            var result = await _controller.GetBySectionId(999);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedReqs = Assert.IsAssignableFrom<IEnumerable<RequirementDto>>(okResult.Value);
            Assert.Empty(returnedReqs);
        }
    }
}