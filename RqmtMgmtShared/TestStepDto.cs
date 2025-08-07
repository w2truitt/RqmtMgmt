namespace RqmtMgmtShared
{
    /// <summary>
    /// Data transfer object for individual test steps within a test case.
    /// </summary>
    public class TestStepDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the test step.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the description of the action to be performed in this test step.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the expected result or outcome after performing this test step.
        /// </summary>
        public string ExpectedResult { get; set; } = string.Empty;
    }
}