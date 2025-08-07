using System;

namespace RqmtMgmtShared
{
    /// <summary>
    /// Data transfer object for requirement version history.
    /// Captures historical snapshots of requirement changes for audit trails and rollback capabilities.
    /// </summary>
    public class RequirementVersionDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for this version record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the requirement this version belongs to.
        /// </summary>
        public int RequirementId { get; set; }

        /// <summary>
        /// Gets or sets the version number for this snapshot of the requirement.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the title of the requirement at the time of this version.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the requirement at the time of this version. Can be null.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the requirement at the time of this version.
        /// </summary>
        public RequirementType Type { get; set; }

        /// <summary>
        /// Gets or sets the status of the requirement at the time of this version.
        /// </summary>
        public RequirementStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who made the modification that created this version.
        /// </summary>
        public int ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when this version was created.
        /// </summary>
        public DateTime ModifiedAt { get; set; }
    }
}