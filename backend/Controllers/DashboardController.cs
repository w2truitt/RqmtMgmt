using Microsoft.AspNetCore.Mvc;
using RqmtMgmtShared;

namespace backend.Controllers
{
    /// <summary>
    /// API controller for dashboard operations providing system statistics and activity feeds.
    /// Offers both basic and enhanced dashboard endpoints with comprehensive error handling and logging.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IEnhancedDashboardService _enhancedDashboardService;
        private readonly ILogger<DashboardController> _logger;

        /// <summary>
        /// Initializes a new instance of the DashboardController with the specified services.
        /// </summary>
        /// <param name="dashboardService">The basic dashboard service for statistics.</param>
        /// <param name="enhancedDashboardService">The enhanced dashboard service with advanced metrics.</param>
        /// <param name="logger">The logger for tracking dashboard operations.</param>
        public DashboardController(IDashboardService dashboardService, IEnhancedDashboardService enhancedDashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _enhancedDashboardService = enhancedDashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves basic dashboard statistics including requirement, test case, test suite, and test plan counts.
        /// </summary>
        /// <returns>Dashboard statistics with counts and status breakdowns.</returns>
        /// <response code="200">Returns the dashboard statistics.</response>
        /// <response code="500">If an error occurs while retrieving statistics.</response>
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
        /// Retrieves enhanced dashboard statistics with optimized queries and comprehensive metrics.
        /// Provides more detailed breakdowns and performance-optimized data aggregation.
        /// </summary>
        /// <returns>Enhanced dashboard statistics including all system metrics.</returns>
        /// <response code="200">Returns the enhanced dashboard statistics.</response>
        /// <response code="500">If an error occurs while retrieving enhanced statistics.</response>
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
        /// Retrieves detailed requirement statistics breakdown by type and status.
        /// </summary>
        /// <returns>Comprehensive requirement statistics with type and status distributions.</returns>
        /// <response code="200">Returns the requirement statistics.</response>
        /// <response code="500">If an error occurs while retrieving requirement statistics.</response>
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
        /// Retrieves test management statistics including test suite, plan, and case metrics.
        /// </summary>
        /// <returns>Test management statistics with coverage and organization metrics.</returns>
        /// <response code="200">Returns the test management statistics.</response>
        /// <response code="500">If an error occurs while retrieving test management statistics.</response>
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
        /// Retrieves test execution statistics including pass rates and execution metrics.
        /// </summary>
        /// <returns>Test execution statistics with results and performance data.</returns>
        /// <response code="200">Returns the test execution statistics.</response>
        /// <response code="500">If an error occurs while retrieving test execution statistics.</response>
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
        /// Retrieves recent system activities for the dashboard activity feed.
        /// </summary>
        /// <param name="count">Number of recent activities to return (default: 5, max: 50).</param>
        /// <returns>List of recent activities across all entity types.</returns>
        /// <response code="200">Returns the list of recent activities.</response>
        /// <response code="400">If the count parameter is invalid (not between 1 and 50).</response>
        /// <response code="500">If an error occurs while retrieving recent activity.</response>
        [HttpGet("recent-activity")]
        public async Task<ActionResult<List<RecentActivityDto>>> GetRecentActivity([FromQuery] int count = 5)
        {
            try
            {
                _logger.LogInformation("Getting recent activity with count: {Count}", count);
                
                // Validate count parameter to prevent excessive data requests
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