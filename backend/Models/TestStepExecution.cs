using RqmtMgmtShared;

namespace backend.Models
{
    /// <summary>
    /// Represents the execution of an individual test step within a test case execution.
    /// </summary>
    public class TestStepExecution
    {
        /// <summary>
        /// Gets or sets the unique identifier for the test step execution.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the test case execution ID this step belongs to.
        /// </summary>
        public int TestCaseExecutionId { get; set; }

        /// <summary>
        /// Gets or sets the test step ID being executed.
        /// </summary>
        public int TestStepId { get; set; }

        /// <summary>
        /// Gets or sets the order of this step within the test case.
        /// </summary>
        public int StepOrder { get; set; }

        /// <summary>
        /// Gets or sets the result of this step execution.
        /// </summary>
        public TestResult Result { get; set; }

        /// <summary>
        /// Gets or sets the actual result observed during execution.
        /// </summary>
        public string? ActualResult { get; set; }

        /// <summary>
        /// Gets or sets notes recorded for this step execution.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets when this step was executed.
        /// </summary>
        public DateTime? ExecutedAt { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the test case execution.
        /// </summary>
        public TestCaseExecution? TestCaseExecution { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the test step.
        /// </summary>
        public TestStep? TestStep { get; set; }
    }
}