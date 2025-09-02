using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using RqmtMgmtShared;
using System.Collections.Generic;
using System;

namespace backend.ApiTests
{
    /// <summary>
    /// API tests for the TestPlan controller endpoints.
    /// These tests run against the actual docker-compose.identity.yml instance with JWT authentication.
    /// </summary>
    [Collection("Integration Tests")]
    public class TestPlanApiTests : BaseIntegrationTest
    {
        [Fact]
        public async Task CanCreateAndGetTestPlan()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First create a project to satisfy foreign key constraint
            var projectDto = new CreateProjectDto
            {
                Name = "TestPlan Test Project",
                Code = "TTP001",
                Description = "Project for TestPlan API test",
                OwnerId = 1
            };
            var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectDto, _jsonOptions);
            projectResponse.EnsureSuccessStatusCode();
            var project = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>(_jsonOptions);
            Assert.NotNull(project);

            var createDto = new TestPlanDto
            {
                Name = "API Test Plan",
                Type = "UserValidation",
                Description = "Created by integration test",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = project.Id
            };
            var response = await _client.PostAsJsonAsync("/api/testplan", createDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<TestPlanDto>(_jsonOptions);
            Assert.NotNull(created);
            Assert.Equal("API Test Plan", created.Name);
            Assert.Equal(project.Id, created.ProjectId);

            var getResp = await _client.GetAsync($"/api/testplan/{created.Id}");
            getResp.EnsureSuccessStatusCode();
            var fetched = await getResp.Content.ReadFromJsonAsync<TestPlanDto>(_jsonOptions);
            Assert.NotNull(fetched);
            Assert.Equal("API Test Plan", fetched.Name);
            Assert.Equal(project.Id, fetched.ProjectId);
        }

        [Fact]
        public async Task CanListTestPlans()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var response = await _client.GetAsync("/api/testplan");
            response.EnsureSuccessStatusCode();
            var list = await response.Content.ReadFromJsonAsync<List<TestPlanDto>>(_jsonOptions);
            Assert.NotNull(list);
            Assert.True(list.Count > 0);
        }

        [Fact]
        public async Task CanUpdateTestPlan()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First create a project to satisfy foreign key constraint
            var projectDto = new CreateProjectDto
            {
                Name = "TestPlan Update Test Project",
                Code = "TUTP001",
                Description = "Project for TestPlan update API test",
                OwnerId = 1
            };
            var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectDto, _jsonOptions);
            projectResponse.EnsureSuccessStatusCode();
            var project = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>(_jsonOptions);
            Assert.NotNull(project);

            // First create
            var createDto = new TestPlanDto
            {
                Name = "Update Test Plan",
                Type = "SoftwareVerification",
                Description = "To be updated",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = project.Id
            };
            var response = await _client.PostAsJsonAsync("/api/testplan", createDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<TestPlanDto>(_jsonOptions);
            Assert.NotNull(created);

            // Now update
            created.Name = "Updated Name";
            created.Description = "Updated Desc";
            var putResp = await _client.PutAsJsonAsync($"/api/testplan/{created.Id}", created, _jsonOptions);
            putResp.EnsureSuccessStatusCode();
            
            // Get the updated test plan to verify
            var getResp = await _client.GetAsync($"/api/testplan/{created.Id}");
            getResp.EnsureSuccessStatusCode();
            var updated = await getResp.Content.ReadFromJsonAsync<TestPlanDto>(_jsonOptions);
            Assert.NotNull(updated);
            Assert.Equal("Updated Name", updated.Name);
            Assert.Equal("Updated Desc", updated.Description);
            Assert.Equal(project.Id, updated.ProjectId);
        }

        [Fact]
        public async Task CanDeleteTestPlan()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First create a project to satisfy foreign key constraint
            var projectDto = new CreateProjectDto
            {
                Name = "TestPlan Delete Test Project",
                Code = "TDTP001",
                Description = "Project for TestPlan delete API test",
                OwnerId = 1
            };
            var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectDto, _jsonOptions);
            projectResponse.EnsureSuccessStatusCode();
            var project = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>(_jsonOptions);
            Assert.NotNull(project);

            // Create
            var createDto = new TestPlanDto
            {
                Name = "Delete Test Plan",
                Type = "UserValidation",
                Description = "To be deleted",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = project.Id
            };
            var response = await _client.PostAsJsonAsync("/api/testplan", createDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<TestPlanDto>(_jsonOptions);
            Assert.NotNull(created);

            // Delete
            var delResp = await _client.DeleteAsync($"/api/testplan/{created.Id}");
            delResp.EnsureSuccessStatusCode();

            // Should not be found
            var getResp = await _client.GetAsync($"/api/testplan/{created.Id}");
            Assert.False(getResp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task GetNonExistentTestPlanReturnsNotFound()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var resp = await _client.GetAsync("/api/testplan/9999999");
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task UpdateNonExistentTestPlanReturnsNotFound()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First create a project to satisfy foreign key constraint
            var projectDto = new CreateProjectDto
            {
                Name = "TestPlan NonExistent Test Project",
                Code = "TNTP001",
                Description = "Project for TestPlan non-existent API test",
                OwnerId = 1
            };
            var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectDto, _jsonOptions);
            projectResponse.EnsureSuccessStatusCode();
            var project = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>(_jsonOptions);
            Assert.NotNull(project);

            var updateDto = new TestPlanDto
            {
                Id = 9999999,
                Name = "Should Fail",
                Type = "UserValidation",
                Description = "No such test plan",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = project.Id
            };
            var resp = await _client.PutAsJsonAsync("/api/testplan/9999999", updateDto, _jsonOptions);
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task DeleteNonExistentTestPlanReturnsNotFound()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var resp = await _client.DeleteAsync("/api/testplan/9999999");
            Assert.False(resp.IsSuccessStatusCode);
        }
    }
}