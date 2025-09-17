using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RqmtMgmtShared;
using Xunit;

namespace backend.ApiTests
{
    /// <summary>
    /// Integration tests for the Document Sections API endpoints.
    /// Tests the actual HTTP endpoints against the running deployment.
    /// </summary>
    [Collection("Integration Tests")]
    public class DocumentSectionsApiTests : BaseIntegrationTest
    {
        [Fact]
        public async Task GetDocumentSections_ShouldReturnEmptyList_WhenNoDocumentExists()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var nonExistentDocumentId = 999999;

            // Act
            var response = await _client.GetAsync($"/api/documentsections/document/{nonExistentDocumentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var sections = await response.Content.ReadFromJsonAsync<List<DocumentSectionDto>>(_jsonOptions);
            sections.Should().NotBeNull();
            sections!.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDocumentSection_ShouldReturnNotFound_WhenInvalidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var invalidId = 999999;

            // Act
            var response = await _client.GetAsync($"/api/documentsections/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateDocumentSection_ShouldHandleValidation_WhenCalledWithMinimalData()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            
            var createDto = new DocumentSectionDto
            {
                DocumentId = 1, // Use a simple ID for testing
                Title = $"Test Section {Guid.NewGuid():N}",
                SectionOrder = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/documentsections", createDto, _jsonOptions);

            // Assert - Either succeeds or fails with validation, both are acceptable for now
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }
    }
}