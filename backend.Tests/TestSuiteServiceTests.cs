#nullable enable
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

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
            var suite = new TestSuite { Name = "Suite1" };
            var result = await service.CreateAsync(suite);
            Assert.NotNull(result);
            Assert.Equal("Suite1", result.Name);
            Assert.Single(db.TestSuites);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllTestSuites()
        {
            using var db = GetDbContext(nameof(GetAllAsync_ReturnsAllTestSuites));
            db.TestSuites.Add(new TestSuite { Name = "S1" });
            db.TestSuites.Add(new TestSuite { Name = "S2" });
            db.SaveChanges();
            var service = new TestSuiteService(db);
            var all = await service.GetAllAsync();
            Assert.Equal(2, System.Linq.Enumerable.Count(all));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectTestSuiteOrNull()
        {
            using var db = GetDbContext(nameof(GetByIdAsync_ReturnsCorrectTestSuiteOrNull));
            var suite = new TestSuite { Name = "S1" };
            db.TestSuites.Add(suite);
            db.SaveChanges();
            var service = new TestSuiteService(db);
            var found = await service.GetByIdAsync(suite.Id);
            Assert.NotNull(found);
            Assert.Equal(suite.Name, found!.Name);
            var notFound = await service.GetByIdAsync(999);
            Assert.Null(notFound);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesTestSuite()
        {
            using var db = GetDbContext(nameof(UpdateAsync_UpdatesTestSuite));
            var suite = new TestSuite { Name = "Old" };
            db.TestSuites.Add(suite);
            db.SaveChanges();
            var service = new TestSuiteService(db);
            suite.Name = "New";
            var updated = await service.UpdateAsync(suite);
            Assert.Equal("New", updated.Name);
        }

        [Fact]
        public async Task DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse()
        {
            using var db = GetDbContext(nameof(DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse));
            var suite = new TestSuite { Name = "Del" };
            db.TestSuites.Add(suite);
            db.SaveChanges();
            var service = new TestSuiteService(db);
            var ok = await service.DeleteAsync(suite.Id);
            Assert.True(ok);
            Assert.Empty(db.TestSuites);
            var fail = await service.DeleteAsync(9999);
            Assert.False(fail);
        }
    }
}
