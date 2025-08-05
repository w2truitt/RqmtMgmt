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
    public class TestCaseServiceTests
    {
        private static RqmtMgmtDbContext GetDbContext(string testName)
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestCaseServiceTestDb_{testName}_{System.Guid.NewGuid()}")
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_AddsTestCase()
        {
            using var db = GetDbContext(nameof(CreateAsync_AddsTestCase));
            var service = new TestCaseService(db);
            var testCase = new TestCaseDto { Title = "Test Case", Description = "Description", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            
            var result = await service.CreateAsync(testCase);
            
            Assert.NotNull(result);
            Assert.Equal("Test Case", result.Title);
            Assert.Single(await db.TestCases.ToListAsync());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllTestCases()
        {
            using var db = GetDbContext(nameof(GetAllAsync_ReturnsAllTestCases));
            db.TestCases.Add(new TestCase { Title = "TC1", Description = "Desc1", CreatedBy = 1, CreatedAt = DateTime.UtcNow });
            db.TestCases.Add(new TestCase { Title = "TC2", Description = "Desc2", CreatedBy = 1, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
            var service = new TestCaseService(db);
            
            var all = await service.GetAllAsync();
            
            Assert.Equal(2, all.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectTestCaseOrNull()
        {
            using var db = GetDbContext(nameof(GetByIdAsync_ReturnsCorrectTestCaseOrNull));
            var testCase = new TestCase { Title = "TC1", Description = "Desc1", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            db.TestCases.Add(testCase);
            await db.SaveChangesAsync();
            var service = new TestCaseService(db);
            
            var found = await service.GetByIdAsync(testCase.Id);
            Assert.NotNull(found);
            Assert.Equal(testCase.Title, found!.Title);
            
            var notFound = await service.GetByIdAsync(999);
            Assert.Null(notFound);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesTestCase()
        {
            using var db = GetDbContext(nameof(UpdateAsync_UpdatesTestCase));
            var testCase = new TestCase { Title = "Old", Description = "Old Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            db.TestCases.Add(testCase);
            await db.SaveChangesAsync();
            var service = new TestCaseService(db);
            
            var dto = new TestCaseDto { Id = testCase.Id, Title = "New", Description = "New Desc", CreatedBy = testCase.CreatedBy, CreatedAt = testCase.CreatedAt };
            var updated = await service.UpdateAsync(dto);
            
            Assert.True(updated);
        }

        [Fact]
        public async Task DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse()
        {
            using var db = GetDbContext(nameof(DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse));
            var testCase = new TestCase { Title = "ToDelete", Description = "Desc", CreatedBy = 1, CreatedAt = DateTime.UtcNow };
            db.TestCases.Add(testCase);
            await db.SaveChangesAsync();
            var service = new TestCaseService(db);
            
            var ok = await service.DeleteAsync(testCase.Id);
            Assert.True(ok);
            Assert.Empty(await db.TestCases.ToListAsync());
            
            var fail = await service.DeleteAsync(9999);
            Assert.False(fail);
        }
    }
}