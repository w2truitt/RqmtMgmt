using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using RqmtMgmtShared;
using System.Collections.Generic;
using System;

namespace backend.ApiTests
{
    /// <summary>
    /// API tests for the TestSuite controller endpoints.
    /// These tests run against the actual docker-compose.identity.yml instance with JWT authentication.
    /// </summary>
    [Collection("Integration Tests")]
    public class TestSuiteApiTests : BaseIntegrationTest
    {
        [Fact]
        public async Task CanCreateAndGetTestSuite()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var createDto = new TestSuiteDto
            {
                Name = "API Test Suite",
                Description = "Created by integration test",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var response = await _client.PostAsJsonAsync("/api/testsuite", createDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<TestSuiteDto>(_jsonOptions);
            Assert.NotNull(created);
            Assert.Equal("API Test Suite", created.Name);

            var getResp = await _client.GetAsync($"/api/testsuite/{created.Id}");
            getResp.EnsureSuccessStatusCode();
            var fetched = await getResp.Content.ReadFromJsonAsync<TestSuiteDto>(_jsonOptions);
            Assert.NotNull(fetched);
            Assert.Equal("API Test Suite", fetched.Name);
        }

        [Fact]
        public async Task CanListTestSuites()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var response = await _client.GetAsync("/api/testsuite");
            response.EnsureSuccessStatusCode();
            var list = await response.Content.ReadFromJsonAsync<List<TestSuiteDto>>(_jsonOptions);
            Assert.NotNull(list);
            Assert.True(list.Count > 0);
        }

        [Fact]
        public async Task CanUpdateTestSuite()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First create
            var createDto = new TestSuiteDto
            {
                Name = "Update Test Suite",
                Description = "To be updated",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var response = await _client.PostAsJsonAsync("/api/testsuite", createDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<TestSuiteDto>(_jsonOptions);
            Assert.NotNull(created);

            // Now update
            created.Name = "Updated Name";
            created.Description = "Updated Desc";
            var putResp = await _client.PutAsJsonAsync($"/api/testsuite/{created.Id}", created, _jsonOptions);
            putResp.EnsureSuccessStatusCode();
            
            // Get the updated test suite to verify
            var getResp = await _client.GetAsync($"/api/testsuite/{created.Id}");
            getResp.EnsureSuccessStatusCode();
            var updated = await getResp.Content.ReadFromJsonAsync<TestSuiteDto>(_jsonOptions);
            Assert.NotNull(updated);
            Assert.Equal("Updated Name", updated.Name);
            Assert.Equal("Updated Desc", updated.Description);
        }

        [Fact]
        public async Task CanDeleteTestSuite()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Create
            var createDto = new TestSuiteDto
            {
                Name = "Delete Test Suite",
                Description = "To be deleted",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var response = await _client.PostAsJsonAsync("/api/testsuite", createDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<TestSuiteDto>(_jsonOptions);
            Assert.NotNull(created);

            // Delete
            var delResp = await _client.DeleteAsync($"/api/testsuite/{created.Id}");
            delResp.EnsureSuccessStatusCode();

            // Should not be found
            var getResp = await _client.GetAsync($"/api/testsuite/{created.Id}");
            Assert.False(getResp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task GetNonExistentTestSuiteReturnsNotFound()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var resp = await _client.GetAsync("/api/testsuite/9999999");
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task UpdateNonExistentTestSuiteReturnsNotFound()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var updateDto = new TestSuiteDto
            {
                Id = 9999999,
                Name = "Should Fail",
                Description = "No such test suite",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var resp = await _client.PutAsJsonAsync("/api/testsuite/9999999", updateDto, _jsonOptions);
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task DeleteNonExistentTestSuiteReturnsNotFound()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var resp = await _client.DeleteAsync("/api/testsuite/9999999");
            Assert.False(resp.IsSuccessStatusCode);
        }
    }
}