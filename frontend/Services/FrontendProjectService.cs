using RqmtMgmtShared;
using frontend.Models;

namespace frontend.Services
{
    /// <summary>
    /// Frontend wrapper for project service with convenience methods.
    /// </summary>
    public class FrontendProjectService : IFrontendProjectService
    {
        private readonly IProjectService _projectService;

        public FrontendProjectService(IProjectService projectService)
        {
            _projectService = projectService;
        }

        // Convenience methods with Result pattern
        public async Task<ServiceResult<ProjectDto>> GetProjectAsync(int projectId)
        {
            try
            {
                var result = await _projectService.GetProjectByIdAsync(projectId);
                if (result != null)
                {
                    return ServiceResult<ProjectDto>.SuccessResult(result);
                }
                return ServiceResult<ProjectDto>.Failure("Project not found");
            }
            catch (Exception ex)
            {
                return ServiceResult<ProjectDto>.Failure($"Error loading project: {ex.Message}");
            }
        }

        public async Task<ServiceResult<PagedResult<ProjectDto>>> GetProjectsAsync()
        {
            try
            {
                var filter = new ProjectFilterDto { Page = 1, PageSize = 100 }; // Default filter
                var result = await _projectService.GetProjectsAsync(filter);
                return ServiceResult<PagedResult<ProjectDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResult<ProjectDto>>.Failure($"Error loading projects: {ex.Message}");
            }
        }

        // Delegate all other methods to the underlying service
        public Task<PagedResult<ProjectDto>> GetProjectsAsync(ProjectFilterDto filter) 
            => _projectService.GetProjectsAsync(filter);

        public Task<ProjectDto?> GetProjectByIdAsync(int projectId) 
            => _projectService.GetProjectByIdAsync(projectId);

        public Task<ProjectDto?> GetProjectByCodeAsync(string code) 
            => _projectService.GetProjectByCodeAsync(code);

        public Task<ProjectDto> CreateProjectAsync(CreateProjectDto createProjectDto) 
            => _projectService.CreateProjectAsync(createProjectDto);

        public Task<ProjectDto?> UpdateProjectAsync(int projectId, UpdateProjectDto updateProjectDto) 
            => _projectService.UpdateProjectAsync(projectId, updateProjectDto);

        public Task<bool> DeleteProjectAsync(int projectId) 
            => _projectService.DeleteProjectAsync(projectId);

        public Task<List<ProjectTeamMemberDto>> GetProjectTeamMembersAsync(int projectId) 
            => _projectService.GetProjectTeamMembersAsync(projectId);

        public Task<ProjectTeamMemberDto?> AddTeamMemberAsync(int projectId, AddProjectTeamMemberDto addTeamMemberDto) 
            => _projectService.AddTeamMemberAsync(projectId, addTeamMemberDto);

        public Task<ProjectTeamMemberDto?> UpdateTeamMemberAsync(int projectId, int userId, UpdateProjectTeamMemberDto updateTeamMemberDto) 
            => _projectService.UpdateTeamMemberAsync(projectId, userId, updateTeamMemberDto);

        public Task<bool> RemoveTeamMemberAsync(int projectId, int userId) 
            => _projectService.RemoveTeamMemberAsync(projectId, userId);

        public Task<List<ProjectDto>> GetUserProjectsAsync(int userId) 
            => _projectService.GetUserProjectsAsync(userId);

        public Task<bool> UserHasAccessToProjectAsync(int userId, int projectId) 
            => _projectService.UserHasAccessToProjectAsync(userId, projectId);

        public Task<bool> UserHasRoleInProjectAsync(int userId, int projectId, ProjectRole role) 
            => _projectService.UserHasRoleInProjectAsync(userId, projectId, role);

        public Task<string> GenerateNextRequirementIdAsync(int projectId) 
            => _projectService.GenerateNextRequirementIdAsync(projectId);
    }
}
