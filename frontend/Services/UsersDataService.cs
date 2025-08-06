using System.Net.Http.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    public class UsersDataService : IUserService
    {
        private readonly HttpClient _http;

        public UsersDataService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<UserDto>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<UserDto>>("/api/User") ?? new();

        public async Task<UserDto?> GetByIdAsync(int id)
            => await _http.GetFromJsonAsync<UserDto>($"/api/User/{id}");

        public async Task<UserDto?> CreateAsync(UserDto dto)
        {
            var resp = await _http.PostAsJsonAsync("/api/User", dto);
            return await resp.Content.ReadFromJsonAsync<UserDto>();
        }

        public async Task<bool> UpdateAsync(UserDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"/api/User/{dto.Id}", dto);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var resp = await _http.DeleteAsync($"/api/User/{id}");
            return resp.IsSuccessStatusCode;
        }

        // Additional methods required by IUserService interface
        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            return await _http.GetFromJsonAsync<List<string>>($"/api/User/{userId}/roles") ?? new();
        }

        public async Task AssignRoleAsync(int userId, string role)
        {
            await _http.PostAsJsonAsync($"/api/User/{userId}/roles", role);
        }

        public async Task RemoveRoleAsync(int userId, string role)
        {
            await _http.DeleteAsync($"/api/User/{userId}/roles/{role}");
        }
    }
}