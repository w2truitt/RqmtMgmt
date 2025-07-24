namespace backend.Models
{
    public class TestPlan
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // UserValidation, SoftwareVerification
        public string Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
