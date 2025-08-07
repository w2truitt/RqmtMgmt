using Microsoft.EntityFrameworkCore;
using RqmtMgmtShared;
using backend.Data;

namespace backend.Services
{
    /// <summary>
    /// Service implementation for dashboard statistics and recent activity tracking.
    /// Provides aggregated data for dashboard display including requirement stats, test metrics, and recent activities.
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly RqmtMgmtDbContext _context;

        /// <summary>
        /// Initializes a new instance of the DashboardService with the specified database context.
        /// </summary>
        /// <param name="context">The database context for dashboard data operations.</param>
        public DashboardService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves comprehensive dashboard statistics including requirement, test suite, test case, and test plan metrics.
        /// Aggregates data from multiple entities to provide a complete system overview.
        /// </summary>
        /// <returns>A DashboardStatisticsDto containing all system metrics and counts.</returns>
        public async Task<DashboardStatisticsDto> GetStatisticsAsync()
        {
            var statistics = new DashboardStatisticsDto();

            // Get requirement statistics grouped by status for detailed breakdown
            var requirementStats = await _context.Requirements
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            statistics.Requirements.Total = requirementStats.Sum(s => s.Count);
            statistics.Requirements.Approved = requirementStats
                .FirstOrDefault(s => s.Status == RequirementStatus.Approved)?.Count ?? 0;
            statistics.Requirements.Draft = requirementStats
                .FirstOrDefault(s => s.Status == RequirementStatus.Draft)?.Count ?? 0;
            statistics.Requirements.Implemented = requirementStats
                .FirstOrDefault(s => s.Status == RequirementStatus.Implemented)?.Count ?? 0;
            statistics.Requirements.Verified = requirementStats
                .FirstOrDefault(s => s.Status == RequirementStatus.Verified)?.Count ?? 0;

            // Get test suite statistics
            statistics.TestSuites.Total = await _context.TestSuites.CountAsync();
            // For now, we'll consider all test suites as "active" since we don't have status tracking
            // This can be enhanced later with proper status tracking
            statistics.TestSuites.Active = statistics.TestSuites.Total;
            statistics.TestSuites.Completed = 0; // Placeholder until we add status tracking

            // Get test case statistics
            statistics.TestCases.Total = await _context.TestCases.CountAsync();
            // Since we don't have test execution results yet, we'll use placeholders
            // These can be enhanced when test execution tracking is implemented
            statistics.TestCases.Passed = 0; // Placeholder
            statistics.TestCases.Failed = 0; // Placeholder
            statistics.TestCases.NotRun = statistics.TestCases.Total; // All are "not run" for now

            // Get test plan statistics
            statistics.TestPlans.Total = await _context.TestPlans.CountAsync();
            // Placeholder values until execution tracking is implemented
            statistics.TestPlans.ExecutionProgress = 0; // Percentage
            statistics.TestPlans.CoveragePercentage = 0; // Percentage

            return statistics;
        }

        /// <summary>
        /// Retrieves recent system activities across all entity types for dashboard activity feed.
        /// Combines activities from requirements, test cases, test suites, and test plans to provide a unified view.
        /// </summary>
        /// <param name="count">The maximum number of recent activities to return (default: 5).</param>
        /// <returns>A list of recent activities sorted by most recent first.</returns>
        public async Task<List<RecentActivityDto>> GetRecentActivityAsync(int count = 5)
        {
            var activities = new List<RecentActivityDto>();

            // Get recent requirements (created or updated) with creator information
            var recentRequirements = await _context.Requirements
                .Include(r => r.Creator)
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .Take(count * 2) // Get more to mix with other activities
                .Select(r => new RecentActivityDto
                {
                    Id = r.Id,
                    Description = $"Requirement '{r.Title}' {(r.UpdatedAt.HasValue ? "updated" : "created")}",
                    EntityType = "Requirement",
                    EntityId = r.Id,
                    Action = r.UpdatedAt.HasValue ? "Updated" : "Created",
                    UserId = r.CreatedBy,
                    UserName = r.Creator != null ? r.Creator.UserName : "Unknown User",
                    CreatedAt = r.UpdatedAt ?? r.CreatedAt
                })
                .ToListAsync();

            activities.AddRange(recentRequirements);

            // Get recent test cases with creator information
            var recentTestCases = await _context.TestCases
                .Include(tc => tc.Creator)
                .OrderByDescending(tc => tc.CreatedAt)
                .Take(count)
                .Select(tc => new RecentActivityDto
                {
                    Id = tc.Id,
                    Description = $"Test case '{tc.Title}' created",
                    EntityType = "TestCase",
                    EntityId = tc.Id,
                    Action = "Created",
                    UserId = tc.CreatedBy,
                    UserName = tc.Creator != null ? tc.Creator.UserName : "Unknown User",
                    CreatedAt = tc.CreatedAt
                })
                .ToListAsync();

            activities.AddRange(recentTestCases);

            // Get recent test suites with creator information
            var recentTestSuites = await _context.TestSuites
                .Include(ts => ts.Creator)
                .OrderByDescending(ts => ts.CreatedAt)
                .Take(count)
                .Select(ts => new RecentActivityDto
                {
                    Id = ts.Id,
                    Description = $"Test suite '{ts.Name}' created",
                    EntityType = "TestSuite",
                    EntityId = ts.Id,
                    Action = "Created",
                    UserId = ts.CreatedBy,
                    UserName = ts.Creator != null ? ts.Creator.UserName : "Unknown User",
                    CreatedAt = ts.CreatedAt
                })
                .ToListAsync();

            activities.AddRange(recentTestSuites);

            // Get recent test plans with creator information
            var recentTestPlans = await _context.TestPlans
                .Include(tp => tp.Creator)
                .OrderByDescending(tp => tp.CreatedAt)
                .Take(count)
                .Select(tp => new RecentActivityDto
                {
                    Id = tp.Id,
                    Description = $"Test plan '{tp.Name}' created",
                    EntityType = "TestPlan",
                    EntityId = tp.Id,
                    Action = "Created",
                    UserId = tp.CreatedBy,
                    UserName = tp.Creator != null ? tp.Creator.UserName : "Unknown User",
                    CreatedAt = tp.CreatedAt
                })
                .ToListAsync();

            activities.AddRange(recentTestPlans);

            // Sort by most recent and take the requested count
            return activities
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToList();
        }
    }
}