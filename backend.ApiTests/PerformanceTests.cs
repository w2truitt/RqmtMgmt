using System.Threading.Tasks;
using Xunit;
using RqmtMgmtShared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Threading;

namespace backend.ApiTests
{
    /// <summary>
    /// Integration performance and stress tests for API endpoints.
    /// These tests run against the actual docker-compose.identity.yml instance with JWT authentication.
    /// </summary>
    [Collection("Integration Tests")]
    public class PerformanceTests : BaseIntegrationTest
    {
        [Fact]
        public async Task CreateMultipleRequirementsPerformanceTest()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            const int requirementCount = 10; // Reduced for integration tests
            var stopwatch = Stopwatch.StartNew();
            var projectId = await GetValidProjectIdAsync();

            // Act
            var tasks = new List<Task>();
            for (int i = 0; i < requirementCount; i++)
            {
                var requirement = new RequirementDto
                {
                    Title = $"Performance Test Requirement {i}",
                    Type = RequirementType.CRD,
                    Status = RequirementStatus.Draft,
                    Description = $"Performance test requirement number {i}",
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow,
                    ProjectId = projectId,
                    SectionId = 1  // Use existing section ID from database
                };

                tasks.Add(_client.PostAsJsonAsync("/api/requirement", requirement, _jsonOptions));
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 30000); // Should complete within 30 seconds
            foreach (var task in tasks.Cast<Task<System.Net.Http.HttpResponseMessage>>())
            {
                var response = await task;
                Assert.True(response.IsSuccessStatusCode);
            }
        }

        [Fact]
        public async Task MassiveDataRetrievalPerformanceTest()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var stopwatch = Stopwatch.StartNew();

            // Act - Get all requirements
            var response = await _client.GetAsync("/api/requirement");
            stopwatch.Stop();

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.True(stopwatch.ElapsedMilliseconds < 10000); // Should complete within 10 seconds

            var requirements = await response.Content.ReadFromJsonAsync<List<RequirementDto>>(_jsonOptions);
            Assert.NotNull(requirements);
        }

        [Fact]
        public async Task BulkRequirementVersioningPerformanceTest()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var projectId = await GetValidProjectIdAsync();
            
            // Create a requirement first
            var requirement = new RequirementDto
            {
                Title = "Versioning Performance Test",
                Type = RequirementType.CRD,
                Status = RequirementStatus.Draft,
                Description = "For versioning performance test",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                ProjectId = projectId,
                    SectionId = 1  // Use existing section ID from database
            };

            var createResponse = await _client.PostAsJsonAsync("/api/requirement", requirement, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);

            const int updateCount = 5; // Reduced for integration tests
            var stopwatch = Stopwatch.StartNew();

            // Act - Update the requirement multiple times to create versions
            for (int i = 0; i < updateCount; i++)
            {
                created!.Description = $"Updated description version {i}";
                var updateResponse = await _client.PutAsJsonAsync($"/api/requirement/{created.Id}", created, _jsonOptions);
                updateResponse.EnsureSuccessStatusCode();
            }

            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 15000); // Should complete within 15 seconds

            // Verify versions were created
            var versionsResponse = await _client.GetAsync($"/api/Redline/requirement/{created!.Id}/versions");
            versionsResponse.EnsureSuccessStatusCode();
            var versions = await versionsResponse.Content.ReadFromJsonAsync<List<RequirementVersionDto>>(_jsonOptions);
            Assert.NotNull(versions);
            Assert.True(versions.Count >= updateCount);
        }

        [Fact]
        public async Task ConcurrentRequestsStressTest()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            const int concurrentRequests = 5; // Reduced for integration tests
            var stopwatch = Stopwatch.StartNew();

            // Act - Make concurrent requests
            var tasks = new List<Task<System.Net.Http.HttpResponseMessage>>();
            for (int i = 0; i < concurrentRequests; i++)
            {
                tasks.Add(_client.GetAsync("/api/requirement"));
            }

            var responses = await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 20000); // Should complete within 20 seconds
            foreach (var response in responses)
            {
                Assert.True(response.IsSuccessStatusCode);
            }
        }

        [Fact]
        public async Task LargeTestCaseWithManyStepsPerformanceTest()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Create a project first to satisfy foreign key constraint
            var projectDto = new CreateProjectDto
            {
                Name = $"Performance Test Project {DateTime.Now.Ticks}",
                Code = $"PTP{DateTime.Now.Ticks % 10000}",
                Description = "Project for performance testing",
                OwnerId = 1
            };
            var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectDto, _jsonOptions);
            projectResponse.EnsureSuccessStatusCode();
            var project = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>(_jsonOptions);
            Assert.NotNull(project);

            // Create a test suite first
            var testSuite = new TestSuiteDto
            {
                Name = "Performance Test Suite",
                Description = "For performance testing",
                ProjectId = project.Id,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var suiteResponse = await _client.PostAsJsonAsync("/api/testsuite", testSuite, _jsonOptions);
            suiteResponse.EnsureSuccessStatusCode();
            var createdSuite = await suiteResponse.Content.ReadFromJsonAsync<TestSuiteDto>(_jsonOptions);

            // Create test case with many steps
            const int stepCount = 20; // Reduced for integration tests
            var steps = new List<TestStepDto>();
            for (int i = 0; i < stepCount; i++)
            {
                steps.Add(new TestStepDto
                {
                    Description = $"Performance test step {i}",
                    ExpectedResult = $"Expected result for step {i}"
                });
            }

            var testCase = new TestCaseDto
            {
                Title = "Large Performance Test Case",
                Description = "Test case with many steps for performance testing",
                SuiteId = createdSuite!.Id,
                Steps = steps,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

            var stopwatch = Stopwatch.StartNew();

            // Act
            var response = await _client.PostAsJsonAsync("/api/testcase", testCase, _jsonOptions);
            stopwatch.Stop();

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.True(stopwatch.ElapsedMilliseconds < 10000); // Should complete within 10 seconds

            var created = await response.Content.ReadFromJsonAsync<TestCaseDto>(_jsonOptions);
            Assert.NotNull(created);
            Assert.Equal(stepCount, created.Steps.Count);
        }

        /// <summary>
        /// Helper method to get a valid project ID for testing.
        /// </summary>
        private async Task<int> GetValidProjectIdAsync()
        {
            var response = await _client.GetAsync("/api/projects");
            response.EnsureSuccessStatusCode();
            var projects = await response.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>(_jsonOptions);
            
            if (projects?.Items?.Count == 0)
            {
                // Create a test project if none exist
                var createDto = new CreateProjectDto
                {
                    Name = $"Test Project for Performance {Guid.NewGuid():N}",
                    Code = $"PERF{DateTime.UtcNow:mmss}",
                    Description = "Auto-created for performance testing",
                    OwnerId = 1,
                    Status = ProjectStatus.Planning
                };

                var createResponse = await _client.PostAsJsonAsync("/api/projects", createDto, _jsonOptions);
                createResponse.EnsureSuccessStatusCode();
                var created = await createResponse.Content.ReadFromJsonAsync<ProjectDto>(_jsonOptions);
                return created!.Id;
            }

            return projects!.Items![0].Id;
        }
    }
}