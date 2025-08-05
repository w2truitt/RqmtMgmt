using backend.Models;
using Xunit;

namespace backend.Tests
{
    public class TestPlanTestCaseTests
    {
        [Fact]
        public void CanSetAndGet_Properties()
        {
            var link = new TestPlanTestCase { TestPlanId = 1, TestCaseId = 2 };
            Assert.Equal(1, link.TestPlanId);
            Assert.Equal(2, link.TestCaseId);
        }
    }
}
