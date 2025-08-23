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
    /// Integration tests for test execution related endpoints.
    /// These tests run against the actual docker-compose.identity.yml instance with JWT authentication.
    /// </summary>
    [Collection("Integration Tests")]
    public class TestExecutionIntegrationTests : BaseIntegrationTest
    {
        [Fact]
        public async Task GetTestRunSessions_ShouldReturnSessionsList_WhenAuthenticated()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Act
            var response = await _client.GetAsync("/api/testrun/sessions");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var sessions = await response.Content.ReadFromJsonAsync<List<TestRunSessionDto>>(_jsonOptions);
            sessions.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateTestRunSession_ShouldCreateSession_WhenValidDataProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var testPlanId = await GetValidTestPlanIdAsync();
            var createDto = new TestRunSessionDto
            {
                Name = $"Integration Test Run {Guid.NewGuid():N}",
                TestPlanId = testPlanId,
                Status = TestRunStatus.InProgress,
                ExecutedBy = 1,
                StartedAt = DateTime.UtcNow
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/testrun/sessions", createDto, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await response.Content.ReadFromJsonAsync<TestRunSessionDto>(_jsonOptions);
            created.Should().NotBeNull();
            created!.Name.Should().Be(createDto.Name);
            created.TestPlanId.Should().Be(createDto.TestPlanId);
            created.Status.Should().Be(createDto.Status);
        }

        [Fact]
        public async Task GetTestRunSession_ShouldReturnSession_WhenValidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First create a test run session
            var testPlanId = await GetValidTestPlanIdAsync();
            var createDto = new TestRunSessionDto
            {
                Name = $"Get Test Run {Guid.NewGuid():N}",
                TestPlanId = testPlanId,
                Status = TestRunStatus.InProgress,
                ExecutedBy = 1,
                StartedAt = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/testrun/sessions", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<TestRunSessionDto>(_jsonOptions);

            // Act
            var response = await _client.GetAsync($"/api/testrun/sessions/{created!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var session = await response.Content.ReadFromJsonAsync<TestRunSessionDto>(_jsonOptions);
            session.Should().NotBeNull();
            session!.Id.Should().Be(created.Id);
            session.Name.Should().Be(created.Name);
        }

        [Fact]
        public async Task UpdateTestRunSession_ShouldUpdateSession_WhenValidDataProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First create a test run session
            var testPlanId = await GetValidTestPlanIdAsync();
            var createDto = new TestRunSessionDto
            {
                Name = $"Update Test Run {Guid.NewGuid():N}",
                TestPlanId = testPlanId,
                Status = TestRunStatus.InProgress,
                ExecutedBy = 1,
                StartedAt = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/testrun/sessions", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<TestRunSessionDto>(_jsonOptions);

            // Update the session
            created!.Status = TestRunStatus.Completed;

            // Act
            var response = await _client.PutAsJsonAsync($"/api/testrun/sessions/{created.Id}", created, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await response.Content.ReadFromJsonAsync<TestRunSessionDto>(_jsonOptions);
            updated.Should().NotBeNull();
            updated!.Status.Should().Be(TestRunStatus.Completed);
        }

        [Fact]
        public async Task GetTestCaseExecutions_ShouldReturnExecutionsList_WhenAuthenticated()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Act
            var response = await _client.GetAsync("/api/testexecution/cases");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var executions = await response.Content.ReadFromJsonAsync<List<TestCaseExecutionDto>>(_jsonOptions);
            executions.Should().NotBeNull();
        }

        [Fact]
        public async Task GetTestRunSession_ShouldReturnNotFound_WhenInvalidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var invalidId = 999999;

            // Act
            var response = await _client.GetAsync($"/api/testrun/sessions/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Helper method to get a valid test plan ID for testing.
        /// </summary>
        private async Task<int> GetValidTestPlanIdAsync()
        {
            var response = await _client.GetAsync("/api/testplan");
            response.EnsureSuccessStatusCode();
            
            var testPlans = await response.Content.ReadFromJsonAsync<List<TestPlanDto>>(_jsonOptions);
            
            if (testPlans?.Count > 0)
            {
                return testPlans[0].Id;
            }

            // Create a test plan if none exist
            var createTestPlanDto = new TestPlanDto
            {
                Name = $"Integration Test Plan {Guid.NewGuid():N}",
                Type = "SoftwareVerification",
                Description = "Created for integration testing",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync()
            };

            var createResponse = await _client.PostAsJsonAsync("/api/testplan", createTestPlanDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<TestPlanDto>(_jsonOptions);
            
            return created!.Id;
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
