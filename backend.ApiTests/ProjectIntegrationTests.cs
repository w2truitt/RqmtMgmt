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
    /// Integration tests for the Projects controller endpoints.
    /// These tests run against the actual docker-compose.identity.yml instance with JWT authentication.
    /// </summary>
    [Collection("Integration Tests")]
    public class ProjectIntegrationTests : BaseIntegrationTest
    {
        [Fact]
        public async Task GetProjects_ShouldReturnProjectList_WhenAuthenticated()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Act
            var response = await _client.GetAsync("/api/projects");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var projects = await response.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
            projects.Should().NotBeNull();
            projects!.Items.Should().NotBeNull();
        }

        [Fact]
        public async Task GetProject_ShouldReturnProject_WhenValidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First get all projects to find a valid ID
            var listResponse = await _client.GetAsync("/api/projects");
            listResponse.EnsureSuccessStatusCode();
            var projects = await listResponse.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
            
            if (projects?.Items?.Count == 0)
            {
                Skip.If(true, "No projects available for testing");
            }

            var projectId = projects!.Items![0].Id;

            // Act
            var response = await _client.GetAsync($"/api/projects/{projectId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var project = await response.Content.ReadFromJsonAsync<ProjectDto>(_jsonOptions);
            project.Should().NotBeNull();
            project!.Id.Should().Be(projectId);
        }

        [Fact]
        public async Task CreateProject_ShouldCreateProject_WhenValidDataProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var createDto = new CreateProjectDto
            {
                Name = $"Integration Test Project {Guid.NewGuid():N}",
                Code = $"INT{DateTime.UtcNow:mmss}",
                Description = "Created by integration test",
                OwnerId = 1, // Assuming admin user with ID 1 exists
                Status = ProjectStatus.Planning
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/projects", createDto, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await response.Content.ReadFromJsonAsync<ProjectDto>(_jsonOptions);
            created.Should().NotBeNull();
            created!.Name.Should().Be(createDto.Name);
            created.Code.Should().Be(createDto.Code);
            created.Status.Should().Be(createDto.Status);
        }

        [Fact]
        public async Task UpdateProject_ShouldUpdateProject_WhenValidDataProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First create a project
            var createDto = new CreateProjectDto
            {
                Name = $"Update Test Project {Guid.NewGuid():N}",
                Code = $"UPD{DateTime.UtcNow:mmss}",
                Description = "To be updated by integration test",
                OwnerId = 1,
                Status = ProjectStatus.Planning
            };

            var createResponse = await _client.PostAsJsonAsync("/api/projects", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<ProjectDto>(_jsonOptions);

            // Update the project
            var updateDto = new UpdateProjectDto
            {
                Name = created!.Name + " - Updated",
                Description = "Updated by integration test",
                Status = ProjectStatus.Active
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/projects/{created.Id}", updateDto, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await response.Content.ReadFromJsonAsync<ProjectDto>(_jsonOptions);
            updated.Should().NotBeNull();
            updated!.Name.Should().Be(updateDto.Name);
            updated.Description.Should().Be(updateDto.Description);
            updated.Status.Should().Be(updateDto.Status);
        }

        [Fact]
        public async Task GetProject_ShouldReturnNotFound_WhenInvalidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var invalidId = 999999;

            // Act
            var response = await _client.GetAsync($"/api/projects/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
