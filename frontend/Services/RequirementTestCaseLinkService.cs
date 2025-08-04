using System.Net.Http.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    public class RequirementTestCaseLinkService
    {
        private readonly HttpClient _http;
        public RequirementTestCaseLinkService(HttpClient http) => _http = http;

        public async Task<List<RequirementTestCaseLinkDto>> GetLinksForRequirement(int requirementId)
            => await _http.GetFromJsonAsync<List<RequirementTestCaseLinkDto>>($"/api/RequirementTestCaseLink?requirementId={requirementId}") ?? new();

        public async Task<List<RequirementTestCaseLinkDto>> GetLinksForTestCase(int testCaseId)
            => await _http.GetFromJsonAsync<List<RequirementTestCaseLinkDto>>($"/api/RequirementTestCaseLink?testCaseId={testCaseId}") ?? new();

        public async Task<bool> AddLink(int requirementId, int testCaseId)
        {
            var dto = new RequirementTestCaseLinkDto { RequirementId = requirementId, TestCaseId = testCaseId };
            var resp = await _http.PostAsJsonAsync("/api/RequirementTestCaseLink", dto);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveLink(int requirementId, int testCaseId)
        {
            var resp = await _http.DeleteAsync($"/api/RequirementTestCaseLink?requirementId={requirementId}&testCaseId={testCaseId}");
            return resp.IsSuccessStatusCode;
        }
    }
}
