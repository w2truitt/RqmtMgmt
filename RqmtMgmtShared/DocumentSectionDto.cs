using System;
using System.ComponentModel.DataAnnotations;

namespace RqmtMgmtShared
{
    /// <summary>
    /// Data transfer object for document sections within requirement documents.
    /// Represents organizational sections like "Requirements", "User Stories", etc.
    /// </summary>
    public class DocumentSectionDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the document section.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the parent document.
        /// </summary>
        public int DocumentId { get; set; }

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
        /// Gets or sets the order/sequence of this section within the document.
        /// </summary>
        public int SectionOrder { get; set; }

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
    }
}