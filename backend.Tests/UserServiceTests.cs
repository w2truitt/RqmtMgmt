#nullable enable
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
            var user = new UserDto { UserName = "testuser", Email = "test@example.com" };
            
            var result = await service.CreateAsync(user);
            
            Assert.NotNull(result);
            Assert.Equal("testuser", result.UserName);
            Assert.Equal("test@example.com", result.Email);
            Assert.Single(await db.Users.ToListAsync());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllUsers()
        {
            using var db = GetDbContext(nameof(GetAllAsync_ReturnsAllUsers));
            db.Users.Add(new User { UserName = "user1", Email = "user1@test.com" });
            db.Users.Add(new User { UserName = "user2", Email = "user2@test.com" });
            await db.SaveChangesAsync();
            var service = new UserService(db);
            
            var all = await service.GetAllAsync();
            
            Assert.Equal(2, all.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectUserOrNull()
        {
            using var db = GetDbContext(nameof(GetByIdAsync_ReturnsCorrectUserOrNull));
            var user = new User { UserName = "user1", Email = "user1@test.com" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
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
            var user = new User { UserName = "oldname", Email = "old@test.com" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            var service = new UserService(db);
            
            var dto = new UserDto { Id = user.Id, UserName = "newname", Email = "new@test.com" };
            var updated = await service.UpdateAsync(dto);
            
            Assert.True(updated);
        }

        [Fact]
        public async Task DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse()
        {
            using var db = GetDbContext(nameof(DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse));
            var user = new User { UserName = "todelete", Email = "delete@test.com" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            var service = new UserService(db);
            
            var ok = await service.DeleteAsync(user.Id);
            Assert.True(ok);
            Assert.Empty(await db.Users.ToListAsync());
            
            var fail = await service.DeleteAsync(9999);
            Assert.False(fail);
        }

        [Fact]
        public async Task AssignRoleAsync_AssignsRole()
        {
            using var db = GetDbContext(nameof(AssignRoleAsync_AssignsRole));
            var user = new User { UserName = "user", Email = "user@test.com" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            var service = new UserService(db);
            
            await service.AssignRoleAsync(user.Id, "Admin");
            
            var roles = await service.GetUserRolesAsync(user.Id);
            Assert.Contains("Admin", roles);
        }

        [Fact]
        public async Task RemoveRoleAsync_RemovesRole()
        {
            using var db = GetDbContext(nameof(RemoveRoleAsync_RemovesRole));
            var role = new Role { Name = "Admin" };
            var user = new User { UserName = "user", Email = "user@test.com" };
            db.Roles.Add(role);
            db.Users.Add(user);
            await db.SaveChangesAsync();
            
            var userRole = new UserRole { UserId = user.Id, RoleId = role.Id };
            db.UserRoles.Add(userRole);
            await db.SaveChangesAsync();
            
            var service = new UserService(db);
            
            await service.RemoveRoleAsync(user.Id, "Admin");
            
            var roles = await service.GetUserRolesAsync(user.Id);
            Assert.DoesNotContain("Admin", roles);
        }
    }
}