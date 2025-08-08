using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using backend;
using RqmtMgmtShared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace backend.ApiTests
{
    /// <summary>
    /// Performance and stress tests for API endpoints
    /// </summary>
    public class PerformanceTests : BaseApiTest
    {
        public PerformanceTests(TestWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CreateMultipleRequirementsPerformanceTest()
        {
            const int requirementCount = 50;
            var stopwatch = Stopwatch.StartNew();
            var createdRequirements = new List<RequirementDto>();

            // Create multiple requirements
            for (int i = 0; i < requirementCount; i++)
            {
                var requirementDto = new RequirementDto
                {
                    Title = $"Performance Test Requirement {i + 1}",
                    Type = RequirementType.CRS,
                    Status = RequirementStatus.Draft,
                    Description = $"Performance test requirement number {i + 1}",
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                };

                var response = await _client.PostAsJsonAsync("/api/requirement", requirementDto, _jsonOptions);
                response.EnsureSuccessStatusCode();
                var created = await response.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
                if (created != null)
                {
                    createdRequirements.Add(created);
                }
            }

            stopwatch.Stop();

            // Performance assertions
            Assert.Equal(requirementCount, createdRequirements.Count);
            Assert.True(stopwatch.ElapsedMilliseconds < 30000, $"Creating {requirementCount} requirements took {stopwatch.ElapsedMilliseconds}ms, expected < 30000ms");
            
            // Test retrieval performance
            var retrievalStopwatch = Stopwatch.StartNew();
            var listResponse = await _client.GetAsync("/api/requirement");
            listResponse.EnsureSuccessStatusCode();
            var allRequirements = await listResponse.Content.ReadFromJsonAsync<List<RequirementDto>>(_jsonOptions);
            retrievalStopwatch.Stop();

            Assert.NotNull(allRequirements);
            Assert.True(allRequirements.Count >= requirementCount);
            Assert.True(retrievalStopwatch.ElapsedMilliseconds < 5000, $"Retrieving requirements took {retrievalStopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
        }

        [Fact]
        public async Task ConcurrentRequestsStressTest()
        {
            const int concurrentRequests = 20;
            var tasks = new List<Task<bool>>();

            // Create concurrent requests
            for (int i = 0; i < concurrentRequests; i++)
            {
                int requestId = i;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var requirementDto = new RequirementDto
                        {
                            Title = $"Concurrent Test Requirement {requestId}",
                            Type = RequirementType.CRS,
                            Status = RequirementStatus.Draft,
                            Description = $"Concurrent test requirement {requestId}",
                            CreatedBy = 1,
                            CreatedAt = DateTime.UtcNow
                        };

                        var response = await _client.PostAsJsonAsync("/api/requirement", requirementDto, _jsonOptions);
                        return response.IsSuccessStatusCode;
                    }
                    catch
                    {
                        return false;
                    }
                }));
            }

            var stopwatch = Stopwatch.StartNew();
            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Verify all requests succeeded
            Assert.True(results.All(r => r), "All concurrent requests should succeed");
            Assert.True(stopwatch.ElapsedMilliseconds < 15000, $"Concurrent requests took {stopwatch.ElapsedMilliseconds}ms, expected < 15000ms");
        }

        [Fact]
        public async Task LargeTestCaseWithManyStepsPerformanceTest()
        {
            const int stepCount = 100;
            var steps = new List<TestStepDto>();

            // Create a test case with many steps
            for (int i = 0; i < stepCount; i++)
            {
                steps.Add(new TestStepDto
                {
                    Description = $"Performance test step {i + 1}: Execute action {i + 1}",
                    ExpectedResult = $"Expected result for step {i + 1}"
                });
            }

            var testCaseDto = new TestCaseDto
            {
                Title = "Large Performance Test Case",
                Description = "Test case with many steps for performance testing",
                SuiteId = 1,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                Steps = steps
            };

            var stopwatch = Stopwatch.StartNew();
            var response = await _client.PostAsJsonAsync("/api/testcase", testCaseDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<TestCaseDto>(_jsonOptions);
            stopwatch.Stop();

            Assert.NotNull(created);
            Assert.Equal(stepCount, created.Steps?.Count ?? 0);
            Assert.True(stopwatch.ElapsedMilliseconds < 10000, $"Creating test case with {stepCount} steps took {stopwatch.ElapsedMilliseconds}ms, expected < 10000ms");

            // Test retrieval performance
            var retrievalStopwatch = Stopwatch.StartNew();
            var getResponse = await _client.GetAsync($"/api/testcase/{created.Id}");
            getResponse.EnsureSuccessStatusCode();
            var retrieved = await getResponse.Content.ReadFromJsonAsync<TestCaseDto>(_jsonOptions);
            retrievalStopwatch.Stop();

            Assert.NotNull(retrieved);
            Assert.Equal(stepCount, retrieved.Steps?.Count ?? 0);
            Assert.True(retrievalStopwatch.ElapsedMilliseconds < 5000, $"Retrieving test case with {stepCount} steps took {retrievalStopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
        }

        [Fact]
        public async Task BulkRequirementVersioningPerformanceTest()
        {
            // Create a requirement
            var requirementDto = new RequirementDto
            {
                Title = "Versioning Performance Test",
                Type = RequirementType.CRS,
                Status = RequirementStatus.Draft,
                Description = "Requirement for versioning performance test",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var createResponse = await _client.PostAsJsonAsync("/api/requirement", requirementDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);

            if (created != null)
            {
                const int updateCount = 20;
                var stopwatch = Stopwatch.StartNew();

                // Perform multiple updates to create versions
                for (int i = 0; i < updateCount; i++)
                {
                    created.Title = $"Versioning Performance Test - Update {i + 1}";
                    created.Description = $"Updated description {i + 1}";
                    
                    var updateResponse = await _client.PutAsJsonAsync($"/api/requirement/{created.Id}", created, _jsonOptions);
                    updateResponse.EnsureSuccessStatusCode();
                }

                stopwatch.Stop();

                // Verify version history performance
                var versionStopwatch = Stopwatch.StartNew();
                var versionsResponse = await _client.GetAsync($"/api/Redline/requirement/{created.Id}/versions");
                versionsResponse.EnsureSuccessStatusCode();
                var versions = await versionsResponse.Content.ReadFromJsonAsync<List<RequirementVersionDto>>(_jsonOptions);
                versionStopwatch.Stop();

                Assert.NotNull(versions);
                Assert.True(versions.Count >= updateCount + 1); // Initial + updates
                Assert.True(stopwatch.ElapsedMilliseconds < 20000, $"Creating {updateCount} versions took {stopwatch.ElapsedMilliseconds}ms, expected < 20000ms");
                Assert.True(versionStopwatch.ElapsedMilliseconds < 3000, $"Retrieving version history took {versionStopwatch.ElapsedMilliseconds}ms, expected < 3000ms");
            }
        }

        [Fact]
        public async Task MassiveDataRetrievalPerformanceTest()
        {
            // First, create some test data
            const int dataCount = 30;
            var createdIds = new List<int>();

            // Create requirements
            for (int i = 0; i < dataCount; i++)
            {
                var req = new RequirementDto
                {
                    Title = $"Mass Data Req {i}",
                    Type = RequirementType.CRS,
                    Status = RequirementStatus.Draft,
                    Description = $"Mass data requirement {i}",
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                };

                var response = await _client.PostAsJsonAsync("/api/requirement", req, _jsonOptions);
                if (response.IsSuccessStatusCode)
                {
                    var created = await response.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
                    if (created != null)
                    {
                        createdIds.Add(created.Id);
                    }
                }
            }

            // Test bulk retrieval performance
            var stopwatch = Stopwatch.StartNew();
            
            // Get all requirements
            var reqResponse = await _client.GetAsync("/api/requirement");
            reqResponse.EnsureSuccessStatusCode();
            var requirements = await reqResponse.Content.ReadFromJsonAsync<List<RequirementDto>>(_jsonOptions);

            // Get all test cases
            var caseResponse = await _client.GetAsync("/api/testcase");
            caseResponse.EnsureSuccessStatusCode();
            var testCases = await caseResponse.Content.ReadFromJsonAsync<List<TestCaseDto>>(_jsonOptions);

            // Get all test suites
            var suiteResponse = await _client.GetAsync("/api/testsuite");
            suiteResponse.EnsureSuccessStatusCode();
            var testSuites = await suiteResponse.Content.ReadFromJsonAsync<List<TestSuiteDto>>(_jsonOptions);

            // Get all test plans
            var planResponse = await _client.GetAsync("/api/testplan");
            planResponse.EnsureSuccessStatusCode();
            var testPlans = await planResponse.Content.ReadFromJsonAsync<List<TestPlanDto>>(_jsonOptions);

            stopwatch.Stop();

            // Performance assertions
            Assert.NotNull(requirements);
            Assert.NotNull(testCases);
            Assert.NotNull(testSuites);
            Assert.NotNull(testPlans);
            Assert.True(stopwatch.ElapsedMilliseconds < 10000, $"Bulk data retrieval took {stopwatch.ElapsedMilliseconds}ms, expected < 10000ms");
        }
    }
}
