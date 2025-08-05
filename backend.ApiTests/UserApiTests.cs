using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using backend;
using RqmtMgmtShared;
using System.Collections.Generic;
using System;

namespace backend.ApiTests
{
    public class UserApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public UserApiTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CanListUsers()
        {
            var response = await _client.GetAsync("/api/user");
            response.EnsureSuccessStatusCode();
            var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
            Assert.NotNull(users);
            Assert.True(users.Count > 0); // assumes at least one user exists (e.g., ID 1)
        }

        [Fact]
        public async Task CanGetUserById()
        {
            // Get all users and pick first
            var usersResp = await _client.GetAsync("/api/user");
            usersResp.EnsureSuccessStatusCode();
            var users = await usersResp.Content.ReadFromJsonAsync<List<UserDto>>();
            Assert.NotNull(users);
            Assert.True(users.Count > 0);
            var userId = users[0].Id;

            var getResp = await _client.GetAsync($"/api/user/{userId}");
            getResp.EnsureSuccessStatusCode();
            var user = await getResp.Content.ReadFromJsonAsync<UserDto>();
            Assert.NotNull(user);
            Assert.Equal(userId, user.Id);
        }

        [Fact]
        public async Task GetNonExistentUserReturnsNotFound()
        {
            var resp = await _client.GetAsync("/api/user/9999999");
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task CanCreateAndDeleteUser()
        {
            var createDto = new UserDto
            {
                UserName = $"apitestuser_{Guid.NewGuid().ToString().Substring(0, 8)}",
                Email = $"apitestuser_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
                Roles = new List<string>()
            };
            var response = await _client.PostAsJsonAsync("/api/user", createDto);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<UserDto>();
            Assert.NotNull(created);
            Assert.Equal(createDto.UserName, created.UserName);

            // Delete
            var delResp = await _client.DeleteAsync($"/api/user/{created.Id}");
            delResp.EnsureSuccessStatusCode();

            // Should not be found
            var getResp = await _client.GetAsync($"/api/user/{created.Id}");
            Assert.False(getResp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task CanUpdateUser()
        {
            // Create
            var createDto = new UserDto
            {
                UserName = $"apitestuser_{Guid.NewGuid().ToString().Substring(0, 8)}",
                Email = $"apitestuser_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
                Roles = new List<string>()
            };
            var response = await _client.PostAsJsonAsync("/api/user", createDto);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<UserDto>();
            Assert.NotNull(created);

            // Update
            created.UserName = created.UserName + "_upd";
            created.Email = "updated_" + created.Email;
            var putResp = await _client.PutAsJsonAsync($"/api/user/{created.Id}", created);
            putResp.EnsureSuccessStatusCode();
            var updated = await putResp.Content.ReadFromJsonAsync<UserDto>();
            Assert.NotNull(updated);
            Assert.Equal(created.UserName, updated.UserName);
            Assert.Equal(created.Email, updated.Email);
        }

        [Fact]
        public async Task UpdateNonExistentUserReturnsNotFound()
        {
            var updateDto = new UserDto
            {
                Id = 9999999,
                UserName = "Should Fail",
                Email = "fail@example.com",
                Roles = new List<string>()
            };
            var resp = await _client.PutAsJsonAsync("/api/user/9999999", updateDto);
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task DeleteNonExistentUserReturnsNotFound()
        {
            var resp = await _client.DeleteAsync("/api/user/9999999");
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task CanAssignGetAndRemoveRoles()
        {
            // Create user
            var createDto = new UserDto
            {
                UserName = $"apitestroleuser_{Guid.NewGuid().ToString().Substring(0, 8)}",
                Email = $"apitestroleuser_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
                Roles = new List<string>()
            };
            var response = await _client.PostAsJsonAsync("/api/user", createDto);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<UserDto>();
            Assert.NotNull(created);

            // Assign roles
            var rolesToAssign = new List<string> { "Admin", "QA" };
            var assignResp = await _client.PostAsJsonAsync($"/api/user/{created.Id}/roles", rolesToAssign);
            assignResp.EnsureSuccessStatusCode();

            // Get roles
            var getRolesResp = await _client.GetAsync($"/api/user/{created.Id}/roles");
            getRolesResp.EnsureSuccessStatusCode();
            var roles = await getRolesResp.Content.ReadFromJsonAsync<List<string>>();
            Assert.NotNull(roles);
            Assert.Contains("Admin", roles, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("QA", roles, StringComparer.OrdinalIgnoreCase);

            // Remove role
            var removeResp = await _client.DeleteAsync($"/api/user/{created.Id}/roles/Admin");
            removeResp.EnsureSuccessStatusCode();

            // Get roles again
            var getRolesResp2 = await _client.GetAsync($"/api/user/{created.Id}/roles");
            getRolesResp2.EnsureSuccessStatusCode();
            var roles2 = await getRolesResp2.Content.ReadFromJsonAsync<List<string>>();
            Assert.NotNull(roles2);
            Assert.DoesNotContain("Admin", roles2, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("QA", roles2, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AssignRolesToNonExistentUserReturnsNotFound()
        {
            var rolesToAssign = new List<string> { "GhostRole" };
            var resp = await _client.PostAsJsonAsync("/api/user/9999999/roles", rolesToAssign);
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task RemoveRoleFromNonExistentUserReturnsNotFound()
        {
            var resp = await _client.DeleteAsync("/api/user/9999999/roles/NoRole");
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task RemoveNonExistentRoleReturnsNotFound()
        {
            // Create user
            var createDto = new UserDto
            {
                UserName = $"apitestnoroleuser_{Guid.NewGuid().ToString().Substring(0, 8)}",
                Email = $"apitestnoroleuser_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
                Roles = new List<string>()
            };
            var response = await _client.PostAsJsonAsync("/api/user", createDto);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<UserDto>();
            Assert.NotNull(created);

            // Remove role that doesn't exist
            var removeResp = await _client.DeleteAsync($"/api/user/{created.Id}/roles/FakeRole");
            Assert.False(removeResp.IsSuccessStatusCode);
        }
    }
}
