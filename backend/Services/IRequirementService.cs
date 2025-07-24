using backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services
{
    /// <summary>
    /// Service interface for managing requirements.
    /// </summary>
    public interface IRequirementService
    {
        /// <summary>
        /// Gets all requirements asynchronously.
        /// </summary>
        Task<IEnumerable<Requirement>> GetAllAsync();

        /// <summary>
        /// Gets a requirement by its unique identifier asynchronously.
        /// Returns null if not found.
        /// </summary>
        Task<Requirement?> GetByIdAsync(int id);

        /// <summary>
        /// Creates a new requirement asynchronously.
        /// </summary>
        Task<Requirement> CreateAsync(Requirement requirement);

        /// <summary>
        /// Updates an existing requirement asynchronously.
        /// </summary>
        Task<Requirement> UpdateAsync(Requirement requirement);

        /// <summary>
        /// Deletes a requirement by its unique identifier asynchronously.
        /// </summary>
        Task<bool> DeleteAsync(int id);
    }
}
