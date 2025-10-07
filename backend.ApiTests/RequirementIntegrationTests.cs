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
                Type = RequirementType.CRD,
                Status = RequirementStatus.Draft,
                Description = "Created by integration test",
                CreatedBy = 1, // Assuming admin user exists
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync(),
                SectionId = 1  // Use existing section ID from database
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/requirement", createDto, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdRequirement = await response.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            createdRequirement.Should().NotBeNull();
            createdRequirement!.Title.Should().Be(createDto.Title);
        }

        [Fact]
        public async Task GetRequirement_ShouldReturnRequirement_WhenValidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var createDto = new RequirementDto
            {
                Title = $"Test Requirement {Guid.NewGuid():N}",
                Type = RequirementType.PRD,
                Status = RequirementStatus.Draft,
                Description = "Test requirement for get operation",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync(),
                SectionId = 1  // Use existing section ID from database
            };

            var createResponse = await _client.PostAsJsonAsync("/api/requirement", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var createdRequirement = await createResponse.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);

            // Act
            var response = await _client.GetAsync($"/api/requirement/{createdRequirement!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var requirement = await response.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            requirement.Should().NotBeNull();
            requirement!.Id.Should().Be(createdRequirement.Id);
            requirement.Title.Should().Be(createDto.Title);
        }

        [Fact]
        public async Task GetRequirement_ShouldReturnNotFound_WhenInvalidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Act
            var response = await _client.GetAsync("/api/requirement/999999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateRequirement_ShouldUpdateRequirement_WhenValidDataProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var createDto = new RequirementDto
            {
                Title = $"Update Test Requirement {Guid.NewGuid():N}",
                Type = RequirementType.SRS,
                Status = RequirementStatus.Draft,
                Description = "Test requirement for update operation",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync(),
                SectionId = 1  // Use existing section ID from database
            };

            var createResponse = await _client.PostAsJsonAsync("/api/requirement", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var createdRequirement = await createResponse.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);

            // Modify the requirement
            createdRequirement!.Title = "Updated Title";
            createdRequirement.Status = RequirementStatus.Approved;

            // Act
            var response = await _client.PutAsJsonAsync($"/api/requirement/{createdRequirement.Id}", createdRequirement, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the update
            var getResponse = await _client.GetAsync($"/api/requirement/{createdRequirement.Id}");
            var updatedRequirement = await getResponse.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            updatedRequirement!.Title.Should().Be("Updated Title");
            updatedRequirement.Status.Should().Be(RequirementStatus.Approved);
        }

        [Fact]
        public async Task UpdateRequirement_ShouldReturnNotFound_WhenInvalidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var updateDto = new RequirementDto
            {
                Id = 999999,
                Title = "Non-existent Requirement",
                Type = RequirementType.CRD,
                Status = RequirementStatus.Draft,
                Description = "This requirement does not exist",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync(),
                SectionId = 1  // Use existing section ID from database
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/requirement/999999", updateDto, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteRequirement_ShouldDeleteRequirement_WhenValidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var createDto = new RequirementDto
            {
                Title = $"Delete Test Requirement {Guid.NewGuid():N}",
                Type = RequirementType.CRD,
                Status = RequirementStatus.Draft,
                Description = "Test requirement for delete operation",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync(),
                SectionId = 1  // Use existing section ID from database
            };

            var createResponse = await _client.PostAsJsonAsync("/api/requirement", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var createdRequirement = await createResponse.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);

            // Act
            var response = await _client.DeleteAsync($"/api/requirement/{createdRequirement!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify deletion
            var getResponse = await _client.GetAsync($"/api/requirement/{createdRequirement.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteRequirement_ShouldReturnNotFound_WhenInvalidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Act
            var response = await _client.DeleteAsync("/api/requirement/999999");

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
                // Create a test project if none exist
                var createDto = new CreateProjectDto
                {
                    Name = $"Test Project {Guid.NewGuid():N}",
                    Code = $"TST{DateTime.UtcNow:mmss}",
                    Description = "Auto-created for integration testing",
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