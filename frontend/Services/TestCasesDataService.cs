using System.Net.Http.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    public class TestCasesDataService : ITestCaseService
    {
        private readonly HttpClient _http;

        public TestCasesDataService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<TestCaseDto>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<TestCaseDto>>("/api/TestCase") ?? new();

        public async Task<TestCaseDto?> GetByIdAsync(int id)
            => await _http.GetFromJsonAsync<TestCaseDto>($"/api/TestCase/{id}");

        public async Task<TestCaseDto?> CreateAsync(TestCaseDto dto)
        {
            var resp = await _http.PostAsJsonAsync("/api/TestCase", dto);
            return await resp.Content.ReadFromJsonAsync<TestCaseDto>();
        }

        public async Task<bool> UpdateAsync(TestCaseDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"/api/TestCase/{dto.Id}", dto);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var resp = await _http.DeleteAsync($"/api/TestCase/{id}");
            return resp.IsSuccessStatusCode;
        }
    }
}