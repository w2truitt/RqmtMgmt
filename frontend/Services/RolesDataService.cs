using System.Net.Http.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    public class RolesDataService : IRoleService
    {
        private readonly HttpClient _http;
        public RolesDataService(HttpClient http) => _http = http;

        public async Task<List<RoleDto>> GetAllRolesAsync()
            => await _http.GetFromJsonAsync<List<RoleDto>>("/api/Role") ?? new();

        public async Task<RoleDto?> CreateRoleAsync(string roleName)
        {
            var resp = await _http.PostAsJsonAsync("/api/Role", roleName);
            return await resp.Content.ReadFromJsonAsync<RoleDto>();
        }

        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            var resp = await _http.DeleteAsync($"/api/Role/{roleId}");
            return resp.IsSuccessStatusCode;
        }
    }
}