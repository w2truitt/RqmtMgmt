using System.Net.Http.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    /// <summary>
    /// Frontend data service for role management operations.
    /// Provides HTTP client-based implementation of IRoleService for communicating with the backend API.
    /// </summary>
    public class RolesDataService : IRoleService
    {
        private readonly HttpClient _http;

        /// <summary>
        /// Initializes a new instance of the RolesDataService with the specified HTTP client.
        /// </summary>
        /// <param name="http">The HTTP client for making API requests.</param>
        public RolesDataService(HttpClient http) => _http = http;

        /// <summary>
        /// Retrieves all roles from the backend API.
        /// </summary>
        /// <returns>A list of all available roles, or an empty list if the request fails.</returns>
        public async Task<List<RoleDto>> GetAllRolesAsync()
            => await _http.GetFromJsonAsync<List<RoleDto>>("/api/Role") ?? new();

        /// <summary>
        /// Creates a new role by sending a POST request to the backend API.
        /// The backend performs case-insensitive duplicate checking and returns existing role if found.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        /// <returns>The created or existing role if successful; otherwise, null.</returns>
        public async Task<RoleDto?> CreateRoleAsync(string roleName)
        {
            var resp = await _http.PostAsJsonAsync("/api/Role", roleName);
            return await resp.Content.ReadFromJsonAsync<RoleDto>();
        }

        /// <summary>
        /// Deletes a role by sending a DELETE request to the backend API.
        /// The backend prevents deletion of roles that are currently assigned to users.
        /// </summary>
        /// <param name="roleId">The unique identifier of the role to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            var resp = await _http.DeleteAsync($"/api/Role/{roleId}");
            return resp.IsSuccessStatusCode;
        }
    }
}