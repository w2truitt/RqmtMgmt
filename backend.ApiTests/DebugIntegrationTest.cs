using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using RqmtMgmtShared;
using System;
using System.Net;

namespace backend.ApiTests
{
    /// <summary>
    /// Temporary debug test to capture actual HTTP error responses
    /// </summary>
    [Collection("Integration Tests")]
    public class DebugIntegrationTest : BaseIntegrationTest
    {
        [Fact]
        public async Task Debug_RequirementCreation_CaptureError()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            try
            {
                // Try to get a project first
                var projectResponse = await _client.GetAsync("/api/projects");
                var projectContent = await projectResponse.Content.ReadAsStringAsync();
                
                if (!projectResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Project API failed: {projectResponse.StatusCode} - {projectContent}");
                }

                var projects = await projectResponse.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
                
                int projectId = 1; // Default fallback
                if (projects?.Items?.Count > 0)
                {
                    projectId = projects.Items[0].Id;
                }

                // Try to create a requirement
                var requirementDto = new RequirementDto
                {
                    Title = "Debug Test Requirement",
                    Type = RequirementType.CRS,
                    Status = RequirementStatus.Draft,
                    Description = "Debug requirement for error analysis",
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow,
                    ProjectId = projectId
                };

                var reqResponse = await _client.PostAsJsonAsync("/api/requirement", requirementDto, _jsonOptions);
                var reqContent = await reqResponse.Content.ReadAsStringAsync();
                
                if (!reqResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Requirement creation failed: {reqResponse.StatusCode} - {reqContent}");
                }

                // If we get here, it worked
                Assert.True(true, "Requirement creation succeeded");
            }
            catch (Exception ex)
            {
                // This will show us the actual error
                throw new Exception($"Debug test failed with details: {ex.Message}", ex);
            }
        }
    }
}