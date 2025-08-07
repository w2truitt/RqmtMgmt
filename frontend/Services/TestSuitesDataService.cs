using System.Net.Http.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    /// <summary>
    /// Frontend data service for test suite management operations.
    /// Provides HTTP client-based implementation of ITestSuiteService for communicating with the backend API.
    /// </summary>
    public class TestSuitesDataService : ITestSuiteService
    {
        private readonly HttpClient _http;

        /// <summary>
        /// Initializes a new instance of the TestSuitesDataService with the specified HTTP client.
        /// </summary>
        /// <param name="http">The HTTP client for making API requests.</param>
        public TestSuitesDataService(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Retrieves all test suites from the backend API.
        /// </summary>
        /// <returns>A list of all test suites, or an empty list if the request fails.</returns>
        public async Task<List<TestSuiteDto>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<TestSuiteDto>>("/api/TestSuite") ?? new();

        /// <summary>
        /// Retrieves a specific test suite by its ID from the backend API.
        /// </summary>
        /// <param name="id">The unique identifier of the test suite.</param>
        /// <returns>The test suite if found; otherwise, null.</returns>
        public async Task<TestSuiteDto?> GetByIdAsync(int id)
            => await _http.GetFromJsonAsync<TestSuiteDto>($"/api/TestSuite/{id}");

        /// <summary>
        /// Creates a new test suite by sending a POST request to the backend API.
        /// </summary>
        /// <param name="dto">The test suite data to create.</param>
        /// <returns>The created test suite with its assigned ID if successful; otherwise, null.</returns>
        public async Task<TestSuiteDto?> CreateAsync(TestSuiteDto dto)
        {
            var resp = await _http.PostAsJsonAsync("/api/TestSuite", dto);
            return await resp.Content.ReadFromJsonAsync<TestSuiteDto>();
        }

        /// <summary>
        /// Updates an existing test suite by sending a PUT request to the backend API.
        /// </summary>
        /// <param name="dto">The test suite data to update.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(TestSuiteDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"/api/TestSuite/{dto.Id}", dto);
            return resp.IsSuccessStatusCode;
        }

        /// <summary>
        /// Deletes a test suite by sending a DELETE request to the backend API.
        /// Associated test cases will have their SuiteId set to null.
        /// </summary>
        /// <param name="id">The unique identifier of the test suite to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var resp = await _http.DeleteAsync($"/api/TestSuite/{id}");
            return resp.IsSuccessStatusCode;
        }
    }
}