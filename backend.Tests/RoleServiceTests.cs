#nullable enable
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Linq;
using System.Collections.Generic;

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
        public async Task CreateAsync_AddsRole()
        {
            using var db = GetDbContext(nameof(CreateAsync_AddsRole));
            var service = new RoleService(db);
            var result = await service.CreateAsync("Admin");
            Assert.NotNull(result);
            Assert.Equal("Admin", result.Name);
            Assert.Single(db.Roles);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllRoles()
        {
            using var db = GetDbContext(nameof(GetAllAsync_ReturnsAllRoles));
            db.Roles.Add(new Role { Name = "R1" });
            db.Roles.Add(new Role { Name = "R2" });
            db.SaveChanges();
            var service = new RoleService(db);
            var all = await service.GetAllAsync();
            Assert.Equal(2, all.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectRoleOrNull()
        {
            using var db = GetDbContext(nameof(GetByIdAsync_ReturnsCorrectRoleOrNull));
            var role = new Role { Name = "R1" };
            db.Roles.Add(role);
            db.SaveChanges();
            var service = new RoleService(db);
            var found = await service.GetByIdAsync(role.Id);
            Assert.NotNull(found);
            Assert.Equal(role.Name, found!.Name);
            var notFound = await service.GetByIdAsync(999);
            Assert.Null(notFound);
        }

        [Fact]
        public async Task DeleteAsync_DeletesWhenNoUserRolesElseFalse()
        {
            using var db = GetDbContext(nameof(DeleteAsync_DeletesWhenNoUserRolesElseFalse));
            var role = new Role { Name = "Del" };
            db.Roles.Add(role);
            db.SaveChanges();
            var service = new RoleService(db);
            var ok = await service.DeleteAsync(role.Id);
            Assert.True(ok);
            Assert.Empty(db.Roles);
            // Add with user role
            var role2 = new Role { Name = "WithUser" };
            db.Roles.Add(role2);
            db.SaveChanges();
            db.UserRoles.Add(new UserRole { RoleId = role2.Id, UserId = 1 });
            db.SaveChanges();
            var fail = await service.DeleteAsync(role2.Id);
            Assert.False(fail);
            // Try non-existent
            var fail2 = await service.DeleteAsync(9999);
            Assert.False(fail2);
        }
    }
}
