using System;

namespace backend.Models
{
    /// <summary>
    /// Represents a historical version of a test case for version tracking and redline comparison.
    /// </summary>
    public class TestCaseVersion
    {
        /// <summary>
        /// Gets or sets the unique identifier for this version entry.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the test case ID this version belongs to.
        /// </summary>
        public int TestCaseId { get; set; }

        /// <summary>
        /// Gets or sets the version number (incrementing integer or timestamp).
        /// </summary>
        public int Version { get; set; }

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
        /// Gets or sets the user who made this version.
        /// </summary>
        public int ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when this version was created.
        /// </summary>
        public DateTime ModifiedAt { get; set; }
    }
}
