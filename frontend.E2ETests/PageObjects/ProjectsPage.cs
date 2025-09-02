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
        await _page.ClickAsync("button:has-text('Add Project')");
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
        await _page.FillAsync("input[placeholder='Search projects...']", searchTerm);
        await _page.PressAsync("input[placeholder='Search projects...']", "Enter");
    }
    
    /// <summary>
    /// Gets the count of visible project rows
    /// </summary>
    /// <returns>Number of project rows</returns>
    public async Task<int> GetProjectCountAsync()
    {
        var rows = await _page.QuerySelectorAllAsync("tbody tr");
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
        await _page.ClickAsync("[data-testid='cancel-button']");
    }
    
    /// <summary>
    /// Confirms deletion in the confirmation dialog
    /// </summary>
    public async Task ConfirmDeleteAsync()
    {
        await _page.ClickAsync("[data-testid='confirm-delete']");
    }
    
    /// <summary>
    /// Gets the value of the project name input field
    /// </summary>
    /// <returns>Project name input value</returns>
    public async Task<string> GetProjectNameInputValueAsync()
    {
        var element = await _page.QuerySelectorAsync("[data-testid='name-input']");
        return element != null ? await element.InputValueAsync() : "";
    }
    
    /// <summary>
    /// Gets the value of the project code input field
    /// </summary>
    /// <returns>Project code input value</returns>
    public async Task<string> GetProjectCodeInputValueAsync()
    {
        var element = await _page.QuerySelectorAsync("[data-testid='code-input']");
        return element != null ? await element.InputValueAsync() : "";
    }
    
    /// <summary>
    /// Gets the value of the project description input field
    /// </summary>
    /// <returns>Project description input value</returns>
    public async Task<string> GetProjectDescriptionInputValueAsync()
    {
        var element = await _page.QuerySelectorAsync("[data-testid='description-input']");
        return element != null ? await element.InputValueAsync() : "";
    }
    
    /// <summary>
    /// Waits for the projects page to load completely
    /// </summary>
    public async Task WaitForPageLoadAsync()
    {
        await _page.WaitForSelectorAsync("table", new PageWaitForSelectorOptions { Timeout = 30000 });
    }
    
    /// <summary>
    /// Clicks the View button for a specific project
    /// </summary>
    /// <param name="projectName">Project name</param>
    public async Task ClickViewProjectButtonAsync(string projectName)
    {
        await _page.ClickAsync($"[data-testid='view-{projectName}']");
    }
    
    /// <summary>
    /// Clicks the project name link for navigation
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
    /// Checks if the project name is clickable
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
    /// <returns>First project name</returns>
    public async Task<string> GetFirstProjectNameAsync()
    {
        // Try to get the first project name from the table
        var firstProjectElement = await _page.QuerySelectorAsync("tbody tr:first-child td:nth-child(2) a");
        if (firstProjectElement != null)
        {
            var text = await firstProjectElement.TextContentAsync();
            return text?.Trim() ?? "";
        }
        return "";
    }
    
    /// <summary>
    /// Navigates to project dashboard using the View button with proper navigation waiting
    /// </summary>
    /// <param name="projectName">Project name</param>
    public async Task NavigateToProjectDashboardUsingViewButtonAsync(string projectName)
    {
        // Look for View button in the actions column - try multiple approaches
        var viewButton = await _page.QuerySelectorAsync($"[data-testid='view-{projectName}']");
        if (viewButton != null)
        {
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await viewButton.ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        else
        {
            // Fallback: look for any View button and use navigation waiting
            await _page.ClickAsync("button:has-text('View')");
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
    }
    
    /// <summary>
    /// Navigates to project dashboard using the clickable project name with proper navigation waiting
    /// </summary>
    /// <param name="projectName">Project name</param>
    public async Task NavigateToProjectDashboardUsingProjectNameAsync(string projectName)
    {
        // Try specific project name link first
        var projectLink = await _page.QuerySelectorAsync($"[data-testid='project-name-link-{projectName}']");
        if (projectLink != null)
        {
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await projectLink.ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        else
        {
            // Fallback: click the first project name link
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await _page.ClickAsync("tbody tr:first-child td:nth-child(2) a");
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
    }
}