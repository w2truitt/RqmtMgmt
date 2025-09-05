using Microsoft.JSInterop;
using RqmtMgmtShared;
using System.Text.Json;

namespace frontend.Services
{
    /// <summary>
    /// Service implementation for managing user's recently accessed projects.
    /// Uses browser localStorage to persist recent projects across sessions.
    /// </summary>
    public class RecentProjectsService : IRecentProjectsService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IProjectService _projectService;
        private const string STORAGE_KEY = "recentProjects";
        private const int DEFAULT_MAX_RECENT = 5;

        public RecentProjectsService(IJSRuntime jsRuntime, IProjectService projectService)
        {
            _jsRuntime = jsRuntime;
            _projectService = projectService;
        }

        /// <summary>
        /// Gets the user's recently accessed projects, ordered by most recent first.
        /// </summary>
        public async Task<List<ProjectDto>> GetRecentProjectsAsync(int maxCount = DEFAULT_MAX_RECENT)
        {
            try
            {
                var recentProjectsData = await GetRecentProjectsDataAsync();
                
                // Return the most recent projects up to maxCount
                return recentProjectsData
                    .OrderByDescending(r => r.LastAccessed)
                    .Take(maxCount)
                    .Select(r => r.Project)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting recent projects: {ex.Message}");
                return new List<ProjectDto>();
            }
        }

        /// <summary>
        /// Records that a project was accessed by the user.
        /// </summary>
        public async Task TrackProjectAccessAsync(ProjectDto project)
        {
            try
            {
                var recentProjectsData = await GetRecentProjectsDataAsync();
                
                // Remove existing entry for this project if it exists
                recentProjectsData.RemoveAll(r => r.Project.Id == project.Id);
                
                // Add the project at the front with current timestamp
                recentProjectsData.Insert(0, new RecentProjectData
                {
                    Project = project,
                    LastAccessed = DateTime.UtcNow
                });

                // Keep only the most recent projects (maintain reasonable storage size)
                if (recentProjectsData.Count > 10)
                {
                    recentProjectsData = recentProjectsData.Take(10).ToList();
                }

                await SaveRecentProjectsDataAsync(recentProjectsData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error tracking project access: {ex.Message}");
            }
        }

        /// <summary>
        /// Records that a project was accessed by the user using project ID.
        /// </summary>
        public async Task TrackProjectAccessAsync(int projectId)
        {
            try
            {
                // Check if we already have this project in recent list
                var recentProjectsData = await GetRecentProjectsDataAsync();
                var existingProject = recentProjectsData.FirstOrDefault(r => r.Project.Id == projectId);
                
                if (existingProject != null)
                {
                    // Update the existing project's last accessed time
                    await TrackProjectAccessAsync(existingProject.Project);
                }
                else
                {
                    // Fetch the project details and add to recent list
                    var project = await _projectService.GetProjectByIdAsync(projectId);
                    if (project != null)
                    {
                        await TrackProjectAccessAsync(project);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error tracking project access by ID: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes all recent projects from localStorage.
        /// </summary>
        public async Task ClearRecentProjectsAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", STORAGE_KEY);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing recent projects: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes a specific project from the recent projects list.
        /// </summary>
        public async Task RemoveRecentProjectAsync(int projectId)
        {
            try
            {
                var recentProjectsData = await GetRecentProjectsDataAsync();
                recentProjectsData.RemoveAll(r => r.Project.Id == projectId);
                await SaveRecentProjectsDataAsync(recentProjectsData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing recent project: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the raw recent projects data from localStorage.
        /// </summary>
        private async Task<List<RecentProjectData>> GetRecentProjectsDataAsync()
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", STORAGE_KEY);
                
                if (string.IsNullOrEmpty(json))
                {
                    return new List<RecentProjectData>();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<List<RecentProjectData>>(json, options) ?? new List<RecentProjectData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing recent projects data: {ex.Message}");
                return new List<RecentProjectData>();
            }
        }

        /// <summary>
        /// Saves the recent projects data to localStorage.
        /// </summary>
        private async Task SaveRecentProjectsDataAsync(List<RecentProjectData> recentProjectsData)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(recentProjectsData, options);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", STORAGE_KEY, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving recent projects data: {ex.Message}");
            }
        }

        /// <summary>
        /// Internal class for storing recent project data with timestamp.
        /// </summary>
        private class RecentProjectData
        {
            public ProjectDto Project { get; set; } = new();
            public DateTime LastAccessed { get; set; }
        }
    }
}
