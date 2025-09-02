using System.Collections.Generic;

namespace backend.Models
{
    /// <summary>
    /// Represents a test suite, a collection of test cases grouped for execution.
    /// </summary>
    public class TestSuite
    {
        /// <summary>
        /// Gets or sets the unique identifier for the test suite.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the test suite.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the test suite.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the user ID of the creator.
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the ID of the project this test suite belongs to.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the creator user.
        /// </summary>
        public User? Creator { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the project.
        /// </summary>
        public Project? Project { get; set; }

        /// <summary>
        /// Gets or sets the collection of test cases in this suite.
        /// </summary>
        public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
    }
}