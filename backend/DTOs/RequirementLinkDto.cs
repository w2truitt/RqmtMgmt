namespace backend.DTOs
{
    /// <summary>
    /// Data transfer object for requirement linkages (traceability between requirements).
    /// </summary>
    public class RequirementLinkDto
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
        public required string LinkType { get; set; }
    }
}
