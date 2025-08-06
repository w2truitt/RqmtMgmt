using System.Net.Http.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    public class RequirementsDataService : IRequirementService
    {
        private readonly HttpClient _http;

        public RequirementsDataService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<RequirementDto>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<RequirementDto>>("/api/Requirement") ?? new();

        public async Task<RequirementDto?> GetByIdAsync(int id)
            => await _http.GetFromJsonAsync<RequirementDto>($"/api/Requirement/{id}");

        public async Task<RequirementDto?> CreateAsync(RequirementDto dto)
        {
            var resp = await _http.PostAsJsonAsync("/api/Requirement", dto);
            return await resp.Content.ReadFromJsonAsync<RequirementDto>();
        }

        public async Task<bool> UpdateAsync(RequirementDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"/api/Requirement/{dto.Id}", dto);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var resp = await _http.DeleteAsync($"/api/Requirement/{id}");
            return resp.IsSuccessStatusCode;
        }

        public async Task<List<RequirementVersionDto>> GetVersionsAsync(int requirementId)
            => await _http.GetFromJsonAsync<List<RequirementVersionDto>>($"/api/Redline/requirement/{requirementId}/versions") ?? new();
    }
}