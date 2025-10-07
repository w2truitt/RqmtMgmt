using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    /// <summary>
    /// Represents a section within a requirement document.
    /// Supports hierarchical organization with parent-child section relationships.
    /// All sections must belong to a project for proper organization and security.
    /// </summary>
    public class DocumentSection
    {
        /// <summary>
        /// Gets or sets the unique identifier for the document section.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the project this section belongs to. This field is required.
        /// </summary>
        [Required]
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the parent document. Null for subsections that only have a parent section.
        /// </summary>
        public int? DocumentId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the parent section for hierarchical organization. Null for root sections.
        /// </summary>
        public int? ParentSectionId { get; set; }

        /// <summary>
        /// Gets or sets the title of the section.
        /// </summary>
        [Required]
        public required string Title { get; set; }

        /// <summary>
        /// Gets or sets the description or content of the section.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the order/sequence of this section within its parent (document or section).
        /// </summary>
        public int SectionOrder { get; set; }

        /// <summary>
        /// Gets or sets the hierarchical level of this section (1 = root, 2 = subsection, 3 = sub-subsection, etc.).
        /// </summary>
        public int Level { get; set; } = 1;

        /// <summary>
        /// Gets or sets the section number for display purposes (e.g., "3.1.2"). Generated based on hierarchy.
        /// </summary>
        [MaxLength(50)]
        public string? SectionNumber { get; set; }

        /// <summary>
        /// Gets or sets whether this section is marked as Not Applicable.
        /// </summary>
        public bool IsNotApplicable { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the section was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the section was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the project this section belongs to.
        /// </summary>
        public Project? Project { get; set; }

        /// <summary>
        /// Gets or sets the parent document (for root sections).
        /// </summary>
        public Document? Document { get; set; }

        /// <summary>
        /// Gets or sets the parent section (for subsections).
        /// </summary>
        public DocumentSection? ParentSection { get; set; }

        /// <summary>
        /// Gets or sets the collection of child sections (subsections) under this section.
        /// </summary>
        public ICollection<DocumentSection> ChildSections { get; set; } = new List<DocumentSection>();

        /// <summary>
        /// Gets or sets the collection of requirements in this section.
        /// </summary>
        public ICollection<Requirement> Requirements { get; set; } = new List<Requirement>();
    }
}