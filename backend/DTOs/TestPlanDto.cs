namespace backend.DTOs
{
    /// <summary>
    /// Data transfer object for test plans.
    /// </summary>
    public class TestPlanDto
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
        public required string Type { get; set; }

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
    }
}
