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