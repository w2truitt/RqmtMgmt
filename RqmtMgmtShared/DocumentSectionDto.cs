using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RqmtMgmtShared
{
    /// <summary>
    /// Data transfer object for document sections within requirement documents.
    /// Supports hierarchical organization with parent-child relationships.
    /// </summary>
    public class DocumentSectionDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the document section.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the project this section belongs to. This field is required.
        /// </summary>
        [Required(ErrorMessage = "Project ID is required")]
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
        /// Gets or sets the title of the section. This field is required.
        /// </summary>
        [Required(ErrorMessage = "Section title is required")]
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
        /// Gets or sets the collection of child sections (subsections) under this section.
        /// </summary>
        public List<DocumentSectionDto>? ChildSections { get; set; }

        /// <summary>
        /// Gets or sets the count of requirements directly in this section (excluding subsections).
        /// </summary>
        public int RequirementCount { get; set; }

        /// <summary>
        /// Gets or sets the total count of requirements including all subsections recursively.
        /// </summary>
        public int TotalRequirementCount { get; set; }
    }
}