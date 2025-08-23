using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using RqmtMgmtShared;
usin            // Act
            var response = await _client.PutAsJsonAsync($"/api/testsuite/{created.Id}", created, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            
            // Verify the update by getting the test suite again
            var getResponse = await _client.GetAsync($"/api/testsuite/{created.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await getResponse.Content.ReadFromJsonAsync<TestSuiteDto>(_jsonOptions);
            updated.Should().NotBeNull();
            updated!.Name.Should().Contain("Updated");;
using System.Collections.Generic;
using FluentAssertions;

namespace backend.ApiTests
{
    /// <summary>
    /// Integration tests for the TestSuites controller endpoints.
    /// These tests run against the actual docker-compose.identity.yml instance with JWT authentication.
    /// </summary>
    [Collection("Integration Tests")]
    public class TestSuiteIntegrationTests : BaseIntegrationTest
    {
        [Fact]
        public async Task GetTestSuites_ShouldReturnTestSuitesList_WhenAuthenticated()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Act
            var response = await _client.GetAsync("/api/testsuite");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var testSuites = await response.Content.ReadFromJsonAsync<List<TestSuiteDto>>(_jsonOptions);
            testSuites.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateTestSuite_ShouldCreateTestSuite_WhenValidDataProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var createDto = new TestSuiteDto
            {
                Name = $"Integration Test Suite {Guid.NewGuid():N}",
                Description = "Created by integration test",
                ProjectId = await GetValidProjectIdAsync(),
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/testsuite", createDto, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await response.Content.ReadFromJsonAsync<TestSuiteDto>(_jsonOptions);
            created.Should().NotBeNull();
            created!.Name.Should().Be(createDto.Name);
            created.Description.Should().Be(createDto.Description);
            created.ProjectId.Should().Be(createDto.ProjectId);
        }

        [Fact]
        public async Task GetTestSuite_ShouldReturnTestSuite_WhenValidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First create a test suite
            var createDto = new TestSuiteDto
            {
                Name = $"Get Test Suite {Guid.NewGuid():N}",
                Description = "Created for get test",
                ProjectId = await GetValidProjectIdAsync(),
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/testsuite", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<TestSuiteDto>(_jsonOptions);

            // Act
            var response = await _client.GetAsync($"/api/testsuite/{created!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var testSuite = await response.Content.ReadFromJsonAsync<TestSuiteDto>(_jsonOptions);
            testSuite.Should().NotBeNull();
            testSuite!.Id.Should().Be(created.Id);
            testSuite.Name.Should().Be(created.Name);
        }

        [Fact]
        public async Task UpdateTestSuite_ShouldUpdateTestSuite_WhenValidDataProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First create a test suite
            var createDto = new TestSuiteDto
            {
                Name = $"Update Test Suite {Guid.NewGuid():N}",
                Description = "To be updated",
                ProjectId = await GetValidProjectIdAsync(),
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/testsuite", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<TestSuiteDto>(_jsonOptions);

            // Update the test suite
            created!.Name = created.Name + " - Updated";
            created.Description = "Updated by integration test";

            // Act
            var response = await _client.PutAsJsonAsync($"/api/testsuite/{created.Id}", created, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await response.Content.ReadFromJsonAsync<TestSuiteDto>(_jsonOptions);
            updated.Should().NotBeNull();
            updated!.Name.Should().Be(created.Name);
            updated.Description.Should().Be(created.Description);
        }

        [Fact]
        public async Task DeleteTestSuite_ShouldDeleteTestSuite_WhenValidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First create a test suite
            var createDto = new TestSuiteDto
            {
                Name = $"Delete Test Suite {Guid.NewGuid():N}",
                Description = "To be deleted",
                ProjectId = await GetValidProjectIdAsync(),
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/testsuite", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<TestSuiteDto>(_jsonOptions);

            // Act
            var response = await _client.DeleteAsync($"/api/testsuite/{created!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify deletion
            var getResponse = await _client.GetAsync($"/api/testsuite/{created.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetTestSuite_ShouldReturnNotFound_WhenInvalidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var invalidId = 999999;

            // Act
            var response = await _client.GetAsync($"/api/testsuite/{invalidId}");

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
            
            if (projects?.Items?.Count > 0)
            {
                return projects.Items[0].Id;
            }

            // Create a project if none exist
            var createProjectDto = new CreateProjectDto
            {
                Name = $"Integration Test Project {Guid.NewGuid():N}",
                Description = "Created for integration testing"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/projects", createProjectDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<ProjectDto>(_jsonOptions);
            
            return created!.Id;
        }
    }
}
