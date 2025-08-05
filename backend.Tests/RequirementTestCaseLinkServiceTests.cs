#nullable enable
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using RqmtMgmtShared;
using System.Linq;

namespace backend.Tests
{
    public class RequirementTestCaseLinkServiceTests
    {
        private static RqmtMgmtDbContext GetDbContext(string testName)
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: $"RequirementTestCaseLinkServiceTestDb_{testName}_{System.Guid.NewGuid()}")
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        [Fact]
        public async Task AddLink_AddsLink()
        {
            using var db = GetDbContext(nameof(AddLink_AddsLink));
            var service = new RequirementTestCaseLinkService(db);
            
            await service.AddLink(1, 2);
            
            var links = await db.RequirementTestCaseLinks.ToListAsync();
            Assert.Single(links);
            Assert.Equal(1, links[0].RequirementId);
            Assert.Equal(2, links[0].TestCaseId);
        }

        [Fact]
        public async Task AddLink_DoesNotAddDuplicate()
        {
            using var db = GetDbContext(nameof(AddLink_DoesNotAddDuplicate));
            db.RequirementTestCaseLinks.Add(new RequirementTestCaseLink { RequirementId = 1, TestCaseId = 2 });
            await db.SaveChangesAsync();
            var service = new RequirementTestCaseLinkService(db);
            
            await service.AddLink(1, 2);
            
            var links = await db.RequirementTestCaseLinks.ToListAsync();
            Assert.Single(links);
        }

        [Fact]
        public async Task RemoveLink_RemovesLink()
        {
            using var db = GetDbContext(nameof(RemoveLink_RemovesLink));
            db.RequirementTestCaseLinks.Add(new RequirementTestCaseLink { RequirementId = 1, TestCaseId = 2 });
            await db.SaveChangesAsync();
            var service = new RequirementTestCaseLinkService(db);
            
            await service.RemoveLink(1, 2);
            
            var links = await db.RequirementTestCaseLinks.ToListAsync();
            Assert.Empty(links);
        }

        [Fact]
        public async Task RemoveLink_DoesNothingWhenNotExists()
        {
            using var db = GetDbContext(nameof(RemoveLink_DoesNothingWhenNotExists));
            var service = new RequirementTestCaseLinkService(db);
            
            await service.RemoveLink(1, 2);
            
            // Should not throw exception
            Assert.True(true);
        }

        [Fact]
        public async Task GetLinksForRequirement_ReturnsCorrectLinks()
        {
            using var db = GetDbContext(nameof(GetLinksForRequirement_ReturnsCorrectLinks));
            db.RequirementTestCaseLinks.Add(new RequirementTestCaseLink { RequirementId = 1, TestCaseId = 2 });
            db.RequirementTestCaseLinks.Add(new RequirementTestCaseLink { RequirementId = 1, TestCaseId = 3 });
            db.RequirementTestCaseLinks.Add(new RequirementTestCaseLink { RequirementId = 2, TestCaseId = 4 });
            await db.SaveChangesAsync();
            var service = new RequirementTestCaseLinkService(db);
            
            var links = await service.GetLinksForRequirement(1);
            
            Assert.Equal(2, links.Count);
            Assert.All(links, l => Assert.Equal(1, l.RequirementId));
        }

        [Fact]
        public async Task GetLinksForTestCase_ReturnsCorrectLinks()
        {
            using var db = GetDbContext(nameof(GetLinksForTestCase_ReturnsCorrectLinks));
            db.RequirementTestCaseLinks.Add(new RequirementTestCaseLink { RequirementId = 1, TestCaseId = 2 });
            db.RequirementTestCaseLinks.Add(new RequirementTestCaseLink { RequirementId = 3, TestCaseId = 2 });
            db.RequirementTestCaseLinks.Add(new RequirementTestCaseLink { RequirementId = 4, TestCaseId = 3 });
            await db.SaveChangesAsync();
            var service = new RequirementTestCaseLinkService(db);
            
            var links = await service.GetLinksForTestCase(2);
            
            Assert.Equal(2, links.Count);
            Assert.All(links, l => Assert.Equal(2, l.TestCaseId));
        }
    }
}