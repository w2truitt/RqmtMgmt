namespace backend.DTOs
{
    /// <summary>
    /// Data transfer object for links between requirements and test cases.
    /// </summary>
    public class RequirementTestCaseLinkDto
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
