#nullable enable
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests
{
    public class RequirementServiceTests
    {
        private static RqmtMgmtDbContext GetDbContext(string testName)
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: $"RequirementServiceTestDb_{testName}_{System.Guid.NewGuid()}")
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_AddsRequirement()
        {
            using var db = GetDbContext(nameof(CreateAsync_AddsRequirement));
            var service = new RequirementService(db);
            var req = new Requirement { Title = "Test Rqmt" };
            var result = await service.CreateAsync(req);
            Assert.NotNull(result);
            Assert.Equal("Test Rqmt", result.Title);
            Assert.Single(db.Requirements);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllRequirements()
        {
            using var db = GetDbContext(nameof(GetAllAsync_ReturnsAllRequirements));
            db.Requirements.Add(new Requirement { Title = "R1" });
            db.Requirements.Add(new Requirement { Title = "R2" });
            db.SaveChanges();
            var service = new RequirementService(db);
            var all = await service.GetAllAsync();
            Assert.Equal(2, System.Linq.Enumerable.Count(all));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectRequirementOrNull()
        {
            using var db = GetDbContext(nameof(GetByIdAsync_ReturnsCorrectRequirementOrNull));
            var req = new Requirement { Title = "R1" };
            db.Requirements.Add(req);
            db.SaveChanges();
            var service = new RequirementService(db);
            var found = await service.GetByIdAsync(req.Id);
            Assert.NotNull(found);
            Assert.Equal(req.Title, found!.Title);
            var notFound = await service.GetByIdAsync(999);
            Assert.Null(notFound);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesRequirement()
        {
            using var db = GetDbContext(nameof(UpdateAsync_UpdatesRequirement));
            var req = new Requirement { Title = "Old" };
            db.Requirements.Add(req);
            db.SaveChanges();
            var service = new RequirementService(db);
            req.Title = "New";
            var updated = await service.UpdateAsync(req);
            Assert.Equal("New", updated.Title);
        }

        [Fact]
        public async Task DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse()
        {
            using var db = GetDbContext(nameof(DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse));
            var req = new Requirement { Title = "Del" };
            db.Requirements.Add(req);
            db.SaveChanges();
            var service = new RequirementService(db);
            var ok = await service.DeleteAsync(req.Id);
            Assert.True(ok);
            Assert.Empty(db.Requirements);
            var fail = await service.DeleteAsync(9999);
            Assert.False(fail);
        }
    }
}
