using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using backend;
using RqmtMgmtShared;
using System.Collections.Generic;

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
    }
}
