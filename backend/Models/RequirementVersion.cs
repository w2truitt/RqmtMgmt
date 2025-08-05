using System;

using RqmtMgmtShared;
namespace backend.Models
{
    /// <summary>
    /// Represents a historical version of a requirement for version tracking and redline comparison.
    /// </summary>
    using RqmtMgmtShared;
    public class RequirementVersion
    {
        /// <summary>
        /// Gets or sets the unique identifier for this version entry.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the requirement ID this version belongs to.
        /// </summary>
        public int RequirementId { get; set; }

        /// <summary>
        /// Gets or sets the version number (incrementing integer or timestamp).
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the type of the requirement (CRS, PRS, SRS).
        /// </summary>
        public RequirementType Type { get; set; }

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
        public RequirementStatus Status { get; set; }

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
