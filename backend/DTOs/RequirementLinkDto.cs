namespace backend.DTOs
{
    public class RequirementLinkDto
    {
        public int Id { get; set; }
        public int FromRequirementId { get; set; }
        public int ToRequirementId { get; set; }
        public string LinkType { get; set; }
    }
}
