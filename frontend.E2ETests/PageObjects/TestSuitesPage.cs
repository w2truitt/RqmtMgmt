using Microsoft.Playwright;

namespace frontend.E2ETests.PageObjects;

/// <summary>
/// Page object for the Test Suites page
/// </summary>
public class TestSuitesPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    
    public TestSuitesPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }
    
    /// <summary>
    /// Navigates to the test suites page
    /// </summary>
    public async Task NavigateToAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/testsuites");
    }
    
    /// <summary>
    /// Clicks the create test suite button
    /// </summary>
    public async Task ClickCreateTestSuiteAsync()
    {
        await _page.ClickAsync("[data-testid='create-testsuite-button']");
    }
    
    /// <summary>
    /// Fills the test suite form
    /// </summary>
    /// <param name="name">Test suite name</param>
    /// <param name="description">Test suite description</param>
    public async Task FillTestSuiteFormAsync(string name, string description)
    {
        await _page.FillAsync("[data-testid='name-input']", name);
        await _page.FillAsync("[data-testid='description-input']", description);
    }
    
    /// <summary>
    /// Saves the test suite form
    /// </summary>
    public async Task SaveTestSuiteAsync()
    {
        await _page.ClickAsync("[data-testid='save-button']");
    }
    
    /// <summary>
    /// Searches for test suites
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    public async Task SearchTestSuitesAsync(string searchTerm)
    {
        await _page.FillAsync("[data-testid='search-input']", searchTerm);
        await _page.PressAsync("[data-testid='search-input']", "Enter");
    }
    
    /// <summary>
    /// Gets the count of visible test suite rows
    /// </summary>
    /// <returns>Number of test suite rows</returns>
    public async Task<int> GetTestSuiteCountAsync()
    {
        var rows = await _page.QuerySelectorAllAsync("[data-testid='testsuite-row']");
        return rows.Count;
    }
    
    /// <summary>
    /// Checks if a test suite is visible in the list
    /// </summary>
    /// <param name="name">Test suite name to look for</param>
    /// <returns>True if test suite is visible</returns>
    public async Task<bool> IsTestSuiteVisibleAsync(string name)
    {
        return await _page.IsVisibleAsync($"text={name}");
    }
    
    /// <summary>
    /// Clicks the edit button for a test suite
    /// </summary>
    /// <param name="name">Test suite name</param>
    public async Task EditTestSuiteAsync(string name)
    {
        await _page.ClickAsync($"[data-testid='edit-{name}']");
    }
    
    /// <summary>
    /// Clicks the delete button for a test suite
    /// </summary>
    /// <param name="name">Test suite name</param>
    public async Task DeleteTestSuiteAsync(string name)
    {
        await _page.ClickAsync($"[data-testid='delete-{name}']");
    }
    
    /// <summary>
    /// Confirms deletion in the confirmation dialog
    /// </summary>
    public async Task ConfirmDeleteAsync()
    {
        await _page.ClickAsync("[data-testid='confirm-delete']");
    }
}