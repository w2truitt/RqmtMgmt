using System.Net.Http.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    /// <summary>
    /// Frontend data service for user management operations and role assignments.
    /// Provides HTTP client-based implementation of IUserService for communicating with the backend API.
    /// Now inherits from BaseDataService to eliminate code duplication and ensure consistency.
    /// </summary>
    public class UsersDataService : BaseDataService, IUserService
    {
        /// <summary>
        /// Initializes a new instance of the UsersDataService with the specified HTTP client.
        /// </summary>
        /// <param name="http">The HTTP client for making API requests.</param>
        public UsersDataService(HttpClient http) : base(http)
        {
        }

        /// <summary>
        /// Retrieves all users from the backend API including their assigned roles.
        /// </summary>
        /// <returns>A list of all users with their role information, or an empty list if the request fails.</returns>
        public async Task<List<UserDto>> GetAllAsync()
            => await GetListAsync<UserDto>("/api/User");

        /// <summary>
        /// Retrieves a specific user by their ID from the backend API including role information.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>The user if found; otherwise, null.</returns>
        public async Task<UserDto?> GetByIdAsync(int id)
            => await GetAsync<UserDto>($"/api/User/{id}");

        /// <summary>
        /// Creates a new user by sending a POST request to the backend API.
        /// </summary>
        /// <param name="user">The user data to create.</param>
        /// <returns>The created user with its assigned ID if successful; otherwise, null.</returns>
        public async Task<UserDto?> CreateAsync(UserDto user)
            => await PostAsync<UserDto, UserDto>("/api/User", user);

        /// <summary>
        /// Updates an existing user by sending a PUT request to the backend API.
        /// </summary>
        /// <param name="user">The user data to update.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(UserDto user)
            => await PutAsync($"/api/User/{user.Id}", user);

        /// <summary>
        /// Deletes a user by sending a DELETE request to the backend API.
        /// </summary>
        /// <param name="id">The unique identifier of the user to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
            => await DeleteAsync($"/api/User/{id}");

        /// <summary>
        /// Retrieves all roles assigned to a specific user from the backend API.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A list of role names assigned to the user, or an empty list if the request fails.</returns>
        public async Task<List<string>> GetUserRolesAsync(int userId)
            => await GetListAsync<string>($"/api/User/{userId}/roles");

        /// <summary>
        /// Retrieves a specific user by their email address from the backend API including role information.
        /// PERFORMANCE FIX: Now uses dedicated backend endpoint instead of client-side filtering.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>The user if found; otherwise, null.</returns>
        public async Task<UserDto?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await GetAsync<UserDto>($"/api/User/by-email?email={Uri.EscapeDataString(email)}");
        }

        /// <summary>
        /// Retrieves the current authenticated user's information from the backend API.
        /// </summary>
        /// <returns>The current user if authenticated and found; otherwise, null.</returns>
        public async Task<UserDto?> GetCurrentUserAsync()
            => await GetAsync<UserDto>("/api/User/me");

        /// <summary>
        /// Assigns a role to a user by sending a POST request to the backend API.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="role">The name of the role to assign to the user.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AssignRoleAsync(int userId, string role)
        {
            await PostAsync($"/api/User/{userId}/roles", role);
        }

        /// <summary>
        /// Removes a role from a user by sending a DELETE request to the backend API.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="role">The name of the role to remove from the user.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RemoveRoleAsync(int userId, string role)
        {
            await DeleteAsync($"/api/User/{userId}/roles/{role}");
        }

        /// <summary>
        /// Retrieves users with pagination, filtering, and sorting capabilities.
        /// </summary>
        /// <param name="parameters">Pagination parameters including page number, size, search term, and sorting options.</param>
        /// <returns>A paginated result containing users and pagination metadata.</returns>
        public async Task<PagedResult<UserDto>> GetPagedAsync(PaginationParameters parameters)
        {
            var queryParams = new Dictionary<string, object?>
            {
                ["page"] = parameters.PageNumber,
                ["pageSize"] = parameters.PageSize,
                ["searchTerm"] = parameters.SearchTerm,
                ["sortBy"] = parameters.SortBy,
                ["sortDescending"] = parameters.SortDescending ? "true" : null
            };

            var queryString = BuildQueryString(queryParams);
            var result = await GetAsync<PagedResult<UserDto>>($"/api/User/paged{queryString}");
            return result ?? new PagedResult<UserDto>();
        }
    }
}