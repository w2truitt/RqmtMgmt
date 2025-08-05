using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class TestStep
    {
        public int Id { get; set; }
        public int TestCaseId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ExpectedResult { get; set; } = string.Empty;
        [ForeignKey("TestCaseId")]
        public TestCase? TestCase { get; set; }
    }
}
