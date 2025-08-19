using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using RqmtMgmtShared;

namespace frontend.Services
{
    /// <summary>
    /// Frontend data service for project management operations.
    /// Provides HTTP client-based implementation of IProjectService for communicating with the backend API.
    /// </summary>
    public class ProjectsDataService : IProjectService
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the ProjectsDataService with the specified HTTP client.
        /// </summary>
        /// <param name="http">The HTTP client for making API requests.</param>
        public ProjectsDataService(HttpClient http)
        {
            _http = http;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        /// <summary>
        /// Retrieves projects with filtering and pagination from the backend API.
        /// </summary>
        /// <param name="filter">Filter criteria for projects.</param>
        /// <returns>A paged result of projects with their metadata.</returns>
        public async Task<PagedResult<ProjectDto>> GetProjectsAsync(ProjectFilterDto filter)
        {
            var queryString = $"?Page={filter.Page}&PageSize={filter.PageSize}";
            if (!string.IsNullOrEmpty(filter.SearchTerm))
                queryString += $"&SearchTerm={Uri.EscapeDataString(filter.SearchTerm)}";
            if (filter.Status.HasValue)
                queryString += $"&Status={filter.Status.Value}";
            if (filter.OwnerId.HasValue)
                queryString += $"&OwnerId={filter.OwnerId.Value}";
            if (filter.UserIsMember.HasValue)
                queryString += $"&UserIsMember={filter.UserIsMember.Value}";

            return await _http.GetFromJsonAsync<PagedResult<ProjectDto>>($"/api/Projects{queryString}", _jsonOptions) ?? new PagedResult<ProjectDto>();
        }

        /// <summary>
        /// Retrieves a specific project by its ID from the backend API.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <returns>The project if found; otherwise, null.</returns>
        public async Task<ProjectDto?> GetProjectByIdAsync(int projectId)
            => await _http.GetFromJsonAsync<ProjectDto>($"/api/Projects/{projectId}", _jsonOptions);

        /// <summary>
        /// Retrieves a specific project by its code from the backend API.
        /// </summary>
        /// <param name="code">The unique code of the project.</param>
        /// <returns>The project if found; otherwise, null.</returns>
        public async Task<ProjectDto?> GetProjectByCodeAsync(string code)
            => await _http.GetFromJsonAsync<ProjectDto>($"/api/Projects/by-code/{code}", _jsonOptions);

        /// <summary>
        /// Creates a new project by sending a POST request to the backend API.
        /// </summary>
        /// <param name="createProjectDto">The project data to create.</param>
        /// <returns>The created project with its assigned ID.</returns>
        public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto createProjectDto)
        {
            var response = await _http.PostAsJsonAsync("/api/Projects", createProjectDto, _jsonOptions);
            return await response.Content.ReadFromJsonAsync<ProjectDto>(_jsonOptions) ?? throw new InvalidOperationException("Failed to create project");
        }

        /// <summary>
        /// Updates an existing project by sending a PUT request to the backend API.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project to update.</param>
        /// <param name="updateProjectDto">The project data to update.</param>
        /// <returns>The updated project if successful; otherwise, null.</returns>
        public async Task<ProjectDto?> UpdateProjectAsync(int projectId, UpdateProjectDto updateProjectDto)
        {
            var response = await _http.PutAsJsonAsync($"/api/Projects/{projectId}", updateProjectDto, _jsonOptions);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ProjectDto>(_jsonOptions) : null;
        }

        /// <summary>
        /// Deletes a project by sending a DELETE request to the backend API.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteProjectAsync(int projectId)
        {
            var response = await _http.DeleteAsync($"/api/Projects/{projectId}");
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Retrieves all team members for a specific project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <returns>A list of team members for the project.</returns>
        public async Task<List<ProjectTeamMemberDto>> GetProjectTeamMembersAsync(int projectId)
            => await _http.GetFromJsonAsync<List<ProjectTeamMemberDto>>($"/api/Projects/{projectId}/team", _jsonOptions) ?? new();

        /// <summary>
        /// Adds a team member to a project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <param name="addTeamMemberDto">The team member data to add.</param>
        /// <returns>The added team member if successful; otherwise, null.</returns>
        public async Task<ProjectTeamMemberDto?> AddTeamMemberAsync(int projectId, AddProjectTeamMemberDto addTeamMemberDto)
        {
            var response = await _http.PostAsJsonAsync($"/api/Projects/{projectId}/team", addTeamMemberDto, _jsonOptions);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ProjectTeamMemberDto>(_jsonOptions) : null;
        }

        /// <summary>
        /// Updates a team member's role in a project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="updateTeamMemberDto">The team member data to update.</param>
        /// <returns>The updated team member if successful; otherwise, null.</returns>
        public async Task<ProjectTeamMemberDto?> UpdateTeamMemberAsync(int projectId, int userId, UpdateProjectTeamMemberDto updateTeamMemberDto)
        {
            var response = await _http.PutAsJsonAsync($"/api/Projects/{projectId}/team/{userId}", updateTeamMemberDto, _jsonOptions);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ProjectTeamMemberDto>(_jsonOptions) : null;
        }

        /// <summary>
        /// Removes a team member from a project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <param name="userId">The unique identifier of the user to remove.</param>
        /// <returns>True if the removal was successful; otherwise, false.</returns>
        public async Task<bool> RemoveTeamMemberAsync(int projectId, int userId)
        {
            var response = await _http.DeleteAsync($"/api/Projects/{projectId}/team/{userId}");
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Retrieves all projects for a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A list of projects the user has access to.</returns>
        public async Task<List<ProjectDto>> GetUserProjectsAsync(int userId)
            => await _http.GetFromJsonAsync<List<ProjectDto>>($"/api/Projects/user/{userId}", _jsonOptions) ?? new();

        /// <summary>
        /// Checks if a user has access to a specific project.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <returns>True if the user has access; otherwise, false.</returns>
        public async Task<bool> UserHasAccessToProjectAsync(int userId, int projectId)
        {
            var response = await _http.GetAsync($"/api/Projects/{projectId}/access/{userId}");
            return response.IsSuccessStatusCode && await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
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
            var response = await _http.GetAsync($"/api/Projects/{projectId}/role/{userId}/{role}");
            return response.IsSuccessStatusCode && await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
        }

        /// <summary>
        /// Generates the next requirement ID for a project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <returns>The next requirement ID in the format PROJECT-REQ-XXX.</returns>
        public async Task<string> GenerateNextRequirementIdAsync(int projectId)
        {
            var response = await _http.GetAsync($"/api/Projects/{projectId}/next-requirement-id");
            return await response.Content.ReadAsStringAsync() ?? "";
        }
    }
}