using System.Collections.Generic;

using RqmtMgmtShared;
namespace backend.Models
{
    /// <summary>
    /// Represents a requirement (CRS, PRS, or SRS) in the requirements management system.
    /// Supports hierarchical relationships and links to test cases.
    /// </summary>
    using RqmtMgmtShared;
    public class Requirement
    {
        /// <summary>
        /// Gets or sets the unique identifier for the requirement.
        /// </summary>
        public int Id { get; set; }

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
        /// Gets or sets the status of the requirement (Draft, Approved, etc.).
        /// </summary>
        public RequirementStatus Status { get; set; }

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

        /// <summary>
        /// Gets or sets the parent requirement for hierarchy navigation.
        /// </summary>
        public Requirement? Parent { get; set; }

        /// <summary>
        /// Gets or sets the collection of child requirements.
        /// </summary>
        public ICollection<Requirement> Children { get; set; } = new List<Requirement>();

        /// <summary>
        /// Gets or sets the creator user object.
        /// </summary>
        public User? Creator { get; set; }

        /// <summary>
        /// Gets or sets outgoing links to other requirements.
        /// </summary>
        public ICollection<RequirementLink> OutgoingLinks { get; set; } = new List<RequirementLink>();

        /// <summary>
        /// Gets or sets incoming links from other requirements.
        /// </summary>
        public ICollection<RequirementLink> IncomingLinks { get; set; } = new List<RequirementLink>();

        /// <summary>
        /// Gets or sets links from this requirement to test cases.
        /// </summary>
        public ICollection<RequirementTestCaseLink> TestCaseLinks { get; set; } = new List<RequirementTestCaseLink>();
    }
}
