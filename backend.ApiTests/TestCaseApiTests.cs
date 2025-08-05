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
    public class TestCaseApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public TestCaseApiTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CanCreateAndGetTestCase()
        {
            var createDto = new TestCaseDto
            {
                Title = "API Test Case",
                Description = "Created by integration test",
                Steps = new List<TestStepDto> {
                    new TestStepDto { Description = "Step 1", ExpectedResult = "Result 1" },
                    new TestStepDto { Description = "Step 2", ExpectedResult = "Result 2" }
                },
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            // Create
            var response = await _client.PostAsJsonAsync("/api/testcase", createDto);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<TestCaseDto>();
            Assert.NotNull(created);
            Assert.Equal("API Test Case", created.Title);
            Assert.Equal(2, created.Steps.Count);

            // Get by ID
            var getResp = await _client.GetAsync($"/api/testcase/{created.Id}");
            getResp.EnsureSuccessStatusCode();
            var fetched = await getResp.Content.ReadFromJsonAsync<TestCaseDto>();
            Assert.NotNull(fetched);
            Assert.Equal("API Test Case", fetched.Title);
            Assert.Equal(2, fetched.Steps.Count);
        }

        [Fact]
        public async Task CanListTestCases()
        {
            var response = await _client.GetAsync("/api/testcase");
            response.EnsureSuccessStatusCode();
            var list = await response.Content.ReadFromJsonAsync<List<TestCaseDto>>();
            Assert.NotNull(list);
            Assert.True(list.Count > 0);
        }

        [Fact]
        public async Task CanUpdateTestCase()
        {
            // First create
            var createDto = new TestCaseDto
            {
                Title = "Update Test Case",
                Description = "To be updated",
                Steps = new List<TestStepDto> {
                    new TestStepDto { Description = "Original Step", ExpectedResult = "Original Result" }
                },
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var response = await _client.PostAsJsonAsync("/api/testcase", createDto);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<TestCaseDto>();
            Assert.NotNull(created);

            // Now update
            created.Title = "Updated Title";
            created.Description = "Updated Desc";
            created.Steps.Add(new TestStepDto { Description = "Added Step", ExpectedResult = "Added Result" });
            var putResp = await _client.PutAsJsonAsync($"/api/testcase/{created.Id}", created);
            putResp.EnsureSuccessStatusCode();
            var updated = await putResp.Content.ReadFromJsonAsync<TestCaseDto>();
            Assert.NotNull(updated);
            Assert.Equal("Updated Title", updated.Title);
            Assert.Equal("Updated Desc", updated.Description);
            Assert.True(updated.Steps.Count >= 2);
        }

        [Fact]
        public async Task CanDeleteTestCase()
        {
            // Create
            var createDto = new TestCaseDto
            {
                Title = "Delete Test Case",
                Description = "To be deleted",
                Steps = new List<TestStepDto>(),
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var response = await _client.PostAsJsonAsync("/api/testcase", createDto);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<TestCaseDto>();
            Assert.NotNull(created);

            // Delete
            var delResp = await _client.DeleteAsync($"/api/testcase/{created.Id}");
            delResp.EnsureSuccessStatusCode();

            // Should not be found
            var getResp = await _client.GetAsync($"/api/testcase/{created.Id}");
            Assert.False(getResp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task GetNonExistentTestCaseReturnsNotFound()
        {
            var resp = await _client.GetAsync("/api/testcase/9999999");
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task UpdateNonExistentTestCaseReturnsNotFound()
        {
            var updateDto = new TestCaseDto
            {
                Id = 9999999,
                Title = "Should Fail",
                Description = "No such testcase",
                Steps = new List<TestStepDto>(),
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var resp = await _client.PutAsJsonAsync("/api/testcase/9999999", updateDto);
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task DeleteNonExistentTestCaseReturnsNotFound()
        {
            var resp = await _client.DeleteAsync("/api/testcase/9999999");
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task CanAddAndRemoveTestStep()
        {
            // Create testcase
            var createDto = new TestCaseDto
            {
                Title = "Step Add/Remove Test",
                Description = "Integration test for steps",
                Steps = new List<TestStepDto>(),
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var response = await _client.PostAsJsonAsync("/api/testcase", createDto);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<TestCaseDto>();
            Assert.NotNull(created);

            // Add step
            var stepDto = new TestStepDto { Description = "Step X", ExpectedResult = "Result X" };
            var addStepResp = await _client.PostAsJsonAsync($"/api/testcase/{created.Id}/steps", stepDto);
            addStepResp.EnsureSuccessStatusCode();
            var addedStep = await addStepResp.Content.ReadFromJsonAsync<TestStepDto>();
            Assert.NotNull(addedStep);
            Assert.Equal("Step X", addedStep.Description);

            // Get testcase and confirm step is present
            var getResp = await _client.GetAsync($"/api/testcase/{created.Id}");
            getResp.EnsureSuccessStatusCode();
            var fetched = await getResp.Content.ReadFromJsonAsync<TestCaseDto>();
            Assert.NotNull(fetched);
            Assert.Contains(fetched.Steps, s => s.Description == "Step X");

            // Get the actual step ID (by matching description)
            var stepToRemove = fetched.Steps.Find(s => s.Description == "Step X");
            Assert.NotNull(stepToRemove);
            var stepIdProp = stepToRemove.GetType().GetProperty("Id");
            Assert.NotNull(stepIdProp);
            var idValue = stepIdProp.GetValue(stepToRemove);
            Assert.NotNull(idValue);
            var stepId = (int)idValue;

            var removeResp = await _client.DeleteAsync($"/api/testcase/{created.Id}/steps/{stepId}");
            removeResp.EnsureSuccessStatusCode();

            // Get testcase and confirm step is gone
            var getResp2 = await _client.GetAsync($"/api/testcase/{created.Id}");
            getResp2.EnsureSuccessStatusCode();
            var fetched2 = await getResp2.Content.ReadFromJsonAsync<TestCaseDto>();
            Assert.NotNull(fetched2);
            Assert.DoesNotContain(fetched2.Steps, s => s.Description == "Step X");
        }

        [Fact]
        public async Task AddStepToNonExistentTestCaseReturnsNotFound()
        {
            var stepDto = new TestStepDto { Description = "Should Fail", ExpectedResult = "No TC" };
            var resp = await _client.PostAsJsonAsync("/api/testcase/9999999/steps", stepDto);
            Assert.False(resp.IsSuccessStatusCode);
        }

        [Fact]
        public async Task RemoveNonExistentStepReturnsNotFound()
        {
            // Create testcase
            var createDto = new TestCaseDto
            {
                Title = "Remove Step NotFound Test",
                Description = "Integration test for remove step error",
                Steps = new List<TestStepDto>(),
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            var response = await _client.PostAsJsonAsync("/api/testcase", createDto);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<TestCaseDto>();
            Assert.NotNull(created);

            // Try to remove stepId 999999
            var removeResp = await _client.DeleteAsync($"/api/testcase/{created.Id}/steps/999999");
            Assert.False(removeResp.IsSuccessStatusCode);
        }
    }
}
