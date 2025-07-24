using System.Collections.Generic;

namespace backend.Models
{
    /// <summary>
    /// Represents a test case for validating requirements or features.
    /// Supports links to test suites, plans, requirements, and test runs.
    /// </summary>
    public class TestCase
    {
        /// <summary>
        /// Gets or sets the unique identifier for the test case.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the test suite ID this test case belongs to, if any.
        /// </summary>
        public int? SuiteId { get; set; }

        /// <summary>
        /// Gets or sets the title of the test case.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the test case.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the steps for executing the test case.
        /// </summary>
        public string? Steps { get; set; }

        /// <summary>
        /// Gets or sets the expected result for the test case.
        /// </summary>
        public string? ExpectedResult { get; set; }

        /// <summary>
        /// Gets or sets the user ID of the creator.
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the test suite.
        /// </summary>
        public TestSuite? Suite { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the creator user.
        /// </summary>
        public User? Creator { get; set; }

        /// <summary>
        /// Gets or sets the collection of links to test plans.
        /// </summary>
        public ICollection<TestPlanTestCase> TestPlanLinks { get; set; } = new List<TestPlanTestCase>();

        /// <summary>
        /// Gets or sets the collection of links to requirements.
        /// </summary>
        public ICollection<RequirementTestCaseLink> RequirementLinks { get; set; } = new List<RequirementTestCaseLink>();

        /// <summary>
        /// Gets or sets the collection of test runs for this test case.
        /// </summary>
        public ICollection<TestRun> TestRuns { get; set; } = new List<TestRun>();
    }
}
