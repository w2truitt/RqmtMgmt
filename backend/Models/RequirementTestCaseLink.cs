namespace backend.Models
{
    /// <summary>
    /// Represents a link between a requirement and a test case for traceability.
    /// </summary>
    public class RequirementTestCaseLink
    {
        /// <summary>
        /// Gets or sets the requirement ID.
        /// </summary>
        public int RequirementId { get; set; }

        /// <summary>
        /// Gets or sets the test case ID.
        /// </summary>
        public int TestCaseId { get; set; }
    }
}
