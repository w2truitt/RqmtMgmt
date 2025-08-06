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

        public DashboardController(IDashboardService dashboardService, IEnhancedDashboardService enhancedDashboardService)
        {
            _dashboardService = dashboardService;
            _enhancedDashboardService = enhancedDashboardService;
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
                var statistics = await _dashboardService.GetStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
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
                var statistics = await _enhancedDashboardService.GetDashboardStatsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
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
                var stats = await _enhancedDashboardService.GetRequirementStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
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
                var stats = await _enhancedDashboardService.GetTestManagementStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
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
                var stats = await _enhancedDashboardService.GetTestExecutionStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
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
                if (count <= 0 || count > 50) // Limit to reasonable range
                {
                    return BadRequest(new { message = "Count must be between 1 and 50." });
                }

                var activities = await _enhancedDashboardService.GetRecentActivityAsync(count);
                return Ok(activities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving recent activity.", error = ex.Message });
            }
        }
    }
}