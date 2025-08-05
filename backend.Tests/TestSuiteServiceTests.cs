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
    public class TestSuiteServiceTests
    {
        private static RqmtMgmtDbContext GetDbContext(string testName)
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestSuiteServiceTestDb_{testName}_{System.Guid.NewGuid()}")
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_AddsTestSuite()
        {
            using var db = GetDbContext(nameof(CreateAsync_AddsTestSuite));
            var service = new TestSuiteService(db);
            var testSuite = new TestSuiteDto { Name = "Test Suite", Description = "Description", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            
            var result = await service.CreateAsync(testSuite);
            
            Assert.NotNull(result);
            Assert.Equal("Test Suite", result.Name);
            Assert.Single(await db.TestSuites.ToListAsync());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllTestSuites()
        {
            using var db = GetDbContext(nameof(GetAllAsync_ReturnsAllTestSuites));
            db.TestSuites.Add(new TestSuite { Name = "TS1", Description = "Desc1", CreatedBy = 1, CreatedAt = DateTime.UtcNow });
            db.TestSuites.Add(new TestSuite { Name = "TS2", Description = "Desc2", CreatedBy = 1, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
            var service = new TestSuiteService(db);
            
            var all = await service.GetAllAsync();
            
            Assert.Equal(2, all.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectTestSuiteOrNull()
        {
            using var db = GetDbContext(nameof(GetByIdAsync_ReturnsCorrectTestSuiteOrNull));
            var testSuite = new TestSuite { Name = "TS1", Description = "Desc1", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            db.TestSuites.Add(testSuite);
            await db.SaveChangesAsync();
            var service = new TestSuiteService(db);
            
            var found = await service.GetByIdAsync(testSuite.Id);
            Assert.NotNull(found);
            Assert.Equal(testSuite.Name, found!.Name);
            
            var notFound = await service.GetByIdAsync(999);
            Assert.Null(notFound);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesTestSuite()
        {
            using var db = GetDbContext(nameof(UpdateAsync_UpdatesTestSuite));
            var testSuite = new TestSuite { Name = "Old", Description = "Old Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            db.TestSuites.Add(testSuite);
            await db.SaveChangesAsync();
            var service = new TestSuiteService(db);
            
            var dto = new TestSuiteDto { Id = testSuite.Id, Name = "New", Description = "New Desc", CreatedBy = testSuite.CreatedBy, CreatedAt = testSuite.CreatedAt };
            var updated = await service.UpdateAsync(dto);
            
            Assert.True(updated);
        }

        [Fact]
        public async Task DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse()
        {
            using var db = GetDbContext(nameof(DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse));
            var testSuite = new TestSuite { Name = "ToDelete", Description = "Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            db.TestSuites.Add(testSuite);
            await db.SaveChangesAsync();
            var service = new TestSuiteService(db);
            
            var ok = await service.DeleteAsync(testSuite.Id);
            Assert.True(ok);
            Assert.Empty(await db.TestSuites.ToListAsync());
            
            var fail = await service.DeleteAsync(9999);
            Assert.False(fail);
        }
    }
}