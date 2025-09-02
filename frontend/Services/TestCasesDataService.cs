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
        /// <param name="testCase">The test case data to create, including test steps.</param>
        /// <returns>The created test case with its assigned ID if successful; otherwise, null.</returns>
        public async Task<TestCaseDto?> CreateAsync(TestCaseDto testCase)
        {
            var resp = await _http.PostAsJsonAsync("/api/TestCase", testCase);
            return await resp.Content.ReadFromJsonAsync<TestCaseDto>();
        }

        /// <summary>
        /// Updates an existing test case by sending a PUT request to the backend API.
        /// </summary>
        /// <param name="testCase">The test case data to update, including test steps.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(TestCaseDto testCase)
        {
            var resp = await _http.PutAsJsonAsync($"/api/TestCase/{testCase.Id}", testCase);
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

        /// <summary>
        /// Retrieves test cases with pagination, filtering, and sorting capabilities.
        /// </summary>
        /// <param name="parameters">Pagination parameters including page number, size, search term, and sorting options.</param>
        /// <returns>A paginated result containing test cases and pagination metadata.</returns>
        public async Task<PagedResult<TestCaseDto>> GetPagedAsync(PaginationParameters parameters)
        {
            var queryString = $"?page={parameters.PageNumber}&pageSize={parameters.PageSize}";
            
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
                queryString += $"&searchTerm={Uri.EscapeDataString(parameters.SearchTerm)}";
            
            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
                queryString += $"&sortBy={Uri.EscapeDataString(parameters.SortBy)}";
            
            if (parameters.SortDescending)
                queryString += "&sortDescending=true";

            var result = await _http.GetFromJsonAsync<PagedResult<TestCaseDto>>($"/api/TestCase{queryString}");
            return result ?? new PagedResult<TestCaseDto>();
        }

        /// <summary>
        /// Retrieves test cases for a specific test suite.
        /// </summary>
        /// <param name="testSuiteId">The unique identifier of the test suite.</param>
        /// <returns>A list of test cases for the test suite.</returns>
        public async Task<List<TestCaseDto>> GetByTestSuiteIdAsync(int testSuiteId)
        {
            var result = await _http.GetFromJsonAsync<List<TestCaseDto>>($"/api/TestSuite/{testSuiteId}/testcases");
            return result ?? new List<TestCaseDto>();
        }

        /// <summary>
        /// Retrieves test cases for a specific test suite with pagination, filtering, and sorting capabilities.
        /// </summary>
        /// <param name="testSuiteId">The unique identifier of the test suite.</param>
        /// <param name="parameters">Pagination parameters including page number, size, search term, and sorting options.</param>
        /// <returns>A paginated result containing test cases for the test suite and pagination metadata.</returns>
        public async Task<PagedResult<TestCaseDto>> GetPagedByTestSuiteIdAsync(int testSuiteId, PaginationParameters parameters)
        {
            var queryString = $"?page={parameters.PageNumber}&pageSize={parameters.PageSize}";
            
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
                queryString += $"&searchTerm={Uri.EscapeDataString(parameters.SearchTerm)}";
            
            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
                queryString += $"&sortBy={Uri.EscapeDataString(parameters.SortBy)}";
            
            if (parameters.SortDescending)
                queryString += "&sortDescending=true";

            var result = await _http.GetFromJsonAsync<PagedResult<TestCaseDto>>($"/api/TestSuite/{testSuiteId}/testcases{queryString}");
            return result ?? new PagedResult<TestCaseDto>();
        }

            /// <summary>
            /// Retrieves test cases for a specific project with pagination, filtering, and sorting capabilities.
            /// This includes test cases from all test suites belonging to the project, plus any unassigned test cases.
            /// </summary>
            /// <param name="projectId">The unique identifier of the project.</param>
            /// <param name="parameters">Pagination parameters including page number, size, search term, and sorting options.</param>
            /// <returns>A paginated result containing test cases for the project and pagination metadata.</returns>
            public async Task<PagedResult<TestCaseDto>> GetPagedByProjectIdAsync(int projectId, PaginationParameters parameters)
            {
                var queryString = $"?page={parameters.PageNumber}&pageSize={parameters.PageSize}";
            
                if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
                    queryString += $"&searchTerm={Uri.EscapeDataString(parameters.SearchTerm)}";
            
                if (!string.IsNullOrWhiteSpace(parameters.SortBy))
                    queryString += $"&sortBy={Uri.EscapeDataString(parameters.SortBy)}";
            
                if (parameters.SortDescending)
                    queryString += "&sortDescending=true";

                var result = await _http.GetFromJsonAsync<PagedResult<TestCaseDto>>($"/api/Projects/{projectId}/test-cases{queryString}");
                return result ?? new PagedResult<TestCaseDto>();
            }
    }
}