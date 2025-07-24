namespace backend.Models
{
    public class RequirementLink
    {
        public int Id { get; set; }
        public int FromRequirementId { get; set; }
        public int ToRequirementId { get; set; }
        public string LinkType { get; set; } // CRS-PRS, SRS-PRS
    }
}
