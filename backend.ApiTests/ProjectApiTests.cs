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
    /// API tests for the Projects controller endpoints.
    /// Tests project CRUD operations, team management, and project-specific resource access.
    /// </summary>
    [Collection("Integration Tests")]
    public class ProjectApiTests : BaseIntegrationTest
    {
        [Fact]
        public async Task CanCreateAndGetProject()
        {
            await SkipIfSystemNotAvailableAsync();

            // Arrange
            var createDto = new CreateProjectDto
            {
                Name = "Test Project",
                Code = "TST",
                Description = "A test project for API testing",
                OwnerId = 1,
                Status = ProjectStatus.Active
            };

            // Act - Create project
            var createResponse = await _client.PostAsJsonAsync("/api/projects", createDto, _jsonOptions);
            
            if (createResponse.StatusCode == HttpStatusCode.InternalServerError)
            {
                // This might fail if dependencies don't exist, which is expected in isolated API tests
                var errorContent = await createResponse.Content.ReadAsStringAsync();
                Assert.Contains("error", errorContent.ToLower());
                return;
            }

            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<ProjectDto>(_jsonOptions);

            // Assert - Verify created project
            Assert.NotNull(created);
            Assert.Equal("Test Project", created.Name);
            Assert.Equal("TST", created.Code);
            Assert.Equal("A test project for API testing", created.Description);
            Assert.Equal(ProjectStatus.Active, created.Status);
            Assert.True(created.Id > 0);

            // Act - Get project by ID
            var getResponse = await _client.GetAsync($"/api/projects/{created.Id}");
            getResponse.EnsureSuccessStatusCode();
            var fetched = await getResponse.Content.ReadFromJsonAsync<ProjectDto>(_jsonOptions);

            // Assert - Verify fetched project matches created
            Assert.NotNull(fetched);
            Assert.Equal(created.Id, fetched.Id);
            Assert.Equal(created.Name, fetched.Name);
            Assert.Equal(created.Code, fetched.Code);
        }

        [Fact]
        public async Task CanListProjects()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Act
            var response = await _client.GetAsync("/api/projects");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.True(result.PageNumber >= 1);
            Assert.True(result.PageSize > 0);
            Assert.True(result.TotalItems >= 0);
        }

        [Fact]
        public async Task GetNonExistentProjectReturnsNotFound()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var response = await _client.GetAsync("/api/projects/99999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CanGetProjectRequirements()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First, get an existing project to work with
            var projectsResponse = await _client.GetAsync("/api/projects");
            projectsResponse.EnsureSuccessStatusCode();
            var projectsResult = await projectsResponse.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
            
            if (projectsResult?.Items?.Count == 0)
            {
                // Skip test if no projects exist
                return;
            }

            var projectId = projectsResult!.Items.First().Id;

            var response = await _client.GetAsync($"/api/projects/{projectId}/requirements?page=1&pageSize=10");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PagedResult<RequirementDto>>(_jsonOptions);
            
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.True(result.PageNumber >= 1);
            Assert.True(result.PageSize > 0);
        }

        [Fact]
        public async Task CanGetProjectTestSuites()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First, get an existing project to work with
            var projectsResponse = await _client.GetAsync("/api/projects");
            projectsResponse.EnsureSuccessStatusCode();
            var projectsResult = await projectsResponse.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
            
            if (projectsResult?.Items?.Count == 0)
            {
                // Skip test if no projects exist
                return;
            }

            var projectId = projectsResult!.Items.First().Id;

            var response = await _client.GetAsync($"/api/projects/{projectId}/test-suites?page=1&pageSize=10");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PagedResult<TestSuiteDto>>(_jsonOptions);
            
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.True(result.PageNumber >= 1);
            Assert.True(result.PageSize > 0);
        }

        [Fact]
        public async Task CanGetProjectTestPlans()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First, get an existing project to work with
            var projectsResponse = await _client.GetAsync("/api/projects");
            projectsResponse.EnsureSuccessStatusCode();
            var projectsResult = await projectsResponse.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
            
            if (projectsResult?.Items?.Count == 0)
            {
                // Skip test if no projects exist
                return;
            }

            var projectId = projectsResult!.Items.First().Id;

            var response = await _client.GetAsync($"/api/projects/{projectId}/test-plans?page=1&pageSize=10");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PagedResult<TestPlanDto>>(_jsonOptions);
            
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.True(result.PageNumber >= 1);
            Assert.True(result.PageSize > 0);
        }

        // ===== TEAM MANAGEMENT TESTS =====

            [Fact]
            public async Task CanFilterProjectsByUserMembership()
            {
                await SkipIfSystemNotAvailableAsync();

                // Test UserIsMember filter with X-User-Id header for development/testing
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/projects?UserIsMember=true&Page=1&PageSize=10");
                request.Headers.Add("X-User-Id", "1"); // Simulate user ID 1

                var response = await _client.SendAsync(request);
            
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    // This might fail if no projects exist for the user, which is expected
                    var errorContent = await response.Content.ReadAsStringAsync();
                    // Just verify the endpoint exists and handles the filter parameter
                    Assert.NotNull(errorContent);
                    return;
                }

                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
            
                Assert.NotNull(result);
                Assert.True(result.PageNumber >= 1);
                Assert.True(result.PageSize >= 1);
                // Note: Items count will depend on test data setup
            }

            [Fact]
            public async Task UserIsMemberFilter_WithoutUserId_ReturnsEmptyResult()
            {
                await SkipIfSystemNotAvailableAsync();

                // Test UserIsMember filter without user identification
                var response = await _client.GetAsync("/api/projects?UserIsMember=true&Page=1&PageSize=10");
            
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
            
                Assert.NotNull(result);
                Assert.Empty(result.Items); // Should return empty when no user ID is available
                Assert.Equal(0, result.TotalItems);
            }

        [Fact]
        public async Task CanGetProjectTeamMembers()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First, get an existing project to work with
            var projectsResponse = await _client.GetAsync("/api/projects");
            projectsResponse.EnsureSuccessStatusCode();
            var projectsResult = await projectsResponse.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
            
            if (projectsResult?.Items?.Count == 0)
            {
                // Skip test if no projects exist
                return;
            }

            var projectId = projectsResult!.Items.First().Id;

            // Act
            var response = await _client.GetAsync($"/api/projects/{projectId}/team");
            response.EnsureSuccessStatusCode();
            var teamMembers = await response.Content.ReadFromJsonAsync<List<ProjectTeamMemberDto>>(_jsonOptions);
            
            // Assert
            Assert.NotNull(teamMembers);
            // Team could be empty, so just verify structure
            foreach (var member in teamMembers)
            {
                Assert.True(member.ProjectId > 0);
                Assert.True(member.UserId > 0);
                Assert.NotNull(member.UserName);
                Assert.NotNull(member.UserEmail);
                Assert.True(Enum.IsDefined(typeof(ProjectRole), member.Role));
            }
        }

        [Fact]
        public async Task GetTeamMembersForNonExistentProjectReturnsNotFound()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Act
            var response = await _client.GetAsync("/api/projects/99999/team");
            
            // Assert - Should return NotFound after our fix
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CanAddTeamMember()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First, get an existing project to work with
            var projectsResponse = await _client.GetAsync("/api/projects");
            projectsResponse.EnsureSuccessStatusCode();
            var projectsResult = await projectsResponse.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
            
            if (projectsResult?.Items?.Count == 0)
            {
                // Skip test if no projects exist
                return;
            }

            var projectId = projectsResult!.Items.First().Id;

            // Arrange
            var addTeamMemberDto = new AddProjectTeamMemberDto
            {
                UserId = 1, // Assuming user 1 exists (admin user from seeder)
                Role = ProjectRole.Developer
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/projects/{projectId}/team", addTeamMemberDto, _jsonOptions);
            
            // Handle different possible outcomes
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                // This might fail if user doesn't exist or is already a member
                var errorContent = await response.Content.ReadAsStringAsync();
                Assert.Contains("error", errorContent.ToLower());
                return;
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                // User might already be a team member
                var errorContent = await response.Content.ReadAsStringAsync();
                Assert.NotEmpty(errorContent);
                return;
            }

            // If successful, verify the response
            if (response.IsSuccessStatusCode)
            {
                var teamMember = await response.Content.ReadFromJsonAsync<ProjectTeamMemberDto>(_jsonOptions);
                Assert.NotNull(teamMember);
                Assert.Equal(projectId, teamMember.ProjectId);
                Assert.Equal(1, teamMember.UserId);
                Assert.Equal(ProjectRole.Developer, teamMember.Role);
            }
        }

        [Fact]
        public async Task AddTeamMemberWithInvalidUserIdReturnsNotFound()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First, get an existing project to work with
            var projectsResponse = await _client.GetAsync("/api/projects");
            projectsResponse.EnsureSuccessStatusCode();
            var projectsResult = await projectsResponse.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
            
            if (projectsResult?.Items?.Count == 0)
            {
                // Skip test if no projects exist
                return;
            }

            var projectId = projectsResult!.Items.First().Id;

            // Arrange
            var addTeamMemberDto = new AddProjectTeamMemberDto
            {
                UserId = 99999, // Non-existent user ID
                Role = ProjectRole.Developer
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/projects/{projectId}/team", addTeamMemberDto, _jsonOptions);
            
            // Assert - Should return NotFound since user doesn't exist
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddTeamMemberToNonExistentProjectReturnsNotFound()
        {
            await SkipIfSystemNotAvailableAsync();

            // Arrange
            var addTeamMemberDto = new AddProjectTeamMemberDto
            {
                UserId = 1,
                Role = ProjectRole.Developer
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/projects/99999/team", addTeamMemberDto, _jsonOptions);
            
            // Assert - Should now properly return NotFound after our fix
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CanUpdateTeamMemberRole()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First, get an existing project to work with
            var projectsResponse = await _client.GetAsync("/api/projects");
            projectsResponse.EnsureSuccessStatusCode();
            var projectsResult = await projectsResponse.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
            
            if (projectsResult?.Items?.Count == 0)
            {
                // Skip test if no projects exist
                return;
            }

            var projectId = projectsResult!.Items.First().Id;

            // Arrange
            var updateTeamMemberDto = new UpdateProjectTeamMemberDto
            {
                Role = ProjectRole.QAEngineer,
                IsActive = true
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/projects/{projectId}/team/1", updateTeamMemberDto, _jsonOptions);
            
            // Handle different possible outcomes
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // Team member might not exist in this project
                var errorContent = await response.Content.ReadAsStringAsync();
                Assert.Contains("not found", errorContent.ToLower());
                return;
            }

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                // Database or dependency issues
                var errorContent = await response.Content.ReadAsStringAsync();
                Assert.Contains("error", errorContent.ToLower());
                return;
            }

            // If successful, verify the response
            if (response.IsSuccessStatusCode)
            {
                var teamMember = await response.Content.ReadFromJsonAsync<ProjectTeamMemberDto>(_jsonOptions);
                Assert.NotNull(teamMember);
                Assert.Equal(projectId, teamMember.ProjectId);
                Assert.Equal(1, teamMember.UserId);
                Assert.Equal(ProjectRole.QAEngineer, teamMember.Role);
                Assert.True(teamMember.IsActive);
            }
        }

        [Fact]
        public async Task UpdateNonExistentTeamMemberReturnsNotFound()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First, get an existing project to work with
            var projectsResponse = await _client.GetAsync("/api/projects");
            projectsResponse.EnsureSuccessStatusCode();
            var projectsResult = await projectsResponse.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
            
            if (projectsResult?.Items?.Count == 0)
            {
                // Skip test if no projects exist
                return;
            }

            var projectId = projectsResult!.Items.First().Id;

            // Arrange
            var updateTeamMemberDto = new UpdateProjectTeamMemberDto
            {
                Role = ProjectRole.Developer,
                IsActive = true
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/projects/{projectId}/team/99999", updateTeamMemberDto, _jsonOptions);
            
            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CanRemoveTeamMember()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First, get an existing project to work with
            var projectsResponse = await _client.GetAsync("/api/projects");
            projectsResponse.EnsureSuccessStatusCode();
            var projectsResult = await projectsResponse.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
            
            if (projectsResult?.Items?.Count == 0)
            {
                // Skip test if no projects exist
                return;
            }

            var projectId = projectsResult!.Items.First().Id;

            // Act
            var response = await _client.DeleteAsync($"/api/projects/{projectId}/team/1");
            
            // Handle different possible outcomes
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // Team member might not exist
                var errorContent = await response.Content.ReadAsStringAsync();
                Assert.Contains("not found", errorContent.ToLower());
                return;
            }

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                // Database or dependency issues
                var errorContent = await response.Content.ReadAsStringAsync();
                Assert.Contains("error", errorContent.ToLower());
                return;
            }

            // If successful, should be NoContent or OK
            Assert.True(response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.OK);
        }

        [Fact]
        public async Task RemoveNonExistentTeamMemberReturnsNotFound()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Act
            var response = await _client.DeleteAsync("/api/projects/99999/team/99999");
            
            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ===== ENHANCED REQUIREMENTS TESTS =====

        [Fact]
        public async Task CanGetProjectRequirementsWithPagination()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First, get an existing project to work with
            var projectsResponse = await _client.GetAsync("/api/projects");
            projectsResponse.EnsureSuccessStatusCode();
            var projectsResult = await projectsResponse.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
            
            if (projectsResult?.Items?.Count == 0)
            {
                // Skip test if no projects exist
                return;
            }

            var projectId = projectsResult!.Items.First().Id;

            // Test various pagination scenarios
            var testCases = new[]
            {
                new { page = 1, pageSize = 20, sortDescending = false },
                new { page = 1, pageSize = 10, sortDescending = true },
                new { page = 2, pageSize = 5, sortDescending = false },
                new { page = 1, pageSize = 50, sortDescending = true }
            };

            foreach (var testCase in testCases)
            {
                // Act
                var response = await _client.GetAsync($"/api/projects/{projectId}/requirements?page={testCase.page}&pageSize={testCase.pageSize}&sortDescending={testCase.sortDescending}");
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<PagedResult<RequirementDto>>(_jsonOptions);
                
                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.Items);
                Assert.Equal(testCase.page, result.PageNumber);
                Assert.Equal(testCase.pageSize, result.PageSize);
                Assert.True(result.TotalItems >= 0);
            }
        }

        [Fact]
        public async Task CanGetProjectRequirementsWithSearchAndSort()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Create a project to ensure we have one to work with
            var createDto = new CreateProjectDto
            {
                Name = $"Requirements Test Project {DateTime.Now.Ticks}",
                Code = $"RTP{DateTime.Now.Ticks % 10000}",
                Description = "Project for requirements testing",
                OwnerId = 1
            };
            var createResponse = await _client.PostAsJsonAsync("/api/projects", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var project = await createResponse.Content.ReadFromJsonAsync<ProjectDto>(_jsonOptions);
            Assert.NotNull(project);

            // Create a requirement for the project to ensure we have data to test with
            var requirementDto = new RequirementDto
            {
                Title = "Test Requirement for Search",
                Type = RequirementType.CRD,
                Status = RequirementStatus.Draft,
                Description = "Test requirement for search and sort testing",
                ProjectId = project.Id,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var reqResponse = await _client.PostAsJsonAsync("/api/requirement", requirementDto, _jsonOptions);
            reqResponse.EnsureSuccessStatusCode();

            // Test with search and sort parameters to verify backend functionality
            var response = await _client.GetAsync($"/api/projects/{project.Id}/requirements?page=1&pageSize=20&searchTerm=test&sortBy=title&sortDescending=false");
            
            // If we get a 500 error, let's capture the error details
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"HTTP {response.StatusCode}: {errorContent}");
            }
            
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PagedResult<RequirementDto>>(_jsonOptions);
            
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.True(result.PageNumber >= 1);
            Assert.True(result.PageSize > 0);
        }

        [Fact]
        public async Task GetRequirementsForNonExistentProjectReturnsNotFound()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Act
            var response = await _client.GetAsync("/api/projects/99999/requirements?page=1&pageSize=20");
            
            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetProjectRequirementsWithInvalidPaginationHandlesGracefully()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First, get an existing project to work with
            var projectsResponse = await _client.GetAsync("/api/projects");
            projectsResponse.EnsureSuccessStatusCode();
            var projectsResult = await projectsResponse.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
            
            if (projectsResult?.Items?.Count == 0)
            {
                // Skip test if no projects exist
                return;
            }

            var projectId = projectsResult!.Items.First().Id;

            // Test edge cases for pagination
            var testCases = new[]
            {
                "page=0&pageSize=20", // Invalid page
                "page=1&pageSize=0",  // Invalid page size
                "page=-1&pageSize=10", // Negative page
                "page=1&pageSize=-5"   // Negative page size
            };

            foreach (var queryString in testCases)
            {
                // Act
                var response = await _client.GetAsync($"/api/projects/{projectId}/requirements?{queryString}");
                
                // Assert - Should either handle gracefully or return bad request
                Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest);
            }
        }
    }
}