namespace backend.Models
{
    /// <summary>
    /// Represents an explicit trace linkage between two requirements (e.g., CRS-PRS, SRS-PRS).
    /// </summary>
    public class RequirementLink
    {
        /// <summary>
        /// Gets or sets the unique identifier for the requirement link.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the source requirement ID.
        /// </summary>
        public int FromRequirementId { get; set; }

        /// <summary>
        /// Gets or sets the target requirement ID.
        /// </summary>
        public int ToRequirementId { get; set; }

        /// <summary>
        /// Gets or sets the type of linkage (e.g., CRS-PRS, SRS-PRS).
        /// </summary>
        public required string LinkType { get; set; } // CRS-PRS, SRS-PRS

        /// <summary>
        /// Gets or sets the navigation property to the source requirement.
        /// </summary>
        public Requirement? FromRequirement { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the target requirement.
        /// </summary>
        public Requirement? ToRequirement { get; set; }
    }
}
