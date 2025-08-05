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
            var testPlan = new TestPlanDto { Name = "Test Plan", Type = "UserValidation", Description = "Description", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            
            var result = await service.CreateAsync(testPlan);
            
            Assert.NotNull(result);
            Assert.Equal("Test Plan", result.Name);
            Assert.Single(await db.TestPlans.ToListAsync());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllTestPlans()
        {
            using var db = GetDbContext(nameof(GetAllAsync_ReturnsAllTestPlans));
            db.TestPlans.Add(new TestPlan { Name = "TP1", Type = TestPlanType.UserValidation, Description = "Desc1", CreatedBy = 1, CreatedAt = DateTime.UtcNow });
            db.TestPlans.Add(new TestPlan { Name = "TP2", Type = TestPlanType.SoftwareVerification, Description = "Desc2", CreatedBy = 1, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
            var service = new TestPlanService(db);
            
            var all = await service.GetAllAsync();
            
            Assert.Equal(2, all.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectTestPlanOrNull()
        {
            using var db = GetDbContext(nameof(GetByIdAsync_ReturnsCorrectTestPlanOrNull));
            var testPlan = new TestPlan { Name = "TP1", Type = TestPlanType.UserValidation, Description = "Desc1", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            db.TestPlans.Add(testPlan);
            await db.SaveChangesAsync();
            var service = new TestPlanService(db);
            
            var found = await service.GetByIdAsync(testPlan.Id);
            Assert.NotNull(found);
            Assert.Equal(testPlan.Name, found!.Name);
            
            var notFound = await service.GetByIdAsync(999);
            Assert.Null(notFound);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesTestPlan()
        {
            using var db = GetDbContext(nameof(UpdateAsync_UpdatesTestPlan));
            var testPlan = new TestPlan { Name = "Old", Type = TestPlanType.UserValidation, Description = "Old Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            db.TestPlans.Add(testPlan);
            await db.SaveChangesAsync();
            var service = new TestPlanService(db);
            
            var dto = new TestPlanDto { Id = testPlan.Id, Name = "New", Type = "UserValidation", Description = "New Desc", CreatedBy = testPlan.CreatedBy, CreatedAt = testPlan.CreatedAt };
            var updated = await service.UpdateAsync(dto);
            
            Assert.True(updated);
        }

        [Fact]
        public async Task DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse()
        {
            using var db = GetDbContext(nameof(DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse));
            var testPlan = new TestPlan { Name = "ToDelete", Type = TestPlanType.UserValidation, Description = "Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            db.TestPlans.Add(testPlan);
            await db.SaveChangesAsync();
            var service = new TestPlanService(db);
            
            var ok = await service.DeleteAsync(testPlan.Id);
            Assert.True(ok);
            Assert.Empty(await db.TestPlans.ToListAsync());
            
            var fail = await service.DeleteAsync(9999);
            Assert.False(fail);
        }
    }
}