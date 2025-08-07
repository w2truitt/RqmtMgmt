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
        /// <param name="dto">The user data to create.</param>
        /// <returns>The created user with its assigned ID if successful; otherwise, null.</returns>
        public async Task<UserDto?> CreateAsync(UserDto dto)
        {
            var resp = await _http.PostAsJsonAsync("/api/User", dto);
            return await resp.Content.ReadFromJsonAsync<UserDto>();
        }

        /// <summary>
        /// Updates an existing user by sending a PUT request to the backend API.
        /// </summary>
        /// <param name="dto">The user data to update.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(UserDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"/api/User/{dto.Id}", dto);
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
    }
}