using backend.Models;
using Xunit;

namespace backend.Tests
{
    public class RequirementLinkTests
    {
        [Fact]
        public void CanSetAndGet_AllProperties()
        {
            var from = new Requirement { Id = 1, Title = "From", Type = RequirementType.CRS };
            var to = new Requirement { Id = 2, Title = "To", Type = RequirementType.PRS };
            var link = new RequirementLink
            {
                Id = 7,
                FromRequirementId = 8,
                ToRequirementId = 9,
                LinkType = "CRS-PRS",
                FromRequirement = from,
                ToRequirement = to
            };
            Assert.Equal(7, link.Id);
            Assert.Equal(8, link.FromRequirementId);
            Assert.Equal(9, link.ToRequirementId);
            Assert.Equal("CRS-PRS", link.LinkType);
            Assert.Equal(from, link.FromRequirement);
            Assert.Equal(to, link.ToRequirement);
        }
    }
}
