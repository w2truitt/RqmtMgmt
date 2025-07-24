namespace backend.Models
{
    public class TestCase
    {
        public int Id { get; set; }
        public int? SuiteId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Steps { get; set; }
        public string ExpectedResult { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
