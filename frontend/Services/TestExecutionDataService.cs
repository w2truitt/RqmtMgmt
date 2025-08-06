using System.Net.Http.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    /// <summary>
    /// Service for managing test execution data operations
    /// </summary>
    public interface ITestExecutionDataService
    {
        Task<TestCaseExecutionDto?> ExecuteTestCaseAsync(TestCaseExecutionDto execution);
        Task<bool> UpdateTestCaseExecutionAsync(TestCaseExecutionDto execution);
        Task<TestStepExecutionDto?> UpdateStepResultAsync(TestStepExecutionDto stepExecution);
        Task<List<TestCaseExecutionDto>> GetExecutionsForSessionAsync(int sessionId);
        Task<List<TestStepExecutionDto>> GetStepExecutionsForCaseAsync(int caseExecutionId);
        Task<TestExecutionStatsDto?> GetExecutionStatsAsync();
        Task<TestExecutionStatsDto?> GetExecutionStatsForSessionAsync(int sessionId);
    }

    public class TestExecutionDataService : ITestExecutionDataService
    {
        private readonly HttpClient _httpClient;

        public TestExecutionDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TestCaseExecutionDto?> ExecuteTestCaseAsync(TestCaseExecutionDto execution)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/testExecution/execute-testcase", execution);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TestCaseExecutionDto>();
                }
                return null;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing test case: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateTestCaseExecutionAsync(TestCaseExecutionDto execution)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/testExecution/testcase-execution/{execution.Id}", execution);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating test case execution: {ex.Message}");
                return false;
            }
        }

        public async Task<TestStepExecutionDto?> UpdateStepResultAsync(TestStepExecutionDto stepExecution)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/testExecution/update-step-result", stepExecution);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TestStepExecutionDto>();
                }
                return null;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating test step result: {ex.Message}");
                return null;
            }
        }

        public async Task<List<TestCaseExecutionDto>> GetExecutionsForSessionAsync(int sessionId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<TestCaseExecutionDto>>($"api/testExecution/session/{sessionId}/executions");
                return response ?? new List<TestCaseExecutionDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                return new List<TestCaseExecutionDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving test case executions for session {sessionId}: {ex.Message}");
                return new List<TestCaseExecutionDto>();
            }
        }

        public async Task<List<TestStepExecutionDto>> GetStepExecutionsForCaseAsync(int caseExecutionId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<TestStepExecutionDto>>($"api/testExecution/case-execution/{caseExecutionId}/steps");
                return response ?? new List<TestStepExecutionDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                return new List<TestStepExecutionDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving test step executions for case {caseExecutionId}: {ex.Message}");
                return new List<TestStepExecutionDto>();
            }
        }

        public async Task<TestExecutionStatsDto?> GetExecutionStatsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<TestExecutionStatsDto>("api/testExecution/statistics");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving test execution statistics: {ex.Message}");
                return null;
            }
        }

        public async Task<TestExecutionStatsDto?> GetExecutionStatsForSessionAsync(int sessionId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<TestExecutionStatsDto>($"api/testExecution/session/{sessionId}/statistics");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving test execution statistics for session {sessionId}: {ex.Message}");
                return null;
            }
        }
    }
}