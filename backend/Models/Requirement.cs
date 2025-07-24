namespace backend.Models
{
    public class Requirement
    {
        public int Id { get; set; }
        public string Type { get; set; } // CRS, PRS, SRS
        public string Title { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public string Status { get; set; }
        public int Version { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
