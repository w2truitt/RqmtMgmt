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
                ProjectId = await GetValidProjectIdAsync(),
                SectionId = 1  // Use existing section ID from database
            };
            var response = await _client.PostAsJsonAsync("/api/requirement", createDto, _jsonOptions);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Requirement creation failed - Status: {response.StatusCode}, Content: {errorContent}");
            }
            
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