using System.Net.Http.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    public class TestSuitesDataService
    {
        private readonly HttpClient _http;

        public TestSuitesDataService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<TestSuiteDto>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<TestSuiteDto>>("/api/TestSuite") ?? new();

        public async Task<TestSuiteDto?> GetByIdAsync(int id)
            => await _http.GetFromJsonAsync<TestSuiteDto>($"/api/TestSuite/{id}");

        public async Task<TestSuiteDto?> CreateAsync(TestSuiteDto dto)
        {
            var resp = await _http.PostAsJsonAsync("/api/TestSuite", dto);
            return await resp.Content.ReadFromJsonAsync<TestSuiteDto>();
        }

        public async Task<bool> UpdateAsync(TestSuiteDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"/api/TestSuite/{dto.Id}", dto);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var resp = await _http.DeleteAsync($"/api/TestSuite/{id}");
            return resp.IsSuccessStatusCode;
        }
    }
}
