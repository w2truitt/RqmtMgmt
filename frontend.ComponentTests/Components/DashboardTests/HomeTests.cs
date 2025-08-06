using Bunit;
using frontend.Pages;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using RqmtMgmtShared;

namespace frontend.ComponentTests.Components.DashboardTests;

/// <summary>
/// Tests for the Home/Dashboard component
/// </summary>
public class HomeTests : ComponentTestBase
{
    [Fact]
    public void Home_RendersCorrectly_WithDashboardTitle()
    {
        // Arrange
        SetupMockDashboardService();
        
        // Act
        var component = RenderComponent<Home>();
        
        // Assert
        Assert.Contains("Dashboard", component.Markup);
        Assert.Contains("<h1>Dashboard</h1>", component.Markup);
    }
    
    [Fact]
    public void Home_DisplaysAllDashboardCards()
    {
        // Arrange
        SetupMockDashboardService();
        
        // Act
        var component = RenderComponent<Home>();
        
        // Assert
        var dashboardCards = component.FindAll(".dashboard-card");
        Assert.Equal(4, dashboardCards.Count);
        
        // Check that all expected cards are present
        var cardTitles = dashboardCards.Select(card => card.QuerySelector("h3")?.TextContent).ToList();
        Assert.Contains("Requirements", cardTitles);
        Assert.Contains("Test Suites", cardTitles);
        Assert.Contains("Test Cases", cardTitles);
        Assert.Contains("Test Plans", cardTitles);
    }
    
    [Fact]
    public void Home_DisplaysRequirementsStatistics()
    {
        // Arrange
        SetupMockDashboardService();
        
        // Act
        var component = RenderComponent<Home>();
        
        // Assert
        var requirementsCard = component.FindAll(".dashboard-card")
            .First(card => card.QuerySelector("h3")?.TextContent == "Requirements");
        
        var stats = requirementsCard.QuerySelectorAll(".dashboard-stat");
        Assert.Equal(3, stats.Length);
        
        // Check that statistics are displayed (mock data should be loaded)
        var totalStat = stats.FirstOrDefault(s => s.QuerySelector(".dashboard-stat-label")?.TextContent == "Total");
        var approvedStat = stats.FirstOrDefault(s => s.QuerySelector(".dashboard-stat-label")?.TextContent == "Approved");
        var draftStat = stats.FirstOrDefault(s => s.QuerySelector(".dashboard-stat-label")?.TextContent == "Draft");
        
        Assert.NotNull(totalStat);
        Assert.NotNull(approvedStat);
        Assert.NotNull(draftStat);
        
        // Verify mock data is loaded (47 total requirements)
        Assert.Equal("47", totalStat.QuerySelector(".dashboard-stat-number")?.TextContent);
        Assert.Equal("32", approvedStat.QuerySelector(".dashboard-stat-number")?.TextContent);
        Assert.Equal("15", draftStat.QuerySelector(".dashboard-stat-number")?.TextContent);
    }
    
    [Fact]
    public void Home_DisplaysTestSuitesStatistics()
    {
        // Arrange
        SetupMockDashboardService();
        
        // Act
        var component = RenderComponent<Home>();
        
        // Assert
        var testSuitesCard = component.FindAll(".dashboard-card")
            .First(card => card.QuerySelector("h3")?.TextContent == "Test Suites");
        
        var stats = testSuitesCard.QuerySelectorAll(".dashboard-stat");
        Assert.Equal(3, stats.Length);
        
        // Check that statistics are displayed
        var totalStat = stats.FirstOrDefault(s => s.QuerySelector(".dashboard-stat-label")?.TextContent == "Total");
        var activeStat = stats.FirstOrDefault(s => s.QuerySelector(".dashboard-stat-label")?.TextContent == "Active");
        var completedStat = stats.FirstOrDefault(s => s.QuerySelector(".dashboard-stat-label")?.TextContent == "Completed");
        
        Assert.NotNull(totalStat);
        Assert.NotNull(activeStat);
        Assert.NotNull(completedStat);
        
        // Verify mock data is loaded
        Assert.Equal("12", totalStat.QuerySelector(".dashboard-stat-number")?.TextContent);
        Assert.Equal("8", activeStat.QuerySelector(".dashboard-stat-number")?.TextContent);
        Assert.Equal("4", completedStat.QuerySelector(".dashboard-stat-number")?.TextContent);
    }
    
    [Fact]
    public void Home_DisplaysTestCasesStatistics()
    {
        // Arrange
        SetupMockDashboardService();
        
        // Act
        var component = RenderComponent<Home>();
        
        // Assert
        var testCasesCard = component.FindAll(".dashboard-card")
            .First(card => card.QuerySelector("h3")?.TextContent == "Test Cases");
        
        var stats = testCasesCard.QuerySelectorAll(".dashboard-stat");
        Assert.Equal(3, stats.Length);
        
        // Check that statistics are displayed
        var totalStat = stats.FirstOrDefault(s => s.QuerySelector(".dashboard-stat-label")?.TextContent == "Total");
        var passedStat = stats.FirstOrDefault(s => s.QuerySelector(".dashboard-stat-label")?.TextContent == "Passed");
        var failedStat = stats.FirstOrDefault(s => s.QuerySelector(".dashboard-stat-label")?.TextContent == "Failed");
        
        Assert.NotNull(totalStat);
        Assert.NotNull(passedStat);
        Assert.NotNull(failedStat);
        
        // Verify mock data is loaded
        Assert.Equal("156", totalStat.QuerySelector(".dashboard-stat-number")?.TextContent);
        Assert.Equal("142", passedStat.QuerySelector(".dashboard-stat-number")?.TextContent);
        Assert.Equal("14", failedStat.QuerySelector(".dashboard-stat-number")?.TextContent);
    }
    
    [Fact]
    public void Home_DisplaysTestPlansStatistics()
    {
        // Arrange
        SetupMockDashboardService();
        
        // Act
        var component = RenderComponent<Home>();
        
        // Assert
        var testPlansCard = component.FindAll(".dashboard-card")
            .First(card => card.QuerySelector("h3")?.TextContent == "Test Plans");
        
        var stats = testPlansCard.QuerySelectorAll(".dashboard-stat");
        Assert.Equal(3, stats.Length);
        
        // Check that statistics are displayed
        var totalStat = stats.FirstOrDefault(s => s.QuerySelector(".dashboard-stat-label")?.TextContent == "Total");
        var completeStat = stats.FirstOrDefault(s => s.QuerySelector(".dashboard-stat-label")?.TextContent == "% Complete");
        var coverageStat = stats.FirstOrDefault(s => s.QuerySelector(".dashboard-stat-label")?.TextContent == "% Coverage");
        
        Assert.NotNull(totalStat);
        Assert.NotNull(completeStat);
        Assert.NotNull(coverageStat);
        
        // Verify mock data is loaded
        Assert.Equal("6", totalStat.QuerySelector(".dashboard-stat-number")?.TextContent);
        Assert.Equal("78", completeStat.QuerySelector(".dashboard-stat-number")?.TextContent);
        Assert.Equal("85", coverageStat.QuerySelector(".dashboard-stat-number")?.TextContent);
    }
    
    [Fact]
    public void Home_DisplaysRecentActivity()
    {
        // Arrange
        SetupMockDashboardService();
        
        // Act
        var component = RenderComponent<Home>();
        
        // Assert
        var activitySection = component.Find(".dashboard-activity");
        Assert.NotNull(activitySection);
        
        var activityItems = activitySection.QuerySelectorAll(".activity-item");
        Assert.Equal(5, activityItems.Length); // Should display 5 recent activities
        
        // Check that activity items have the expected structure
        foreach (var item in activityItems)
        {
            Assert.NotNull(item.QuerySelector(".activity-text"));
            Assert.NotNull(item.QuerySelector(".activity-time"));
        }
    }
    
    [Fact]
    public void Home_HasCorrectBootstrapIcons()
    {
        // Arrange
        SetupMockDashboardService();
        
        // Act
        var component = RenderComponent<Home>();
        
        // Assert
        var dashboardCards = component.FindAll(".dashboard-card");
        Assert.Equal(4, dashboardCards.Count);
        
        // Check that each card has quick action buttons
        foreach (var card in dashboardCards)
        {
            var quickActions = card.QuerySelectorAll(".quick-action-btn");
            Assert.True(quickActions.Length >= 1); // At least one quick action button
        }
    }

    /// <summary>
    /// Sets up the mock dashboard service with test data that matches the expected values in tests
    /// </summary>
    private void SetupMockDashboardService()
    {
        var mockDashboardService = GetMockService<IDashboardService>();
        
        // Set up mock statistics to return the expected test values
        var mockStatistics = new DashboardStatisticsDto
        {
            Requirements = new RequirementStatisticsDto
            {
                Total = 47,
                Approved = 32,
                Draft = 15,
                Implemented = 0,
                Verified = 0
            },
            TestSuites = new TestSuiteStatisticsDto
            {
                Total = 12,
                Active = 8,
                Completed = 4
            },
            TestCases = new TestCaseStatisticsDto
            {
                Total = 156,
                Passed = 142,
                Failed = 14,
                NotRun = 0
            },
            TestPlans = new TestPlanStatisticsDto
            {
                Total = 6,
                ExecutionProgress = 78,
                CoveragePercentage = 85
            }
        };
        
        // Set up mock recent activities
        var mockActivities = new List<RecentActivityDto>
        {
            new RecentActivityDto
            {
                Id = 1,
                Description = "New requirement 'User Authentication' created",
                EntityType = "Requirement",
                EntityId = 1,
                Action = "Created",
                UserId = 1,
                UserName = "Test User",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new RecentActivityDto
            {
                Id = 2,
                Description = "Test suite 'Login Tests' completed",
                EntityType = "TestSuite",
                EntityId = 1,
                Action = "Completed",
                UserId = 1,
                UserName = "Test User",
                CreatedAt = DateTime.UtcNow.AddHours(-4)
            },
            new RecentActivityDto
            {
                Id = 3,
                Description = "Requirement 'Data Validation' approved",
                EntityType = "Requirement",
                EntityId = 2,
                Action = "Approved",
                UserId = 1,
                UserName = "Test User",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new RecentActivityDto
            {
                Id = 4,
                Description = "Test case 'Password Reset' updated",
                EntityType = "TestCase",
                EntityId = 1,
                Action = "Updated",
                UserId = 1,
                UserName = "Test User",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new RecentActivityDto
            {
                Id = 5,
                Description = "Test plan 'User Validation' created",
                EntityType = "TestPlan",
                EntityId = 1,
                Action = "Created",
                UserId = 1,
                UserName = "Test User",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };
        
        mockDashboardService.Setup(s => s.GetStatisticsAsync())
            .ReturnsAsync(mockStatistics);
        
        mockDashboardService.Setup(s => s.GetRecentActivityAsync(It.IsAny<int>()))
            .ReturnsAsync(mockActivities);
    }
}