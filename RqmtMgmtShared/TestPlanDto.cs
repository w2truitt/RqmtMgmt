using System;

namespace RqmtMgmtShared
{
    /// <summary>
    /// Data transfer object for test plans containing test plan information and metadata.
    /// Used for organizing and managing collections of test cases for validation activities.
    /// </summary>
    public class TestPlanDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the test plan.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the test plan. This field is required and cannot be null.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of test plan (UserValidation, SoftwareVerification). This field is required and cannot be null.
        /// </summary>
        public required string Type { get; set; }

        /// <summary>
        /// Gets or sets the detailed description of the test plan. Can be null for self-explanatory test plans.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this test plan.
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the test plan was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}