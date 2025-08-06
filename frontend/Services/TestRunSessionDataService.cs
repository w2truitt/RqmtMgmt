using System.Net.Http.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    /// <summary>
    /// Service for managing test run session data operations
    /// </summary>
    public interface ITestRunSessionDataService
    {
        Task<List<TestRunSessionDto>> GetAllAsync();
        Task<TestRunSessionDto?> GetByIdAsync(int id);
        Task<TestRunSessionDto?> CreateAsync(TestRunSessionDto testRunSession);
        Task<bool> UpdateAsync(TestRunSessionDto testRunSession);
        Task<bool> DeleteAsync(int id);
        Task<TestRunSessionDto?> StartTestRunSessionAsync(TestRunSessionDto testRunSession);
        Task<bool> CompleteTestRunSessionAsync(int id);
        Task<bool> AbortTestRunSessionAsync(int id);
        Task<List<TestRunSessionDto>> GetActiveSessionsAsync();
    }

    public class TestRunSessionDataService : ITestRunSessionDataService
    {
        private readonly HttpClient _httpClient;

        public TestRunSessionDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<TestRunSessionDto>> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<TestRunSessionDto>>("api/testRunSession");
                return response ?? new List<TestRunSessionDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                return new List<TestRunSessionDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving test run sessions: {ex.Message}");
                return new List<TestRunSessionDto>();
            }
        }

        public async Task<TestRunSessionDto?> GetByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<TestRunSessionDto>($"api/testRunSession/{id}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving test run session {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<TestRunSessionDto?> CreateAsync(TestRunSessionDto testRunSession)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/testRunSession", testRunSession);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TestRunSessionDto>();
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
                Console.WriteLine($"Error creating test run session: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateAsync(TestRunSessionDto testRunSession)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/testRunSession/{testRunSession.Id}", testRunSession);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating test run session: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/testRunSession/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting test run session: {ex.Message}");
                return false;
            }
        }

        public async Task<TestRunSessionDto?> StartTestRunSessionAsync(TestRunSessionDto testRunSession)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/testRunSession/start", testRunSession);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TestRunSessionDto>();
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
                Console.WriteLine($"Error starting test run session: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CompleteTestRunSessionAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/testRunSession/{id}/complete", null);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error completing test run session: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AbortTestRunSessionAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/testRunSession/{id}/abort", null);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error aborting test run session: {ex.Message}");
                return false;
            }
        }

        public async Task<List<TestRunSessionDto>> GetActiveSessionsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<TestRunSessionDto>>("api/testRunSession/active");
                return response ?? new List<TestRunSessionDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                return new List<TestRunSessionDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving active test run sessions: {ex.Message}");
                return new List<TestRunSessionDto>();
            }
        }
    }
}