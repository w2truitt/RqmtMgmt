using System.Net.Http.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    /// <summary>
    /// Frontend data service for test plan management operations.
    /// Provides HTTP client-based implementation of ITestPlanService for communicating with the backend API.
    /// </summary>
    public class TestPlansDataService : ITestPlanService
    {
        private readonly HttpClient _http;

        /// <summary>
        /// Initializes a new instance of the TestPlansDataService with the specified HTTP client.
        /// </summary>
        /// <param name="http">The HTTP client for making API requests.</param>
        public TestPlansDataService(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Retrieves all test plans from the backend API including their type information.
        /// </summary>
        /// <returns>A list of all test plans with their metadata, or an empty list if the request fails.</returns>
        public async Task<List<TestPlanDto>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<TestPlanDto>>("/api/TestPlan") ?? new();

        /// <summary>
        /// Retrieves a specific test plan by its ID from the backend API.
        /// </summary>
        /// <param name="id">The unique identifier of the test plan.</param>
        /// <returns>The test plan if found; otherwise, null.</returns>
        public async Task<TestPlanDto?> GetByIdAsync(int id)
            => await _http.GetFromJsonAsync<TestPlanDto>($"/api/TestPlan/{id}");

        /// <summary>
        /// Creates a new test plan by sending a POST request to the backend API.
        /// </summary>
        /// <param name="testPlan">The test plan data to create, including type specification.</param>
        /// <returns>The created test plan with its assigned ID if successful; otherwise, null.</returns>
        public async Task<TestPlanDto?> CreateAsync(TestPlanDto testPlan)
        {
            var resp = await _http.PostAsJsonAsync("/api/TestPlan", testPlan);
            return await resp.Content.ReadFromJsonAsync<TestPlanDto>();
        }

        /// <summary>
        /// Updates an existing test plan by sending a PUT request to the backend API.
        /// </summary>
        /// <param name="testPlan">The test plan data to update, including type changes.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(TestPlanDto testPlan)
        {
            var resp = await _http.PutAsJsonAsync($"/api/TestPlan/{testPlan.Id}", testPlan);
            return resp.IsSuccessStatusCode;
        }

        /// <summary>
        /// Deletes a test plan by sending a DELETE request to the backend API.
        /// Associated test case links are cascade deleted.
        /// </summary>
        /// <param name="id">The unique identifier of the test plan to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var resp = await _http.DeleteAsync($"/api/TestPlan/{id}");
            return resp.IsSuccessStatusCode;
        }

        /// <summary>
        /// Retrieves test plans with pagination, filtering, and sorting capabilities.
        /// </summary>
        /// <param name="parameters">Pagination parameters including page number, size, search term, and sorting options.</param>
        /// <returns>A paginated result containing test plans and pagination metadata.</returns>
        public async Task<PagedResult<TestPlanDto>> GetPagedAsync(PaginationParameters parameters)
        {
            var queryString = $"?page={parameters.PageNumber}&pageSize={parameters.PageSize}";
            
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
                queryString += $"&searchTerm={Uri.EscapeDataString(parameters.SearchTerm)}";
            
            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
                queryString += $"&sortBy={Uri.EscapeDataString(parameters.SortBy)}";
            
            if (parameters.SortDescending)
                queryString += "&sortDescending=true";

            var result = await _http.GetFromJsonAsync<PagedResult<TestPlanDto>>($"/api/TestPlan{queryString}");
            return result ?? new PagedResult<TestPlanDto>();
        }

        /// <summary>
        /// Retrieves test plans for a specific project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <returns>A list of test plans for the project.</returns>
        public async Task<List<TestPlanDto>> GetByProjectIdAsync(int projectId)
        {
            var result = await _http.GetFromJsonAsync<List<TestPlanDto>>($"/api/Projects/{projectId}/testplans");
            return result ?? new List<TestPlanDto>();
        }

        /// <summary>
        /// Retrieves test plans for a specific project with pagination, filtering, and sorting capabilities.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <param name="parameters">Pagination parameters including page number, size, search term, and sorting options.</param>
        /// <returns>A paginated result containing test plans for the project and pagination metadata.</returns>
        public async Task<PagedResult<TestPlanDto>> GetPagedByProjectIdAsync(int projectId, PaginationParameters parameters)
        {
            var queryString = $"?page={parameters.PageNumber}&pageSize={parameters.PageSize}";
            
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
                queryString += $"&searchTerm={Uri.EscapeDataString(parameters.SearchTerm)}";
            
            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
                queryString += $"&sortBy={Uri.EscapeDataString(parameters.SortBy)}";
            
            if (parameters.SortDescending)
                queryString += "&sortDescending=true";

            var result = await _http.GetFromJsonAsync<PagedResult<TestPlanDto>>($"/api/Projects/{projectId}/testplans{queryString}");
            return result ?? new PagedResult<TestPlanDto>();
        }
    }
}