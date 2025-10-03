using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using backend.Controllers;
using RqmtMgmtShared;
using Xunit;

namespace backend.Tests
{
    /// <summary>
    /// Unit tests for DocumentSectionsController to verify section management API endpoints.
    /// </summary>
    public class DocumentSectionsControllerTests
    {
        private readonly Mock<IDocumentSectionService> _mockService;
        private readonly DocumentSectionsController _controller;

        public DocumentSectionsControllerTests()
        {
            _mockService = new Mock<IDocumentSectionService>();
            _controller = new DocumentSectionsController(_mockService.Object);
        }

        [Fact]
        public async Task GetByDocumentId_ReturnsOkResult_WithSections()
        {
            // Arrange
            var sections = new List<DocumentSectionDto>
            {
                new DocumentSectionDto { Id = 1, DocumentId = 1, Title = "Section 1", SectionOrder = 1, CreatedAt = DateTime.UtcNow },
                new DocumentSectionDto { Id = 2, DocumentId = 1, Title = "Section 2", SectionOrder = 2, CreatedAt = DateTime.UtcNow }
            };
            _mockService.Setup(s => s.GetByDocumentIdAsync(1)).ReturnsAsync(sections);

            // Act
            var result = await _controller.GetByDocumentId(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSections = Assert.IsAssignableFrom<IEnumerable<DocumentSectionDto>>(okResult.Value);
            Assert.Equal(2, returnedSections.Count());
        }

        [Fact]
        public async Task GetById_ReturnsOkResult_WhenSectionExists()
        {
            // Arrange
            var section = new DocumentSectionDto 
            { 
                Id = 1, 
                DocumentId = 1, 
                Title = "Test Section", 
                SectionOrder = 1,
                CreatedAt = DateTime.UtcNow 
            };
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(section);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSection = Assert.IsType<DocumentSectionDto>(okResult.Value);
            Assert.Equal("Test Section", returnedSection.Title);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenSectionDoesNotExist()
        {
            // Arrange
            _mockService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((DocumentSectionDto?)null);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var inputSection = new DocumentSectionDto 
            { 
                DocumentId = 1, 
                Title = "New Section", 
                SectionOrder = 1,
                CreatedAt = DateTime.UtcNow 
            };
            var createdSection = new DocumentSectionDto 
            { 
                Id = 1, 
                DocumentId = 1, 
                Title = "New Section", 
                SectionOrder = 1,
                CreatedAt = DateTime.UtcNow 
            };
            _mockService.Setup(s => s.CreateAsync(inputSection)).ReturnsAsync(createdSection);

            // Act
            var result = await _controller.Create(inputSection);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
            Assert.Equal(1, createdResult.RouteValues!["id"]);
            var returnedSection = Assert.IsType<DocumentSectionDto>(createdResult.Value);
            Assert.Equal("New Section", returnedSection.Title);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenServiceReturnsNull()
        {
            // Arrange
            var inputSection = new DocumentSectionDto 
            { 
                DocumentId = 1, 
                Title = "Invalid Section", 
                SectionOrder = 1,
                CreatedAt = DateTime.UtcNow 
            };
            _mockService.Setup(s => s.CreateAsync(inputSection)).ReturnsAsync((DocumentSectionDto?)null);

            // Act
            var result = await _controller.Create(inputSection);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Failed to create document section. Check parent reference and circular dependencies.", badRequestResult.Value);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var updateSection = new DocumentSectionDto 
            { 
                Id = 1, 
                DocumentId = 1, 
                Title = "Updated Section", 
                SectionOrder = 1,
                CreatedAt = DateTime.UtcNow 
            };
            _mockService.Setup(s => s.UpdateAsync(updateSection)).ReturnsAsync(true);

            // Act
            var result = await _controller.Update(1, updateSection);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var updateSection = new DocumentSectionDto 
            { 
                Id = 2, 
                DocumentId = 1, 
                Title = "Updated Section", 
                SectionOrder = 1,
                CreatedAt = DateTime.UtcNow 
            };

            // Act
            var result = await _controller.Update(1, updateSection);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ID mismatch", badRequestResult.Value);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenServiceReturnsFalse()
        {
            // Arrange
            var updateSection = new DocumentSectionDto 
            { 
                Id = 1, 
                DocumentId = 1, 
                Title = "Updated Section", 
                SectionOrder = 1,
                CreatedAt = DateTime.UtcNow 
            };
            _mockService.Setup(s => s.UpdateAsync(updateSection)).ReturnsAsync(false);

            // Act
            var result = await _controller.Update(1, updateSection);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsBadRequest_WhenServiceReturnsFalse()
        {
            // Arrange
            _mockService.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(999);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ReorderSections_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var request = new ReorderSectionsRequest 
            { 
                ParentId = null, 
                SectionIds = new List<int> { 3, 1, 2 } 
            };
            _mockService.Setup(s => s.ReorderSectionsAsync(null, request.SectionIds)).ReturnsAsync(true);

            // Act
            var result = await _controller.ReorderSections(request);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ReorderSections_ReturnsBadRequest_WhenServiceReturnsFalse()
        {
            // Arrange
            var request = new ReorderSectionsRequest 
            { 
                ParentId = 1, 
                SectionIds = new List<int> { 3, 1, 2 } 
            };
            _mockService.Setup(s => s.ReorderSectionsAsync(1, request.SectionIds)).ReturnsAsync(false);

            // Act
            var result = await _controller.ReorderSections(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to reorder sections", badRequestResult.Value);
        }

        [Fact]
        public async Task ReorderSections_ReturnsBadRequest_WhenSectionIdsIsNull()
        {
            // Arrange
            var request = new ReorderSectionsRequest 
            { 
                ParentId = 1, 
                SectionIds = null!
            };

            // Act
            var result = await _controller.ReorderSections(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Section IDs are required", badRequestResult.Value);
        }

        [Fact]
        public async Task ReorderSections_ReturnsBadRequest_WhenSectionIdsIsEmpty()
        {
            // Arrange
            var request = new ReorderSectionsRequest 
            { 
                ParentId = 1, 
                SectionIds = new List<int>() 
            };

            // Act
            var result = await _controller.ReorderSections(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Section IDs are required", badRequestResult.Value);
        }
    }
}