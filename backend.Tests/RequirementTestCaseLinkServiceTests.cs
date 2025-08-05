using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using RqmtMgmtShared;
using Xunit;

namespace backend.Tests
{
    public class RequirementTestCaseLinkServiceTests
    {
        private static RqmtMgmtDbContext GetDbContext(string testName)
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: $"RequirementTestCaseLinkService_{testName}_{System.Guid.NewGuid()}")
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        [Fact]
        public async Task CreateLinkAsync_AddsLink_WhenNotExists()
        {
            using var db = GetDbContext(nameof(CreateLinkAsync_AddsLink_WhenNotExists));
            var service = new RequirementTestCaseLinkService(db);
            var dto = new RequirementTestCaseLinkDto { RequirementId = 1, TestCaseId = 2 };
            var result = await service.CreateLinkAsync(dto);
            Assert.True(result);
            var link = db.RequirementTestCaseLinks.FirstOrDefault();
            Assert.NotNull(link);
            Assert.Equal(1, link.RequirementId);
            Assert.Equal(2, link.TestCaseId);
        }

        [Fact]
        public async Task CreateLinkAsync_ReturnsFalse_WhenLinkExists()
        {
            using var db = GetDbContext(nameof(CreateLinkAsync_ReturnsFalse_WhenLinkExists));
            db.RequirementTestCaseLinks.Add(new RequirementTestCaseLink { RequirementId = 1, TestCaseId = 2 });
            db.SaveChanges();
            var service = new RequirementTestCaseLinkService(db);
            var dto = new RequirementTestCaseLinkDto { RequirementId = 1, TestCaseId = 2 };
            var result = await service.CreateLinkAsync(dto);
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteLinkAsync_RemovesLinkAndReturnsTrue()
        {
            using var db = GetDbContext(nameof(DeleteLinkAsync_RemovesLinkAndReturnsTrue));
            db.RequirementTestCaseLinks.Add(new RequirementTestCaseLink { RequirementId = 1, TestCaseId = 2 });
            db.SaveChanges();
            var service = new RequirementTestCaseLinkService(db);
            var result = await service.DeleteLinkAsync(1, 2);
            Assert.True(result);
            Assert.Empty(db.RequirementTestCaseLinks);
        }

        [Fact]
        public async Task DeleteLinkAsync_ReturnsFalse_WhenNotExists()
        {
            using var db = GetDbContext(nameof(DeleteLinkAsync_ReturnsFalse_WhenNotExists));
            var service = new RequirementTestCaseLinkService(db);
            var result = await service.DeleteLinkAsync(1, 2);
            Assert.False(result);
        }

        [Fact]
        public async Task GetLinksForRequirementAsync_ReturnsCorrectLinks()
        {
            using var db = GetDbContext(nameof(GetLinksForRequirementAsync_ReturnsCorrectLinks));
            db.RequirementTestCaseLinks.Add(new RequirementTestCaseLink { RequirementId = 1, TestCaseId = 2 });
            db.RequirementTestCaseLinks.Add(new RequirementTestCaseLink { RequirementId = 1, TestCaseId = 3 });
            db.RequirementTestCaseLinks.Add(new RequirementTestCaseLink { RequirementId = 2, TestCaseId = 2 });
            db.SaveChanges();
            var service = new RequirementTestCaseLinkService(db);
            var links = await service.GetLinksForRequirementAsync(1);
            Assert.Equal(2, links.Count);
            Assert.All(links, l => Assert.Equal(1, l.RequirementId));
        }

        [Fact]
        public async Task GetLinksForTestCaseAsync_ReturnsCorrectLinks()
        {
            using var db = GetDbContext(nameof(GetLinksForTestCaseAsync_ReturnsCorrectLinks));
            db.RequirementTestCaseLinks.Add(new RequirementTestCaseLink { RequirementId = 1, TestCaseId = 2 });
            db.RequirementTestCaseLinks.Add(new RequirementTestCaseLink { RequirementId = 1, TestCaseId = 3 });
            db.RequirementTestCaseLinks.Add(new RequirementTestCaseLink { RequirementId = 2, TestCaseId = 2 });
            db.SaveChanges();
            var service = new RequirementTestCaseLinkService(db);
            var links = await service.GetLinksForTestCaseAsync(2);
            Assert.Equal(2, links.Count);
            Assert.All(links, l => Assert.Equal(2, l.TestCaseId));
        }
    }
}
