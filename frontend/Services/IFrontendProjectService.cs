using RqmtMgmtShared;
using frontend.Models;

namespace frontend.Services
{
    /// <summary>
    /// Extended project service interface with convenience methods for frontend operations.
    /// </summary>
    public interface IFrontendProjectService : IProjectService
    {
        /// <summary>
        /// Convenience method that wraps GetProjectByIdAsync with Result pattern.
        /// </summary>
        Task<ServiceResult<ProjectDto>> GetProjectAsync(int projectId);
        
        /// <summary>
        /// Convenience method that wraps GetProjectsAsync with Result pattern.
        /// </summary>
        Task<ServiceResult<PagedResult<ProjectDto>>> GetProjectsAsync();
    }
}
