using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using backend.Controllers;
using backend.Data;
using backend.Services;
using RqmtMgmtShared;
using Xunit;

namespace backend.Tests
{
    /// <summary>
    /// Unit tests for Enhanced Search endpoints in DocumentSectionsController.
    /// Tests API endpoint behavior, parameter validation, and response formatting.
    /// </summary>
    public class DocumentSectionsControllerEnhancedSearchTests
    {
        private readonly Mock<IDocumentSectionService> _mockService;
        private readonly DocumentSectionsController _controller;

        public DocumentSectionsControllerEnhancedSearchTests()
        {
            _mockService = new Mock<IDocumentSectionService>();
            _controller = new DocumentSectionsController(_mockService.Object);
        }

        [Fact]
        public async Task SearchSections_ValidParameters_ReturnsOkResult()
        {
            // Arrange
            var documentId = 1;
            var searchTerm = "Authentication";
            var expectedResults = new List<DocumentSectionDto>
            {
                new DocumentSectionDto { Id = 1, Title = "User Authentication", DocumentId = documentId },
                new DocumentSectionDto { Id = 2, Title = "3.1 User Authentication", DocumentId = documentId }
            };

            _mockService.Setup(s => s.SearchSectionsAsync(documentId, searchTerm, true, 0.8))
                       .ReturnsAsync(expectedResults);

            // Act
            var result = await _controller.SearchSections(documentId, searchTerm, true, 0.8, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResults = Assert.IsAssignableFrom<IEnumerable<DocumentSectionDto>>(okResult.Value);
            Assert.Equal(2, returnedResults.Count());
        }

        [Fact]
        public async Task SearchSections_EmptySearchTerm_ReturnsBadRequest()
        {
            // Arrange
            var documentId = 1;
            var emptySearchTerm = "";

            // Act
            var result = await _controller.SearchSections(documentId, emptySearchTerm, true, 0.8, null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Search term is required", badRequestResult.Value);
        }

        [Fact]
        public async Task SearchSections_InvalidSimilarityThreshold_ReturnsBadRequest()
        {
            // Arrange
            var documentId = 1;
            var searchTerm = "Authentication";
            var invalidThreshold = 1.5; // Above 1.0

            // Act
            var result = await _controller.SearchSections(documentId, searchTerm, true, invalidThreshold, null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Similarity threshold must be between 0.0 and 1.0", badRequestResult.Value);
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(1.1)]
        [InlineData(2.0)]
        public async Task SearchSections_InvalidSimilarityThresholdRange_ReturnsBadRequest(double invalidThreshold)
        {
            // Arrange
            var documentId = 1;
            var searchTerm = "Authentication";

            // Act
            var result = await _controller.SearchSections(documentId, searchTerm, true, invalidThreshold, null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Similarity threshold must be between 0.0 and 1.0", badRequestResult.Value);
        }

        [Fact]
        public async Task SearchSections_WithParentIdFilter_FiltersResults()
        {
            // Arrange
            var documentId = 1;
            var parentId = 100;
            var searchTerm = "Authentication";
            var allResults = new List<DocumentSectionDto>
            {
                new DocumentSectionDto { Id = 1, Title = "User Authentication", DocumentId = documentId, ParentSectionId = parentId },
                new DocumentSectionDto { Id = 2, Title = "Admin Authentication", DocumentId = documentId, ParentSectionId = 200 }
            };

            _mockService.Setup(s => s.SearchSectionsAsync(documentId, searchTerm, true, 0.8))
                       .ReturnsAsync(allResults);

            // Act
            var result = await _controller.SearchSections(documentId, searchTerm, true, 0.8, parentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResults = Assert.IsAssignableFrom<IEnumerable<DocumentSectionDto>>(okResult.Value);
            var filteredResults = returnedResults.ToList();
            
            Assert.Single(filteredResults);
            Assert.Equal(parentId, filteredResults[0].ParentSectionId);
        }

        [Fact]
        public async Task FindPotentialDuplicates_ValidParameters_ReturnsOkResult()
        {
            // Arrange
            var documentId = 1;
            var title = "User Authentication";
            var expectedDuplicates = new List<DocumentSectionDto>
            {
                new DocumentSectionDto { Id = 1, Title = "User Authentication", DocumentId = documentId },
                new DocumentSectionDto { Id = 2, Title = "3.1 User Authentication", DocumentId = documentId }
            };

            _mockService.Setup(s => s.FindPotentialDuplicatesAsync(documentId, title, 0.8))
                       .ReturnsAsync(expectedDuplicates);

            // Act
            var result = await _controller.FindPotentialDuplicates(documentId, title, 0.8, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResults = Assert.IsAssignableFrom<IEnumerable<DocumentSectionDto>>(okResult.Value);
            Assert.Equal(2, returnedResults.Count());
        }

        [Fact]
        public async Task FindPotentialDuplicates_EmptyTitle_ReturnsBadRequest()
        {
            // Arrange
            var documentId = 1;
            var emptyTitle = "";

            // Act
            var result = await _controller.FindPotentialDuplicates(documentId, emptyTitle, 0.8, null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Title is required", badRequestResult.Value);
        }

        [Fact]
        public async Task FindPotentialDuplicates_NoDuplicatesFound_ReturnsEmptyList()
        {
            // Arrange
            var documentId = 1;
            var title = "Unique Title";
            var emptyResults = new List<DocumentSectionDto>();

            _mockService.Setup(s => s.FindPotentialDuplicatesAsync(documentId, title, 0.8))
                       .ReturnsAsync(emptyResults);

            // Act
            var result = await _controller.FindPotentialDuplicates(documentId, title, 0.8, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResults = Assert.IsAssignableFrom<IEnumerable<DocumentSectionDto>>(okResult.Value);
            Assert.Empty(returnedResults);
        }

        [Fact]
        public async Task FindExistingSection_ValidParameters_ReturnsOkResult()
        {
            // Arrange
            var documentId = 1;
            var title = "Performance Requirements";
            var expectedSection = new DocumentSectionDto 
            { 
                Id = 1, 
                Title = "Performance Requirements", 
                DocumentId = documentId 
            };

            _mockService.Setup(s => s.FindExistingSectionAsync(documentId, title, null, 0.9))
                       .ReturnsAsync(expectedSection);

            // Act
            var result = await _controller.FindExistingSection(documentId, title, null, 0.9);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSection = Assert.IsType<DocumentSectionDto>(okResult.Value);
            Assert.Equal(expectedSection.Id, returnedSection.Id);
            Assert.Equal(expectedSection.Title, returnedSection.Title);
        }

        [Fact]
        public async Task FindExistingSection_NoMatchFound_ReturnsOkWithNull()
        {
            // Arrange
            var documentId = 1;
            var title = "Non-existent Section";

            _mockService.Setup(s => s.FindExistingSectionAsync(documentId, title, null, 0.9))
                       .ReturnsAsync((DocumentSectionDto)null);

            // Act
            var result = await _controller.FindExistingSection(documentId, title, null, 0.9);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Null(okResult.Value);
        }

        [Fact]
        public async Task FindExistingSection_EmptyTitle_ReturnsBadRequest()
        {
            // Arrange
            var documentId = 1;
            var emptyTitle = "";

            // Act
            var result = await _controller.FindExistingSection(documentId, emptyTitle, null, 0.9);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Title is required", badRequestResult.Value);
        }

        [Fact]
        public async Task FindExistingSection_WithParentId_PassesToService()
        {
            // Arrange
            var documentId = 1;
            var title = "Child Section";
            var parentId = 100;
            var expectedSection = new DocumentSectionDto 
            { 
                Id = 1, 
                Title = "Child Section", 
                DocumentId = documentId,
                ParentSectionId = parentId 
            };

            _mockService.Setup(s => s.FindExistingSectionAsync(documentId, title, parentId, 0.9))
                       .ReturnsAsync(expectedSection);

            // Act
            var result = await _controller.FindExistingSection(documentId, title, parentId, 0.9);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSection = Assert.IsType<DocumentSectionDto>(okResult.Value);
            Assert.Equal(parentId, returnedSection.ParentSectionId);
            
            // Verify the service was called with the correct parameters
            _mockService.Verify(s => s.FindExistingSectionAsync(documentId, title, parentId, 0.9), Times.Once);
        }

        [Theory]
        [InlineData("Authentication", true, 0.8)]
        [InlineData("Performance", false, 1.0)]
        [InlineData("Security", true, 0.5)]
        public async Task SearchSections_VariousParameters_CallsServiceCorrectly(string searchTerm, bool fuzzyMatch, double threshold)
        {
            // Arrange
            var documentId = 1;
            var expectedResults = new List<DocumentSectionDto>();

            _mockService.Setup(s => s.SearchSectionsAsync(documentId, searchTerm, fuzzyMatch, threshold))
                       .ReturnsAsync(expectedResults);

            // Act
            var result = await _controller.SearchSections(documentId, searchTerm, fuzzyMatch, threshold, null);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            _mockService.Verify(s => s.SearchSectionsAsync(documentId, searchTerm, fuzzyMatch, threshold), Times.Once);
        }
    }
}
