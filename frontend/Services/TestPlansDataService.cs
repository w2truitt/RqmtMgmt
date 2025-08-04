using System.Net.Http.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    public class TestPlansDataService
    {
        private readonly HttpClient _http;

        public TestPlansDataService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<TestPlanDto>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<TestPlanDto>>("/api/TestPlan") ?? new();

        public async Task<TestPlanDto?> GetByIdAsync(int id)
            => await _http.GetFromJsonAsync<TestPlanDto>($"/api/TestPlan/{id}");

        public async Task<TestPlanDto?> CreateAsync(TestPlanDto dto)
        {
            var resp = await _http.PostAsJsonAsync("/api/TestPlan", dto);
            return await resp.Content.ReadFromJsonAsync<TestPlanDto>();
        }

        public async Task<bool> UpdateAsync(TestPlanDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"/api/TestPlan/{dto.Id}", dto);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var resp = await _http.DeleteAsync($"/api/TestPlan/{id}");
            return resp.IsSuccessStatusCode;
        }
    }
}
