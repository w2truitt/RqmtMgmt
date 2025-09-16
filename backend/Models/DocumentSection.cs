using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    /// <summary>
    /// Represents a section within a requirement document.
    /// Organizes requirements into logical groupings within documents.
    /// </summary>
    public class DocumentSection
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
        /// Gets or sets the title of the section.
        /// </summary>
        [Required]
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

        /// <summary>
        /// Gets or sets the parent document.
        /// </summary>
        public Document? Document { get; set; }

        /// <summary>
        /// Gets or sets the collection of requirements in this section.
        /// </summary>
        public ICollection<Requirement> Requirements { get; set; } = new List<Requirement>();
    }
}