using RqmtMgmtShared;

namespace RqmtMgmtShared
{
    /// <summary>
    /// Data transfer object for test run session management and tracking.
    /// Represents a complete test execution session containing multiple test case executions.
    /// </summary>
    public class TestRunSessionDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the test run session.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the test run session.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the detailed description of the test run session. Can be null.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the ID of the test plan being executed in this session.
        /// </summary>
        public int TestPlanId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user executing the test session.
        /// </summary>
        public int ExecutedBy { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the test session was started.
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the test session was completed. Null if still in progress.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Gets or sets the current status of the test run session.
        /// </summary>
        public TestRunStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the environment where the tests are being executed. Can be null.
        /// </summary>
        public string? Environment { get; set; }

        /// <summary>
        /// Gets or sets the build version being tested. Can be null.
        /// </summary>
        public string? BuildVersion { get; set; }

        /// <summary>
        /// Gets or sets the name of the test plan for display purposes. Can be null.
        /// </summary>
        public string? TestPlanName { get; set; }

        /// <summary>
        /// Gets or sets the name of the executor for display purposes. Can be null.
        /// </summary>
        public string? ExecutorName { get; set; }

        /// <summary>
        /// Gets or sets the collection of test case executions within this session.
        /// </summary>
        public List<TestCaseExecutionDto> TestCaseExecutions { get; set; } = new List<TestCaseExecutionDto>();
    }

    /// <summary>
    /// Data transfer object for individual test case execution results within a test run session.
    /// Tracks the execution status and results of a single test case.
    /// </summary>
    public class TestCaseExecutionDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the test case execution.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the test run session this execution belongs to.
        /// </summary>
        public int TestRunSessionId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the test case being executed.
        /// </summary>
        public int TestCaseId { get; set; }

        /// <summary>
        /// Gets or sets the overall result of the test case execution.
        /// </summary>
        public TestResult OverallResult { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the test case was executed. Null if not yet executed.
        /// </summary>
        public DateTime? ExecutedAt { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who executed the test case. Null if not yet executed.
        /// </summary>
        public int? ExecutedBy { get; set; }

        /// <summary>
        /// Gets or sets any notes or comments about the test execution. Can be null.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets the defect ID if the test case failed. Can be null.
        /// </summary>
        public string? DefectId { get; set; }

        /// <summary>
        /// Gets or sets the title of the test case for display purposes. Can be null.
        /// </summary>
        public string? TestCaseTitle { get; set; }

        /// <summary>
        /// Gets or sets the name of the executor for display purposes. Can be null.
        /// </summary>
        public string? ExecutorName { get; set; }

        /// <summary>
        /// Gets or sets the collection of individual test step executions within this test case.
        /// </summary>
        public List<TestStepExecutionDto> TestStepExecutions { get; set; } = new List<TestStepExecutionDto>();
    }

    /// <summary>
    /// Data transfer object for individual test step execution results within a test case execution.
    /// Tracks the detailed results of each step in a test case.
    /// </summary>
    public class TestStepExecutionDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the test step execution.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the test case execution this step belongs to.
        /// </summary>
        public int TestCaseExecutionId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the test step being executed.
        /// </summary>
        public int TestStepId { get; set; }

        /// <summary>
        /// Gets or sets the order of this step within the test case.
        /// </summary>
        public int StepOrder { get; set; }

        /// <summary>
        /// Gets or sets the result of executing this test step.
        /// </summary>
        public TestResult Result { get; set; }

        /// <summary>
        /// Gets or sets the actual result observed during step execution. Can be null.
        /// </summary>
        public string? ActualResult { get; set; }

        /// <summary>
        /// Gets or sets any notes or comments about the step execution. Can be null.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the step was executed. Null if not yet executed.
        /// </summary>
        public DateTime? ExecutedAt { get; set; }

        /// <summary>
        /// Gets or sets the description of the test step for display purposes. Can be null.
        /// </summary>
        public string? StepDescription { get; set; }

        /// <summary>
        /// Gets or sets the expected result of the test step for display purposes. Can be null.
        /// </summary>
        public string? ExpectedResult { get; set; }
    }

    /// <summary>
    /// Data transfer object for aggregated dashboard statistics.
    /// Provides a comprehensive overview of system metrics for dashboard display.
    /// </summary>
    public class DashboardStatsDto
    {
        /// <summary>
        /// Gets or sets the requirements-related statistics.
        /// </summary>
        public RequirementStatsDto Requirements { get; set; } = new RequirementStatsDto();

        /// <summary>
        /// Gets or sets the test management-related statistics.
        /// </summary>
        public TestManagementStatsDto TestManagement { get; set; } = new TestManagementStatsDto();

        /// <summary>
        /// Gets or sets the test execution-related statistics.
        /// </summary>
        public TestExecutionStatsDto TestExecution { get; set; } = new TestExecutionStatsDto();

        /// <summary>
        /// Gets or sets the collection of recent system activities.
        /// </summary>
        public List<RecentActivityDto> RecentActivities { get; set; } = new List<RecentActivityDto>();
    }

    /// <summary>
    /// Data transfer object for requirements statistics breakdown.
    /// Provides detailed metrics about requirements status and types.
    /// </summary>
    public class RequirementStatsDto
    {
        /// <summary>
        /// Gets or sets the total number of requirements in the system.
        /// </summary>
        public int TotalRequirements { get; set; }

        /// <summary>
        /// Gets or sets the number of requirements in draft status.
        /// </summary>
        public int DraftRequirements { get; set; }

        /// <summary>
        /// Gets or sets the number of approved requirements.
        /// </summary>
        public int ApprovedRequirements { get; set; }

        /// <summary>
        /// Gets or sets the number of implemented requirements.
        /// </summary>
        public int ImplementedRequirements { get; set; }

        /// <summary>
        /// Gets or sets the number of verified requirements.
        /// </summary>
        public int VerifiedRequirements { get; set; }

        /// <summary>
        /// Gets or sets the breakdown of requirements by type.
        /// </summary>
        public Dictionary<RequirementType, int> ByType { get; set; } = new Dictionary<RequirementType, int>();

        /// <summary>
        /// Gets or sets the breakdown of requirements by status.
        /// </summary>
        public Dictionary<RequirementStatus, int> ByStatus { get; set; } = new Dictionary<RequirementStatus, int>();
    }

    /// <summary>
    /// Data transfer object for test management statistics.
    /// Provides metrics about test artifacts and coverage.
    /// </summary>
    public class TestManagementStatsDto
    {
        /// <summary>
        /// Gets or sets the total number of test suites in the system.
        /// </summary>
        public int TotalTestSuites { get; set; }

        /// <summary>
        /// Gets or sets the total number of test plans in the system.
        /// </summary>
        public int TotalTestPlans { get; set; }

        /// <summary>
        /// Gets or sets the total number of test cases in the system.
        /// </summary>
        public int TotalTestCases { get; set; }

        /// <summary>
        /// Gets or sets the number of test cases that have defined test steps.
        /// </summary>
        public int TestCasesWithSteps { get; set; }

        /// <summary>
        /// Gets or sets the number of requirement-to-test case links in the system.
        /// </summary>
        public int RequirementTestCaseLinks { get; set; }

        /// <summary>
        /// Gets or sets the percentage of requirements covered by test cases.
        /// </summary>
        public double TestCoveragePercentage { get; set; }
    }

    /// <summary>
    /// Data transfer object for test execution statistics.
    /// Provides metrics about test execution results and performance.
    /// </summary>
    public class TestExecutionStatsDto
    {
        /// <summary>
        /// Gets or sets the total number of test runs in the system.
        /// </summary>
        public int TotalTestRuns { get; set; }

        /// <summary>
        /// Gets or sets the number of currently active test runs.
        /// </summary>
        public int ActiveTestRuns { get; set; }

        /// <summary>
        /// Gets or sets the number of completed test runs.
        /// </summary>
        public int CompletedTestRuns { get; set; }

        /// <summary>
        /// Gets or sets the total number of test case executions across all runs.
        /// </summary>
        public int TotalTestCaseExecutions { get; set; }

        /// <summary>
        /// Gets or sets the number of test case executions that passed.
        /// </summary>
        public int PassedExecutions { get; set; }

        /// <summary>
        /// Gets or sets the number of test case executions that failed.
        /// </summary>
        public int FailedExecutions { get; set; }

        /// <summary>
        /// Gets or sets the number of test case executions that were blocked.
        /// </summary>
        public int BlockedExecutions { get; set; }

        /// <summary>
        /// Gets or sets the number of test case executions that were not run.
        /// </summary>
        public int NotRunExecutions { get; set; }

        /// <summary>
        /// Gets or sets the overall pass rate as a percentage.
        /// </summary>
        public double PassRate { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the most recent test execution. Null if no executions exist.
        /// </summary>
        public DateTime? LastExecutionDate { get; set; }
    }
}