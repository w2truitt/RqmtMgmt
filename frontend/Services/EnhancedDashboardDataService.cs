using System.Text.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    /// <summary>
    /// Frontend data service for enhanced dashboard operations providing advanced metrics and analytics.
    /// Implements IEnhancedDashboardService with HTTP client-based communication to the backend API and comprehensive error handling.
    /// </summary>
    public class EnhancedDashboardDataService : IEnhancedDashboardService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the EnhancedDashboardDataService with the specified HTTP client.
        /// Configures JSON serialization options for case-insensitive property matching and enum conversion.
        /// </summary>
        /// <param name="httpClient">The HTTP client for making API requests.</param>
        public EnhancedDashboardDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };
        }

        /// <summary>
        /// Retrieves comprehensive enhanced dashboard statistics from the backend API.
        /// Combines requirement, test management, and execution metrics with recent activities.
        /// </summary>
        /// <returns>DashboardStatsDto with complete system metrics, or empty statistics if the request fails.</returns>
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

        /// <summary>
        /// Retrieves detailed requirement statistics from the backend API with type and status breakdowns.
        /// Provides comprehensive requirement metrics including distributions and totals.
        /// </summary>
        /// <returns>RequirementStatsDto with detailed requirement metrics, or empty statistics if the request fails.</returns>
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

        /// <summary>
        /// Retrieves test management statistics from the backend API including coverage metrics.
        /// Provides comprehensive test organization metrics and coverage calculations.
        /// </summary>
        /// <returns>TestManagementStatsDto with test suite, plan, and coverage metrics, or empty statistics if the request fails.</returns>
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

        /// <summary>
        /// Retrieves test execution statistics from the backend API including pass rates and execution trends.
        /// Provides comprehensive execution metrics with results analysis and timing information.
        /// </summary>
        /// <returns>TestExecutionStatsDto with execution results and pass rate analysis, or empty statistics if the request fails.</returns>
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

        /// <summary>
        /// Retrieves recent system activities from the backend API for enhanced dashboard activity feed.
        /// Includes comprehensive activity tracking across requirements, test executions, and test run sessions.
        /// </summary>
        /// <param name="count">The maximum number of recent activities to return (default: 5).</param>
        /// <returns>A list of recent activities with enhanced detail, or an empty list if the request fails.</returns>
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