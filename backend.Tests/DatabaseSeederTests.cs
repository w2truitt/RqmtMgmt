using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests
{
    public class DatabaseSeederTests
    {
        private RqmtMgmtDbContext GetDbContext(string name)
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: $"DbSeedTest_{name}_{System.Guid.NewGuid()}")
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        [Fact]
        public async Task SeedAsync_SetsUpRolesAndAdminUser()
        {
            using var db = GetDbContext(nameof(SeedAsync_SetsUpRolesAndAdminUser));
            await DatabaseSeeder.SeedAsync(db, false);
            Assert.True(await db.Roles.AnyAsync());
            Assert.True(await db.Users.AnyAsync());
            Assert.True(await db.UserRoles.AnyAsync());
            var admin = await db.Users.FirstAsync();
            Assert.Equal("admin", admin.UserName);
            Assert.Equal("admin@example.com", admin.Email);
        }

        [Fact]
        public async Task SeedAsync_WithTestData_SeedsRequirementsAndTestCases()
        {
            using var db = GetDbContext(nameof(SeedAsync_WithTestData_SeedsRequirementsAndTestCases));
            await DatabaseSeeder.SeedAsync(db, true);
            Assert.True(await db.Projects.AnyAsync());
            Assert.True(await db.Requirements.AnyAsync());
            Assert.True(await db.TestSuites.AnyAsync());
            Assert.True(await db.TestCases.AnyAsync());
            Assert.True(await db.TestSteps.AnyAsync());
        }

        [Fact]
        public async Task SeedAsync_Idempotent_DoesNotDuplicateData()
        {
            using var db = GetDbContext(nameof(SeedAsync_Idempotent_DoesNotDuplicateData));
            await DatabaseSeeder.SeedAsync(db, true);
            var userCount1 = await db.Users.CountAsync();
            var roleCount1 = await db.Roles.CountAsync();
            await DatabaseSeeder.SeedAsync(db, true);
            Assert.Equal(userCount1, await db.Users.CountAsync());
            Assert.Equal(roleCount1, await db.Roles.CountAsync());
        }
    }
}
