namespace backend.DTOs
{
    /// <summary>
    /// Represents a change in a single field between two versions.
    /// </summary>
    public class RedlineFieldChangeDto
    {
        public string Field { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string ChangeType { get; set; } // Added, Removed, Modified
    }
}
