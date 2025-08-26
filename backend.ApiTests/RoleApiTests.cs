using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using System;
using RqmtMgmtShared;

namespace backend.ApiTests
{
    /// <summary>
    /// Integration tests for the Role API endpoints.
    /// These tests run against the actual docker-compose.identity.yml instance with JWT authentication.
    /// </summary>
    [Collection("Integration Tests")]
    public class RoleApiTests : BaseIntegrationTest
    {
        [Fact]
        public async Task CanCreateAndListRoles()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var roleName = $"apitestrole_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var response = await _client.PostAsJsonAsync("/api/role", roleName);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<RoleDto>();
            Assert.NotNull(created);
            Assert.Equal(roleName, created.Name, StringComparer.OrdinalIgnoreCase);

            var listResp = await _client.GetAsync("/api/role");
            listResp.EnsureSuccessStatusCode();
            var roles = await listResp.Content.ReadFromJsonAsync<List<RoleDto>>();
            Assert.NotNull(roles);
            Assert.Contains(roles, r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task CanDeleteRole()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var roleName = $"apitestrole_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var response = await _client.PostAsJsonAsync("/api/role", roleName);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<RoleDto>();
            Assert.NotNull(created);

            var delResp = await _client.DeleteAsync($"/api/role/{created.Id}");
            delResp.EnsureSuccessStatusCode();

            // Should not be found
            var listResp = await _client.GetAsync("/api/role");
            listResp.EnsureSuccessStatusCode();
            var roles = await listResp.Content.ReadFromJsonAsync<List<RoleDto>>();
            Assert.NotNull(roles);
            Assert.DoesNotContain(roles, r => r.Id == created.Id);
        }

        [Fact]
        public async Task DeleteNonExistentRoleReturnsNotFound()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var resp = await _client.DeleteAsync("/api/role/9999999");
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task DuplicateRoleCreateDoesNotFailButReturnsSameRole()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var roleName = $"apitestrole_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var response = await _client.PostAsJsonAsync("/api/role", roleName);
            response.EnsureSuccessStatusCode();
            var created1 = await response.Content.ReadFromJsonAsync<RoleDto>();
            Assert.NotNull(created1);

            var response2 = await _client.PostAsJsonAsync("/api/role", roleName);
            response2.EnsureSuccessStatusCode();
            var created2 = await response2.Content.ReadFromJsonAsync<RoleDto>();
            Assert.NotNull(created2);
            Assert.Equal(created1.Id, created2.Id);
            Assert.Equal(created1.Name, created2.Name, StringComparer.OrdinalIgnoreCase);
        }
    }
}