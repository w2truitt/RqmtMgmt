#nullable enable
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using RqmtMgmtShared;

namespace backend.Tests
{
    public class RoleServiceTests
    {
        private static RqmtMgmtDbContext GetDbContext(string testName)
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: $"RoleServiceTestDb_{testName}_{System.Guid.NewGuid()}")
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        [Fact]
        public async Task CreateRoleAsync_AddsRole()
        {
            using var db = GetDbContext(nameof(CreateRoleAsync_AddsRole));
            var service = new RoleService(db);
            
            var result = await service.CreateRoleAsync("TestRole");
            
            var roles = await db.Roles.ToListAsync();
            Assert.Single(roles);
            Assert.Equal("TestRole", roles[0].Name);
            Assert.NotNull(result);
            Assert.Equal("TestRole", result.Name);
        }

        [Fact]
        public async Task GetAllRolesAsync_ReturnsAllRoles()
        {
            using var db = GetDbContext(nameof(GetAllRolesAsync_ReturnsAllRoles));
            db.Roles.Add(new Role { Name = "Admin" });
            db.Roles.Add(new Role { Name = "User" });
            await db.SaveChangesAsync();
            var service = new RoleService(db);
            
            var all = await service.GetAllRolesAsync();
            Assert.Equal(2, all.Count);
            Assert.Contains(all, r => r.Name == "Admin");
            Assert.Contains(all, r => r.Name == "User");
        }

        [Fact]
        public async Task DeleteRoleAsync_DeletesWhenExists()
        {
            using var db = GetDbContext(nameof(DeleteRoleAsync_DeletesWhenExists));
            var role = new Role { Name = "ToDelete" };
            db.Roles.Add(role);
            await db.SaveChangesAsync();
            var service = new RoleService(db);
            var firstRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "ToDelete");
            var result = await service.DeleteRoleAsync(firstRole!.Id);
            var roles = await db.Roles.ToListAsync();
            Assert.Empty(roles);
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteRoleAsync_DoesNothingWhenNotExists()
        {
            using var db = GetDbContext(nameof(DeleteRoleAsync_DoesNothingWhenNotExists));
            var service = new RoleService(db);
            var result = await service.DeleteRoleAsync(999);
            Assert.False(result);
        }
    }
}