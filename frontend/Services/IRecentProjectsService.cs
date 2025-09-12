using RqmtMgmtShared;

namespace frontend.Services
{
    /// <summary>
    /// Service interface for managing user's recently accessed projects.
    /// Uses browser localStorage to persist recent projects across sessions.
    /// </summary>
    public interface IRecentProjectsService
    {
        /// <summary>
        /// Gets the user's recently accessed projects, ordered by most recent first.
        /// </summary>
        /// <param name="maxCount">Maximum number of recent projects to return (default: 5)</param>
        /// <returns>List of recent projects, empty if none found</returns>
        Task<List<ProjectDto>> GetRecentProjectsAsync(int maxCount = 5);

        /// <summary>
        /// Records that a project was accessed by the user.
        /// Updates the access timestamp and moves project to top of recent list.
        /// </summary>
        /// <param name="project">The project that was accessed</param>
        Task TrackProjectAccessAsync(ProjectDto project);

        /// <summary>
        /// Records that a project was accessed by the user using project ID.
        /// Will fetch project details if not already in recent list.
        /// </summary>
        /// <param name="projectId">The ID of the project that was accessed</param>
        Task TrackProjectAccessAsync(int projectId);

        /// <summary>
        /// Removes all recent projects from localStorage.
        /// </summary>
        Task ClearRecentProjectsAsync();

        /// <summary>
        /// Removes a specific project from the recent projects list.
        /// </summary>
        /// <param name="projectId">The ID of the project to remove</param>
        Task RemoveRecentProjectAsync(int projectId);

        /// <summary>
        /// Gets the count of recently accessed projects without loading full project data.
        /// Useful for UI display purposes.
        /// </summary>
        /// <returns>Number of recent projects stored</returns>
        Task<int> GetRecentProjectsCountAsync();
    }
}