using Microsoft.AspNetCore.Mvc;
using RqmtMgmtShared;

namespace backend.Controllers
{
    /// <summary>
    /// API controller for dashboard operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IEnhancedDashboardService _enhancedDashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, IEnhancedDashboardService enhancedDashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _enhancedDashboardService = enhancedDashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        /// <returns>Dashboard statistics including counts for requirements, test cases, etc.</returns>
        [HttpGet("statistics")]
        public async Task<ActionResult<DashboardStatisticsDto>> GetStatistics()
        {
            try
            {
                _logger.LogInformation("Getting dashboard statistics");
                var statistics = await _dashboardService.GetStatisticsAsync();
                _logger.LogInformation("Successfully retrieved dashboard statistics");
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard statistics");
                return StatusCode(500, new { message = "An error occurred while retrieving dashboard statistics.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get enhanced dashboard statistics with optimized queries
        /// </summary>
        /// <returns>Enhanced dashboard statistics including all metrics</returns>
        [HttpGet("enhanced-statistics")]
        public async Task<ActionResult<DashboardStatsDto>> GetEnhancedStatistics()
        {
            try
            {
                _logger.LogInformation("Getting enhanced dashboard statistics");
                var statistics = await _enhancedDashboardService.GetDashboardStatsAsync();
                _logger.LogInformation("Successfully retrieved enhanced dashboard statistics: {@Statistics}", statistics);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving enhanced dashboard statistics");
                return StatusCode(500, new { message = "An error occurred while retrieving enhanced dashboard statistics.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get requirement statistics breakdown
        /// </summary>
        /// <returns>Detailed requirement statistics</returns>
        [HttpGet("requirements-stats")]
        public async Task<ActionResult<RequirementStatsDto>> GetRequirementStats()
        {
            try
            {
                _logger.LogInformation("Getting requirement statistics");
                var stats = await _enhancedDashboardService.GetRequirementStatsAsync();
                _logger.LogInformation("Successfully retrieved requirement statistics");
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving requirement statistics");
                return StatusCode(500, new { message = "An error occurred while retrieving requirement statistics.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get test management statistics
        /// </summary>
        /// <returns>Test management statistics</returns>
        [HttpGet("test-management-stats")]
        public async Task<ActionResult<TestManagementStatsDto>> GetTestManagementStats()
        {
            try
            {
                _logger.LogInformation("Getting test management statistics");
                var stats = await _enhancedDashboardService.GetTestManagementStatsAsync();
                _logger.LogInformation("Successfully retrieved test management statistics");
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving test management statistics");
                return StatusCode(500, new { message = "An error occurred while retrieving test management statistics.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get test execution statistics
        /// </summary>
        /// <returns>Test execution statistics</returns>
        [HttpGet("test-execution-stats")]
        public async Task<ActionResult<TestExecutionStatsDto>> GetTestExecutionStats()
        {
            try
            {
                _logger.LogInformation("Getting test execution statistics");
                var stats = await _enhancedDashboardService.GetTestExecutionStatsAsync();
                _logger.LogInformation("Successfully retrieved test execution statistics");
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving test execution statistics");
                return StatusCode(500, new { message = "An error occurred while retrieving test execution statistics.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get recent activity for dashboard
        /// </summary>
        /// <param name="count">Number of recent activities to return (default: 5)</param>
        /// <returns>List of recent activities</returns>
        [HttpGet("recent-activity")]
        public async Task<ActionResult<List<RecentActivityDto>>> GetRecentActivity([FromQuery] int count = 5)
        {
            try
            {
                _logger.LogInformation("Getting recent activity with count: {Count}", count);
                
                if (count <= 0 || count > 50) // Limit to reasonable range
                {
                    _logger.LogWarning("Invalid count requested: {Count}", count);
                    return BadRequest(new { message = "Count must be between 1 and 50." });
                }

                var activities = await _enhancedDashboardService.GetRecentActivityAsync(count);
                _logger.LogInformation("Successfully retrieved {Count} recent activities", activities.Count);
                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent activity");
                return StatusCode(500, new { message = "An error occurred while retrieving recent activity.", error = ex.Message });
            }
        }
    }
}