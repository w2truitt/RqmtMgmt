using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using RqmtMgmtShared;
using System;
using System.Text;
using System.Text.Json;
using System.IO;

namespace backend.ApiTests
{
    /// <summary>
    /// Diagnostic test to understand what's causing the 500/400 errors in integration tests
    /// </summary>
    [Collection("Integration Tests")]
    public class DiagnosticIntegrationTest : BaseIntegrationTest
    {
        [Fact]
        public async Task DiagnoseRequirementCreationError()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Try to get projects first
            try
            {
                var projectsResponse = await _client.GetAsync("/api/projects");
                var projectsContent = await projectsResponse.Content.ReadAsStringAsync();
                
                // Log the response for debugging
                if (!projectsResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Projects API failed: {projectsResponse.StatusCode} - {projectsContent}");
                }
                
                var projects = await JsonSerializer.DeserializeAsync<PagedResult<ProjectDto>>(
                    new MemoryStream(Encoding.UTF8.GetBytes(projectsContent)), _jsonOptions);
                
                int projectId = 1; // Default fallback
                if (projects?.Items?.Count > 0)
                {
                    projectId = projects.Items[0].Id;
                }

                // Try to create a simple requirement
                var requirementDto = new RequirementDto
                {
                    Title = "Diagnostic Test Requirement",
                    Type = RequirementType.CRD,
                    Status = RequirementStatus.Draft,
                    Description = "Testing requirement creation",
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow,
                    ProjectId = projectId,
                    SectionId = 1  // Use existing section ID from database
                };

                var reqResponse = await _client.PostAsJsonAsync("/api/requirement", requirementDto, _jsonOptions);
                var reqContent = await reqResponse.Content.ReadAsStringAsync();
                
                if (!reqResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Requirement creation failed: {reqResponse.StatusCode} - {reqContent}");
                }

                // If we get here, requirement creation worked
                Assert.True(true, "Requirement creation succeeded");
            }
            catch (Exception ex)
            {
                // Log the full error for debugging
                throw new Exception($"Diagnostic test failed: {ex.Message}", ex);
            }
        }

        [Fact]
        public async Task DiagnoseRoleCreationError()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            try
            {
                // Try to create a simple role
                var roleDto = new RoleDto
                {
                    Name = $"DiagnosticRole_{Guid.NewGuid():N}"
                };

                var roleResponse = await _client.PostAsJsonAsync("/api/role/dto", roleDto, _jsonOptions);
                var roleContent = await roleResponse.Content.ReadAsStringAsync();
                
                if (!roleResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Role creation failed: {roleResponse.StatusCode} - {roleContent}");
                }

                // If we get here, role creation worked
                Assert.True(true, "Role creation succeeded");
            }
            catch (Exception ex)
            {
                // Log the full error for debugging
                throw new Exception($"Role diagnostic test failed: {ex.Message}", ex);
            }
        }
    }
}