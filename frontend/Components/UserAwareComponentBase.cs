using Microsoft.AspNetCore.Components;
using RqmtMgmtShared;

namespace RqmtMgmt.Frontend.Components
{
    /// <summary>
    /// Base component that provides current user loading functionality.
    /// Components that need to access the current user should inherit from this base class.
    /// </summary>
    public abstract class UserAwareComponentBase : ComponentBase
    {
        [Inject]
        protected IUserService UserService { get; set; } = default!;

        protected UserDto? currentUser = null;

        /// <summary>
        /// Loads the current user asynchronously with error handling.
        /// Sets currentUser to null if loading fails.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected async Task LoadCurrentUserAsync()
        {
            try
            {
                currentUser = await UserService.GetCurrentUserAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading current user: {ex.Message}");
                // If we can't get the current user, we'll fall back to a default
                currentUser = null;
            }
        }
    }
}
