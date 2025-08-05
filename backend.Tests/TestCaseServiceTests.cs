#nullable enable
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

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
            var tc = new TestCase { Title = "Test Case" };
            var result = await service.CreateAsync(tc);
            Assert.NotNull(result);
            Assert.Equal("Test Case", result.Title);
            Assert.Single(db.TestCases);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllTestCases()
        {
            using var db = GetDbContext(nameof(GetAllAsync_ReturnsAllTestCases));
            db.TestCases.Add(new TestCase { Title = "TC1" });
            db.TestCases.Add(new TestCase { Title = "TC2" });
            db.SaveChanges();
            var service = new TestCaseService(db);
            var all = await service.GetAllAsync();
            Assert.Equal(2, System.Linq.Enumerable.Count(all));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectTestCaseOrNull()
        {
            using var db = GetDbContext(nameof(GetByIdAsync_ReturnsCorrectTestCaseOrNull));
            var tc = new TestCase { Title = "TC1" };
            db.TestCases.Add(tc);
            db.SaveChanges();
            var service = new TestCaseService(db);
            var found = await service.GetByIdAsync(tc.Id);
            Assert.NotNull(found);
            Assert.Equal(tc.Title, found!.Title);
            var notFound = await service.GetByIdAsync(999);
            Assert.Null(notFound);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesTestCase()
        {
            using var db = GetDbContext(nameof(UpdateAsync_UpdatesTestCase));
            var tc = new TestCase { Title = "Old" };
            db.TestCases.Add(tc);
            db.SaveChanges();
            var service = new TestCaseService(db);
            tc.Title = "New";
            var updated = await service.UpdateAsync(tc);
            Assert.Equal("New", updated.Title);
        }

        [Fact]
        public async Task DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse()
        {
            using var db = GetDbContext(nameof(DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse));
            var tc = new TestCase { Title = "Del" };
            db.TestCases.Add(tc);
            db.SaveChanges();
            var service = new TestCaseService(db);
            var ok = await service.DeleteAsync(tc.Id);
            Assert.True(ok);
            Assert.Empty(db.TestCases);
            var fail = await service.DeleteAsync(9999);
            Assert.False(fail);
        }
    }
}
