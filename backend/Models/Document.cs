using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RqmtMgmtShared;

namespace backend.Models
{
    /// <summary>
    /// Represents a requirement document (CRD, PRD, SRS) in the requirements management system.
    /// Contains document-level metadata and sections with requirements.
    /// </summary>
    public class Document
    {
        /// <summary>
        /// Gets or sets the unique identifier for the document.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the type of document (CRD, PRD, SRS).
        /// </summary>
        public DocumentType Type { get; set; }

        /// <summary>
        /// Gets or sets the title of the document.
        /// </summary>
        [Required]
        public required string Title { get; set; }

        /// <summary>
        /// Gets or sets the document version number.
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Gets or sets the document owner/author.
        /// </summary>
        public string? DocumentOwner { get; set; }

        /// <summary>
        /// Gets or sets the objective/purpose of the document.
        /// </summary>
        public string? Objective { get; set; }

        /// <summary>
        /// Gets or sets the background and context information.
        /// </summary>
        public string? Background { get; set; }

        /// <summary>
        /// Gets or sets what is in scope for this document.
        /// </summary>
        public string? InScope { get; set; }

        /// <summary>
        /// Gets or sets what is out of scope for this document.
        /// </summary>
        public string? OutOfScope { get; set; }

        /// <summary>
        /// Gets or sets user personas and stakeholders information.
        /// </summary>
        public string? UserPersonas { get; set; }

        /// <summary>
        /// Gets or sets the success criteria for the document.
        /// </summary>
        public string? SuccessCriteria { get; set; }

        /// <summary>
        /// Gets or sets dependencies information.
        /// </summary>
        public string? Dependencies { get; set; }

        /// <summary>
        /// Gets or sets assumptions and constraints.
        /// </summary>
        public string? AssumptionsConstraints { get; set; }

        /// <summary>
        /// Gets or sets timeline and milestones information.
        /// </summary>
        public string? Timeline { get; set; }

        /// <summary>
        /// Gets or sets appendix and supporting materials.
        /// </summary>
        public string? Appendix { get; set; }

        /// <summary>
        /// Gets or sets the current status of the document.
        /// </summary>
        public DocumentStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this document.
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the document was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the document was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the ID of the project this document belongs to.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the creator user object.
        /// </summary>
        public User? Creator { get; set; }

        /// <summary>
        /// Gets or sets the project this document belongs to.
        /// </summary>
        public Project? Project { get; set; }

        /// <summary>
        /// Gets or sets the collection of sections in this document.
        /// </summary>
        public ICollection<DocumentSection> Sections { get; set; } = new List<DocumentSection>();

        /// <summary>
        /// Gets or sets the collection of requirements directly associated with this document.
        /// </summary>
        public ICollection<Requirement> Requirements { get; set; } = new List<Requirement>();
    }
}