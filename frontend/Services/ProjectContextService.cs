using Microsoft.JSInterop;
using RqmtMgmtShared;

namespace frontend.Services
{
    public class ProjectContextService : IProjectContextService
    {
        private readonly IProjectService _projectService;
        private readonly IJSRuntime _jsRuntime;
        private ProjectDto? _currentProject;

        public ProjectDto? CurrentProject => _currentProject;
        public bool IsInProjectContext => _currentProject != null;

        public event Action<ProjectDto?>? ProjectChanged;

        public ProjectContextService(IProjectService projectService, IJSRuntime jsRuntime)
        {
            _projectService = projectService;
            _jsRuntime = jsRuntime;
        }

        public async Task SetCurrentProjectAsync(ProjectDto? project)
        {
            var previousProject = _currentProject;
            _currentProject = project;

            // Persist to local storage
            if (project != null)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "currentProjectId", project.Id.ToString());
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "currentProjectId");
            }

            // Notify subscribers only if project actually changed
            if (previousProject?.Id != project?.Id)
            {
                ProjectChanged?.Invoke(_currentProject);
            }
        }

        public async Task SetCurrentProjectAsync(int projectId)
        {
            try
            {
                var result = await _projectService.GetProjectByIdAsync(projectId);
                if (result != null)
                {
                    await SetCurrentProjectAsync(result);
                }
                else
                {
                    Console.WriteLine($"Failed to load project {projectId}: Project not found");
                    await ClearProjectContextAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting current project {projectId}: {ex.Message}");
                await ClearProjectContextAsync();
            }
        }

        public async Task ClearProjectContextAsync()
        {
            await SetCurrentProjectAsync(null);
        }

        public async Task<bool> LoadProjectContextFromRouteAsync(string relativePath)
        {
            try
            {
                // Parse project ID from route like "/projects/1/requirements"
                var segments = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length >= 2 && segments[0] == "projects" && int.TryParse(segments[1], out int projectId))
                {
                    // Only load if we don't already have this project
                    if (_currentProject?.Id != projectId)
                    {
                        await SetCurrentProjectAsync(projectId);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading project context from route: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LoadPersistedProjectContextAsync()
        {
            try
            {
                var storedProjectId = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "currentProjectId");
                if (!string.IsNullOrEmpty(storedProjectId) && int.TryParse(storedProjectId, out int projectId))
                {
                    // Only load if we don't already have this project
                    if (_currentProject?.Id != projectId)
                    {
                        await SetCurrentProjectAsync(projectId);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading persisted project context: {ex.Message}");
                return false;
            }
        }
    }
}
