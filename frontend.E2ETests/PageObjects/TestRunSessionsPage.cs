using Microsoft.Playwright;

namespace frontend.E2ETests.PageObjects;

/// <summary>
/// Page object for the Test Run Sessions page
/// </summary>
public class TestRunSessionsPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    
    public TestRunSessionsPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }
    
    /// <summary>
    /// Navigates to the test run sessions page
    /// </summary>
    public async Task NavigateToAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/test-run-sessions");
    }
    
    /// <summary>
    /// Clicks the create test run session button
    /// </summary>
    public async Task ClickCreateSessionAsync()
    {
        await _page.ClickAsync("[data-testid='create-session-button']");
    }
    
    /// <summary>
    /// Fills the test run session form
    /// </summary>
    /// <param name="name">Session name</param>
    /// <param name="description">Session description</param>
    /// <param name="testPlanId">Test plan ID</param>
    /// <param name="executorId">Executor user ID</param>
    public async Task FillSessionFormAsync(string name, string description, string testPlanId, string executorId)
    {
        await _page.FillAsync("[data-testid='name-input']", name);
        await _page.FillAsync("[data-testid='description-input']", description);
        await _page.SelectOptionAsync("[data-testid='testplan-select']", testPlanId);
        await _page.SelectOptionAsync("[data-testid='executor-select']", executorId);
    }
    
    /// <summary>
    /// Saves the test run session form
    /// </summary>
    public async Task SaveSessionAsync()
    {
        await _page.ClickAsync("[data-testid='save-button']");
    }
    
    /// <summary>
    /// Searches for test run sessions
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    public async Task SearchSessionsAsync(string searchTerm)
    {
        await _page.FillAsync("[data-testid='search-input']", searchTerm);
        await _page.PressAsync("[data-testid='search-input']", "Enter");
    }
    
    /// <summary>
    /// Gets the count of visible session rows
    /// </summary>
    /// <returns>Number of session rows</returns>
    public async Task<int> GetSessionCountAsync()
    {
        var rows = await _page.QuerySelectorAllAsync("[data-testid='session-row']");
        return rows.Count;
    }
    
    /// <summary>
    /// Checks if a session is visible in the list
    /// </summary>
    /// <param name="name">Session name to look for</param>
    /// <returns>True if session is visible</returns>
    public async Task<bool> IsSessionVisibleAsync(string name)
    {
        return await _page.IsVisibleAsync($"text={name}");
    }
    
    /// <summary>
    /// Clicks the start session button for a session
    /// </summary>
    /// <param name="name">Session name</param>
    public async Task StartSessionAsync(string name)
    {
        await _page.ClickAsync($"[data-testid='start-{name}']");
    }
    
    /// <summary>
    /// Clicks the complete session button for a session
    /// </summary>
    /// <param name="name">Session name</param>
    public async Task CompleteSessionAsync(string name)
    {
        await _page.ClickAsync($"[data-testid='complete-{name}']");
    }
    
    /// <summary>
    /// Clicks the abort session button for a session
    /// </summary>
    /// <param name="name">Session name</param>
    public async Task AbortSessionAsync(string name)
    {
        await _page.ClickAsync($"[data-testid='abort-{name}']");
    }
}