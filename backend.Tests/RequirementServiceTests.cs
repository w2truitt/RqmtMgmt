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
        [Fact]
        public async Task CreateAsync_AddsRequirement()
        {
            using var db = TestDataHelper.GetDbContext(nameof(CreateAsync_AddsRequirement));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);
            var service = new RequirementService(db);
            
            var req = TestDataHelper.CreateTestRequirementDto(project.Id, user.Id, "Test Requirement");
            var result = await service.CreateAsync(req);
            
            Assert.NotNull(result);
            Assert.Equal("Test Requirement", result.Title);
            Assert.Equal(project.Id, result.ProjectId);
            Assert.Single(await db.Requirements.ToListAsync());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllRequirements()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetAllAsync_ReturnsAllRequirements));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);
            
            db.Requirements.Add(TestDataHelper.CreateTestRequirement(project.Id, user.Id, "R1"));
            db.Requirements.Add(TestDataHelper.CreateTestRequirement(project.Id, user.Id, "R2"));
            await db.SaveChangesAsync();
            
            var service = new RequirementService(db);
            var all = await service.GetAllAsync();
            
            Assert.Equal(2, all.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectRequirementOrNull()
        {
            using var db = TestDataHelper.GetDbContext(nameof(GetByIdAsync_ReturnsCorrectRequirementOrNull));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);
            
            var req = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "R1");
            db.Requirements.Add(req);
            await db.SaveChangesAsync();
            
            var service = new RequirementService(db);
            var found = await service.GetByIdAsync(req.Id);
            
            Assert.NotNull(found);
            Assert.Equal(req.Title, found!.Title);
            Assert.Equal(project.Id, found.ProjectId);
            
            var notFound = await service.GetByIdAsync(999);
            Assert.Null(notFound);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesRequirement()
        {
            using var db = TestDataHelper.GetDbContext(nameof(UpdateAsync_UpdatesRequirement));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);
            
            var req = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Old Title");
            db.Requirements.Add(req);
            await db.SaveChangesAsync();
            
            var service = new RequirementService(db);
            var dto = new RequirementDto 
            { 
                Id = req.Id, 
                Title = "New Title", 
                Type = req.Type, 
                Status = req.Status, 
                ProjectId = project.Id,
                CreatedBy = req.CreatedBy, 
                CreatedAt = req.CreatedAt 
            };
            
            var updated = await service.UpdateAsync(dto);
            
            Assert.True(updated);
        }

        [Fact]
        public async Task DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse()
        {
            using var db = TestDataHelper.GetDbContext(nameof(DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse));
            var (user, project) = await TestDataHelper.SetupBasicTestDataAsync(db);
            
            var req = TestDataHelper.CreateTestRequirement(project.Id, user.Id, "Delete Me");
            db.Requirements.Add(req);
            await db.SaveChangesAsync();
            
            var service = new RequirementService(db);
            var ok = await service.DeleteAsync(req.Id);
            
            Assert.True(ok);
            Assert.Empty(await db.Requirements.ToListAsync());
            
            var fail = await service.DeleteAsync(9999);
            Assert.False(fail);
        }
    }
}