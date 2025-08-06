using Microsoft.EntityFrameworkCore;
using RqmtMgmtShared;
using backend.Data;

namespace backend.Services
{
    /// <summary>
    /// Enhanced dashboard service with optimized statistics queries
    /// </summary>
    public class EnhancedDashboardService : IEnhancedDashboardService
    {
        private readonly RqmtMgmtDbContext _context;

        public EnhancedDashboardService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

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

        public async Task<RequirementStatsDto> GetRequirementStatsAsync()
        {
            // Optimized single query for requirement statistics
            var requirementStats = await _context.Requirements
                .GroupBy(r => new { r.Status, r.Type })
                .Select(g => new { g.Key.Status, g.Key.Type, Count = g.Count() })
                .ToListAsync();

            var stats = new RequirementStatsDto
            {
                TotalRequirements = requirementStats.Sum(s => s.Count)
            };

            // Group by status
            var statusGroups = requirementStats.GroupBy(s => s.Status);
            foreach (var group in statusGroups)
            {
                var count = group.Sum(g => g.Count);
                stats.ByStatus[group.Key] = count;

                switch (group.Key)
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

            // Group by type
            var typeGroups = requirementStats.GroupBy(s => s.Type);
            foreach (var group in typeGroups)
            {
                stats.ByType[group.Key] = group.Sum(g => g.Count);
            }

            return stats;
        }

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
                TestCoveragePercentage = totalRequirements > 0 
                    ? Math.Round((double)requirementTestCaseLinks / totalRequirements * 100, 2)
                    : 0
            };

            return stats;
        }

        public async Task<TestExecutionStatsDto> GetTestExecutionStatsAsync()
        {
            // Optimized queries for test execution statistics
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

            var totalTestRuns = testRunSessionStats.Sum(s => s.Count);
            var activeTestRuns = testRunSessionStats
                .Where(s => s.Status == TestRunStatus.InProgress || s.Status == TestRunStatus.Paused)
                .Sum(s => s.Count);
            var completedTestRuns = testRunSessionStats
                .Where(s => s.Status == TestRunStatus.Completed)
                .Sum(s => s.Count);

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
                PassRate = totalExecutions > 0 
                    ? Math.Round((double)passedExecutions / totalExecutions * 100, 2)
                    : 0,
                LastExecutionDate = lastExecutionDate
            };

            return stats;
        }

        public async Task<List<RecentActivityDto>> GetRecentActivityAsync(int count = 5)
        {
            var activities = new List<RecentActivityDto>();

            // Get recent requirements (created or updated)
            var recentRequirements = await _context.Requirements
                .Include(r => r.Creator)
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .Take(count)
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

            // Get recent test case executions
            var recentExecutions = await _context.TestCaseExecutions
                .Include(tce => tce.TestCase)
                .Include(tce => tce.Executor)
                .Where(tce => tce.ExecutedAt.HasValue)
                .OrderByDescending(tce => tce.ExecutedAt)
                .Take(count)
                .Select(tce => new RecentActivityDto
                {
                    Id = tce.Id,
                    Description = $"Test case '{tce.TestCase!.Title}' executed with result: {tce.OverallResult}",
                    EntityType = "TestCaseExecution",
                    EntityId = tce.TestCaseId,
                    Action = "Executed",
                    UserId = tce.ExecutedBy ?? 0,
                    UserName = tce.Executor != null ? tce.Executor.UserName : "Unknown User",
                    CreatedAt = tce.ExecutedAt!.Value
                })
                .ToListAsync();

            activities.AddRange(recentExecutions);

            // Get recent test run sessions
            var recentTestRuns = await _context.TestRunSessions
                .Include(trs => trs.Executor)
                .Include(trs => trs.TestPlan)
                .OrderByDescending(trs => trs.StartedAt)
                .Take(count)
                .Select(trs => new RecentActivityDto
                {
                    Id = trs.Id,
                    Description = $"Test run '{trs.Name}' {trs.Status.ToString().ToLower()}",
                    EntityType = "TestRunSession",
                    EntityId = trs.Id,
                    Action = trs.Status.ToString(),
                    UserId = trs.ExecutedBy,
                    UserName = trs.Executor != null ? trs.Executor.UserName : "Unknown User",
                    CreatedAt = trs.CompletedAt ?? trs.StartedAt
                })
                .ToListAsync();

            activities.AddRange(recentTestRuns);

            // Sort by most recent and take the requested count
            return activities
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToList();
        }
    }
}