using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using backend.Controllers;
using RqmtMgmtShared;
using Xunit;

namespace backend.Tests
{
    /// <summary>
    /// Unit tests for DocumentsController to verify API endpoints and HTTP responses.
    /// </summary>
    public class DocumentsControllerTests
    {
        private readonly Mock<IDocumentService> _mockService;
        private readonly DocumentsController _controller;

        public DocumentsControllerTests()
        {
            _mockService = new Mock<IDocumentService>();
            _controller = new DocumentsController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfDocuments()
        {
            // Arrange
            var documents = new List<DocumentDto>
            {
                new DocumentDto { Id = 1, Title = "Doc 1", Type = DocumentType.CRD, CreatedBy = 1, ProjectId = 1 },
                new DocumentDto { Id = 2, Title = "Doc 2", Type = DocumentType.PRD, CreatedBy = 1, ProjectId = 1 }
            };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(documents);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDocs = Assert.IsAssignableFrom<IEnumerable<DocumentDto>>(okResult.Value);
            Assert.Equal(2, returnedDocs.Count());
        }

        [Fact]
        public async Task GetById_ReturnsOkResult_WhenDocumentExists()
        {
            // Arrange
            var document = new DocumentDto { Id = 1, Title = "Test Doc", Type = DocumentType.SRS, CreatedBy = 1, ProjectId = 1 };
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(document);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDoc = Assert.IsType<DocumentDto>(okResult.Value);
            Assert.Equal("Test Doc", returnedDoc.Title);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenDocumentDoesNotExist()
        {
            // Arrange
            _mockService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((DocumentDto?)null);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var inputDoc = new DocumentDto { Title = "New Doc", Type = DocumentType.CRD, CreatedBy = 1, ProjectId = 1 };
            var createdDoc = new DocumentDto { Id = 1, Title = "New Doc", Type = DocumentType.CRD, CreatedBy = 1, ProjectId = 1 };
            _mockService.Setup(s => s.CreateAsync(inputDoc)).ReturnsAsync(createdDoc);

            // Act
            var result = await _controller.Create(inputDoc);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
            Assert.Equal(1, createdResult.RouteValues!["id"]);
            var returnedDoc = Assert.IsType<DocumentDto>(createdResult.Value);
            Assert.Equal("New Doc", returnedDoc.Title);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenServiceReturnsNull()
        {
            // Arrange
            var inputDoc = new DocumentDto { Title = "Invalid Doc", Type = DocumentType.CRD, CreatedBy = 1, ProjectId = 1 };
            _mockService.Setup(s => s.CreateAsync(inputDoc)).ReturnsAsync((DocumentDto?)null);

            // Act
            var result = await _controller.Create(inputDoc);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Failed to create document", badRequestResult.Value);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var updateDoc = new DocumentDto { Id = 1, Title = "Updated Doc", Type = DocumentType.CRD, CreatedBy = 1, ProjectId = 1 };
            _mockService.Setup(s => s.UpdateAsync(updateDoc)).ReturnsAsync(true);

            // Act
            var result = await _controller.Update(1, updateDoc);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var updateDoc = new DocumentDto { Id = 2, Title = "Updated Doc", Type = DocumentType.CRD, CreatedBy = 1, ProjectId = 1 };

            // Act
            var result = await _controller.Update(1, updateDoc);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ID mismatch", badRequestResult.Value);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenServiceReturnsFalse()
        {
            // Arrange
            var updateDoc = new DocumentDto { Id = 1, Title = "Updated Doc", Type = DocumentType.CRD, CreatedBy = 1, ProjectId = 1 };
            _mockService.Setup(s => s.UpdateAsync(updateDoc)).ReturnsAsync(false);

            // Act
            var result = await _controller.Update(1, updateDoc);

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
        public async Task Delete_ReturnsNotFound_WhenServiceReturnsFalse()
        {
            // Arrange
            _mockService.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetPaged_ReturnsOkResult_WithPagedDocuments()
        {
            // Arrange
            var pagedResult = new PagedResult<DocumentDto>
            {
                Items = new List<DocumentDto>
                {
                    new DocumentDto { Id = 1, Title = "Doc 1", Type = DocumentType.CRD, CreatedBy = 1, ProjectId = 1 }
                },
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 1
            };
            _mockService.Setup(s => s.GetPagedAsync(It.IsAny<PaginationParameters>())).ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetPaged(1, 10, "test", "title", false);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResult = Assert.IsType<PagedResult<DocumentDto>>(okResult.Value);
            Assert.Single(returnedResult.Items);
        }

        [Fact]
        public async Task GetByProjectId_ReturnsOkResult_WithProjectDocuments()
        {
            // Arrange
            var documents = new List<DocumentDto>
            {
                new DocumentDto { Id = 1, Title = "Project Doc", Type = DocumentType.CRD, CreatedBy = 1, ProjectId = 1 }
            };
            _mockService.Setup(s => s.GetByProjectIdAsync(1)).ReturnsAsync(documents);

            // Act
            var result = await _controller.GetByProjectId(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDocs = Assert.IsAssignableFrom<IEnumerable<DocumentDto>>(okResult.Value);
            Assert.Single(returnedDocs);
        }

        [Fact]
        public async Task GetByType_ReturnsOkResult_WithDocumentsOfType()
        {
            // Arrange
            var documents = new List<DocumentDto>
            {
                new DocumentDto { Id = 1, Title = "CRD Doc", Type = DocumentType.CRD, CreatedBy = 1, ProjectId = 1 }
            };
            _mockService.Setup(s => s.GetByTypeAsync(DocumentType.CRD)).ReturnsAsync(documents);

            // Act
            var result = await _controller.GetByType(DocumentType.CRD);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDocs = Assert.IsAssignableFrom<IEnumerable<DocumentDto>>(okResult.Value);
            Assert.Single(returnedDocs);
            Assert.Equal(DocumentType.CRD, returnedDocs.First().Type);
        }

        [Fact]
        public async Task GetPagedByProjectId_ReturnsOkResult_WithPagedProjectDocuments()
        {
            // Arrange
            var pagedResult = new PagedResult<DocumentDto>
            {
                Items = new List<DocumentDto>
                {
                    new DocumentDto { Id = 1, Title = "Project Doc", Type = DocumentType.PRD, CreatedBy = 1, ProjectId = 1 }
                },
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 1
            };
            _mockService.Setup(s => s.GetPagedByProjectIdAsync(1, It.IsAny<PaginationParameters>())).ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetPagedByProjectId(1, 1, 10, "test", "title", false);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResult = Assert.IsType<PagedResult<DocumentDto>>(okResult.Value);
            Assert.Single(returnedResult.Items);
            Assert.Equal(1, returnedResult.Items.First().ProjectId);
        }

	[Fact]
	public async Task GetTraceability_ValidRequest_ReturnsOk()
	{
	    // Arrange
	    var documentId = 1;
	    var direction = "downstream";
	    var traceabilityMatrix = new TraceabilityMatrixDto
		{
		    DocumentId = documentId,
		    DocumentName = "Test Document",
		    DocumentType = DocumentType.PRD,
		    Direction = direction,
		    Traceability = new List<RequirementTraceabilityDto>(),
		    CoverageStats = new CoverageStatsDto()
		};

	    _mockService.Setup(s => s.GetTraceabilityMatrixAsync(documentId, direction, false))
		.ReturnsAsync(traceabilityMatrix);

	    // Act
	    var result = await _controller.GetTraceability(documentId, direction);

	    // Assert
	    var okResult = Assert.IsType<OkObjectResult>(result.Result);
	    var returnedMatrix = Assert.IsType<TraceabilityMatrixDto>(okResult.Value);
	    Assert.Equal(documentId, returnedMatrix.DocumentId);
	    Assert.Equal(direction, returnedMatrix.Direction);
	}

        [Fact]
        public async Task GetTraceability_DocumentNotFound_ReturnsNotFound()
	{
	    // Arrange
	    var documentId = 999;
	    var direction = "downstream";

	    _mockService.Setup(s => s.GetTraceabilityMatrixAsync(documentId, direction, false))
		.ReturnsAsync((TraceabilityMatrixDto?)null);

	    // Act
	    var result = await _controller.GetTraceability(documentId, direction);

	    // Assert
	    Assert.IsType<NotFoundResult>(result.Result);
	}

        [Theory]
        [InlineData("")]
        [InlineData("invalid")]
        public async Task GetTraceability_InvalidDirection_ReturnsBadRequest(string direction)
	{
	    // Arrange
	    var documentId = 1;

	    // Act
	    var result = await _controller.GetTraceability(documentId, direction);

	    // Assert
	    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
	    Assert.Contains("Direction parameter must be", badRequestResult.Value?.ToString());
	}

        [Fact]
        public async Task GetTraceability_UncoveredOnlyTrue_CallsServiceCorrectly()
	{
	    // Arrange
	    var documentId = 1;
	    var direction = "upstream";
	    var uncoveredOnly = true;

	    _mockService.Setup(s => s.GetTraceabilityMatrixAsync(documentId, direction, uncoveredOnly))
		.ReturnsAsync(new TraceabilityMatrixDto());

	    // Act
	    await _controller.GetTraceability(documentId, direction, uncoveredOnly);

	    // Assert
	    _mockService.Verify(s => s.GetTraceabilityMatrixAsync(documentId, direction, uncoveredOnly), Times.Once);
	}
    }
}
