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
    public class TestPlanApiTests : BaseApiTest
    {
        public TestPlanApiTests(TestWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CanCreateAndGetTestPlan()
        {
            var createDto = new TestPlanDto
            {
                Name = "API Test Plan",
                Type = "UserValidation",
                Description = "Created by integration test",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var response = await _client.PostAsJsonAsync("/api/testplan", createDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<TestPlanDto>(_jsonOptions);
            Assert.NotNull(created);
            Assert.Equal("API Test Plan", created.Name);

            var getResp = await _client.GetAsync($"/api/testplan/{created.Id}");
            getResp.EnsureSuccessStatusCode();
            var fetched = await getResp.Content.ReadFromJsonAsync<TestPlanDto>(_jsonOptions);
            Assert.NotNull(fetched);
            Assert.Equal("API Test Plan", fetched.Name);
        }

        [Fact]
        public async Task CanListTestPlans()
        {
            var response = await _client.GetAsync("/api/testplan");
            response.EnsureSuccessStatusCode();
            var list = await response.Content.ReadFromJsonAsync<List<TestPlanDto>>(_jsonOptions);
            Assert.NotNull(list);
            Assert.True(list.Count > 0);
        }

        [Fact]
        public async Task CanUpdateTestPlan()
        {
            // First create
            var createDto = new TestPlanDto
            {
                Name = "Update Test Plan",
                Type = "SoftwareVerification",
                Description = "To be updated",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var response = await _client.PostAsJsonAsync("/api/testplan", createDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<TestPlanDto>(_jsonOptions);
            Assert.NotNull(created);

            // Now update
            created.Name = "Updated Name";
            created.Description = "Updated Desc";
            var putResp = await _client.PutAsJsonAsync($"/api/testplan/{created.Id}", created, _jsonOptions);
            putResp.EnsureSuccessStatusCode();
            
            // Get the updated test plan to verify
            var getResp = await _client.GetAsync($"/api/testplan/{created.Id}");
            getResp.EnsureSuccessStatusCode();
            var updated = await getResp.Content.ReadFromJsonAsync<TestPlanDto>(_jsonOptions);
            Assert.NotNull(updated);
            Assert.Equal("Updated Name", updated.Name);
            Assert.Equal("Updated Desc", updated.Description);
        }

        [Fact]
        public async Task CanDeleteTestPlan()
        {
            // Create
            var createDto = new TestPlanDto
            {
                Name = "Delete Test Plan",
                Type = "UserValidation",
                Description = "To be deleted",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var response = await _client.PostAsJsonAsync("/api/testplan", createDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<TestPlanDto>(_jsonOptions);
            Assert.NotNull(created);

            // Delete
            var delResp = await _client.DeleteAsync($"/api/testplan/{created.Id}");
            delResp.EnsureSuccessStatusCode();

            // Should not be found
            var getResp = await _client.GetAsync($"/api/testplan/{created.Id}");
            Assert.False(getResp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task GetNonExistentTestPlanReturnsNotFound()
        {
            var resp = await _client.GetAsync("/api/testplan/9999999");
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task UpdateNonExistentTestPlanReturnsNotFound()
        {
            var updateDto = new TestPlanDto
            {
                Id = 9999999,
                Name = "Should Fail",
                Type = "UserValidation",
                Description = "No such test plan",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var resp = await _client.PutAsJsonAsync("/api/testplan/9999999", updateDto, _jsonOptions);
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task DeleteNonExistentTestPlanReturnsNotFound()
        {
            var resp = await _client.DeleteAsync("/api/testplan/9999999");
            Assert.False(resp.IsSuccessStatusCode);
        }
    }
}
