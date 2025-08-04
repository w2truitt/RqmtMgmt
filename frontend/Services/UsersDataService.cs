using System.Net.Http.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    public class UsersDataService
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
    }
}
