using System.Collections.Generic;
using RqmtMgmtShared;

namespace backend.Models
{
    /// <summary>
    /// Represents a test plan document grouping test cases for validation or verification.
    /// </summary>
    public class TestPlan
    {
        /// <summary>
        /// Gets or sets the unique identifier for the test plan.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the test plan.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the test plan (UserValidation, SoftwareVerification).
        /// </summary>
        public TestPlanType Type { get; set; }

        /// <summary>
        /// Gets or sets the description of the test plan.
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
        /// Gets or sets the ID of the project this test plan belongs to.
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
        /// Gets or sets the collection of links to test cases.
        /// </summary>
        public ICollection<TestPlanTestCase> TestCaseLinks { get; set; } = new List<TestPlanTestCase>();
    }
}