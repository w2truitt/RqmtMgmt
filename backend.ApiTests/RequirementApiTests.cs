using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using RqmtMgmtShared;
using System;
using System.Collections.Generic;

namespace backend.ApiTests
{
    /// <summary>
    /// API tests for the Requirements controller endpoints.
    /// These tests run against the actual docker-compose.identity.yml instance with JWT authentication.
    /// </summary>
    [Collection("Integration Tests")]
    public class RequirementApiTests : BaseIntegrationTest
    {
        [Fact]
        public async Task CanCreateAndGetRequirement()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var createDto = new RequirementDto
            {
                Title = "API Requirement",
                Type = RequirementType.CRD,
                Status = RequirementStatus.Draft,
                Description = "Created by API test",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync()
            };
            var response = await _client.PostAsJsonAsync("/api/requirement", createDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            Assert.NotNull(created);
            Assert.Equal("API Requirement", created.Title);

            var getResp = await _client.GetAsync($"/api/requirement/{created.Id}");
            getResp.EnsureSuccessStatusCode();
            var fetched = await getResp.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            Assert.NotNull(fetched);
            Assert.Equal("API Requirement", fetched.Title);
        }

        [Fact]
        public async Task CanListRequirements()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var response = await _client.GetAsync("/api/requirement");
            response.EnsureSuccessStatusCode();
            var list = await response.Content.ReadFromJsonAsync<List<RequirementDto>>(_jsonOptions);
            Assert.NotNull(list);
            Assert.True(list.Count > 0);
        }

        [Fact]
        public async Task CanUpdateRequirement()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First create
            var createDto = new RequirementDto
            {
                Title = "Update Requirement",
                Type = RequirementType.PRD,
                Status = RequirementStatus.Draft,
                Description = "To be updated",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync()
            };
            var response = await _client.PostAsJsonAsync("/api/requirement", createDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            Assert.NotNull(created);

            // Now update
            created.Title = "Updated Title";
            created.Status = RequirementStatus.Approved;
            var putResp = await _client.PutAsJsonAsync($"/api/requirement/{created.Id}", created, _jsonOptions);
            putResp.EnsureSuccessStatusCode();
            
            // Get the updated requirement to verify
            var getResp = await _client.GetAsync($"/api/requirement/{created.Id}");
            getResp.EnsureSuccessStatusCode();
            var updated = await getResp.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            Assert.NotNull(updated);
            Assert.Equal("Updated Title", updated.Title);
            Assert.Equal(RequirementStatus.Approved, updated.Status);
        }

        [Fact]
        public async Task CanDeleteRequirement()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Create
            var createDto = new RequirementDto
            {
                Title = "Delete Requirement",
                Type = RequirementType.SRS,
                Status = RequirementStatus.Draft,
                Description = "To be deleted",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync()
            };
            var response = await _client.PostAsJsonAsync("/api/requirement", createDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            Assert.NotNull(created);

            // Delete
            var delResp = await _client.DeleteAsync($"/api/requirement/{created.Id}");
            delResp.EnsureSuccessStatusCode();

            // Should not be found
            var getResp = await _client.GetAsync($"/api/requirement/{created.Id}");
            Assert.False(getResp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task GetNonExistentRequirementReturnsNotFound()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var resp = await _client.GetAsync("/api/requirement/9999999");
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task UpdateNonExistentRequirementReturnsNotFound()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var updateDto = new RequirementDto
            {
                Id = 9999999,
                Title = "Should Fail",
                Type = RequirementType.CRD,
                Status = RequirementStatus.Draft,
                Description = "No such requirement",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync()
            };
            var resp = await _client.PutAsJsonAsync("/api/requirement/9999999", updateDto, _jsonOptions);
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task DeleteNonExistentRequirementReturnsNotFound()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var resp = await _client.DeleteAsync("/api/requirement/9999999");
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task CreatingRequirementAddsInitialVersion()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var createDto = new RequirementDto
            {
                Title = "Versioned Requirement",
                Type = RequirementType.CRD,
                Status = RequirementStatus.Draft,
                Description = "Initial version test",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync()
            };
            var response = await _client.PostAsJsonAsync("/api/requirement", createDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            Assert.NotNull(created);

            var versionsResp = await _client.GetAsync($"/api/Redline/requirement/{created.Id}/versions");
            versionsResp.EnsureSuccessStatusCode();
            var versions = await versionsResp.Content.ReadFromJsonAsync<List<RequirementVersionDto>>(_jsonOptions);
            Assert.NotNull(versions);
            Assert.Single(versions);
            Assert.Equal(created.Title, versions[0].Title);
        }

        [Fact]
        public async Task UpdatingRequirementAddsNewVersion()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var createDto = new RequirementDto
            {
                Title = "Versioned Requirement Update",
                Type = RequirementType.CRD,
                Status = RequirementStatus.Draft,
                Description = "Initial version",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync()
            };
            var response = await _client.PostAsJsonAsync("/api/requirement", createDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            Assert.NotNull(created);

            // First check initial version
            var versionsResp1 = await _client.GetAsync($"/api/Redline/requirement/{created.Id}/versions");
            versionsResp1.EnsureSuccessStatusCode();
            var versions1 = await versionsResp1.Content.ReadFromJsonAsync<List<RequirementVersionDto>>(_jsonOptions);
            Assert.NotNull(versions1);
            Assert.Single(versions1);

            // Now update
            created.Title = "Updated Title";
            created.Description = "Updated Desc";
            var putResp = await _client.PutAsJsonAsync($"/api/requirement/{created.Id}", created, _jsonOptions);
            putResp.EnsureSuccessStatusCode();

            // Should now have 2 versions
            var versionsResp2 = await _client.GetAsync($"/api/Redline/requirement/{created.Id}/versions");
            versionsResp2.EnsureSuccessStatusCode();
            var versions2 = await versionsResp2.Content.ReadFromJsonAsync<List<RequirementVersionDto>>(_jsonOptions);
            Assert.NotNull(versions2);
            Assert.Equal(2, versions2.Count);
            Assert.Equal("Versioned Requirement Update", versions2[0].Title); // original
            Assert.Equal("Versioned Requirement Update", versions2[1].Title); // previous state before update
        }

        [Fact]
        public async Task MultipleUpdatesAddAllIntermediateVersions()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var createDto = new RequirementDto
            {
                Title = "Initial Title",
                Type = RequirementType.CRD,
                Status = RequirementStatus.Draft,
                Description = "Initial description",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync()
            };
            var response = await _client.PostAsJsonAsync("/api/requirement", createDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            Assert.NotNull(created);

            // First update
            created.Title = "Intermediate Title";
            var putResp1 = await _client.PutAsJsonAsync($"/api/requirement/{created.Id}", created, _jsonOptions);
            putResp1.EnsureSuccessStatusCode();

            // Second update
            created.Title = "Final Title";
            var putResp2 = await _client.PutAsJsonAsync($"/api/requirement/{created.Id}", created, _jsonOptions);
            putResp2.EnsureSuccessStatusCode();

            // Version history should have 3 entries: initial, after first update, after second update
            var versionsResp = await _client.GetAsync($"/api/Redline/requirement/{created.Id}/versions");
            versionsResp.EnsureSuccessStatusCode();
            var versions = await versionsResp.Content.ReadFromJsonAsync<List<RequirementVersionDto>>(_jsonOptions);
            Assert.NotNull(versions);
            Assert.Equal(3, versions.Count);
            Assert.Equal("Initial Title", versions[0].Title);
            Assert.Equal("Initial Title", versions[1].Title);
            Assert.Equal("Intermediate Title", versions[2].Title);
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