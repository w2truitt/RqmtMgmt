using Microsoft.EntityFrameworkCore;
using RqmtMgmtShared;
using backend.Data;

namespace backend.Services
{
    /// <summary>
    /// Service implementation for dashboard statistics and recent activity tracking.
    /// Provides aggregated data for dashboard display including requirement stats, test metrics, and recent activities.
    /// ARCHITECTURAL FIX: Updated to implement consolidated IDashboardService interface.
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
        /// LEGACY METHOD: Maintained for backward compatibility.
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
        /// Retrieves comprehensive dashboard statistics combining all system metrics.
        /// ENHANCED METHOD: Provides comprehensive system metrics.
        /// </summary>
        /// <returns>A DashboardStatsDto containing all dashboard metrics and recent activities.</returns>
        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            // Execute all queries sequentially to avoid DbContext threading issues
            var requirements = await GetRequirementStatsAsync();
            var testManagement = await GetTestManagementStatsAsync();
            var testExecution = await GetTestExecutionStatsAsync();
            var recentActivities = await GetRecentActivityAsync(5);

            var dashboardStats = new DashboardStatsDto
            {
                Requirements = requirements,
                TestManagement = testManagement,
                TestExecution = testExecution,
                RecentActivities = recentActivities
            };

            return dashboardStats;
        }

        /// <summary>
        /// Retrieves detailed requirement statistics with optimized grouping queries.
        /// Provides comprehensive breakdowns by status and type with efficient single-query execution.
        /// </summary>
        /// <returns>RequirementStatsDto with detailed requirement metrics and distributions.</returns>
        public async Task<RequirementStatsDto> GetRequirementStatsAsync()
        {
            // Optimized single query for requirement statistics with grouping
            var requirementStats = await _context.Requirements
                .GroupBy(r => new { r.Status, r.Type })
                .Select(g => new { g.Key.Status, g.Key.Type, Count = g.Count() })
                .ToListAsync();

            var stats = new RequirementStatsDto
            {
                TotalRequirements = requirementStats.Sum(s => s.Count)
            };

            // Process status and type statistics
            PopulateStatusStatistics(stats, requirementStats);
            PopulateTypeStatistics(stats, requirementStats);

            return stats;
        }

        /// <summary>
        /// Populates requirement statistics grouped by status.
        /// </summary>
        /// <param name="stats">The stats object to populate.</param>
        /// <param name="requirementStats">The raw requirement statistics data.</param>
        private static void PopulateStatusStatistics(RequirementStatsDto stats, IEnumerable<object> requirementStats)
        {
            var statusGroups = requirementStats.Cast<dynamic>().GroupBy(s => (RequirementStatus)s.Status);
            foreach (var group in statusGroups)
            {
                var count = group.Sum(g => (int)g.Count);
                stats.ByStatus[group.Key] = count;
                MapStatusToProperty(stats, group.Key, count);
            }
        }

        /// <summary>
        /// Maps status counts to individual properties on the stats object.
        /// </summary>
        /// <param name="stats">The stats object to update.</param>
        /// <param name="status">The requirement status.</param>
        /// <param name="count">The count for this status.</param>
        private static void MapStatusToProperty(RequirementStatsDto stats, RequirementStatus status, int count)
        {
            switch (status)
            {
                case RequirementStatus.Draft:
                    stats.DraftRequirements = count;
                    break;
                case RequirementStatus.Approved:
                    stats.ApprovedRequirements = count;
                    break;
                case RequirementStatus.Implemented:
                    stats.ImplementedRequirements = count;
                    break;
                case RequirementStatus.Verified:
                    stats.VerifiedRequirements = count;
                    break;
            }
        }

        /// <summary>
        /// Populates requirement statistics grouped by type.
        /// </summary>
        /// <param name="stats">The stats object to populate.</param>
        /// <param name="requirementStats">The raw requirement statistics data.</param>
        private static void PopulateTypeStatistics(RequirementStatsDto stats, IEnumerable<object> requirementStats)
        {
            var typeGroups = requirementStats.Cast<dynamic>().GroupBy(s => (RequirementType)s.Type);
            foreach (var group in typeGroups)
            {
                stats.ByType[group.Key] = group.Sum(g => (int)g.Count);
            }
        }

        /// <summary>
        /// Retrieves test management statistics including coverage calculations.
        /// Executes sequential queries to gather comprehensive test organization metrics.
        /// </summary>
        /// <returns>TestManagementStatsDto with test suite, plan, case metrics and coverage percentage.</returns>
        public async Task<TestManagementStatsDto> GetTestManagementStatsAsync()
        {
            // Execute queries sequentially to avoid DbContext threading issues
            var testSuiteCount = await _context.TestSuites.CountAsync();
            var testPlanCount = await _context.TestPlans.CountAsync();
            var testCaseCount = await _context.TestCases.CountAsync();
            var testCasesWithSteps = await _context.TestCases
                .Where(tc => tc.Steps.Any())
                .CountAsync();
            var requirementTestCaseLinks = await _context.RequirementTestCaseLinks.CountAsync();
            var totalRequirements = await _context.Requirements.CountAsync();

            var stats = new TestManagementStatsDto
            {
                TotalTestSuites = testSuiteCount,
                TotalTestPlans = testPlanCount,
                TotalTestCases = testCaseCount,
                TestCasesWithSteps = testCasesWithSteps,
                RequirementTestCaseLinks = requirementTestCaseLinks,
                // Calculate test coverage percentage based on requirement-test case links
                TestCoveragePercentage = totalRequirements > 0 
                    ? Math.Round((double)requirementTestCaseLinks / totalRequirements * 100, 2)
                    : 0
            };

            return stats;
        }

        /// <summary>
        /// Retrieves test execution statistics with optimized grouping and pass rate calculations.
        /// Provides comprehensive execution metrics including pass rates and execution trends.
        /// </summary>
        /// <returns>TestExecutionStatsDto with execution results, pass rates, and timing information.</returns>
        public async Task<TestExecutionStatsDto> GetTestExecutionStatsAsync()
        {
            // Optimized queries for test execution statistics with grouping
            var testRunSessionStats = await _context.TestRunSessions
                .GroupBy(trs => trs.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var testCaseExecutionStats = await _context.TestCaseExecutions
                .GroupBy(tce => tce.OverallResult)
                .Select(g => new { Result = g.Key, Count = g.Count() })
                .ToListAsync();

            var lastExecutionDate = await _context.TestCaseExecutions
                .Where(tce => tce.ExecutedAt.HasValue)
                .OrderByDescending(tce => tce.ExecutedAt)
                .Select(tce => tce.ExecutedAt)
                .FirstOrDefaultAsync();

            // Calculate session statistics
            var totalTestRuns = testRunSessionStats.Sum(s => s.Count);
            var activeTestRuns = testRunSessionStats
                .Where(s => s.Status == TestRunStatus.InProgress || s.Status == TestRunStatus.Paused)
                .Sum(s => s.Count);
            var completedTestRuns = testRunSessionStats
                .Where(s => s.Status == TestRunStatus.Completed)
                .Sum(s => s.Count);

            // Calculate execution statistics
            var totalExecutions = testCaseExecutionStats.Sum(s => s.Count);
            var passedExecutions = testCaseExecutionStats
                .Where(s => s.Result == TestResult.Passed)
                .Sum(s => s.Count);
            var failedExecutions = testCaseExecutionStats
                .Where(s => s.Result == TestResult.Failed)
                .Sum(s => s.Count);
            var blockedExecutions = testCaseExecutionStats
                .Where(s => s.Result == TestResult.Blocked)
                .Sum(s => s.Count);
            var notRunExecutions = testCaseExecutionStats
                .Where(s => s.Result == TestResult.NotRun)
                .Sum(s => s.Count);

            var stats = new TestExecutionStatsDto
            {
                TotalTestRuns = totalTestRuns,
                ActiveTestRuns = activeTestRuns,
                CompletedTestRuns = completedTestRuns,
                TotalTestCaseExecutions = totalExecutions,
                PassedExecutions = passedExecutions,
                FailedExecutions = failedExecutions,
                BlockedExecutions = blockedExecutions,
                NotRunExecutions = notRunExecutions,
                // Calculate pass rate percentage
                PassRate = totalExecutions > 0 
                    ? Math.Round((double)passedExecutions / totalExecutions * 100, 2)
                    : 0,
                LastExecutionDate = lastExecutionDate
            };

            return stats;
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