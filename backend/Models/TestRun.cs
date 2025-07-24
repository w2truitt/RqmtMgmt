namespace backend.Models
{
    /// <summary>
    /// Represents an execution instance of a test case or test suite.
    /// </summary>
    public class TestRun
    {
        /// <summary>
        /// Gets or sets the unique identifier for the test run.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the test case executed.
        /// </summary>
        public int TestCaseId { get; set; }

        /// <summary>
        /// Gets or sets the test plan ID, if the run was part of a plan.
        /// </summary>
        public int? TestPlanId { get; set; }

        /// <summary>
        /// Gets or sets the user ID of the runner.
        /// </summary>
        public int RunBy { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the test was run (UTC).
        /// </summary>
        public DateTime RunAt { get; set; }

        /// <summary>
        /// Gets or sets the result of the test run.
        /// </summary>
        public TestResult Result { get; set; }

        /// <summary>
        /// Gets or sets notes recorded during the run.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets a URL to evidence (e.g., screenshot, log) for the run.
        /// </summary>
        public string? EvidenceUrl { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the test case.
        /// </summary>
        public TestCase? TestCase { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the test plan.
        /// </summary>
        public TestPlan? TestPlan { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the user who ran the test.
        /// </summary>
        public User? Runner { get; set; }
    }
}
