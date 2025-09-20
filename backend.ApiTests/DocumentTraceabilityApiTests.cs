using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RqmtMgmtShared;
using Xunit;

namespace backend.ApiTests
{
    /// <summary>
    /// Integration tests for the Document Traceability Matrix API endpoints.
    /// Tests the actual HTTP endpoints against the running deployment.
    /// </summary>
    [Collection("Integration Tests")]
    public class DocumentTraceabilityApiTests : BaseIntegrationTest
    {
        [Fact]
        public async Task GetDocumentTraceability_ShouldReturnTraceabilityMatrix_WhenValidIdAndDirectionProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var projectId = await GetValidProjectIdAsync();

            // First create a document to test traceability for
            var createDto = new DocumentDto
            {
                Title = $"Traceability Test Document {Guid.NewGuid():N}",
                Type = DocumentType.PRD,
                Status = DocumentStatus.Draft,
                ProjectId = projectId,
                Objective = "Test document for traceability matrix",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/documents", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<DocumentDto>(_jsonOptions);

            // Act - Test downstream traceability
            var response = await _client.GetAsync($"/api/documents/{created!.Id}/traceability?direction=downstream");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var traceabilityMatrix = await response.Content.ReadFromJsonAsync<TraceabilityMatrixDto>(_jsonOptions);
            traceabilityMatrix.Should().NotBeNull();
            traceabilityMatrix!.DocumentId.Should().Be(created.Id);
            traceabilityMatrix.DocumentName.Should().Be(created.Title);
            traceabilityMatrix.DocumentType.Should().Be(DocumentType.PRD);
            traceabilityMatrix.Direction.Should().Be("downstream");
            traceabilityMatrix.Traceability.Should().NotBeNull();
            traceabilityMatrix.CoverageStats.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDocumentTraceability_ShouldReturnTraceabilityMatrix_WhenUpstreamDirectionProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var projectId = await GetValidProjectIdAsync();

            // Create a document to test upstream traceability for
            var createDto = new DocumentDto
            {
                Title = $"Upstream Traceability Test {Guid.NewGuid():N}",
                Type = DocumentType.SRS,
                Status = DocumentStatus.Draft,
                ProjectId = projectId,
                Objective = "Test document for upstream traceability",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/documents", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<DocumentDto>(_jsonOptions);

            // Act - Test upstream traceability
            var response = await _client.GetAsync($"/api/documents/{created!.Id}/traceability?direction=upstream");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var traceabilityMatrix = await response.Content.ReadFromJsonAsync<TraceabilityMatrixDto>(_jsonOptions);
            traceabilityMatrix.Should().NotBeNull();
            traceabilityMatrix!.DocumentId.Should().Be(created.Id);
            traceabilityMatrix.Direction.Should().Be("upstream");
            traceabilityMatrix.CoverageStats.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDocumentTraceability_ShouldReturnUncoveredOnly_WhenUncoveredOnlyFlagSet()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var projectId = await GetValidProjectIdAsync();

            // Create a document for uncovered requirements testing
            var createDto = new DocumentDto
            {
                Title = $"Uncovered Requirements Test {Guid.NewGuid():N}",
                Type = DocumentType.CRD,
                Status = DocumentStatus.Draft,
                ProjectId = projectId,
                Objective = "Test document for uncovered requirements analysis",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/documents", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<DocumentDto>(_jsonOptions);

            // Act - Test uncovered only filter
            var response = await _client.GetAsync($"/api/documents/{created!.Id}/traceability?direction=downstream&uncoveredOnly=true");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var traceabilityMatrix = await response.Content.ReadFromJsonAsync<TraceabilityMatrixDto>(_jsonOptions);
            traceabilityMatrix.Should().NotBeNull();
            traceabilityMatrix!.DocumentId.Should().Be(created.Id);
            traceabilityMatrix.Direction.Should().Be("downstream");
            // When uncoveredOnly=true, only requirements without traces should be returned
            // The actual content will depend on whether there are requirements and traces in the test data
        }

        [Fact]
        public async Task GetDocumentTraceability_ShouldReturnBadRequest_WhenInvalidDirectionProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var projectId = await GetValidProjectIdAsync();

            // Create a document
            var createDto = new DocumentDto
            {
                Title = $"Invalid Direction Test {Guid.NewGuid():N}",
                Type = DocumentType.PRD,
                Status = DocumentStatus.Draft,
                ProjectId = projectId,
                Objective = "Test document for invalid direction parameter",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/documents", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<DocumentDto>(_jsonOptions);

            // Act - Test with invalid direction
            var response = await _client.GetAsync($"/api/documents/{created!.Id}/traceability?direction=invalid");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetDocumentTraceability_ShouldReturnBadRequest_WhenDirectionParameterMissing()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var projectId = await GetValidProjectIdAsync();

            // Create a document
            var createDto = new DocumentDto
            {
                Title = $"Missing Direction Test {Guid.NewGuid():N}",
                Type = DocumentType.SRS,
                Status = DocumentStatus.Draft,
                ProjectId = projectId,
                Objective = "Test document for missing direction parameter",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/documents", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<DocumentDto>(_jsonOptions);

            // Act - Test without direction parameter
            var response = await _client.GetAsync($"/api/documents/{created!.Id}/traceability");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetDocumentTraceability_ShouldReturnNotFound_WhenInvalidDocumentIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var invalidId = 999999;

            // Act
            var response = await _client.GetAsync($"/api/documents/{invalidId}/traceability?direction=downstream");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Theory]
        [InlineData("upstream")]
        [InlineData("downstream")]
        [InlineData("UPSTREAM")]
        [InlineData("DOWNSTREAM")]
        public async Task GetDocumentTraceability_ShouldBeCaseInsensitive_ForDirectionParameter(string direction)
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var projectId = await GetValidProjectIdAsync();

            // Create a document
            var createDto = new DocumentDto
            {
                Title = $"Case Sensitivity Test {Guid.NewGuid():N}",
                Type = DocumentType.PRD,
                Status = DocumentStatus.Draft,
                ProjectId = projectId,
                Objective = "Test document for case sensitivity",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/documents", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<DocumentDto>(_jsonOptions);

            // Act
            var response = await _client.GetAsync($"/api/documents/{created!.Id}/traceability?direction={direction}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var traceabilityMatrix = await response.Content.ReadFromJsonAsync<TraceabilityMatrixDto>(_jsonOptions);
            traceabilityMatrix.Should().NotBeNull();
            traceabilityMatrix!.Direction.Should().Be(direction.ToLower());
        }

        [Fact]
        public async Task GetDocumentTraceability_ShouldHandleCoverageStats_WhenDocumentHasRequirements()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var projectId = await GetValidProjectIdAsync();

            // Create a document
            var createDto = new DocumentDto
            {
                Title = $"Coverage Stats Test {Guid.NewGuid():N}",
                Type = DocumentType.CRD,
                Status = DocumentStatus.Draft,
                ProjectId = projectId,
                Objective = "Test document for coverage statistics",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/documents", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<DocumentDto>(_jsonOptions);

            // Act
            var response = await _client.GetAsync($"/api/documents/{created!.Id}/traceability?direction=downstream");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var traceabilityMatrix = await response.Content.ReadFromJsonAsync<TraceabilityMatrixDto>(_jsonOptions);
            traceabilityMatrix.Should().NotBeNull();
            traceabilityMatrix!.CoverageStats.Should().NotBeNull();
            traceabilityMatrix.CoverageStats.TotalRequirements.Should().BeGreaterThanOrEqualTo(0);
            traceabilityMatrix.CoverageStats.CoveredRequirements.Should().BeGreaterThanOrEqualTo(0);
            traceabilityMatrix.CoverageStats.CoveragePercentage.Should().BeInRange(0, 100);
            traceabilityMatrix.CoverageStats.UncoveredRequirements.Should().NotBeNull();
        }

        /// <summary>
        /// Helper method to get a valid project ID for testing.
        /// </summary>
        private async Task<int> GetValidProjectIdAsync()
        {
            var response = await _client.GetAsync("/api/projects");
            response.EnsureSuccessStatusCode();
            var projects = await response.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
            
            if (projects?.Items?.Count == 0)
            {
                throw new InvalidOperationException("No projects available for testing. Please ensure test data is seeded.");
            }
            
            return projects!.Items!.First().Id;
        }
    }
}