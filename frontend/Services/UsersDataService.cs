using System.Net.Http.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    /// <summary>
    /// Frontend data service for user management operations and role assignments.
    /// Provides HTTP client-based implementation of IUserService for communicating with the backend API.
    /// </summary>
    public class UsersDataService : IUserService
    {
        private readonly HttpClient _http;

        /// <summary>
        /// Initializes a new instance of the UsersDataService with the specified HTTP client.
        /// </summary>
        /// <param name="http">The HTTP client for making API requests.</param>
        public UsersDataService(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Retrieves all users from the backend API including their assigned roles.
        /// </summary>
        /// <returns>A list of all users with their role information, or an empty list if the request fails.</returns>
        public async Task<List<UserDto>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<UserDto>>("/api/User") ?? new();

        /// <summary>
        /// Retrieves a specific user by their ID from the backend API including role information.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>The user if found; otherwise, null.</returns>
        public async Task<UserDto?> GetByIdAsync(int id)
            => await _http.GetFromJsonAsync<UserDto>($"/api/User/{id}");

        /// <summary>
        /// Creates a new user by sending a POST request to the backend API.
        /// </summary>
        /// <param name="user">The user data to create.</param>
        /// <returns>The created user with its assigned ID if successful; otherwise, null.</returns>
        public async Task<UserDto?> CreateAsync(UserDto user)
        {
            var resp = await _http.PostAsJsonAsync("/api/User", user);
            return await resp.Content.ReadFromJsonAsync<UserDto>();
        }

        /// <summary>
        /// Updates an existing user by sending a PUT request to the backend API.
        /// </summary>
        /// <param name="user">The user data to update.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(UserDto user)
        {
            var resp = await _http.PutAsJsonAsync($"/api/User/{user.Id}", user);
            return resp.IsSuccessStatusCode;
        }

        /// <summary>
        /// Deletes a user by sending a DELETE request to the backend API.
        /// </summary>
        /// <param name="id">The unique identifier of the user to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var resp = await _http.DeleteAsync($"/api/User/{id}");
            return resp.IsSuccessStatusCode;
        }

        /// <summary>
        /// Retrieves all roles assigned to a specific user from the backend API.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A list of role names assigned to the user, or an empty list if the request fails.</returns>
        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            return await _http.GetFromJsonAsync<List<string>>($"/api/User/{userId}/roles") ?? new();
        }

        /// <summary>
        /// Retrieves a specific user by their email address from the backend API including role information.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>The user if found; otherwise, null.</returns>
        public async Task<UserDto?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var users = await GetAllAsync();
            return users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Retrieves the current authenticated user's information from the backend API.
        /// </summary>
        /// <returns>The current user if authenticated and found; otherwise, null.</returns>
        public async Task<UserDto?> GetCurrentUserAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<UserDto>("/api/User/me");
            }
            catch (HttpRequestException)
            {
                // User not authenticated or not found
                return null;
            }
        }

        /// <summary>
        /// Assigns a role to a user by sending a POST request to the backend API.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="role">The name of the role to assign to the user.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AssignRoleAsync(int userId, string role)
        {
            await _http.PostAsJsonAsync($"/api/User/{userId}/roles", role);
        }

        /// <summary>
        /// Removes a role from a user by sending a DELETE request to the backend API.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="role">The name of the role to remove from the user.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RemoveRoleAsync(int userId, string role)
        {
            await _http.DeleteAsync($"/api/User/{userId}/roles/{role}");
        }

        /// <summary>
        /// Retrieves users with pagination, filtering, and sorting capabilities.
        /// </summary>
        /// <param name="parameters">Pagination parameters including page number, size, search term, and sorting options.</param>
        /// <returns>A paginated result containing users and pagination metadata.</returns>
        public async Task<PagedResult<UserDto>> GetPagedAsync(PaginationParameters parameters)
        {
            var queryString = $"?page={parameters.PageNumber}&pageSize={parameters.PageSize}";
            
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
                queryString += $"&searchTerm={Uri.EscapeDataString(parameters.SearchTerm)}";
            
            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
                queryString += $"&sortBy={Uri.EscapeDataString(parameters.SortBy)}";
            
            if (parameters.SortDescending)
                queryString += "&sortDescending=true";

            var result = await _http.GetFromJsonAsync<PagedResult<UserDto>>($"/api/User{queryString}");
            return result ?? new PagedResult<UserDto>();
        }
    }
}