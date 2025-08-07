namespace RqmtMgmtShared
{
    /// <summary>
    /// Data transfer object for individual field changes in redline comparisons.
    /// Represents a single field modification between two requirement versions.
    /// </summary>
    public class RedlineFieldChangeDto
    {
        /// <summary>
        /// Gets or sets the name of the field that was changed. This field is required and cannot be null.
        /// Examples include "Title", "Description", "Status", "Type".
        /// </summary>
        public required string Field { get; set; }

        /// <summary>
        /// Gets or sets the previous value of the field. Can be null for newly added fields.
        /// </summary>
        public string? OldValue { get; set; }

        /// <summary>
        /// Gets or sets the new value of the field. Can be null for removed fields.
        /// </summary>
        public string? NewValue { get; set; }

        /// <summary>
        /// Gets or sets the type of change that occurred. This field is required and cannot be null.
        /// Valid values are "Added", "Removed", "Modified".
        /// </summary>
        public required string ChangeType { get; set; }
    }
}