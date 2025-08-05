using System;
using backend.Models;
using RqmtMgmtShared;
using Xunit;

namespace backend.Tests
{
    public class TestRunTests
    {
        [Fact]
        public void CanSetAndGet_AllProperties()
        {
            var now = DateTime.UtcNow;
            var tc = new TestCase { Id = 2, Title = "Test", CreatedBy = 1, CreatedAt = now };
            var plan = new TestPlan { Id = 3, Name = "Plan", Type = TestPlanType.UserValidation, CreatedBy = 1, CreatedAt = now };
            var user = new User { Id = 4, UserName = "runner", Email = "runner@test.com" };
            var run = new TestRun
            {
                Id = 10,
                TestCaseId = 11,
                TestPlanId = 12,
                RunBy = 13,
                RunAt = now,
                Result = TestResult.Passed,
                Notes = "Some notes",
                EvidenceUrl = "http://evidence",
                TestCase = tc,
                TestPlan = plan,
                Runner = user
            };
            Assert.Equal(10, run.Id);
            Assert.Equal(11, run.TestCaseId);
            Assert.Equal(12, run.TestPlanId);
            Assert.Equal(13, run.RunBy);
            Assert.Equal(now, run.RunAt);
            Assert.Equal(TestResult.Passed, run.Result);
            Assert.Equal("Some notes", run.Notes);
            Assert.Equal("http://evidence", run.EvidenceUrl);
            Assert.Equal(tc, run.TestCase);
            Assert.Equal(plan, run.TestPlan);
            Assert.Equal(user, run.Runner);
        }

        [Fact]
        public void NotesAndEvidenceUrl_AcceptNull()
        {
            var run = new TestRun();
            Assert.Null(run.Notes);
            Assert.Null(run.EvidenceUrl);
        }
    }
}