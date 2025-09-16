using System;
using RqmtMgmtShared;

namespace backend.Models
{
    /// <summary>
    /// Represents a traceability link between requirements.
    /// Tracks relationships between requirements across different document types (CRD -> PRD -> SRS).
    /// </summary>
    public class RequirementTrace
    {
        /// <summary>
        /// Gets or sets the unique identifier for this trace link.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the source requirement (parent in hierarchy).
        /// </summary>
        public int SourceRequirementId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the target requirement (child in hierarchy).
        /// </summary>
        public int TargetRequirementId { get; set; }

        /// <summary>
        /// Gets or sets the type of trace relationship.
        /// </summary>
        public TraceType TraceType { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when this trace link was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this trace link.
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the source requirement.
        /// </summary>
        public Requirement? SourceRequirement { get; set; }

        /// <summary>
        /// Gets or sets the target requirement.
        /// </summary>
        public Requirement? TargetRequirement { get; set; }

        /// <summary>
        /// Gets or sets the user who created this trace link.
        /// </summary>
        public User? Creator { get; set; }
    }
}