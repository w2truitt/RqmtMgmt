using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

namespace frontend.Services
{
    public class RolesDataService
    {
        private readonly HttpClient _http;
        public RolesDataService(HttpClient http) => _http = http;

        public async Task<List<RoleDto>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<RoleDto>>("/api/Role") ?? new();

        public async Task<RoleDto?> CreateAsync(string name)
        {
            var resp = await _http.PostAsJsonAsync("/api/Role", name);
            return await resp.Content.ReadFromJsonAsync<RoleDto>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var resp = await _http.DeleteAsync($"/api/Role/{id}");
            return resp.IsSuccessStatusCode;
        }
    }
}
