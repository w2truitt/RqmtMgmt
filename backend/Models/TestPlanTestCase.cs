namespace backend.Models
{
    /// <summary>
    /// Represents a link between a test plan and a test case.
    /// </summary>
    public class TestPlanTestCase
    {
        /// <summary>
        /// Gets or sets the test plan ID.
        /// </summary>
        public int TestPlanId { get; set; }

        /// <summary>
        /// Gets or sets the test case ID.
        /// </summary>
        public int TestCaseId { get; set; }
    }
}
