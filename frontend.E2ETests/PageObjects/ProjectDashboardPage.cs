using Microsoft.Playwright;

namespace frontend.E2ETests.PageObjects;

/// <summary>
/// Page object for the Project Dashboard page
/// </summary>
public class ProjectDashboardPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    
    public ProjectDashboardPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }
    
    /// <summary>
    /// Navigates to the project dashboard
    /// </summary>
    /// <param name="projectId">Project ID</param>
    public async Task NavigateToAsync(int projectId)
    {
        await _page.GotoAsync($"{_baseUrl}/projects/{projectId}");
    }
    
    /// <summary>
    /// Waits for the project dashboard to load
    /// </summary>
    public async Task WaitForPageLoadAsync()
    {
        await _page.WaitForSelectorAsync("h1.project-title", new PageWaitForSelectorOptions { Timeout = 30000 });
    }
    
    /// <summary>
    /// Gets the project name from the dashboard header
    /// </summary>
    /// <returns>The project name</returns>
    public async Task<string> GetProjectNameAsync()
    {
        var nameElement = await _page.QuerySelectorAsync("h1.project-title");
        if (nameElement != null)
        {
            var text = await nameElement.TextContentAsync();
            return text ?? string.Empty;
        }
        return string.Empty;
    }
    
    /// <summary>
    /// Gets the requirements count from the stats card
    /// </summary>
    /// <returns>The requirements count</returns>
    public async Task<int> GetRequirementsCountAsync()
    {
        var countElement = await _page.QuerySelectorAsync(".requirements-card .stats-number");
        if (countElement != null)
        {
            var countText = await countElement.TextContentAsync();
            return int.TryParse(countText ?? "0", out var count) ? count : 0;
        }
        return 0;
    }
    
    /// <summary>
    /// Gets the team members count from the stats card
    /// </summary>
    /// <returns>The team members count</returns>
    public async Task<int> GetTeamMembersCountAsync()
    {
        var countElement = await _page.QuerySelectorAsync(".team-card .stats-number");
        if (countElement != null)
        {
            var countText = await countElement.TextContentAsync();
            return int.TryParse(countText ?? "0", out var count) ? count : 0;
        }
        return 0;
    }
    
    /// <summary>
    /// Clicks the "View Team" link
    /// </summary>
    public async Task ClickViewTeamLinkAsync()
    {
        await _page.ClickAsync("a:has-text('View Team')");
    }
    
    /// <summary>
    /// Clicks the "New Requirement" quick action button
    /// </summary>
    public async Task ClickNewRequirementButtonAsync()
    {
        await _page.ClickAsync("button:has-text('New Requirement')");
    }
    
    /// <summary>
    /// Clicks the "View All Requirements" quick action button
    /// </summary>
    public async Task ClickViewAllRequirementsButtonAsync()
    {
        await _page.ClickAsync("button:has-text('View All Requirements')");
    }
    
    /// <summary>
    /// Checks if the project dashboard loaded successfully
    /// </summary>
    /// <returns>True if dashboard is loaded</returns>
    public async Task<bool> IsDashboardLoadedAsync()
    {
        return await _page.IsVisibleAsync("h2") && 
               await _page.IsVisibleAsync(".stats-card") &&
               await _page.IsVisibleAsync(".quick-actions-card");
    }
    
    /// <summary>
    /// Checks if the team card is visible
    /// </summary>
    /// <returns>True if team card is visible</returns>
    public async Task<bool> IsTeamCardVisibleAsync()
    {
        return await _page.IsVisibleAsync(".team-card");
    }
    
    /// <summary>
    /// Checks if the requirements card is visible
    /// </summary>
    /// <returns>True if requirements card is visible</returns>
    public async Task<bool> IsRequirementsCardVisibleAsync()
    {
        return await _page.IsVisibleAsync(".requirements-card");
    }
    
    /// <summary>
    /// Checks if currently on the project dashboard page
    /// </summary>
    /// <returns>True if on project dashboard</returns>
    public async Task<bool> IsOnProjectDashboardAsync()
    {
        return _page.Url.Contains("/projects/") && !_page.Url.Contains("/dashboard") &&
               await _page.IsVisibleAsync("h1.project-title");
    }
    
    /// <summary>
    /// Navigates to the requirements page for this project
    /// </summary>
    public async Task NavigateToRequirementsAsync()
    {
        await _page.ClickAsync("a[href*='requirements']");
    }
    
    /// <summary>
    /// Navigates to the test cases page for this project
    /// </summary>
    public async Task NavigateToTestCasesAsync()
    {
        await _page.ClickAsync("a[href*='testcases']");
    }
    
    /// <summary>
    /// Navigates to the test plans page for this project
    /// </summary>
    public async Task NavigateToTestPlansAsync()
    {
        await _page.ClickAsync("a[href*='testplans']");
    }
}
