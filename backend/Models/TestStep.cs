using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Represents an individual step within a test case.
    /// Contains the action to perform and the expected outcome for validation.
    /// </summary>
    public class TestStep
    {
        /// <summary>
        /// Gets or sets the unique identifier for the test step.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the test case this step belongs to.
        /// </summary>
        public int TestCaseId { get; set; }

        /// <summary>
        /// Gets or sets the description of the action to be performed in this test step.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the expected result or outcome after performing this test step.
        /// </summary>
        public string ExpectedResult { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the navigation property to the parent test case.
        /// </summary>
        [ForeignKey("TestCaseId")]
        public TestCase? TestCase { get; set; }
    }
}