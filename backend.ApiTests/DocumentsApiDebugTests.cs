using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RqmtMgmtShared;
using Xunit;

namespace backend.ApiTests
{
    /// <summary>
    /// Debug test to investigate the Documents API issue.
    /// </summary>
    [Collection("Integration Tests")]
    public class DocumentsApiDebugTests : BaseIntegrationTest
    {
        [Fact]
        public async Task Debug_CreateDocument_ShouldShowActualError()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var projectId = await GetValidProjectIdAsync();

            var createDto = new DocumentDto
            {
                Title = $"Debug Test Document {Guid.NewGuid():N}",
                Type = DocumentType.CRD,
                Status = DocumentStatus.Draft,
                ProjectId = projectId,
                Objective = "Debug test objective",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/documents", createDto, _jsonOptions);

            // Debug: Show the actual error response
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API returned {response.StatusCode}: {errorContent}");
            }

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
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