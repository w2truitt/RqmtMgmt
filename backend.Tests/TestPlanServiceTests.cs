#nullable enable
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests
{
    public class TestPlanServiceTests
    {
        private static RqmtMgmtDbContext GetDbContext(string testName)
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestPlanServiceTestDb_{testName}_{System.Guid.NewGuid()}")
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_AddsTestPlan()
        {
            using var db = GetDbContext(nameof(CreateAsync_AddsTestPlan));
            var service = new TestPlanService(db);
            var plan = new TestPlan { Name = "Plan1" };
            var result = await service.CreateAsync(plan);
            Assert.NotNull(result);
            Assert.Equal("Plan1", result.Name);
            Assert.Single(db.TestPlans);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllTestPlans()
        {
            using var db = GetDbContext(nameof(GetAllAsync_ReturnsAllTestPlans));
            db.TestPlans.Add(new TestPlan { Name = "P1" });
            db.TestPlans.Add(new TestPlan { Name = "P2" });
            db.SaveChanges();
            var service = new TestPlanService(db);
            var all = await service.GetAllAsync();
            Assert.Equal(2, System.Linq.Enumerable.Count(all));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectTestPlanOrNull()
        {
            using var db = GetDbContext(nameof(GetByIdAsync_ReturnsCorrectTestPlanOrNull));
            var plan = new TestPlan { Name = "P1" };
            db.TestPlans.Add(plan);
            db.SaveChanges();
            var service = new TestPlanService(db);
            var found = await service.GetByIdAsync(plan.Id);
            Assert.NotNull(found);
            Assert.Equal(plan.Name, found!.Name);
            var notFound = await service.GetByIdAsync(999);
            Assert.Null(notFound);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesTestPlan()
        {
            using var db = GetDbContext(nameof(UpdateAsync_UpdatesTestPlan));
            var plan = new TestPlan { Name = "Old" };
            db.TestPlans.Add(plan);
            db.SaveChanges();
            var service = new TestPlanService(db);
            plan.Name = "New";
            var updated = await service.UpdateAsync(plan);
            Assert.Equal("New", updated.Name);
        }

        [Fact]
        public async Task DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse()
        {
            using var db = GetDbContext(nameof(DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse));
            var plan = new TestPlan { Name = "Del" };
            db.TestPlans.Add(plan);
            db.SaveChanges();
            var service = new TestPlanService(db);
            var ok = await service.DeleteAsync(plan.Id);
            Assert.True(ok);
            Assert.Empty(db.TestPlans);
            var fail = await service.DeleteAsync(9999);
            Assert.False(fail);
        }
    }
}
