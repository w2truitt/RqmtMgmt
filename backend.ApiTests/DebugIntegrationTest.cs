using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using RqmtMgmtShared;
using System;

namespace backend.ApiTests
{
    /// <summary>
    /// Debug test to investigate integration test failures
    /// </summary>
    [Collection("Integration Tests")]
    public class DebugIntegrationTest : BaseIntegrationTest
    {
        [Fact]
        public async Task Debug_RequirementCreation()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // 1. Try to create a simple requirement
            var requirementDto = new RequirementDto
            {
                Title = "Debug Test Requirement",
                Type = RequirementType.CRD,
                Status = RequirementStatus.Draft,
                Description = "A requirement for debugging",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = await GetValidProjectIdAsync(),
                SectionId = 1  // Use existing section ID from database
            };

            var reqResponse = await _client.PostAsJsonAsync("/api/requirement", requirementDto, _jsonOptions);
            
            if (!reqResponse.IsSuccessStatusCode)
            {
                var errorContent = await reqResponse.Content.ReadAsStringAsync();
                throw new Exception($"Requirement creation failed - Status: {reqResponse.StatusCode}, Content: {errorContent}");
            }
            
            var createdRequirement = await reqResponse.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
            Assert.NotNull(createdRequirement);
        }

        /// <summary>
        /// Helper method to get a valid project ID for testing.
        /// </summary>
        private async Task<int> GetValidProjectIdAsync()
        {
            var response = await _client.GetAsync("/api/projects");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Projects fetch failed - Status: {response.StatusCode}, Content: {errorContent}");
            }
            
            var projects = await response.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
            
            if (projects?.Items?.Count == 0)
            {
                // Create a test project if none exist
                var createDto = new CreateProjectDto
                {
                    Name = $"Debug Test Project {Guid.NewGuid():N}",
                    Code = $"DBG{DateTime.UtcNow:mmss}",
                    Description = "Auto-created for debugging",
                    OwnerId = 1,
                    Status = ProjectStatus.Planning
                };

                var createResponse = await _client.PostAsJsonAsync("/api/projects", createDto, _jsonOptions);
                
                if (!createResponse.IsSuccessStatusCode)
                {
                    var errorContent = await createResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Project creation failed - Status: {createResponse.StatusCode}, Content: {errorContent}");
                }
                
                var created = await createResponse.Content.ReadFromJsonAsync<ProjectDto>(_jsonOptions);
                return created!.Id;
            }

            return projects!.Items![0].Id;
        }
    }
}