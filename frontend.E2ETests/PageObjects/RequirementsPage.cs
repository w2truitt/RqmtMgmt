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
    /// Clicks the create requirement button
    /// </summary>
    public async Task ClickCreateRequirementAsync()
    {
        await _page.ClickAsync("[data-testid='create-requirement-button']");
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
        await _page.ClickAsync("[data-testid='save-button']");
    }
    
    /// <summary>
    /// Cancels the requirement form
    /// </summary>
    public async Task CancelRequirementAsync()
    {
        await _page.ClickAsync("[data-testid='cancel-button']");
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
        await _page.FillAsync("[data-testid='search-input']", searchTerm);
        await _page.PressAsync("[data-testid='search-input']", "Enter");
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