#nullable enable
using System;
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
            var req = new RequirementDto { Title = "Test Rqmt", Type = RequirementType.CRS, Status = RequirementStatus.Draft, CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            var result = await service.CreateAsync(req);
            Assert.NotNull(result);
            Assert.Equal("Test Rqmt", result.Title);
            Assert.Single(await db.Requirements.ToListAsync());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllRequirements()
        {
            using var db = GetDbContext(nameof(GetAllAsync_ReturnsAllRequirements));
            db.Requirements.Add(new Requirement { Title = "R1", Type = RequirementType.CRS, Status = RequirementStatus.Draft, CreatedBy = 1, CreatedAt = DateTime.UtcNow });
            db.Requirements.Add(new Requirement { Title = "R2", Type = RequirementType.PRS, Status = RequirementStatus.Draft, CreatedBy = 1, CreatedAt = DateTime.UtcNow });
            db.SaveChanges();
            var service = new RequirementService(db);
            var all = await service.GetAllAsync();
            Assert.Equal(2, System.Linq.Enumerable.Count(all));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectRequirementOrNull()
        {
            using var db = GetDbContext(nameof(GetByIdAsync_ReturnsCorrectRequirementOrNull));
            var req = new Requirement { Title = "R1", Type = RequirementType.CRS, Status = RequirementStatus.Draft, CreatedBy = 1, CreatedAt = DateTime.UtcNow };
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
            var req = new Requirement { Title = "Old", Type = RequirementType.CRS, Status = RequirementStatus.Draft, CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            db.Requirements.Add(req);
            db.SaveChanges();
            var service = new RequirementService(db);
            var dto = new RequirementDto { Id = req.Id, Title = "New", Type = req.Type, Status = req.Status, CreatedBy = req.CreatedBy, CreatedAt = req.CreatedAt };
            var updated = await service.UpdateAsync(dto);
            Assert.True(updated);
        }

        [Fact]
        public async Task DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse()
        {
            using var db = GetDbContext(nameof(DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse));
            var req = new Requirement { Title = "Del", Type = RequirementType.CRS, Status = RequirementStatus.Draft, CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            db.Requirements.Add(req);
            db.SaveChanges();
            var service = new RequirementService(db);
            var ok = await service.DeleteAsync(req.Id);
            Assert.True(ok);
            Assert.Empty(await db.Requirements.ToListAsync());
            var fail = await service.DeleteAsync(9999);
            Assert.False(fail);
        }
    }
}