using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using RqmtMgmtShared;
using System;
using System.Collections.Generic;
using FluentAssertions;

namespace backend.ApiTests
{
    /// <summary>
    /// Integration tests for the Requirements controller endpoints.
    /// These tests run against the actual docker-compose.identity.yml instance with JWT authentication.
    /// </summary>
    [Collection("Integration Tests")]
    public class RequirementIntegrationTests : BaseIntegrationTest
    {
        [Fact]
        public async Task GetRequirements_ShouldReturnRequirementsList_WhenAuthenticated()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Act
            var response = await _client.GetAsync("/api/requirement");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var requirements = await response.Content.ReadFromJsonAsync<List<RequirementDto>>(_jsonOptions);
            requirements.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateRequirement_ShouldCreateRequirement_WhenValidDataProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var createDto = new RequirementDto
            {
                Title = $"Integration Test Requirement {Guid.NewGuid():N}",
                Type = RequirementType.CRS,
                Status = RequirementStatus.Draft,
                Description = "Created by integration test",
                CreatedBy = 1, // Assuming admin user exists
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/requirement", createDto, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await response.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            created.Should().NotBeNull();
            created!.Title.Should().Be(createDto.Title);
            created.Type.Should().Be(createDto.Type);
            created.Status.Should().Be(createDto.Status);
        }

        [Fact]
        public async Task GetRequirement_ShouldReturnRequirement_WhenValidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First create a requirement
            var createDto = new RequirementDto
            {
                Title = $"Get Test Requirement {Guid.NewGuid():N}",
                Type = RequirementType.CRS,
                Status = RequirementStatus.Draft,
                Description = "For get testing",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync()
            };

            var createResponse = await _client.PostAsJsonAsync("/api/requirement", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);

            // Act
            var response = await _client.GetAsync($"/api/requirement/{created!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var requirement = await response.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            requirement.Should().NotBeNull();
            requirement!.Id.Should().Be(created.Id);
            requirement.Title.Should().Be(createDto.Title);
        }

        [Fact]
        public async Task UpdateRequirement_ShouldUpdateRequirement_WhenValidDataProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First create a requirement
            var createDto = new RequirementDto
            {
                Title = $"Update Test Requirement {Guid.NewGuid():N}",
                Type = RequirementType.CRS,
                Status = RequirementStatus.Draft,
                Description = "To be updated",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync()
            };

            var createResponse = await _client.PostAsJsonAsync("/api/requirement", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);

            // Update the requirement
            created!.Title = created.Title + " - Updated";
            created.Description = "Updated by integration test";
            created.Status = RequirementStatus.Approved;

            // Act
            var response = await _client.PutAsJsonAsync($"/api/requirement/{created.Id}", created, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            
            // Verify the update by getting the requirement again
            var getResponse = await _client.GetAsync($"/api/requirement/{created.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await getResponse.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            updated.Should().NotBeNull();
            updated!.Title.Should().Contain("Updated");
            updated.Status.Should().Be(RequirementStatus.Approved);
        }

        [Fact]
        public async Task DeleteRequirement_ShouldDeleteRequirement_WhenValidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First create a requirement
            var createDto = new RequirementDto
            {
                Title = $"Delete Test Requirement {Guid.NewGuid():N}",
                Type = RequirementType.CRS,
                Status = RequirementStatus.Draft,
                Description = "To be deleted",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync()
            };

            var createResponse = await _client.PostAsJsonAsync("/api/requirement", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/requirement/{created!.Id}");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify it's deleted
            var getResponse = await _client.GetAsync($"/api/requirement/{created.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
                // Create a test project if none exist
                var createDto = new CreateProjectDto
                {
                    Name = $"Test Project for Requirements {Guid.NewGuid():N}",
                    Code = $"REQ{DateTime.UtcNow:mmss}",
                    Description = "Auto-created for requirement testing",
                    OwnerId = 1,
                    Status = ProjectStatus.Planning
                };

                var createResponse = await _client.PostAsJsonAsync("/api/projects", createDto, _jsonOptions);
                createResponse.EnsureSuccessStatusCode();
                var created = await createResponse.Content.ReadFromJsonAsync<ProjectDto>(_jsonOptions);
                return created!.Id;
            }

            return projects!.Items![0].Id;
        }
    }
}
