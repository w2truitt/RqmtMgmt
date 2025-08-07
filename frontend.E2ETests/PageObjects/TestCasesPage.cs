using Microsoft.Playwright;

namespace frontend.E2ETests.PageObjects;

/// <summary>
/// Page object for the Test Cases page
/// </summary>
public class TestCasesPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    
    public TestCasesPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }
    
    /// <summary>
    /// Navigates to the test cases page
    /// </summary>
    public async Task NavigateToAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/testcases");
    }
    
    /// <summary>
    /// Clicks the create test case button
    /// </summary>
    public async Task ClickCreateTestCaseAsync()
    {
        await _page.ClickAsync("[data-testid='create-testcase-button']");
    }
    
    /// <summary>
    /// Searches for test cases
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    public async Task SearchTestCasesAsync(string searchTerm)
    {
        await _page.FillAsync("[data-testid='search-input']", searchTerm);
        await _page.PressAsync("[data-testid='search-input']", "Enter");
    }
    
    /// <summary>
    /// Gets the count of visible test case rows
    /// </summary>
    /// <returns>Number of test case rows</returns>
    public async Task<int> GetTestCaseCountAsync()
    {
        var rows = await _page.QuerySelectorAllAsync("[data-testid='testcase-row']");
        return rows.Count;
    }
    
    /// <summary>
    /// Checks if a test case is visible in the list
    /// </summary>
    /// <param name="title">Test case title to look for</param>
    /// <returns>True if test case is visible</returns>
    public async Task<bool> IsTestCaseVisibleAsync(string title)
    {
        return await _page.IsVisibleAsync($"text={title}");
    }
    
    /// <summary>
    /// Clicks the edit button for a test case
    /// </summary>
    /// <param name="title">Test case title</param>
    public async Task EditTestCaseAsync(string title)
    {
        await _page.ClickAsync($"[data-testid='edit-{title}']");
    }
    
    /// <summary>
    /// Clicks the delete button for a test case
    /// </summary>
    /// <param name="title">Test case title</param>
    public async Task DeleteTestCaseAsync(string title)
    {
        await _page.ClickAsync($"[data-testid='delete-{title}']");
    }
    
    /// <summary>
    /// Confirms deletion in the confirmation dialog
    /// </summary>
    public async Task ConfirmDeleteAsync()
    {
        await _page.ClickAsync("[data-testid='confirm-delete']");
    }
}