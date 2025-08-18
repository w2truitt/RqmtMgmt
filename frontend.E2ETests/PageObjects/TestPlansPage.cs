using Microsoft.Playwright;

namespace frontend.E2ETests.PageObjects;

/// <summary>
/// Page object for the Test Plans page
/// </summary>
public class TestPlansPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    
    public TestPlansPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }
    
    /// <summary>
    /// Navigates to the test plans page
    /// </summary>
    public async Task NavigateToAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/testplans");
    }
    
    /// <summary>
    /// Clicks the create test plan button
    /// </summary>
    public async Task ClickCreateTestPlanAsync()
    {
        await _page.ClickAsync("[data-testid='create-testplan-button']");
        // Wait for the modal to appear
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
    }
    
    /// <summary>
    /// Fills the test plan form
    /// </summary>
    /// <param name="name">Test plan name</param>
    /// <param name="type">Test plan type</param>
    /// <param name="description">Test plan description</param>
    public async Task FillTestPlanFormAsync(string name, string type, string description)
    {
        // Wait for modal to be fully loaded
        await _page.WaitForSelectorAsync(".modal.show [data-testid='name-input']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
        
        await _page.FillAsync(".modal.show [data-testid='name-input']", name);
        
        // Wait for the select element to be visible and ready (within the modal)
        await _page.WaitForSelectorAsync(".modal.show [data-testid='type-select']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
        
        // Use SelectOptionAsync with the modal-scoped selector
        await _page.SelectOptionAsync(".modal.show [data-testid='type-select']", type);
        
        await _page.FillAsync(".modal.show [data-testid='description-input']", description);
    }
    
    /// <summary>
    /// Saves the test plan form
    /// </summary>
    public async Task SaveTestPlanAsync()
    {
        await _page.ClickAsync(".modal.show [data-testid='save-button']");
    }
    
    /// <summary>
    /// Searches for test plans
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    public async Task SearchTestPlansAsync(string searchTerm)
    {
        await _page.FillAsync("[data-testid='search-input']", searchTerm);
        await _page.PressAsync("[data-testid='search-input']", "Enter");
    }
    
    /// <summary>
    /// Gets the count of visible test plan rows
    /// </summary>
    /// <returns>Number of test plan rows</returns>
    public async Task<int> GetTestPlanCountAsync()
    {
        var rows = await _page.QuerySelectorAllAsync("[data-testid='testplan-row']");
        return rows.Count;
    }
    
    /// <summary>
    /// Checks if a test plan is visible in the list
    /// </summary>
    /// <param name="name">Test plan name to look for</param>
    /// <returns>True if test plan is visible</returns>
    public async Task<bool> IsTestPlanVisibleAsync(string name)
    {
        return await _page.IsVisibleAsync($"text={name}");
    }
    
    /// <summary>
    /// Clicks the edit button for a test plan
    /// </summary>
    /// <param name="name">Test plan name</param>
    public async Task EditTestPlanAsync(string name)
    {
        await _page.ClickAsync($"[data-testid='edit-{name}']");
    }
    
    /// <summary>
    /// Clicks the delete button for a test plan
    /// </summary>
    /// <param name="name">Test plan name</param>
    public async Task DeleteTestPlanAsync(string name)
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