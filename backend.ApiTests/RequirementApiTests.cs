using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using backend;
using RqmtMgmtShared;
using System;
using System.Collections.Generic;

namespace backend.ApiTests
{
    public class RequirementApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public RequirementApiTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CanCreateAndGetRequirement()
        {
            var createDto = new RequirementDto
            {
                Title = "API Requirement",
                Type = "CRS",
                Status = "Draft",
                Description = "Created by API test",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var response = await _client.PostAsJsonAsync("/api/requirement", createDto);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<RequirementDto>();
            Assert.NotNull(created);
            Assert.Equal("API Requirement", created.Title);

            var getResp = await _client.GetAsync($"/api/requirement/{created.Id}");
            getResp.EnsureSuccessStatusCode();
            var fetched = await getResp.Content.ReadFromJsonAsync<RequirementDto>();
            Assert.NotNull(fetched);
            Assert.Equal("API Requirement", fetched.Title);
        }

        [Fact]
        public async Task CanListRequirements()
        {
            var response = await _client.GetAsync("/api/requirement");
            response.EnsureSuccessStatusCode();
            var list = await response.Content.ReadFromJsonAsync<List<RequirementDto>>();
            Assert.NotNull(list);
            Assert.True(list.Count > 0);
        }

        [Fact]
        public async Task CanUpdateRequirement()
        {
            // First create
            var createDto = new RequirementDto
            {
                Title = "Update Requirement",
                Type = "PRS",
                Status = "Draft",
                Description = "To be updated",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var response = await _client.PostAsJsonAsync("/api/requirement", createDto);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<RequirementDto>();
            Assert.NotNull(created);

            // Now update
            created.Title = "Updated Title";
            created.Status = "Approved";
            var putResp = await _client.PutAsJsonAsync($"/api/requirement/{created.Id}", created);
            putResp.EnsureSuccessStatusCode();
            var updated = await putResp.Content.ReadFromJsonAsync<RequirementDto>();
            Assert.NotNull(updated);
            Assert.Equal("Updated Title", updated.Title);
            Assert.Equal("Approved", updated.Status);
        }

        [Fact]
        public async Task CanDeleteRequirement()
        {
            // Create
            var createDto = new RequirementDto
            {
                Title = "Delete Requirement",
                Type = "SRS",
                Status = "Draft",
                Description = "To be deleted",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var response = await _client.PostAsJsonAsync("/api/requirement", createDto);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<RequirementDto>();
            Assert.NotNull(created);

            // Delete
            var delResp = await _client.DeleteAsync($"/api/requirement/{created.Id}");
            delResp.EnsureSuccessStatusCode();

            // Should not be found
            var getResp = await _client.GetAsync($"/api/requirement/{created.Id}");
            Assert.False(getResp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task GetNonExistentRequirementReturnsNotFound()
        {
            var resp = await _client.GetAsync("/api/requirement/9999999");
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task UpdateNonExistentRequirementReturnsNotFound()
        {
            var updateDto = new RequirementDto
            {
                Id = 9999999,
                Title = "Should Fail",
                Type = "CRS",
                Status = "Draft",
                Description = "No such requirement",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var resp = await _client.PutAsJsonAsync("/api/requirement/9999999", updateDto);
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task DeleteNonExistentRequirementReturnsNotFound()
        {
            var resp = await _client.DeleteAsync("/api/requirement/9999999");
            Assert.False(resp.IsSuccessStatusCode);
        }
    }
}
