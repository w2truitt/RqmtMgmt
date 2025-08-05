using System;

namespace RqmtMgmtShared
{
    public class TestCaseDto
    {
        public int Id { get; set; }
        public int? SuiteId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public List<TestStepDto> Steps { get; set; } = new();
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
