namespace RqmtMgmtShared
{
    /// <summary>
    /// Data transfer object for linking requirements to test cases.
    /// Represents the many-to-many relationship between requirements and their validation test cases.
    /// </summary>
    public class RequirementTestCaseLinkDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the requirement being linked.
        /// </summary>
        public int RequirementId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the test case that validates the requirement.
        /// </summary>
        public int TestCaseId { get; set; }
    }
}