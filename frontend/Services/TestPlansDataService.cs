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
        /// <param name="dto">The test plan data to create, including type specification.</param>
        /// <returns>The created test plan with its assigned ID if successful; otherwise, null.</returns>
        public async Task<TestPlanDto?> CreateAsync(TestPlanDto dto)
        {
            var resp = await _http.PostAsJsonAsync("/api/TestPlan", dto);
            return await resp.Content.ReadFromJsonAsync<TestPlanDto>();
        }

        /// <summary>
        /// Updates an existing test plan by sending a PUT request to the backend API.
        /// </summary>
        /// <param name="dto">The test plan data to update, including type changes.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(TestPlanDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"/api/TestPlan/{dto.Id}", dto);
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
    }
}