using System.Net.Http.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    /// <summary>
    /// Frontend data service for test case management operations.
    /// Provides HTTP client-based implementation of ITestCaseService for communicating with the backend API.
    /// </summary>
    public class TestCasesDataService : ITestCaseService
    {
        private readonly HttpClient _http;

        /// <summary>
        /// Initializes a new instance of the TestCasesDataService with the specified HTTP client.
        /// </summary>
        /// <param name="http">The HTTP client for making API requests.</param>
        public TestCasesDataService(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Retrieves all test cases from the backend API including their test steps.
        /// </summary>
        /// <returns>A list of all test cases with their test steps, or an empty list if the request fails.</returns>
        public async Task<List<TestCaseDto>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<TestCaseDto>>("/api/TestCase") ?? new();

        /// <summary>
        /// Retrieves a specific test case by its ID from the backend API including test steps.
        /// </summary>
        /// <param name="id">The unique identifier of the test case.</param>
        /// <returns>The test case if found; otherwise, null.</returns>
        public async Task<TestCaseDto?> GetByIdAsync(int id)
            => await _http.GetFromJsonAsync<TestCaseDto>($"/api/TestCase/{id}");

        /// <summary>
        /// Creates a new test case by sending a POST request to the backend API.
        /// </summary>
        /// <param name="dto">The test case data to create, including test steps.</param>
        /// <returns>The created test case with its assigned ID if successful; otherwise, null.</returns>
        public async Task<TestCaseDto?> CreateAsync(TestCaseDto dto)
        {
            var resp = await _http.PostAsJsonAsync("/api/TestCase", dto);
            return await resp.Content.ReadFromJsonAsync<TestCaseDto>();
        }

        /// <summary>
        /// Updates an existing test case by sending a PUT request to the backend API.
        /// </summary>
        /// <param name="dto">The test case data to update, including test steps.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(TestCaseDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"/api/TestCase/{dto.Id}", dto);
            return resp.IsSuccessStatusCode;
        }

        /// <summary>
        /// Deletes a test case by sending a DELETE request to the backend API.
        /// Associated test steps are automatically deleted.
        /// </summary>
        /// <param name="id">The unique identifier of the test case to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var resp = await _http.DeleteAsync($"/api/TestCase/{id}");
            return resp.IsSuccessStatusCode;
        }
    }
}