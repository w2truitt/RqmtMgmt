using System;

namespace RqmtMgmtShared
{
    /// <summary>
    /// Data transfer object for dashboard statistics
    /// </summary>
    public class DashboardStatisticsDto
    {
        public RequirementStatisticsDto Requirements { get; set; } = new();
        public TestSuiteStatisticsDto TestSuites { get; set; } = new();
        public TestCaseStatisticsDto TestCases { get; set; } = new();
        public TestPlanStatisticsDto TestPlans { get; set; } = new();
    }

    /// <summary>
    /// Statistics for requirements
    /// </summary>
    public class RequirementStatisticsDto
    {
        public int Total { get; set; }
        public int Approved { get; set; }
        public int Draft { get; set; }
        public int Implemented { get; set; }
        public int Verified { get; set; }
    }

    /// <summary>
    /// Statistics for test suites
    /// </summary>
    public class TestSuiteStatisticsDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Completed { get; set; }
    }

    /// <summary>
    /// Statistics for test cases
    /// </summary>
    public class TestCaseStatisticsDto
    {
        public int Total { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int NotRun { get; set; }
    }

    /// <summary>
    /// Statistics for test plans
    /// </summary>
    public class TestPlanStatisticsDto
    {
        public int Total { get; set; }
        public int ExecutionProgress { get; set; } // Percentage
        public int CoveragePercentage { get; set; }
    }

    /// <summary>
    /// Recent activity item for dashboard
    /// </summary>
    public class RecentActivityDto
    {
        public int Id { get; set; }
        public required string Description { get; set; }
        public required string EntityType { get; set; } // "Requirement", "TestCase", etc.
        public int EntityId { get; set; }
        public required string Action { get; set; } // "Created", "Updated", "Approved", etc.
        public int UserId { get; set; }
        public required string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TimeAgo => GetTimeAgo(CreatedAt);

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;
            
            if (timeSpan.TotalMinutes < 1)
                return "just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays} days ago";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} months ago";
            
            return $"{(int)(timeSpan.TotalDays / 365)} years ago";
        }
    }
}