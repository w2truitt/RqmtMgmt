#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests
{
    public class UserServiceTests
    {
        private static RqmtMgmtDbContext GetDbContext(string testName)
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: $"UserServiceTestDb_{testName}_{System.Guid.NewGuid()}")
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_AddsUser()
        {
            using var db = GetDbContext(nameof(CreateAsync_AddsUser));
            var service = new UserService(db);
            var user = new User { UserName = "test@example.com", Email = "test@example.com" };
            var result = await service.CreateAsync(user);
            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.UserName);
            Assert.Single(db.Users);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllUsers()
        {
            using var db = GetDbContext(nameof(GetAllAsync_ReturnsAllUsers));
            db.Users.Add(new User { UserName = "U1", Email = "u1@example.com" });
            db.Users.Add(new User { UserName = "U2", Email = "u2@example.com" });
            db.SaveChanges();
            var service = new UserService(db);
            var all = await service.GetAllAsync();
            Assert.Equal(2, all.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectUserOrNull()
        {
            using var db = GetDbContext(nameof(GetByIdAsync_ReturnsCorrectUserOrNull));
            var user = new User { UserName = "U1", Email = "u1@example.com" };
            db.Users.Add(user);
            db.SaveChanges();
            var service = new UserService(db);
            var found = await service.GetByIdAsync(user.Id);
            Assert.NotNull(found);
            Assert.Equal(user.UserName, found!.UserName);
            var notFound = await service.GetByIdAsync(999);
            Assert.Null(notFound);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesUser()
        {
            using var db = GetDbContext(nameof(UpdateAsync_UpdatesUser));
            var user = new User { UserName = "Old", Email = "old@example.com" };
            db.Users.Add(user);
            db.SaveChanges();
            var service = new UserService(db);
            user.UserName = "New";
            var updated = await service.UpdateAsync(user);
            Assert.Equal("New", updated.UserName);
        }

        [Fact]
        public async Task DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse()
        {
            using var db = GetDbContext(nameof(DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse));
            var user = new User { UserName = "Del", Email = "del@example.com" };
            db.Users.Add(user);
            db.SaveChanges();
            var service = new UserService(db);
            var ok = await service.DeleteAsync(user.Id);
            Assert.True(ok);
            Assert.Empty(db.Users);
            var fail = await service.DeleteAsync(9999);
            Assert.False(fail);
        }

        [Fact]
        public async Task AssignRolesAsync_AssignsRolesToUser()
        {
            using var db = GetDbContext(nameof(AssignRolesAsync_AssignsRolesToUser));
            var user = new User { UserName = "roleuser", Email = "roleuser@example.com" };
            db.Users.Add(user);
            db.Roles.Add(new Role { Name = "Admin" });
            db.SaveChanges();
            var service = new UserService(db);
            var ok = await service.AssignRolesAsync(user.Id, new List<string> { "Admin" });
            Assert.True(ok);
            var dbUser = db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).First(u => u.Id == user.Id);
            Assert.Contains(dbUser.UserRoles, ur => ur.Role.Name == "Admin");
        }

        [Fact]
        public async Task AssignRolesAsync_ReturnsFalse_WhenUserNotFound()
        {
            using var db = GetDbContext(nameof(AssignRolesAsync_ReturnsFalse_WhenUserNotFound));
            var service = new UserService(db);
            var ok = await service.AssignRolesAsync(999, new List<string> { "Admin" });
            Assert.False(ok);
        }

        [Fact]
        public async Task RemoveRoleAsync_RemovesRoleOrReturnsFalse()
        {
            using var db = GetDbContext(nameof(RemoveRoleAsync_RemovesRoleOrReturnsFalse));
            var user = new User { UserName = "user", Email = "user@example.com" };
            var role = new Role { Name = "Role1" };
            db.Users.Add(user);
            db.Roles.Add(role);
            db.SaveChanges();
            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
            db.SaveChanges();
            var service = new UserService(db);
            var ok = await service.RemoveRoleAsync(user.Id, "Role1");
            Assert.True(ok);
            var fail = await service.RemoveRoleAsync(user.Id, "MissingRole");
            Assert.False(fail);
            var fail2 = await service.RemoveRoleAsync(999, "Role1");
            Assert.False(fail2);
        }
    }
}
