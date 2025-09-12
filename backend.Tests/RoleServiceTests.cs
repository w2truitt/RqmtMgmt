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

        [Fact]
        public async Task CreateRoleAsync_ReturnsNull_WhenRoleNameIsNull()
        {
            using var db = GetDbContext(nameof(CreateRoleAsync_ReturnsNull_WhenRoleNameIsNull));
            var service = new RoleService(db);
            
            var result = await service.CreateRoleAsync(null!);
            
            Assert.Null(result);
            Assert.Empty(await db.Roles.ToListAsync());
        }

        [Fact]
        public async Task CreateRoleAsync_ReturnsNull_WhenRoleNameIsEmpty()
        {
            using var db = GetDbContext(nameof(CreateRoleAsync_ReturnsNull_WhenRoleNameIsEmpty));
            var service = new RoleService(db);
            
            var result = await service.CreateRoleAsync("");
            
            Assert.Null(result);
            Assert.Empty(await db.Roles.ToListAsync());
        }

        [Fact]
        public async Task CreateRoleAsync_ReturnsNull_WhenRoleNameIsWhitespace()
        {
            using var db = GetDbContext(nameof(CreateRoleAsync_ReturnsNull_WhenRoleNameIsWhitespace));
            var service = new RoleService(db);
            
            var result = await service.CreateRoleAsync("   ");
            
            Assert.Null(result);
            Assert.Empty(await db.Roles.ToListAsync());
        }

        [Fact]
        public async Task CreateRoleAsync_ReturnsExisting_WhenRoleAlreadyExists()
        {
            using var db = GetDbContext(nameof(CreateRoleAsync_ReturnsExisting_WhenRoleAlreadyExists));
            var existingRole = new Role { Name = "ExistingRole" };
            db.Roles.Add(existingRole);
            await db.SaveChangesAsync();
            var service = new RoleService(db);
            
            var result = await service.CreateRoleAsync("ExistingRole");
            
            Assert.NotNull(result);
            Assert.Equal(existingRole.Id, result.Id);
            Assert.Equal("ExistingRole", result.Name);
            Assert.Single(await db.Roles.ToListAsync()); // Still only one role
        }

        [Fact]
        public async Task CreateRoleAsync_HandlesCaseInsensitiveDuplicates()
        {
            using var db = GetDbContext(nameof(CreateRoleAsync_HandlesCaseInsensitiveDuplicates));
            var existingRole = new Role { Name = "Admin" };
            db.Roles.Add(existingRole);
            await db.SaveChangesAsync();
            var service = new RoleService(db);
            
            var result = await service.CreateRoleAsync("ADMIN"); // Different case
            
            Assert.NotNull(result);
            Assert.Equal(existingRole.Id, result.Id);
            Assert.Equal("Admin", result.Name); // Should return original case
            Assert.Single(await db.Roles.ToListAsync()); // Still only one role
        }

        [Fact]
        public async Task DeleteRoleAsync_ReturnsFalse_WhenRoleHasAssignedUsers()
        {
            using var db = GetDbContext(nameof(DeleteRoleAsync_ReturnsFalse_WhenRoleHasAssignedUsers));
            
            var role = new Role { Name = "RoleWithUsers" };
            var user = new User 
            { 
                UserName = "testuser",
                Email = "test@example.com",
                CreatedAt = System.DateTime.UtcNow
            };
            
            db.Roles.Add(role);
            db.Users.Add(user);
            await db.SaveChangesAsync();

            // Create user-role relationship
            var userRole = new UserRole { UserId = user.Id, RoleId = role.Id };
            db.UserRoles.Add(userRole);
            await db.SaveChangesAsync();
            
            var service = new RoleService(db);
            var result = await service.DeleteRoleAsync(role.Id);
            
            Assert.False(result);
            Assert.Single(await db.Roles.ToListAsync()); // Role should still exist
        }

        [Fact]
        public async Task GetAllRolesAsync_ReturnsEmptyList_WhenNoRoles()
        {
            using var db = GetDbContext(nameof(GetAllRolesAsync_ReturnsEmptyList_WhenNoRoles));
            var service = new RoleService(db);
            
            var result = await service.GetAllRolesAsync();
            
            Assert.Empty(result);
        }
    }
}