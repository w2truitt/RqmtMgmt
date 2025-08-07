using System;

namespace RqmtMgmtShared
{
    /// <summary>
    /// Data transfer object for requirements containing all requirement information for API communication.
    /// </summary>
    public class RequirementDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the requirement.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the type of requirement (CRS, PRS, SRS, UserStory, BusinessRule, EntityName).
        /// </summary>
        public RequirementType Type { get; set; }

        /// <summary>
        /// Gets or sets the title of the requirement. This field is required and cannot be null.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Gets or sets the detailed description of the requirement. Can be null for brief requirements.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the ID of the parent requirement for hierarchical relationships. Null for top-level requirements.
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the current status of the requirement (Draft, Approved, Implemented, Verified).
        /// </summary>
        public RequirementStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the version number of the requirement for change tracking.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this requirement.
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the requirement was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the requirement was last updated. Null if never updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}