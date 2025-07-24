namespace backend.Models
{
    public class TestRun
    {
        public int Id { get; set; }
        public int TestCaseId { get; set; }
        public int? TestPlanId { get; set; }
        public int RunBy { get; set; }
        public DateTime RunAt { get; set; }
        public string Result { get; set; }
        public string Notes { get; set; }
        public string EvidenceUrl { get; set; }
    }
}
