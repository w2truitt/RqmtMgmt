using Microsoft.Playwright;

namespace frontend.E2ETests.PageObjects;

/// <summary>
/// Page object for the Dashboard page
/// </summary>
public class DashboardPage
{
    private readonly IPage _page;
    
    public DashboardPage(IPage page)
    {
        _page = page;
    }
    
    /// <summary>
    /// Navigates to the dashboard page
    /// </summary>
    public async Task NavigateToAsync()
    {
        await _page.GotoAsync("/");
    }
    
    /// <summary>
    /// Checks if the dashboard is loaded
    /// </summary>
    /// <returns>True if dashboard is loaded</returns>
    public async Task<bool> IsDashboardLoadedAsync()
    {
        return await _page.IsVisibleAsync("[data-testid='dashboard-container']");
    }
    
    /// <summary>
    /// Gets the requirements summary count
    /// </summary>
    /// <returns>Requirements count text</returns>
    public async Task<string> GetRequirementsCountAsync()
    {
        return await _page.TextContentAsync("[data-testid='requirements-count']") ?? "0";
    }
    
    /// <summary>
    /// Gets the test cases summary count
    /// </summary>
    /// <returns>Test cases count text</returns>
    public async Task<string> GetTestCasesCountAsync()
    {
        return await _page.TextContentAsync("[data-testid='test-cases-count']") ?? "0";
    }
    
    /// <summary>
    /// Gets the test plans summary count
    /// </summary>
    /// <returns>Test plans count text</returns>
    public async Task<string> GetTestPlansCountAsync()
    {
        return await _page.TextContentAsync("[data-testid='test-plans-count']") ?? "0";
    }
    
    /// <summary>
    /// Gets the users summary count
    /// </summary>
    /// <returns>Users count text</returns>
    public async Task<string> GetUsersCountAsync()
    {
        return await _page.TextContentAsync("[data-testid='users-count']") ?? "0";
    }
    
    /// <summary>
    /// Clicks on the requirements summary widget
    /// </summary>
    public async Task ClickRequirementsSummaryAsync()
    {
        await _page.ClickAsync("[data-testid='requirements-summary']");
    }
    
    /// <summary>
    /// Clicks on the test cases summary widget
    /// </summary>
    public async Task ClickTestCasesSummaryAsync()
    {
        await _page.ClickAsync("[data-testid='test-cases-summary']");
    }
    
    /// <summary>
    /// Clicks on the test plans summary widget
    /// </summary>
    public async Task ClickTestPlansSummaryAsync()
    {
        await _page.ClickAsync("[data-testid='test-plans-summary']");
    }
    
    /// <summary>
    /// Clicks on the users summary widget
    /// </summary>
    public async Task ClickUsersSummaryAsync()
    {
        await _page.ClickAsync("[data-testid='users-summary']");
    }
}