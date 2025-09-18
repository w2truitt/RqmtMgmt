using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using backend.Controllers;
using RqmtMgmtShared;
using Xunit;

namespace backend.Tests
{
    /// <summary>
    /// Unit tests for RequirementTracesController to verify traceability API endpoints.
    /// </summary>
    public class RequirementTracesControllerTests
    {
        private readonly Mock<IRequirementTraceService> _mockService;
        private readonly RequirementTracesController _controller;

        public RequirementTracesControllerTests()
        {
            _mockService = new Mock<IRequirementTraceService>();
            _controller = new RequirementTracesController(_mockService.Object);
        }

        [Fact]
        public async Task GetBySourceRequirementId_ReturnsOkResult_WithOutgoingTraces()
        {
            // Arrange
            var traces = new List<RequirementTraceDto>
            {
                new RequirementTraceDto { Id = 1, SourceRequirementId = 1, TargetRequirementId = 2, TraceType = TraceType.DerivedFrom, CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new RequirementTraceDto { Id = 2, SourceRequirementId = 1, TargetRequirementId = 3, TraceType = TraceType.ImplementedBy, CreatedBy = 1, CreatedAt = DateTime.UtcNow }
            };
            _mockService.Setup(s => s.GetBySourceRequirementIdAsync(1)).ReturnsAsync(traces);

            // Act
            var result = await _controller.GetBySourceRequirementId(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTraces = Assert.IsAssignableFrom<IEnumerable<RequirementTraceDto>>(okResult.Value);
            Assert.Equal(2, returnedTraces.Count());
        }

        [Fact]
        public async Task GetByTargetRequirementId_ReturnsOkResult_WithIncomingTraces()
        {
            // Arrange
            var traces = new List<RequirementTraceDto>
            {
                new RequirementTraceDto { Id = 1, SourceRequirementId = 2, TargetRequirementId = 1, TraceType = TraceType.ValidatedBy, CreatedBy = 1, CreatedAt = DateTime.UtcNow }
            };
            _mockService.Setup(s => s.GetByTargetRequirementIdAsync(1)).ReturnsAsync(traces);

            // Act
            var result = await _controller.GetByTargetRequirementId(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTraces = Assert.IsAssignableFrom<IEnumerable<RequirementTraceDto>>(okResult.Value);
            Assert.Single(returnedTraces);
        }

        [Fact]
        public async Task GetTraceChain_ReturnsOkResult_WithCompleteChain()
        {
            // Arrange
            var traces = new List<RequirementTraceDto>
            {
                new RequirementTraceDto { Id = 1, SourceRequirementId = 1, TargetRequirementId = 2, TraceType = TraceType.DerivedFrom, CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new RequirementTraceDto { Id = 2, SourceRequirementId = 2, TargetRequirementId = 3, TraceType = TraceType.ImplementedBy, CreatedBy = 1, CreatedAt = DateTime.UtcNow }
            };
            _mockService.Setup(s => s.GetTraceChainAsync(2)).ReturnsAsync(traces);

            // Act
            var result = await _controller.GetTraceChain(2);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTraces = Assert.IsAssignableFrom<IEnumerable<RequirementTraceDto>>(okResult.Value);
            Assert.Equal(2, returnedTraces.Count());
        }

        [Fact]
        public async Task GetById_ReturnsOkResult_WhenTraceExists()
        {
            // Arrange
            var trace = new RequirementTraceDto 
            { 
                Id = 1, 
                SourceRequirementId = 1, 
                TargetRequirementId = 2, 
                TraceType = TraceType.RelatedTo, 
                CreatedBy = 1, 
                CreatedAt = DateTime.UtcNow 
            };
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(trace);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTrace = Assert.IsType<RequirementTraceDto>(okResult.Value);
            Assert.Equal(TraceType.RelatedTo, returnedTrace.TraceType);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenTraceDoesNotExist()
        {
            // Arrange
            _mockService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((RequirementTraceDto?)null);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var inputTrace = new RequirementTraceDto 
            { 
                SourceRequirementId = 1, 
                TargetRequirementId = 2, 
                TraceType = TraceType.DerivedFrom, 
                CreatedBy = 1 
            };
            var createdTrace = new RequirementTraceDto 
            { 
                Id = 1, 
                SourceRequirementId = 1, 
                TargetRequirementId = 2, 
                TraceType = TraceType.DerivedFrom, 
                CreatedBy = 1, 
                CreatedAt = DateTime.UtcNow 
            };
            _mockService.Setup(s => s.CreateAsync(inputTrace)).ReturnsAsync(createdTrace);

            // Act
            var result = await _controller.Create(inputTrace);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
            Assert.Equal(1, createdResult.RouteValues!["id"]);
            var returnedTrace = Assert.IsType<RequirementTraceDto>(createdResult.Value);
            Assert.Equal(TraceType.DerivedFrom, returnedTrace.TraceType);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenServiceReturnsNull()
        {
            // Arrange
            var inputTrace = new RequirementTraceDto 
            { 
                SourceRequirementId = 1, 
                TargetRequirementId = 1, // Invalid self-reference
                TraceType = TraceType.DerivedFrom, 
                CreatedBy = 1 
            };
            _mockService.Setup(s => s.CreateAsync(inputTrace)).ReturnsAsync((RequirementTraceDto?)null);

            // Act
            var result = await _controller.Create(inputTrace);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Failed to create requirement trace. Check validation rules.", badRequestResult.Value);
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
        public async Task ValidateTrace_ReturnsOkResult_WithValidationResult()
        {
            // Arrange
            _mockService.Setup(s => s.ValidateTraceAsync(1, 2, TraceType.DerivedFrom)).ReturnsAsync(true);

            // Act
            var result = await _controller.ValidateTrace(1, 2, TraceType.DerivedFrom);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var validationResult = Assert.IsType<bool>(okResult.Value);
            Assert.True(validationResult);
        }

        [Fact]
        public async Task ValidateTrace_ReturnsOkResult_WithFalseForInvalidTrace()
        {
            // Arrange
            _mockService.Setup(s => s.ValidateTraceAsync(1, 1, TraceType.DerivedFrom)).ReturnsAsync(false); // Self-reference

            // Act
            var result = await _controller.ValidateTrace(1, 1, TraceType.DerivedFrom);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var validationResult = Assert.IsType<bool>(okResult.Value);
            Assert.False(validationResult);
        }
    }
}