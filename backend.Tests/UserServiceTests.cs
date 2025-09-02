#nullable enable
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using RqmtMgmtShared;
using System.Linq;
using System.Collections.Generic;

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
        public async Task UpdateAsync_WithNonExistentUser_ReturnsFalse()
        {
            using var db = GetDbContext(nameof(UpdateAsync_WithNonExistentUser_ReturnsFalse));
            var service = new UserService(db);
            
            var dto = new UserDto { Id = 999, UserName = "newname", Email = "new@test.com" };
            var updated = await service.UpdateAsync(dto);
            
            Assert.False(updated);
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("no-at-symbol")]
        [InlineData("@missing-local-part.com")]
        [InlineData("missing-domain@")]
        public async Task UpdateAsync_WithInvalidEmail_ReturnsFalse(string invalidEmail)
        {
            using var db = GetDbContext(nameof(UpdateAsync_WithInvalidEmail_ReturnsFalse) + invalidEmail.GetHashCode());
            var user = new User { UserName = "user", Email = "valid@test.com" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            var service = new UserService(db);
            
            var dto = new UserDto { Id = user.Id, UserName = "user", Email = invalidEmail };
            var updated = await service.UpdateAsync(dto);
            
            Assert.False(updated);
        }

        [Fact]
        public async Task UpdateAsync_WithDuplicateEmail_ReturnsFalse()
        {
            using var db = GetDbContext(nameof(UpdateAsync_WithDuplicateEmail_ReturnsFalse));
            var user1 = new User { UserName = "user1", Email = "user1@test.com" };
            var user2 = new User { UserName = "user2", Email = "user2@test.com" };
            db.Users.AddRange(user1, user2);
            await db.SaveChangesAsync();
            var service = new UserService(db);
            
            var dto = new UserDto { Id = user1.Id, UserName = "user1", Email = "user2@test.com" };
            var updated = await service.UpdateAsync(dto);
            
            Assert.False(updated);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public async Task UpdateAsync_WithInvalidUserName_ReturnsFalse(string invalidUserName)
        {
            using var db = GetDbContext(nameof(UpdateAsync_WithInvalidUserName_ReturnsFalse) + invalidUserName.GetHashCode());
            var user = new User { UserName = "validuser", Email = "valid@test.com" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            var service = new UserService(db);
            
            var dto = new UserDto { Id = user.Id, UserName = invalidUserName, Email = "valid@test.com" };
            var updated = await service.UpdateAsync(dto);
            
            Assert.False(updated);
        }

        [Fact]
        public async Task UpdateAsync_WithValidRoles_UpdatesRolesCorrectly()
        {
            using var db = GetDbContext(nameof(UpdateAsync_WithValidRoles_UpdatesRolesCorrectly));
            var adminRole = new Role { Name = "Admin" };
            var userRole = new Role { Name = "User" };
            db.Roles.AddRange(adminRole, userRole);
            
            var user = new User { UserName = "user", Email = "user@test.com" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            
            var service = new UserService(db);
            
            var dto = new UserDto { Id = user.Id, UserName = "user", Email = "user@test.com", Roles = new List<string> { "Admin", "User" } };
            var updated = await service.UpdateAsync(dto);
            
            Assert.True(updated);
            var roles = await service.GetUserRolesAsync(user.Id);
            Assert.Contains("Admin", roles);
            Assert.Contains("User", roles);
            Assert.Equal(2, roles.Count);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentRoles_IgnoresInvalidRoles()
        {
            using var db = GetDbContext(nameof(UpdateAsync_WithNonExistentRoles_IgnoresInvalidRoles));
            var adminRole = new Role { Name = "Admin" };
            db.Roles.Add(adminRole);
            
            var user = new User { UserName = "user", Email = "user@test.com" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            
            var service = new UserService(db);
            
            var dto = new UserDto { Id = user.Id, UserName = "user", Email = "user@test.com", Roles = new List<string> { "Admin", "NonExistentRole" } };
            var updated = await service.UpdateAsync(dto);
            
            Assert.True(updated);
            var roles = await service.GetUserRolesAsync(user.Id);
            Assert.Contains("Admin", roles);
            Assert.DoesNotContain("NonExistentRole", roles);
            Assert.Single(roles);
        }

        [Fact]
        public async Task UpdateAsync_WithNullRoles_DoesNotUpdateRoles()
        {
            using var db = GetDbContext(nameof(UpdateAsync_WithNullRoles_DoesNotUpdateRoles));
            var adminRole = new Role { Name = "Admin" };
            db.Roles.Add(adminRole);
            
            var user = new User { UserName = "user", Email = "user@test.com" };
            db.Users.Add(user);
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = adminRole.Id, User = user, Role = adminRole });
            await db.SaveChangesAsync();
            
            var service = new UserService(db);
            
            var dto = new UserDto { Id = user.Id, UserName = "newname", Email = "newemail@test.com", Roles = null! };
            var updated = await service.UpdateAsync(dto);
            
            Assert.True(updated);
            var roles = await service.GetUserRolesAsync(user.Id);
            Assert.Contains("Admin", roles);
        }

        [Fact]
        public async Task UpdateAsync_WithEmptyRoles_ClearsAllRoles()
        {
            using var db = GetDbContext(nameof(UpdateAsync_WithEmptyRoles_ClearsAllRoles));
            var adminRole = new Role { Name = "Admin" };
            db.Roles.Add(adminRole);
            
            var user = new User { UserName = "user", Email = "user@test.com" };
            db.Users.Add(user);
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = adminRole.Id, User = user, Role = adminRole });
            await db.SaveChangesAsync();
            
            var service = new UserService(db);
            
            var dto = new UserDto { Id = user.Id, UserName = "user", Email = "user@test.com", Roles = new List<string>() };
            var updated = await service.UpdateAsync(dto);
            
            Assert.True(updated);
            var roles = await service.GetUserRolesAsync(user.Id);
            Assert.Empty(roles);
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

        [Fact]
        public async Task GetByEmailAsync_ReturnsCorrectUser()
        {
            using var db = GetDbContext(nameof(GetByEmailAsync_ReturnsCorrectUser));
            var user = new User { UserName = "testuser", Email = "test@example.com" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            var service = new UserService(db);
            
            var found = await service.GetByEmailAsync("test@example.com");
            
            Assert.NotNull(found);
            Assert.Equal("testuser", found!.UserName);
            Assert.Equal("test@example.com", found.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_WithNonExistentEmail_ReturnsNull()
        {
            using var db = GetDbContext(nameof(GetByEmailAsync_WithNonExistentEmail_ReturnsNull));
            var service = new UserService(db);
            
            var found = await service.GetByEmailAsync("nonexistent@example.com");
            
            Assert.Null(found);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("invalid-email")]
        [InlineData("missing@")]
        [InlineData("@missing.com")]
        public async Task CreateAsync_WithInvalidEmail_ReturnsNull(string invalidEmail)
        {
            using var db = GetDbContext(nameof(CreateAsync_WithInvalidEmail_ReturnsNull) + invalidEmail.GetHashCode());
            var service = new UserService(db);
            var user = new UserDto { UserName = "testuser", Email = invalidEmail };
            
            var result = await service.CreateAsync(user);
            
            Assert.Null(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public async Task CreateAsync_WithInvalidUserName_ReturnsNull(string invalidUserName)
        {
            using var db = GetDbContext(nameof(CreateAsync_WithInvalidUserName_ReturnsNull) + invalidUserName.GetHashCode());
            var service = new UserService(db);
            var user = new UserDto { UserName = invalidUserName, Email = "valid@example.com" };
            
            var result = await service.CreateAsync(user);
            
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateEmail_ReturnsNull()
        {
            using var db = GetDbContext(nameof(CreateAsync_WithDuplicateEmail_ReturnsNull));
            var existingUser = new User { UserName = "existing", Email = "duplicate@example.com" };
            db.Users.Add(existingUser);
            await db.SaveChangesAsync();
            
            var service = new UserService(db);
            var user = new UserDto { UserName = "newuser", Email = "duplicate@example.com" };
            
            var result = await service.CreateAsync(user);
            
            Assert.Null(result);
        }

        [Fact]
        public async Task AssignRoleAsync_WithNonExistentUser_DoesNotThrow()
        {
            using var db = GetDbContext(nameof(AssignRoleAsync_WithNonExistentUser_DoesNotThrow));
            var service = new UserService(db);
            
            // Should not throw exception
            await service.AssignRoleAsync(999, "Admin");
            
            // Verify no roles were created
            Assert.Empty(await db.UserRoles.ToListAsync());
        }

        [Fact]
        public async Task AssignRoleAsync_CreatesRoleIfNotExists()
        {
            using var db = GetDbContext(nameof(AssignRoleAsync_CreatesRoleIfNotExists));
            var user = new User { UserName = "user", Email = "user@test.com" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            var service = new UserService(db);
            
            await service.AssignRoleAsync(user.Id, "NewRole");
            
            var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == "NewRole");
            Assert.NotNull(role);
            
            var roles = await service.GetUserRolesAsync(user.Id);
            Assert.Contains("NewRole", roles);
        }

        [Fact]
        public async Task AssignRoleAsync_DoesNotCreateDuplicateAssignment()
        {
            using var db = GetDbContext(nameof(AssignRoleAsync_DoesNotCreateDuplicateAssignment));
            var role = new Role { Name = "Admin" };
            var user = new User { UserName = "user", Email = "user@test.com" };
            db.Roles.Add(role);
            db.Users.Add(user);
            await db.SaveChangesAsync();
            
            // First assignment
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
            await db.SaveChangesAsync();
            
            var service = new UserService(db);
            
            // Try to assign the same role again
            await service.AssignRoleAsync(user.Id, "Admin");
            
            var userRoles = await db.UserRoles.Where(ur => ur.UserId == user.Id).ToListAsync();
            Assert.Single(userRoles);
        }

        [Fact]
        public async Task RemoveRoleAsync_WithNonExistentUser_DoesNotThrow()
        {
            using var db = GetDbContext(nameof(RemoveRoleAsync_WithNonExistentUser_DoesNotThrow));
            var service = new UserService(db);
            
            // Should not throw exception
            await service.RemoveRoleAsync(999, "Admin");
            
            Assert.Empty(await db.UserRoles.ToListAsync());
        }

        [Fact]
        public async Task RemoveRoleAsync_WithNonExistentRole_DoesNotThrow()
        {
            using var db = GetDbContext(nameof(RemoveRoleAsync_WithNonExistentRole_DoesNotThrow));
            var user = new User { UserName = "user", Email = "user@test.com" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            var service = new UserService(db);
            
            // Should not throw exception
            await service.RemoveRoleAsync(user.Id, "NonExistentRole");
            
            Assert.Empty(await db.UserRoles.ToListAsync());
        }

        [Fact]
        public async Task GetUserRolesAsync_WithNonExistentUser_ReturnsEmptyList()
        {
            using var db = GetDbContext(nameof(GetUserRolesAsync_WithNonExistentUser_ReturnsEmptyList));
            var service = new UserService(db);
            
            var roles = await service.GetUserRolesAsync(999);
            
            Assert.NotNull(roles);
            Assert.Empty(roles);
        }
    }
}