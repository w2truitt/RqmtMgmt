using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using backend;
using System.Collections.Generic;
using System;

namespace backend.ApiTests
{
    public class RoleApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public RoleApiTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CanCreateAndListRoles()
        {
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
            var resp = await _client.DeleteAsync("/api/role/9999999");
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task DuplicateRoleCreateDoesNotFailButReturnsSameRole()
        {
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

    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
