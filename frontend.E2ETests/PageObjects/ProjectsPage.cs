using Microsoft.Playwright;

namespace frontend.E2ETests.PageObjects;

/// <summary>
/// Page object for the Projects page
/// </summary>
public class ProjectsPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    
    public ProjectsPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }
    
    /// <summary>
    /// Navigates to the projects page
    /// </summary>
    public async Task NavigateToAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/projects");
    }
    
    /// <summary>
    /// Clicks the create project button
    /// </summary>
    public async Task ClickCreateProjectAsync()
    {
        await _page.ClickAsync("[data-testid='create-project-button']");
    }
    
    /// <summary>
    /// Fills the project form
    /// </summary>
    /// <param name="name">Project name</param>
    /// <param name="code">Project code</param>
    /// <param name="description">Project description</param>
    /// <param name="status">Project status</param>
    /// <param name="ownerId">Owner user ID</param>
    public async Task FillProjectFormAsync(string name, string code, string description, string status, int ownerId)
    {
        await _page.FillAsync("[data-testid='name-input']", name);
        await _page.FillAsync("[data-testid='code-input']", code);
        await _page.FillAsync("[data-testid='description-input']", description);
        await _page.SelectOptionAsync("[data-testid='status-select']", status);
        await _page.SelectOptionAsync("[data-testid='owner-select']", ownerId.ToString());
    }
    
    /// <summary>
    /// Saves the project form
    /// </summary>
    public async Task SaveProjectAsync()
    {
        await _page.ClickAsync("[data-testid='save-button']");
    }
    
    /// <summary>
    /// Searches for projects
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    public async Task SearchProjectsAsync(string searchTerm)
    {
        await _page.FillAsync("[data-testid='search-input']", searchTerm);
        await _page.PressAsync("[data-testid='search-input']", "Enter");
    }
    
    /// <summary>
    /// Gets the count of visible project rows
    /// </summary>
    /// <returns>Number of project rows</returns>
    public async Task<int> GetProjectCountAsync()
    {
        var rows = await _page.QuerySelectorAllAsync("[data-testid='project-row']");
        return rows.Count;
    }
    
    /// <summary>
    /// Checks if a project is visible in the list
    /// </summary>
    /// <param name="name">Project name to look for</param>
    /// <returns>True if project is visible</returns>
    public async Task<bool> IsProjectVisibleAsync(string name)
    {
        return await _page.IsVisibleAsync($"text={name}");
    }
    
    /// <summary>
    /// Clicks the edit button for a project
    /// </summary>
    /// <param name="name">Project name</param>
    public async Task EditProjectAsync(string name)
    {
        await _page.ClickAsync($"[data-testid='edit-{name}']");
    }
    
    /// <summary>
    /// Clicks the delete button for a project
    /// </summary>
    /// <param name="name">Project name</param>
    public async Task DeleteProjectAsync(string name)
    {
        await _page.ClickAsync($"[data-testid='delete-{name}']");
    }
    
    /// <summary>
    /// Waits for the project form modal to be visible
    /// </summary>
    public async Task WaitForFormModalAsync()
    {
        await _page.WaitForSelectorAsync(".modal.show");
    }
    
    /// <summary>
    /// Waits for the project form modal to be hidden
    /// </summary>
    public async Task WaitForFormModalToHideAsync()
    {
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Detached, Timeout = 5000 });
    }
    
    /// <summary>
    /// Cancels the project form
    /// </summary>
    public async Task CancelFormAsync()
    {
        await _page.ClickAsync("button:has-text('Cancel')");
    }
    
    /// <summary>
    /// Confirms a delete dialog
    /// </summary>
    public async Task ConfirmDeleteAsync()
    {
        await _page.ClickAsync("button:has-text('OK')");
    }
    
    /// <summary>
    /// Gets the project name input value
    /// </summary>
    /// <returns>Current value in the name input field</returns>
    public async Task<string> GetProjectNameInputValueAsync()
    {
        var value = await _page.InputValueAsync("[data-testid='name-input']");
        return value ?? "";
    }
    
    /// <summary>
    /// Gets the project code input value
    /// </summary>
    /// <returns>Current value in the code input field</returns>
    public async Task<string> GetProjectCodeInputValueAsync()
    {
        var value = await _page.InputValueAsync("[data-testid='code-input']");
        return value ?? "";
    }
    
    /// <summary>
    /// Gets the project description input value
    /// </summary>
    /// <returns>Current value in the description input field</returns>
    public async Task<string> GetProjectDescriptionInputValueAsync()
    {
        var value = await _page.InputValueAsync("[data-testid='description-input']");
        return value ?? "";
    }
    
    /// <summary>
    /// Waits for the page to load
    /// </summary>
    public async Task WaitForPageLoadAsync()
    {
        await _page.WaitForSelectorAsync("h3:has-text('Projects')");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
    
    /// <summary>
    /// Clicks the View button for a project
    /// </summary>
    /// <param name="projectName">Project name</param>
    public async Task ClickViewProjectButtonAsync(string projectName)
    {
        await _page.ClickAsync($"[data-testid='view-{projectName}']");
    }
    
    /// <summary>
    /// Clicks on a project name link to navigate to the project dashboard
    /// </summary>
    /// <param name="projectName">Project name</param>
    public async Task ClickProjectNameLinkAsync(string projectName)
    {
        await _page.ClickAsync($"[data-testid='project-name-link-{projectName}']");
    }
    
    /// <summary>
    /// Checks if the View button is visible for a project
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <returns>True if View button is visible</returns>
    public async Task<bool> IsViewButtonVisibleAsync(string projectName)
    {
        return await _page.IsVisibleAsync($"[data-testid='view-{projectName}']");
    }
    
    /// <summary>
    /// Checks if the project name is clickable (appears as a link)
    /// </summary>
    /// <param name="projectName">Project name</param>
    /// <returns>True if project name is clickable</returns>
    public async Task<bool> IsProjectNameClickableAsync(string projectName)
    {
        return await _page.IsVisibleAsync($"[data-testid='project-name-link-{projectName}']");
    }
    
    /// <summary>
    /// Gets the name of the first project in the list
    /// </summary>
    /// <returns>First project name or empty string if no projects</returns>
    public async Task<string> GetFirstProjectNameAsync()
    {
        var firstProjectElement = await _page.QuerySelectorAsync(".project-name-link, .mud-table-row .mud-table-cell:first-child");
        if (firstProjectElement != null)
        {
            var text = await firstProjectElement.TextContentAsync();
            return text?.Trim() ?? "";
        }
        return "";
    }
    
    /// <summary>
    /// Navigates to project dashboard using the View button
    /// </summary>
    /// <param name="projectName">Project name</param>
    public async Task NavigateToProjectDashboardUsingViewButtonAsync(string projectName)
    {
        // Look for View button in the actions column
        await _page.ClickAsync($"button:has-text('View')");
    }
    
    /// <summary>
    /// Navigates to project dashboard using the clickable project name
    /// </summary>
    /// <param name="projectName">Project name</param>
    public async Task NavigateToProjectDashboardUsingProjectNameAsync(string projectName)
    {
        // Click on the clickable project name link
        await _page.ClickAsync(".project-name-link");
    }
}
