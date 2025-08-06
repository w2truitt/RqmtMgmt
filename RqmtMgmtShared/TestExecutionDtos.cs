using RqmtMgmtShared;

namespace RqmtMgmtShared
{
    /// <summary>
    /// DTO for test run session management.
    /// </summary>
    public class TestRunSessionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TestPlanId { get; set; }
        public int ExecutedBy { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TestRunStatus Status { get; set; }
        public string? Environment { get; set; }
        public string? BuildVersion { get; set; }
        public string? TestPlanName { get; set; }
        public string? ExecutorName { get; set; }
        public List<TestCaseExecutionDto> TestCaseExecutions { get; set; } = new List<TestCaseExecutionDto>();
    }

    /// <summary>
    /// DTO for test case execution results.
    /// </summary>
    public class TestCaseExecutionDto
    {
        public int Id { get; set; }
        public int TestRunSessionId { get; set; }
        public int TestCaseId { get; set; }
        public TestResult OverallResult { get; set; }
        public DateTime? ExecutedAt { get; set; }
        public int? ExecutedBy { get; set; }
        public string? Notes { get; set; }
        public string? DefectId { get; set; }
        public string? TestCaseTitle { get; set; }
        public string? ExecutorName { get; set; }
        public List<TestStepExecutionDto> TestStepExecutions { get; set; } = new List<TestStepExecutionDto>();
    }

    /// <summary>
    /// DTO for individual test step execution results.
    /// </summary>
    public class TestStepExecutionDto
    {
        public int Id { get; set; }
        public int TestCaseExecutionId { get; set; }
        public int TestStepId { get; set; }
        public int StepOrder { get; set; }
        public TestResult Result { get; set; }
        public string? ActualResult { get; set; }
        public string? Notes { get; set; }
        public DateTime? ExecutedAt { get; set; }
        public string? StepDescription { get; set; }
        public string? ExpectedResult { get; set; }
    }

    /// <summary>
    /// DTO for aggregated dashboard statistics.
    /// </summary>
    public class DashboardStatsDto
    {
        public RequirementStatsDto Requirements { get; set; } = new RequirementStatsDto();
        public TestManagementStatsDto TestManagement { get; set; } = new TestManagementStatsDto();
        public TestExecutionStatsDto TestExecution { get; set; } = new TestExecutionStatsDto();
        public List<RecentActivityDto> RecentActivities { get; set; } = new List<RecentActivityDto>();
    }

    /// <summary>
    /// DTO for requirements statistics breakdown.
    /// </summary>
    public class RequirementStatsDto
    {
        public int TotalRequirements { get; set; }
        public int DraftRequirements { get; set; }
        public int ApprovedRequirements { get; set; }
        public int ImplementedRequirements { get; set; }
        public int VerifiedRequirements { get; set; }
        public Dictionary<RequirementType, int> ByType { get; set; } = new Dictionary<RequirementType, int>();
        public Dictionary<RequirementStatus, int> ByStatus { get; set; } = new Dictionary<RequirementStatus, int>();
    }

    /// <summary>
    /// DTO for test management statistics.
    /// </summary>
    public class TestManagementStatsDto
    {
        public int TotalTestSuites { get; set; }
        public int TotalTestPlans { get; set; }
        public int TotalTestCases { get; set; }
        public int TestCasesWithSteps { get; set; }
        public int RequirementTestCaseLinks { get; set; }
        public double TestCoveragePercentage { get; set; }
    }

    /// <summary>
    /// DTO for test execution statistics.
    /// </summary>
    public class TestExecutionStatsDto
    {
        public int TotalTestRuns { get; set; }
        public int ActiveTestRuns { get; set; }
        public int CompletedTestRuns { get; set; }
        public int TotalTestCaseExecutions { get; set; }
        public int PassedExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public int BlockedExecutions { get; set; }
        public int NotRunExecutions { get; set; }
        public double PassRate { get; set; }
        public DateTime? LastExecutionDate { get; set; }
    }
}