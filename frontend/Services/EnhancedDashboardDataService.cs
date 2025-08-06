using System.Text.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    /// <summary>
    /// Frontend service for enhanced dashboard data operations
    /// </summary>
    public class EnhancedDashboardDataService : IEnhancedDashboardService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public EnhancedDashboardDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/dashboard/enhanced-statistics");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var statistics = JsonSerializer.Deserialize<DashboardStatsDto>(json, _jsonOptions);
                    return statistics ?? new DashboardStatsDto();
                }
                else
                {
                    Console.WriteLine($"Failed to get enhanced dashboard statistics: {response.StatusCode}");
                    return new DashboardStatsDto();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting enhanced dashboard statistics: {ex.Message}");
                return new DashboardStatsDto();
            }
        }

        public async Task<RequirementStatsDto> GetRequirementStatsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/dashboard/requirements-stats");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var stats = JsonSerializer.Deserialize<RequirementStatsDto>(json, _jsonOptions);
                    return stats ?? new RequirementStatsDto();
                }
                else
                {
                    Console.WriteLine($"Failed to get requirement statistics: {response.StatusCode}");
                    return new RequirementStatsDto();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting requirement statistics: {ex.Message}");
                return new RequirementStatsDto();
            }
        }

        public async Task<TestManagementStatsDto> GetTestManagementStatsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/dashboard/test-management-stats");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var stats = JsonSerializer.Deserialize<TestManagementStatsDto>(json, _jsonOptions);
                    return stats ?? new TestManagementStatsDto();
                }
                else
                {
                    Console.WriteLine($"Failed to get test management statistics: {response.StatusCode}");
                    return new TestManagementStatsDto();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting test management statistics: {ex.Message}");
                return new TestManagementStatsDto();
            }
        }

        public async Task<TestExecutionStatsDto> GetTestExecutionStatsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/dashboard/test-execution-stats");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var stats = JsonSerializer.Deserialize<TestExecutionStatsDto>(json, _jsonOptions);
                    return stats ?? new TestExecutionStatsDto();
                }
                else
                {
                    Console.WriteLine($"Failed to get test execution statistics: {response.StatusCode}");
                    return new TestExecutionStatsDto();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting test execution statistics: {ex.Message}");
                return new TestExecutionStatsDto();
            }
        }

        public async Task<List<RecentActivityDto>> GetRecentActivityAsync(int count = 5)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/dashboard/recent-activity?count={count}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var activities = JsonSerializer.Deserialize<List<RecentActivityDto>>(json, _jsonOptions);
                    return activities ?? new List<RecentActivityDto>();
                }
                else
                {
                    Console.WriteLine($"Failed to get recent activity: {response.StatusCode}");
                    return new List<RecentActivityDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting recent activity: {ex.Message}");
                return new List<RecentActivityDto>();
            }
        }
    }
}