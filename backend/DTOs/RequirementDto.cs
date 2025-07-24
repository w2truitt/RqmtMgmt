namespace backend.DTOs
{
    /// <summary>
    /// Data transfer object for requirements.
    /// </summary>
    public class RequirementDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the requirement.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the requirement (CRS, PRS, SRS).
        /// </summary>
        public required string Type { get; set; }

        /// <summary>
        /// Gets or sets the title of the requirement.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the requirement.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the parent requirement ID for hierarchy.
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the status of the requirement.
        /// </summary>
        public required string Status { get; set; }

        /// <summary>
        /// Gets or sets the version number of the requirement.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created the requirement.
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last updated timestamp (UTC), if applicable.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
