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

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
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

                var activities = await _dashboardService.GetRecentActivityAsync(count);
                return Ok(activities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving recent activity.", error = ex.Message });
            }
        }
    }
}