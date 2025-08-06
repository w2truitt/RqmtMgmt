using System.Text.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    /// <summary>
    /// Frontend service for dashboard data operations
    /// </summary>
    public class DashboardDataService : IDashboardService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public DashboardDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };
        }

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
                // Log error
                Console.WriteLine($"Error getting dashboard statistics: {ex.Message}");
                return new DashboardStatisticsDto(); // Return empty statistics
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
                    // Log error or handle specific status codes
                    Console.WriteLine($"Failed to get recent activity: {response.StatusCode}");
                    return new List<RecentActivityDto>(); // Return empty list
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error getting recent activity: {ex.Message}");
                return new List<RecentActivityDto>(); // Return empty list
            }
        }
    }
}