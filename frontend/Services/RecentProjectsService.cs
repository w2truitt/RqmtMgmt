using Microsoft.JSInterop;
using RqmtMgmtShared;
using System.Text.Json;

namespace frontend.Services
{
    /// <summary>
    /// Service implementation for managing user's recently accessed projects.
    /// Uses browser localStorage to persist recent projects across sessions with in-memory caching for performance.
    /// </summary>
    public class RecentProjectsService : IRecentProjectsService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IProjectService _projectService;
        private const string STORAGE_KEY = "recentProjects";
        private const int DEFAULT_MAX_RECENT = 5;
        private List<RecentProjectData>? _cachedRecentProjects;
        private bool _isInitialized = false;

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
                await EnsureInitializedAsync();
                var recentProjectsData = GetCachedRecentProjectsData();
                
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
                await EnsureInitializedAsync();
                var recentProjectsData = GetCachedRecentProjectsData();
                
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
                await EnsureInitializedAsync();
                // Check if we already have this project in recent list
                var recentProjectsData = GetCachedRecentProjectsData();
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
                _cachedRecentProjects = new List<RecentProjectData>();
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
                await EnsureInitializedAsync();
                var recentProjectsData = GetCachedRecentProjectsData();
                recentProjectsData.RemoveAll(r => r.Project.Id == projectId);
                await SaveRecentProjectsDataAsync(recentProjectsData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing recent project: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the count of recently accessed projects without loading full project data.
        /// </summary>
        public async Task<int> GetRecentProjectsCountAsync()
        {
            try
            {
                await EnsureInitializedAsync();
                return GetCachedRecentProjectsData().Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting recent projects count: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Ensures the service is initialized and cache is loaded from localStorage.
        /// Only performs localStorage read once per service lifetime.
        /// </summary>
        private async Task EnsureInitializedAsync()
        {
            if (_isInitialized)
                return;

            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", STORAGE_KEY);
                
                if (string.IsNullOrEmpty(json))
                {
                    _cachedRecentProjects = new List<RecentProjectData>();
                }
                else
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    _cachedRecentProjects = JsonSerializer.Deserialize<List<RecentProjectData>>(json, options) ?? new List<RecentProjectData>();
                }
                
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing recent projects cache: {ex.Message}");
                _cachedRecentProjects = new List<RecentProjectData>();
                _isInitialized = true;
            }
        }

        /// <summary>
        /// Gets the cached recent projects data without localStorage access.
        /// </summary>
        private List<RecentProjectData> GetCachedRecentProjectsData()
        {
            return _cachedRecentProjects ?? new List<RecentProjectData>();
        }

        /// <summary>
        /// Saves the recent projects data to localStorage and updates the cache.
        /// </summary>
        private async Task SaveRecentProjectsDataAsync(List<RecentProjectData> recentProjectsData)
        {
            try
            {
                // Update the cache first
                _cachedRecentProjects = recentProjectsData;
                
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