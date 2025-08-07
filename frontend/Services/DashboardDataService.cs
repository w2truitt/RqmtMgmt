using System.Text.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    /// <summary>
    /// Frontend data service for dashboard operations providing system statistics and activity feeds.
    /// Implements IDashboardService with HTTP client-based communication to the backend API and comprehensive error handling.
    /// </summary>
    public class DashboardDataService : IDashboardService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the DashboardDataService with the specified HTTP client.
        /// Configures JSON serialization options for case-insensitive property matching and enum conversion.
        /// </summary>
        /// <param name="httpClient">The HTTP client for making API requests.</param>
        public DashboardDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };
        }

        /// <summary>
        /// Retrieves dashboard statistics from the backend API including requirement, test case, test suite, and test plan metrics.
        /// Provides comprehensive error handling and returns empty statistics on failure.
        /// </summary>
        /// <returns>Dashboard statistics with system counts and breakdowns, or empty statistics if the request fails.</returns>
        public async Task<DashboardStatisticsDto> GetStatisticsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/dashboard/statistics");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var statistics = JsonSerializer.Deserialize<DashboardStatisticsDto>(json, _jsonOptions);
                    return statistics ?? new DashboardStatisticsDto();
                }
                else
                {
                    // Log error or handle specific status codes
                    Console.WriteLine($"Failed to get dashboard statistics: {response.StatusCode}");
                    return new DashboardStatisticsDto(); // Return empty statistics
                }
            }
            catch (Exception ex)
            {
                // Log error and return fallback data
                Console.WriteLine($"Error getting dashboard statistics: {ex.Message}");
                return new DashboardStatisticsDto(); // Return empty statistics
            }
        }

        /// <summary>
        /// Retrieves recent system activities from the backend API for the dashboard activity feed.
        /// Includes activities from requirements, test cases, test suites, and test plans with error handling.
        /// </summary>
        /// <param name="count">The maximum number of recent activities to return (default: 5).</param>
        /// <returns>A list of recent activities sorted by most recent first, or an empty list if the request fails.</returns>
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
                    // Log error or handle specific status codes
                    Console.WriteLine($"Failed to get recent activity: {response.StatusCode}");
                    return new List<RecentActivityDto>(); // Return empty list
                }
            }
            catch (Exception ex)
            {
                // Log error and return fallback data
                Console.WriteLine($"Error getting recent activity: {ex.Message}");
                return new List<RecentActivityDto>(); // Return empty list
            }
        }
    }
}