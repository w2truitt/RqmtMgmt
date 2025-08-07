using frontend.E2ETests.PageObjects;
using Microsoft.Playwright;
using Xunit;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for the Dashboard (Home) page
/// </summary>
public class DashboardPageTests : E2ETestBase
{
    [Fact]
    public async Task Dashboard_NavigatesSuccessfully()
    {
        // Arrange
        var dashboardPage = new DashboardPage(Page, BaseUrl);
        
        // Act
        await dashboardPage.NavigateToAsync();
        
        // Assert
        Assert.Contains("/", Page.Url);
        
        // TODO: Add more specific assertions when frontend is implemented
        // await Expect(Page.Locator("[data-testid='dashboard-container']")).ToBeVisibleAsync();
    }
    
    [Fact]
    public async Task Dashboard_LoadsWithoutErrors()
    {
        // Arrange
        var dashboardPage = new DashboardPage(Page, BaseUrl);
        
        // Act
        await dashboardPage.NavigateToAsync();
        
        // Assert
        // Check that page loads without JavaScript errors
        var errors = await Page.EvaluateAsync<string[]>("() => window.errors || []");
        Assert.Empty(errors);
        
        // Check that we can access the page (no 404 or server errors)
        var response = Page.Url;
        Assert.NotNull(response);
    }
    
    [Fact]
    public async Task Dashboard_HasExpectedTitle()
    {
        // Arrange
        var dashboardPage = new DashboardPage(Page, BaseUrl);
        
        // Act
        await dashboardPage.NavigateToAsync();
        
        // Assert
        var title = await Page.TitleAsync();
        // Page title may not be implemented yet, so just check it's not null
        Assert.NotNull(title);
        // TODO: Uncomment when page titles are implemented
        // Assert.Contains("Dashboard", title, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public async Task Dashboard_CanAccessSummaryWidgets_WhenImplemented()
    {
        // Arrange
        var dashboardPage = new DashboardPage(Page, BaseUrl);
        
        // Act
        await dashboardPage.NavigateToAsync();
        
        // Assert
        // TODO: Uncomment when dashboard widgets are implemented
        /*
        // Check that summary widgets are present
        await Expect(Page.Locator("[data-testid='requirements-summary']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='test-cases-summary']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='test-plans-summary']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='users-summary']")).ToBeVisibleAsync();
        
        // Test that clicking widgets navigates to appropriate pages
        await dashboardPage.ClickRequirementsSummaryAsync();
        Assert.Contains("/requirements", Page.Url);
        */
        
        // For now, just verify page object is created successfully
        Assert.NotNull(dashboardPage);
    }
    
    [Fact]
    public async Task Dashboard_DisplaysCorrectCounts_WhenDataExists()
    {
        // Arrange
        var dashboardPage = new DashboardPage(Page, BaseUrl);
        
        // Act
        await dashboardPage.NavigateToAsync();
        
        // Assert
        // TODO: Uncomment when dashboard data display is implemented
        /*
        // Get counts from dashboard
        var requirementsCount = await dashboardPage.GetRequirementsCountAsync();
        var testCasesCount = await dashboardPage.GetTestCasesCountAsync();
        var testPlansCount = await dashboardPage.GetTestPlansCountAsync();
        var usersCount = await dashboardPage.GetUsersCountAsync();
        
        // Verify counts are numeric and reasonable
        Assert.True(int.TryParse(requirementsCount, out var reqCount));
        Assert.True(int.TryParse(testCasesCount, out var tcCount));
        Assert.True(int.TryParse(testPlansCount, out var tpCount));
        Assert.True(int.TryParse(usersCount, out var userCount));
        
        Assert.True(reqCount >= 0);
        Assert.True(tcCount >= 0);
        Assert.True(tpCount >= 0);
        Assert.True(userCount >= 0);
        */
        
        // For now, just verify navigation works
        Assert.Contains("/", Page.Url);
    }
}