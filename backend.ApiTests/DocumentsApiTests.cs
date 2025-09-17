using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RqmtMgmtShared;
using Xunit;

namespace backend.ApiTests
{
    /// <summary>
    /// Integration tests for the Documents API endpoints.
    /// Tests the actual HTTP endpoints against the running deployment.
    /// </summary>
    [Collection("Integration Tests")]
    public class DocumentsApiTests : BaseIntegrationTest
    {
        [Fact]
        public async Task GetDocuments_ShouldReturnDocumentsList_WhenAuthenticated()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Act
            var response = await _client.GetAsync("/api/documents");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var documents = await response.Content.ReadFromJsonAsync<List<DocumentDto>>(_jsonOptions);
            documents.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDocumentsPaged_ShouldReturnPagedDocumentsList_WhenAuthenticated()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Act
            var response = await _client.GetAsync("/api/documents/paged");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var documents = await response.Content.ReadFromJsonAsync<PagedResult<DocumentDto>>(_jsonOptions);
            documents.Should().NotBeNull();
            documents!.Items.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateDocument_ShouldCreateDocument_WhenValidDataProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var projectId = await GetValidProjectIdAsync();

            var createDto = new DocumentDto
            {
                Title = $"Test Document {Guid.NewGuid():N}",
                Type = DocumentType.CRD,
                Status = DocumentStatus.Draft,
                ProjectId = projectId,
                Objective = "Test objective for integration test",
                CreatedBy = 1, // Use admin user ID (same pattern as RequirementApiTests)
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/documents", createDto, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await response.Content.ReadFromJsonAsync<DocumentDto>(_jsonOptions);
            created.Should().NotBeNull();
            created!.Title.Should().Be(createDto.Title);
            created.Type.Should().Be(createDto.Type);
            created.Status.Should().Be(createDto.Status);
            created.ProjectId.Should().Be(createDto.ProjectId);
            created.CreatedBy.Should().Be(createDto.CreatedBy);
        }

        [Fact]
        public async Task GetDocument_ShouldReturnDocument_WhenValidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var projectId = await GetValidProjectIdAsync();

            // First create a document
            var createDto = new DocumentDto
            {
                Title = $"Get Test Document {Guid.NewGuid():N}",
                Type = DocumentType.PRD,
                Status = DocumentStatus.Draft,
                ProjectId = projectId,
                Objective = "Test objective for get test",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/documents", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<DocumentDto>(_jsonOptions);

            // Act
            var response = await _client.GetAsync($"/api/documents/{created!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var document = await response.Content.ReadFromJsonAsync<DocumentDto>(_jsonOptions);
            document.Should().NotBeNull();
            document!.Id.Should().Be(created.Id);
            document.Title.Should().Be(created.Title);
        }

        [Fact]
        public async Task UpdateDocument_ShouldUpdateDocument_WhenValidDataProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var projectId = await GetValidProjectIdAsync();

            // First create a document
            var createDto = new DocumentDto
            {
                Title = $"Update Test Document {Guid.NewGuid():N}",
                Type = DocumentType.SRS,
                Status = DocumentStatus.Draft,
                ProjectId = projectId,
                Objective = "Original objective",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/documents", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<DocumentDto>(_jsonOptions);

            // Update the document
            created!.Title = created.Title + " Updated";
            created.Status = DocumentStatus.InReview;
            created.Objective = "Updated objective";

            // Act
            var response = await _client.PutAsJsonAsync($"/api/documents/{created.Id}", created, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            
            // Verify the update by fetching the document
            var getResponse = await _client.GetAsync($"/api/documents/{created.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await getResponse.Content.ReadFromJsonAsync<DocumentDto>(_jsonOptions);
            updated.Should().NotBeNull();
            updated!.Title.Should().Be(created.Title);
            updated.Status.Should().Be(DocumentStatus.InReview);
            updated.Objective.Should().Be("Updated objective");
        }

        [Fact]
        public async Task DeleteDocument_ShouldDeleteDocument_WhenValidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var projectId = await GetValidProjectIdAsync();

            // First create a document
            var createDto = new DocumentDto
            {
                Title = $"Delete Test Document {Guid.NewGuid():N}",
                Type = DocumentType.CRD,
                Status = DocumentStatus.Draft,
                ProjectId = projectId,
                Objective = "Test objective for deletion",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/documents", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<DocumentDto>(_jsonOptions);

            // Act
            var response = await _client.DeleteAsync($"/api/documents/{created!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify deletion
            var getResponse = await _client.GetAsync($"/api/documents/{created.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetDocumentsByProject_ShouldReturnProjectDocuments_WhenValidProjectIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var projectId = await GetValidProjectIdAsync();

            // Act
            var response = await _client.GetAsync($"/api/documents/project/{projectId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var documents = await response.Content.ReadFromJsonAsync<List<DocumentDto>>(_jsonOptions);
            documents.Should().NotBeNull();
            // Don't assert content filtering if no documents exist yet
            if (documents!.Any())
            {
                documents.Should().OnlyContain(d => d.ProjectId == projectId);
            }
        }

        [Fact]
        public async Task GetDocumentsByType_ShouldReturnFilteredDocuments_WhenValidTypeProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Act
            var response = await _client.GetAsync($"/api/documents/type/{DocumentType.CRD}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var documents = await response.Content.ReadFromJsonAsync<List<DocumentDto>>(_jsonOptions);
            documents.Should().NotBeNull();
            // Don't assert content filtering if no documents exist yet
            if (documents!.Any())
            {
                documents.Should().OnlyContain(d => d.Type == DocumentType.CRD);
            }
        }

        [Fact]
        public async Task GetDocument_ShouldReturnNotFound_WhenInvalidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var invalidId = 999999;

            // Act
            var response = await _client.GetAsync($"/api/documents/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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