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
    public class DashboardApiTests : BaseApiTest
    {
        public DashboardApiTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CanGetDashboardStatistics()
        {
            var response = await _client.GetAsync("/api/dashboard/statistics");
            response.EnsureSuccessStatusCode();
            var statistics = await response.Content.ReadFromJsonAsync<DashboardStatisticsDto>(_jsonOptions);
            
            Assert.NotNull(statistics);
            Assert.True(statistics.Requirements.Total >= 0);
            Assert.True(statistics.TestCases.Total >= 0);
            Assert.True(statistics.TestPlans.Total >= 0);
            Assert.True(statistics.TestSuites.Total >= 0);
        }

        [Fact]
        public async Task CanGetEnhancedDashboardStatistics()
        {
            var response = await _client.GetAsync("/api/dashboard/enhanced-statistics");
            response.EnsureSuccessStatusCode();
            var statistics = await response.Content.ReadFromJsonAsync<DashboardStatsDto>(_jsonOptions);
            
            Assert.NotNull(statistics);
            Assert.NotNull(statistics.Requirements);
            Assert.NotNull(statistics.TestManagement);
            Assert.NotNull(statistics.TestExecution);
            Assert.NotNull(statistics.RecentActivities);
            Assert.True(statistics.Requirements.TotalRequirements >= 0);
            Assert.True(statistics.TestManagement.TotalTestCases >= 0);
            Assert.True(statistics.TestExecution.TotalTestRuns >= 0);
        }

        [Fact]
        public async Task CanGetRequirementStats()
        {
            var response = await _client.GetAsync("/api/dashboard/requirements-stats");
            response.EnsureSuccessStatusCode();
            var stats = await response.Content.ReadFromJsonAsync<RequirementStatsDto>(_jsonOptions);
            
            Assert.NotNull(stats);
            Assert.True(stats.TotalRequirements >= 0);
            Assert.True(stats.DraftRequirements >= 0);
            Assert.True(stats.ApprovedRequirements >= 0);
            Assert.True(stats.ImplementedRequirements >= 0);
            Assert.True(stats.VerifiedRequirements >= 0);
            Assert.NotNull(stats.ByType);
            Assert.NotNull(stats.ByStatus);
        }

        [Fact]
        public async Task CanGetTestManagementStats()
        {
            var response = await _client.GetAsync("/api/dashboard/test-management-stats");
            response.EnsureSuccessStatusCode();
            var stats = await response.Content.ReadFromJsonAsync<TestManagementStatsDto>(_jsonOptions);
            
            Assert.NotNull(stats);
            Assert.True(stats.TotalTestSuites >= 0);
            Assert.True(stats.TotalTestPlans >= 0);
            Assert.True(stats.TotalTestCases >= 0);
            Assert.True(stats.TestCasesWithSteps >= 0);
            Assert.True(stats.RequirementTestCaseLinks >= 0);
            Assert.True(stats.TestCoveragePercentage >= 0 && stats.TestCoveragePercentage <= 100);
        }

        [Fact]
        public async Task CanGetTestExecutionStats()
        {
            var response = await _client.GetAsync("/api/dashboard/test-execution-stats");
            response.EnsureSuccessStatusCode();
            var stats = await response.Content.ReadFromJsonAsync<TestExecutionStatsDto>(_jsonOptions);
            
            Assert.NotNull(stats);
            Assert.True(stats.TotalTestRuns >= 0);
            Assert.True(stats.ActiveTestRuns >= 0);
            Assert.True(stats.CompletedTestRuns >= 0);
            Assert.True(stats.TotalTestCaseExecutions >= 0);
            Assert.True(stats.PassedExecutions >= 0);
            Assert.True(stats.FailedExecutions >= 0);
            Assert.True(stats.BlockedExecutions >= 0);
            Assert.True(stats.NotRunExecutions >= 0);
            Assert.True(stats.PassRate >= 0 && stats.PassRate <= 100);
        }

        [Fact]
        public async Task CanGetRecentActivityWithDefaultCount()
        {
            var response = await _client.GetAsync("/api/dashboard/recent-activity");
            response.EnsureSuccessStatusCode();
            var activities = await response.Content.ReadFromJsonAsync<List<RecentActivityDto>>(_jsonOptions);
            
            Assert.NotNull(activities);
            Assert.True(activities.Count <= 5); // Default count
        }

        [Fact]
        public async Task CanGetRecentActivityWithCustomCount()
        {
            var response = await _client.GetAsync("/api/dashboard/recent-activity?count=10");
            response.EnsureSuccessStatusCode();
            var activities = await response.Content.ReadFromJsonAsync<List<RecentActivityDto>>(_jsonOptions);
            
            Assert.NotNull(activities);
            Assert.True(activities.Count <= 10);
        }

        [Fact]
        public async Task RecentActivityRejectsInvalidCount()
        {
            // Test negative count
            var response1 = await _client.GetAsync("/api/dashboard/recent-activity?count=-1");
            Assert.False(response1.IsSuccessStatusCode);

            // Test zero count
            var response2 = await _client.GetAsync("/api/dashboard/recent-activity?count=0");
            Assert.False(response2.IsSuccessStatusCode);

            // Test count too large
            var response3 = await _client.GetAsync("/api/dashboard/recent-activity?count=100");
            Assert.False(response3.IsSuccessStatusCode);
        }
    }
}