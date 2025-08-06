using RqmtMgmtShared;

namespace backend.Models
{
    /// <summary>
    /// Represents a test run session that contains multiple test case executions.
    /// </summary>
    public class TestRunSession
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
        /// Gets or sets the description of the test run session.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the test plan ID associated with this run.
        /// </summary>
        public int TestPlanId { get; set; }

        /// <summary>
        /// Gets or sets the user ID who is executing the test run.
        /// </summary>
        public int ExecutedBy { get; set; }

        /// <summary>
        /// Gets or sets when the test run session was started.
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// Gets or sets when the test run session was completed (null if still in progress).
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Gets or sets the current status of the test run session.
        /// </summary>
        public TestRunStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the environment where tests are being executed.
        /// </summary>
        public string? Environment { get; set; }

        /// <summary>
        /// Gets or sets the build version being tested.
        /// </summary>
        public string? BuildVersion { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the test plan.
        /// </summary>
        public TestPlan? TestPlan { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the user executing the run.
        /// </summary>
        public User? Executor { get; set; }

        /// <summary>
        /// Gets or sets the collection of test case executions in this run.
        /// </summary>
        public List<TestCaseExecution> TestCaseExecutions { get; set; } = new List<TestCaseExecution>();
    }
}