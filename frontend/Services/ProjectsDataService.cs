using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using RqmtMgmtShared;

namespace frontend.Services
{
    /// <summary>
    /// Frontend data service for project management operations.
    /// Provides HTTP client-based implementation of IProjectService for communicating with the backend API.
    /// Now inherits from BaseDataService to eliminate code duplication and ensure consistency.
    /// </summary>
    public class ProjectsDataService : BaseDataService, IProjectService
    {
        /// <summary>
        /// Initializes a new instance of the ProjectsDataService with the specified HTTP client.
        /// </summary>
        /// <param name="http">The HTTP client for making API requests.</param>
        public ProjectsDataService(HttpClient http) : base(http)
        {
        }

        /// <summary>
        /// Retrieves projects with filtering and pagination from the backend API.
        /// </summary>
        /// <param name="filter">Filter criteria for projects.</param>
        /// <returns>A paged result of projects with their metadata.</returns>
        public async Task<PagedResult<ProjectDto>> GetProjectsAsync(ProjectFilterDto filter)
        {
            var queryParams = new Dictionary<string, object?>
            {
                ["Page"] = filter.Page,
                ["PageSize"] = filter.PageSize,
                ["SearchTerm"] = filter.SearchTerm,
                ["Status"] = filter.Status?.ToString(),
                ["OwnerId"] = filter.OwnerId,
                ["UserIsMember"] = filter.UserIsMember
            };

            var queryString = BuildQueryString(queryParams);
            var result = await GetAsync<PagedResult<ProjectDto>>($"/api/Projects{queryString}");
            return result ?? new PagedResult<ProjectDto>();
        }

        /// <summary>
        /// Retrieves a specific project by its ID from the backend API.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <returns>The project if found; otherwise, null.</returns>
        public async Task<ProjectDto?> GetProjectByIdAsync(int projectId)
            => await GetAsync<ProjectDto>($"/api/Projects/{projectId}");

        /// <summary>
        /// Retrieves a specific project by its code from the backend API.
        /// </summary>
        /// <param name="code">The unique code of the project.</param>
        /// <returns>The project if found; otherwise, null.</returns>
        public async Task<ProjectDto?> GetProjectByCodeAsync(string code)
            => await GetAsync<ProjectDto>($"/api/Projects/by-code/{code}");

        /// <summary>
        /// Creates a new project by sending a POST request to the backend API.
        /// </summary>
        /// <param name="createProjectDto">The project data to create.</param>
        /// <returns>The created project with its assigned ID.</returns>
        public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto createProjectDto)
        {
            var result = await PostAsync<CreateProjectDto, ProjectDto>("/api/Projects", createProjectDto);
            return result ?? throw new InvalidOperationException("Failed to create project");
        }

        /// <summary>
        /// Updates an existing project by sending a PUT request to the backend API.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project to update.</param>
        /// <param name="updateProjectDto">The project data to update.</param>
        /// <returns>The updated project if successful; otherwise, null.</returns>
        public async Task<ProjectDto?> UpdateProjectAsync(int projectId, UpdateProjectDto updateProjectDto)
            => await PutAsync<UpdateProjectDto, ProjectDto>($"/api/Projects/{projectId}", updateProjectDto);

        /// <summary>
        /// Deletes a project by sending a DELETE request to the backend API.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteProjectAsync(int projectId)
            => await DeleteAsync($"/api/Projects/{projectId}");

        /// <summary>
        /// Retrieves all team members for a specific project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <returns>A list of team members for the project.</returns>
        public async Task<List<ProjectTeamMemberDto>> GetProjectTeamMembersAsync(int projectId)
            => await GetListAsync<ProjectTeamMemberDto>($"/api/Projects/{projectId}/team");

        /// <summary>
        /// Adds a team member to a project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <param name="addTeamMemberDto">The team member data to add.</param>
        /// <returns>The added team member if successful; otherwise, null.</returns>
        public async Task<ProjectTeamMemberDto?> AddTeamMemberAsync(int projectId, AddProjectTeamMemberDto addTeamMemberDto)
            => await PostAsync<AddProjectTeamMemberDto, ProjectTeamMemberDto>($"/api/Projects/{projectId}/team", addTeamMemberDto);

        /// <summary>
        /// Updates a team member's role in a project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="updateTeamMemberDto">The team member data to update.</param>
        /// <returns>The updated team member if successful; otherwise, null.</returns>
        public async Task<ProjectTeamMemberDto?> UpdateTeamMemberAsync(int projectId, int userId, UpdateProjectTeamMemberDto updateTeamMemberDto)
            => await PutAsync<UpdateProjectTeamMemberDto, ProjectTeamMemberDto>($"/api/Projects/{projectId}/team/{userId}", updateTeamMemberDto);

        /// <summary>
        /// Removes a team member from a project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <param name="userId">The unique identifier of the user to remove.</param>
        /// <returns>True if the removal was successful; otherwise, false.</returns>
        public async Task<bool> RemoveTeamMemberAsync(int projectId, int userId)
            => await DeleteAsync($"/api/Projects/{projectId}/team/{userId}");

        /// <summary>
        /// Retrieves all projects for a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A list of projects the user has access to.</returns>
        public async Task<List<ProjectDto>> GetUserProjectsAsync(int userId)
            => await GetListAsync<ProjectDto>($"/api/Projects/user/{userId}");

        /// <summary>
        /// Checks if a user has access to a specific project.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <returns>True if the user has access; otherwise, false.</returns>
        public async Task<bool> UserHasAccessToProjectAsync(int userId, int projectId)
        {
            try
            {
                var response = await _http.GetAsync($"/api/Projects/{projectId}/access/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
                    return result;
                }
                return false;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a user has a specific role in a project.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <param name="role">The role to check for.</param>
        /// <returns>True if the user has the specified role; otherwise, false.</returns>
        public async Task<bool> UserHasRoleInProjectAsync(int userId, int projectId, ProjectRole role)
        {
            try
            {
                var response = await _http.GetAsync($"/api/Projects/{projectId}/role/{userId}/{role}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
                    return result;
                }
                return false;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        /// <summary>
        /// Generates the next requirement ID for a project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <returns>The next requirement ID in the format PROJECT-REQ-XXX.</returns>
        public async Task<string> GenerateNextRequirementIdAsync(int projectId)
        {
            try
            {
                var response = await _http.GetAsync($"/api/Projects/{projectId}/next-requirement-id");
                return await response.Content.ReadAsStringAsync() ?? "";
            }
            catch (HttpRequestException)
            {
                return "";
            }
        }
    }
}