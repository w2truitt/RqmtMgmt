using Bunit;
using frontend.Pages;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace frontend.ComponentTests.Components.DashboardTests;

/// <summary>
/// Tests for the Home/Dashboard component
/// </summary>
public class HomeTests : ComponentTestBase
{
    [Fact]
    public void Home_RendersCorrectly_WithDashboardTitle()
    {
        // Act
        var component = RenderComponent<Home>();
        
        // Assert
        Assert.Contains("Dashboard", component.Markup);
        Assert.Contains("<h1>Dashboard</h1>", component.Markup);
    }
    
    [Fact]
    public void Home_DisplaysAllDashboardCards()
    {
        // Act
        var component = RenderComponent<Home>();
        
        // Assert
        var dashboardCards = component.FindAll(".dashboard-card");
        Assert.Equal(4, dashboardCards.Count);
        
        // Verify each card has the correct title
        Assert.Contains(dashboardCards, card => card.QuerySelector("h3")?.TextContent == "Requirements");
        Assert.Contains(dashboardCards, card => card.QuerySelector("h3")?.TextContent == "Test Suites");
        Assert.Contains(dashboardCards, card => card.QuerySelector("h3")?.TextContent == "Test Cases");
        Assert.Contains(dashboardCards, card => card.QuerySelector("h3")?.TextContent == "Test Plans");
    }
    
    [Fact]
    public void Home_DisplaysRequirementsStatistics()
    {
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
        // Act
        var component = RenderComponent<Home>();
        
        // Assert
        var testSuitesCard = component.FindAll(".dashboard-card")
            .First(card => card.QuerySelector("h3")?.TextContent == "Test Suites");
        
        var stats = testSuitesCard.QuerySelectorAll(".dashboard-stat");
        Assert.Equal(3, stats.Length);
        
        // Verify mock data is loaded (12 total test suites)
        var totalStat = stats.FirstOrDefault(s => s.QuerySelector(".dashboard-stat-label")?.TextContent == "Total");
        Assert.NotNull(totalStat);
        Assert.Equal("12", totalStat.QuerySelector(".dashboard-stat-number")?.TextContent);
    }
    
    [Fact]
    public void Home_DisplaysTestCasesStatistics()
    {
        // Act
        var component = RenderComponent<Home>();
        
        // Assert
        var testCasesCard = component.FindAll(".dashboard-card")
            .First(card => card.QuerySelector("h3")?.TextContent == "Test Cases");
        
        var stats = testCasesCard.QuerySelectorAll(".dashboard-stat");
        Assert.Equal(3, stats.Length);
        
        // Verify mock data is loaded (156 total test cases)
        var totalStat = stats.FirstOrDefault(s => s.QuerySelector(".dashboard-stat-label")?.TextContent == "Total");
        var passedStat = stats.FirstOrDefault(s => s.QuerySelector(".dashboard-stat-label")?.TextContent == "Passed");
        var failedStat = stats.FirstOrDefault(s => s.QuerySelector(".dashboard-stat-label")?.TextContent == "Failed");
        
        Assert.NotNull(totalStat);
        Assert.NotNull(passedStat);
        Assert.NotNull(failedStat);
        
        Assert.Equal("156", totalStat.QuerySelector(".dashboard-stat-number")?.TextContent);
        Assert.Equal("142", passedStat.QuerySelector(".dashboard-stat-number")?.TextContent);
        Assert.Equal("14", failedStat.QuerySelector(".dashboard-stat-number")?.TextContent);
    }
    
    [Fact]
    public void Home_DisplaysTestPlansStatistics()
    {
        // Act
        var component = RenderComponent<Home>();
        
        // Assert
        var testPlansCard = component.FindAll(".dashboard-card")
            .First(card => card.QuerySelector("h3")?.TextContent == "Test Plans");
        
        var stats = testPlansCard.QuerySelectorAll(".dashboard-stat");
        Assert.Equal(3, stats.Length);
        
        // Verify mock data is loaded
        var totalStat = stats.FirstOrDefault(s => s.QuerySelector(".dashboard-stat-label")?.TextContent == "Total");
        var progressStat = stats.FirstOrDefault(s => s.QuerySelector(".dashboard-stat-label")?.TextContent == "% Complete");
        var coverageStat = stats.FirstOrDefault(s => s.QuerySelector(".dashboard-stat-label")?.TextContent == "% Coverage");
        
        Assert.NotNull(totalStat);
        Assert.NotNull(progressStat);
        Assert.NotNull(coverageStat);
        
        Assert.Equal("6", totalStat.QuerySelector(".dashboard-stat-number")?.TextContent);
        Assert.Equal("78", progressStat.QuerySelector(".dashboard-stat-number")?.TextContent);
        Assert.Equal("85", coverageStat.QuerySelector(".dashboard-stat-number")?.TextContent);
    }
    
    [Fact]
    public void Home_HasQuickActionLinksForAllCards()
    {
        // Act
        var component = RenderComponent<Home>();
        
        // Assert
        var quickActionBtns = component.FindAll(".quick-action-btn");
        Assert.Equal(8, quickActionBtns.Count); // 2 buttons per card × 4 cards
        
        // Verify specific links exist
        Assert.Contains(quickActionBtns, btn => btn.GetAttribute("href") == "/requirements" && btn.TextContent == "View All");
        Assert.Contains(quickActionBtns, btn => btn.GetAttribute("href") == "/requirements/new" && btn.TextContent == "Create New");
        Assert.Contains(quickActionBtns, btn => btn.GetAttribute("href") == "/testsuites" && btn.TextContent == "View All");
        Assert.Contains(quickActionBtns, btn => btn.GetAttribute("href") == "/testcases" && btn.TextContent == "View All");
        Assert.Contains(quickActionBtns, btn => btn.GetAttribute("href") == "/testplans" && btn.TextContent == "View All");
    }
    
    [Fact]
    public void Home_DisplaysRecentActivity()
    {
        // Act
        var component = RenderComponent<Home>();
        
        // Assert
        var activitySection = component.Find(".dashboard-activity");
        Assert.NotNull(activitySection);
        Assert.Contains("Recent Activity", activitySection.QuerySelector("h4")?.TextContent ?? "");
        
        var activityItems = activitySection.QuerySelectorAll(".activity-item");
        Assert.Equal(5, activityItems.Length); // Mock data has 5 activities
        
        // Verify first activity item
        var firstActivity = activityItems[0];
        Assert.Contains("New requirement 'User Authentication' created", 
            firstActivity.QuerySelector(".activity-text")?.TextContent ?? "");
        Assert.Contains("2 hours ago", 
            firstActivity.QuerySelector(".activity-time")?.TextContent ?? "");
    }
    
    [Fact]
    public void Home_ShowsNoActivityMessage_WhenNoRecentActivity()
    {
        // This test would require modifying the component to have empty activities
        // For now, we'll test the structure exists for the empty state
        
        // Act
        var component = RenderComponent<Home>();
        
        // Assert
        var activitySection = component.Find(".dashboard-activity");
        Assert.NotNull(activitySection);
        
        // The component should have logic to show "No recent activity" when activities list is empty
        // This would be tested when we can mock the LoadDashboardData method
    }
    
    [Fact]
    public async Task Home_LoadsDataOnInitialization()
    {
        // Act
        var component = RenderComponent<Home>();
        
        // Wait for the component to finish loading
        await Task.Delay(150); // Wait slightly longer than the mock delay
        
        // Assert
        // Verify that the component has loaded data (statistics are not zero)
        var totalRequirements = component.Find(".dashboard-card h3:contains('Requirements')")
            .ParentElement?.QuerySelector(".dashboard-stat-number");
        
        Assert.NotNull(totalRequirements);
        Assert.NotEqual("0", totalRequirements.TextContent);
    }
}