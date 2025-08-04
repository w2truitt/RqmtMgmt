using System;

namespace RqmtMgmtShared
{
    /// <summary>
    /// Data transfer object for requirements.
    /// </summary>
    public class RequirementDto
    {
        public int Id { get; set; }
        public required string Type { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public required string Status { get; set; }
        public int Version { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
