using System;

namespace RqmtMgmtShared
{
    public class RequirementVersionDto
    {
        public int Id { get; set; }
        public int RequirementId { get; set; }
        public int Version { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
