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
        // Try both button texts to handle both global and project-specific requirements pages
        var addRequirementButton = _page.Locator("text=Add Requirement");
        var newRequirementButton = _page.Locator("text=New Requirement");
        
        if (await addRequirementButton.IsVisibleAsync())
        {
            await addRequirementButton.ClickAsync();
        }
        else if (await newRequirementButton.IsVisibleAsync())
        {
            await newRequirementButton.ClickAsync();
        }
        else
        {
            // Fallback: try both texts with a timeout
            try
            {
                await _page.ClickAsync("text=Add Requirement", new PageClickOptions { Timeout = 5000 });
            }
            catch
            {
                await _page.ClickAsync("text=New Requirement", new PageClickOptions { Timeout = 5000 });
            }
        }
    }
    
    /// <summary>
    /// Waits for the form modal to appear
    /// </summary>
    public async Task WaitForFormModalAsync()
    {
        // Wait for requirement modal structure: class="modal show d-block"
        try 
        {
            // Requirements.razor uses: <div class="modal show d-block" tabindex="-1" role="dialog">
            await _page.WaitForSelectorAsync("div.modal.show.d-block[role='dialog']", new PageWaitForSelectorOptions { Timeout = 10000 });
        }
        catch
        {
            // Fallback patterns for different modal structures
            try 
            {
                await _page.WaitForSelectorAsync(".modal.show.d-block", new PageWaitForSelectorOptions { Timeout = 5000 });
            }
            catch
            {
                await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { Timeout = 5000 });
            }
        }
    }
    
    /// <summary>
    /// Waits for the form modal to hide
    /// </summary>
    public async Task WaitForFormModalToHideAsync()
    {
        try
        {
            await _page.WaitForSelectorAsync("div.modal.show.d-block[role='dialog']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = 10000 });
        }
        catch
        {
            // Fallback patterns
            try 
            {
                await _page.WaitForSelectorAsync(".modal.show.d-block", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = 5000 });
            }
            catch
            {
                await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = 5000 });
            }
        }
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
        // Clear and fill title with proper input events for Blazor binding
        await _page.ClickAsync("[data-testid='title-input']");
        await _page.FillAsync("[data-testid='title-input']", title);
        await _page.DispatchEventAsync("[data-testid='title-input']", "input");
        await _page.DispatchEventAsync("[data-testid='title-input']", "change");
        
        // Clear and fill description with proper input events for Blazor binding  
        await _page.ClickAsync("[data-testid='description-input']");
        await _page.FillAsync("[data-testid='description-input']", description);
        await _page.DispatchEventAsync("[data-testid='description-input']", "input");
        await _page.DispatchEventAsync("[data-testid='description-input']", "change");
        
        // Select options and trigger change events for Blazor binding
        await _page.SelectOptionAsync("[data-testid='type-select']", type);
        await _page.DispatchEventAsync("[data-testid='type-select']", "change");
        
        await _page.SelectOptionAsync("[data-testid='status-select']", status);
        await _page.DispatchEventAsync("[data-testid='status-select']", "change");
        
        // Wait a moment for Blazor to process the binding updates
        await _page.WaitForTimeoutAsync(500);
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