using System;

namespace RqmtMgmtShared
{
    /// <summary>
    /// Data transfer object for test suites containing test suite information and metadata.
    /// Used for grouping related test cases into logical collections for better organization.
    /// </summary>
    public class TestSuiteDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the test suite.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the test suite. This field is required and cannot be null.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the detailed description of the test suite. Can be null for self-explanatory test suites.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this test suite.
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the test suite was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}