using backend.Models;
using Xunit;

namespace backend.Tests
{
    public class RequirementTestCaseLinkTests
    {
        [Fact]
        public void CanSetAndGet_Properties()
        {
            var link = new RequirementTestCaseLink { RequirementId = 1, TestCaseId = 2 };
            Assert.Equal(1, link.RequirementId);
            Assert.Equal(2, link.TestCaseId);
        }
    }
}
