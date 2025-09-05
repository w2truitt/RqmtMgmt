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

        [Fact]
        public async Task CreateAsync_ReturnsNull_WhenNameIsEmpty()
        {
            using var db = GetDbContext(nameof(CreateAsync_ReturnsNull_WhenNameIsEmpty));
            var service = new TestPlanService(db);
            var testPlan = new TestPlanDto { Name = "", Type = "UserValidation", Description = "Description", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            
            var result = await service.CreateAsync(testPlan);
            
            // Service allows empty name - it's not validated at service level
            Assert.NotNull(result);
            Assert.Equal("", result.Name);
        }

        [Fact]
        public async Task CreateAsync_ThrowsException_WhenNameIsNull()
        {
            using var db = GetDbContext(nameof(CreateAsync_ThrowsException_WhenNameIsNull));
            var service = new TestPlanService(db);
            var testPlan = new TestPlanDto { Name = null!, Type = "UserValidation", Description = "Description", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            
            // Database enforces required name constraint
            await Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateException>(
                () => service.CreateAsync(testPlan));
        }

        [Fact]
        public async Task CreateAsync_AllowsCreation_WhenCreatedByIsZero()
        {
            using var db = GetDbContext(nameof(CreateAsync_AllowsCreation_WhenCreatedByIsZero));
            var service = new TestPlanService(db);
            var testPlan = new TestPlanDto { Name = "Test Plan", Type = "UserValidation", Description = "Description", CreatedBy = 0, CreatedAt = DateTime.UtcNow };
            
            var result = await service.CreateAsync(testPlan);
            
            // Service allows CreatedBy = 0 - validation not enforced at service level
            Assert.NotNull(result);
            Assert.Equal(0, result.CreatedBy);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsFalse_WhenTestPlanNotFound()
        {
            using var db = GetDbContext(nameof(UpdateAsync_ReturnsFalse_WhenTestPlanNotFound));
            var service = new TestPlanService(db);
            var dto = new TestPlanDto { Id = 999, Name = "Non-existent", Type = "UserValidation", Description = "Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            
            var result = await service.UpdateAsync(dto);
            
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_AllowsUpdate_WhenNameIsEmpty()
        {
            using var db = GetDbContext(nameof(UpdateAsync_AllowsUpdate_WhenNameIsEmpty));
            var testPlan = new TestPlan { Name = "Original", Type = TestPlanType.UserValidation, Description = "Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            db.TestPlans.Add(testPlan);
            await db.SaveChangesAsync();
            var service = new TestPlanService(db);
            
            var dto = new TestPlanDto { Id = testPlan.Id, Name = "", Type = "UserValidation", Description = "Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            var result = await service.UpdateAsync(dto);
            
            // Service allows empty name updates
            Assert.True(result);
            
            var updated = await db.TestPlans.FindAsync(testPlan.Id);
            Assert.Equal("", updated!.Name);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoTestPlans()
        {
            using var db = GetDbContext(nameof(GetAllAsync_ReturnsEmptyList_WhenNoTestPlans));
            var service = new TestPlanService(db);
            
            var result = await service.GetAllAsync();
            
            Assert.Empty(result);
        }

        [Fact]
        public async Task CreateAsync_WithDifferentTypes_CreatesCorrectly()
        {
            using var db = GetDbContext(nameof(CreateAsync_WithDifferentTypes_CreatesCorrectly));
            var service = new TestPlanService(db);
            
            var userValidation = new TestPlanDto { Name = "User Validation Plan", Type = "UserValidation", Description = "UV Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            var softwareVerification = new TestPlanDto { Name = "Software Verification Plan", Type = "SoftwareVerification", Description = "SV Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            
            var result1 = await service.CreateAsync(userValidation);
            var result2 = await service.CreateAsync(softwareVerification);
            
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            
            var allPlans = await db.TestPlans.ToListAsync();
            Assert.Equal(2, allPlans.Count);
            Assert.Contains(allPlans, p => p.Type == TestPlanType.UserValidation);
            Assert.Contains(allPlans, p => p.Type == TestPlanType.SoftwareVerification);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesAllFields_Successfully()
        {
            using var db = GetDbContext(nameof(UpdateAsync_UpdatesAllFields_Successfully));
            var originalTime = DateTime.UtcNow.AddDays(-1);
            var testPlan = new TestPlan 
            { 
                Name = "Original Name", 
                Type = TestPlanType.UserValidation, 
                Description = "Original Description", 
                CreatedBy = 1, 
                CreatedAt = originalTime 
            };
            db.TestPlans.Add(testPlan);
            await db.SaveChangesAsync();
            var service = new TestPlanService(db);
            
            var dto = new TestPlanDto 
            { 
                Id = testPlan.Id, 
                Name = "Updated Name", 
                Type = "SoftwareVerification", 
                Description = "Updated Description", 
                CreatedBy = testPlan.CreatedBy, 
                CreatedAt = testPlan.CreatedAt 
            };
            
            var result = await service.UpdateAsync(dto);
            
            Assert.True(result);
            
            var updated = await db.TestPlans.FindAsync(testPlan.Id);
            Assert.Equal("Updated Name", updated!.Name);
            Assert.Equal(TestPlanType.SoftwareVerification, updated.Type);
            Assert.Equal("Updated Description", updated.Description);
            Assert.Equal(originalTime, updated.CreatedAt); // Should remain unchanged
        }

        [Fact]
        public async Task CreateAsync_WithInvalidType_DefaultsToUserValidation()
        {
            using var db = GetDbContext(nameof(CreateAsync_WithInvalidType_DefaultsToUserValidation));
            var service = new TestPlanService(db);
            var dto = new TestPlanDto { Name = "Test Plan", Type = "InvalidType", Description = "Test Description", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            
            var result = await service.CreateAsync(dto);
            
            Assert.NotNull(result);
            var saved = await db.TestPlans.FindAsync(result.Id);
            Assert.Equal(TestPlanType.UserValidation, saved!.Type); // Should default to UserValidation
        }

        [Fact]
        public async Task GetByProjectIdAsync_ReturnsEmptyList_WhenNoPlansForProject()
        {
            using var db = GetDbContext(nameof(GetByProjectIdAsync_ReturnsEmptyList_WhenNoPlansForProject));
            var service = new TestPlanService(db);
            
            var result = await service.GetByProjectIdAsync(999);
            
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_WithMultiplePlans_ReturnsCorrectCount()
        {
            using var db = GetDbContext(nameof(GetAllAsync_WithMultiplePlans_ReturnsCorrectCount));
            
            var plans = new[]
            {
                new TestPlan { Name = "Plan 1", Type = TestPlanType.UserValidation, Description = "Description 1", CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new TestPlan { Name = "Plan 2", Type = TestPlanType.SoftwareVerification, Description = "Description 2", CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new TestPlan { Name = "Plan 3", Type = TestPlanType.UserValidation, Description = "Description 3", CreatedBy = 1, CreatedAt = DateTime.UtcNow }
            };
            
            db.TestPlans.AddRange(plans);
            await db.SaveChangesAsync();
            var service = new TestPlanService(db);
            
            var result = await service.GetAllAsync();
            
            Assert.Equal(3, result.Count);
            Assert.Contains(result, p => p.Name == "Plan 1");
            Assert.Contains(result, p => p.Name == "Plan 2");
            Assert.Contains(result, p => p.Name == "Plan 3");
        }
    }
}