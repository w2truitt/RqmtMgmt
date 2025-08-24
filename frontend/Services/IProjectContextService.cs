using RqmtMgmtShared;

namespace frontend.Services
{
    public interface IProjectContextService
    {
        ProjectDto? CurrentProject { get; }
        event Action<ProjectDto?>? ProjectChanged;
        Task SetCurrentProjectAsync(ProjectDto? project);
        Task SetCurrentProjectAsync(int projectId);
        Task ClearProjectContextAsync();
        bool IsInProjectContext { get; }
        Task<bool> LoadProjectContextFromRouteAsync(string relativePath);
        Task<bool> LoadPersistedProjectContextAsync();
    }
}
