using Microsoft.Playwright;

namespace frontend.E2ETests.PageObjects;

/// <summary>
/// Page object for the Requirements page
/// </summary>
public class RequirementsPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    
    public RequirementsPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }
    
    /// <summary>
    /// Navigates to the requirements page
    /// </summary>
    public async Task NavigateToAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/requirements");
    }
    
    /// <summary>
    /// Waits for the page to load
    /// </summary>
    public async Task WaitForPageLoadAsync()
    {
        await _page.WaitForSelectorAsync("h1:has-text('Requirements')", new PageWaitForSelectorOptions { Timeout = 30000 });
    }
    
    /// <summary>
    /// Clicks the create requirement button
    /// </summary>
    public async Task ClickCreateRequirementAsync()
    {
        await _page.ClickAsync("text=Add Requirement");
    }
    
    /// <summary>
    /// Waits for the form modal to appear
    /// </summary>
    public async Task WaitForFormModalAsync()
    {
        await _page.WaitForSelectorAsync(".modal.show.d-block", new PageWaitForSelectorOptions { Timeout = 10000 });
    }
    
    /// <summary>
    /// Waits for the form modal to hide
    /// </summary>
    public async Task WaitForFormModalToHideAsync()
    {
        await _page.WaitForSelectorAsync(".modal.show.d-block", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = 10000 });
    }
    
    /// <summary>
    /// Fills the requirement form
    /// </summary>
    /// <param name="title">Requirement title</param>
    /// <param name="description">Requirement description</param>
    /// <param name="type">Requirement type</param>
    /// <param name="status">Requirement status</param>
    public async Task FillRequirementFormAsync(string title, string description, string type = "CRS", string status = "Draft")
    {
        await _page.FillAsync("[data-testid='title-input']", title);
        await _page.FillAsync("[data-testid='description-input']", description);
        await _page.SelectOptionAsync("[data-testid='type-select']", type);
        await _page.SelectOptionAsync("[data-testid='status-select']", status);
    }
    
    /// <summary>
    /// Saves the requirement form
    /// </summary>
    public async Task SaveRequirementAsync()
    {
        await _page.ClickAsync("text=Save");
    }
    
    /// <summary>
    /// Cancels the requirement form
    /// </summary>
    public async Task CancelRequirementAsync()
    {
        await _page.ClickAsync("text=Cancel");
    }
    
    /// <summary>
    /// Gets the current value of the requirement title input
    /// </summary>
    public async Task<string> GetRequirementTitleInputValueAsync()
    {
        return await _page.InputValueAsync("[data-testid='title-input']");
    }
    
    /// <summary>
    /// Checks if a requirement is visible in the list
    /// </summary>
    /// <param name="title">Requirement title to look for</param>
    /// <returns>True if requirement is visible</returns>
    public async Task<bool> IsRequirementVisibleAsync(string title)
    {
        return await _page.IsVisibleAsync($"text={title}");
    }
    
    /// <summary>
    /// Clicks the edit button for a requirement
    /// </summary>
    /// <param name="title">Requirement title</param>
    public async Task EditRequirementAsync(string title)
    {
        await _page.ClickAsync($"[data-testid='edit-{title}']");
    }
    
    /// <summary>
    /// Clicks the delete button for a requirement
    /// </summary>
    /// <param name="title">Requirement title</param>
    public async Task DeleteRequirementAsync(string title)
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
    
    /// <summary>
    /// Searches for requirements
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    public async Task SearchRequirementsAsync(string searchTerm)
    {
        await _page.FillAsync("input[placeholder*='Search requirements']", searchTerm);
        await _page.ClickAsync("text=Search");
    }
    
    /// <summary>
    /// Gets the count of visible requirement rows
    /// </summary>
    /// <returns>Number of requirement rows</returns>
    public async Task<int> GetRequirementCountAsync()
    {
        var rows = await _page.QuerySelectorAllAsync("[data-testid='requirement-row']");
        return rows.Count;
    }
}