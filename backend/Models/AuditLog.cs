namespace backend.Models
{
    /// <summary>
    /// Represents an audit log entry recording user actions for traceability and compliance.
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// Gets or sets the unique identifier for the audit log entry.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the identifier of the user who performed the action.
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the action performed (e.g., "Created", "Updated", "Deleted").
        /// </summary>
        public required string Action { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the entity affected (e.g., "Requirement", "TestCase").
        /// </summary>
        public string? Entity { get; set; }
        
        /// <summary>
        /// Gets or sets the identifier of the affected entity, if applicable.
        /// </summary>
        public int? EntityId { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp when the action occurred (UTC).
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Gets or sets additional details about the action, if any.
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the user who performed the action.
        /// </summary>
        public User? User { get; set; }
    }
}
