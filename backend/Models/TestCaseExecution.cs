using RqmtMgmtShared;

namespace backend.Models
{
    /// <summary>
    /// Represents the execution of a test case within a test run session.
    /// </summary>
    public class TestCaseExecution
    {
        /// <summary>
        /// Gets or sets the unique identifier for the test case execution.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the test run session ID this execution belongs to.
        /// </summary>
        public int TestRunSessionId { get; set; }

        /// <summary>
        /// Gets or sets the test case ID being executed.
        /// </summary>
        public int TestCaseId { get; set; }

        /// <summary>
        /// Gets or sets the overall result of the test case execution.
        /// </summary>
        public TestResult OverallResult { get; set; }

        /// <summary>
        /// Gets or sets when this test case was executed.
        /// </summary>
        public DateTime? ExecutedAt { get; set; }

        /// <summary>
        /// Gets or sets the user ID who executed this test case.
        /// </summary>
        public int? ExecutedBy { get; set; }

        /// <summary>
        /// Gets or sets notes recorded during the execution.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets the defect ID if a defect was found.
        /// </summary>
        public string? DefectId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the test run session.
        /// </summary>
        public TestRunSession? TestRunSession { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the test case.
        /// </summary>
        public TestCase? TestCase { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the user who executed this test case.
        /// </summary>
        public User? Executor { get; set; }

        /// <summary>
        /// Gets or sets the collection of test step executions for this test case.
        /// </summary>
        public List<TestStepExecution> TestStepExecutions { get; set; } = new List<TestStepExecution>();
    }
}