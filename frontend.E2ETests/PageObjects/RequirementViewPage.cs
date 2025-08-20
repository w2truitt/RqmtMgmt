using Microsoft.Playwright;

namespace frontend.E2ETests.PageObjects;

/// <summary>
/// Page object for the Requirement View page
/// </summary>
public class RequirementViewPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    
    public RequirementViewPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }
    
    /// <summary>
    /// Navigates to the requirement view page within a project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="requirementId">Requirement ID</param>
    public async Task NavigateToAsync(int projectId, int requirementId)
    {
        await _page.GotoAsync($"{_baseUrl}/projects/{projectId}/requirements/{requirementId}");
    }
    
    /// <summary>
    /// Waits for the requirement view page to load
    /// </summary>
    public async Task WaitForPageLoadAsync()
    {
        await _page.WaitForSelectorAsync("h3", new PageWaitForSelectorOptions { Timeout = 30000 });
    }
    
    /// <summary>
    /// Gets the requirement title
    /// </summary>
    /// <returns>The requirement title</returns>
    public async Task<string> GetRequirementTitleAsync()
    {
        var titleElement = await _page.QuerySelectorAsync("h3");
        return await titleElement?.TextContentAsync() ?? string.Empty;
    }
    
    /// <summary>
    /// Gets the requirement description
    /// </summary>
    /// <returns>The requirement description</returns>
    public async Task<string> GetRequirementDescriptionAsync()
    {
        var descElement = await _page.QuerySelectorAsync(".requirement-description");
        return await descElement?.TextContentAsync() ?? string.Empty;
    }
    
    /// <summary>
    /// Gets the requirement type
    /// </summary>
    /// <returns>The requirement type</returns>
    public async Task<string> GetRequirementTypeAsync()
    {
        var typeElement = await _page.QuerySelectorAsync(".badge:has-text('Type')");
        return await typeElement?.TextContentAsync() ?? string.Empty;
    }
    
    /// <summary>
    /// Gets the requirement status
    /// </summary>
    /// <returns>The requirement status</returns>
    public async Task<string> GetRequirementStatusAsync()
    {
        var statusElement = await _page.QuerySelectorAsync(".badge:has-text('Status')");
        return await statusElement?.TextContentAsync() ?? string.Empty;
    }
    
    /// <summary>
    /// Clicks the edit button
    /// </summary>
    public async Task ClickEditButtonAsync()
    {
        await _page.ClickAsync("button:has-text('Edit')");
    }
    
    /// <summary>
    /// Clicks the back button
    /// </summary>
    public async Task ClickBackButtonAsync()
    {
        await _page.ClickAsync("button:has-text('Back')");
    }
    
    /// <summary>
    /// Checks if the edit button is visible
    /// </summary>
    /// <returns>True if edit button is visible</returns>
    public async Task<bool> IsEditButtonVisibleAsync()
    {
        return await _page.IsVisibleAsync("button:has-text('Edit')");
    }
    
    /// <summary>
    /// Checks if the requirement has a parent requirement displayed
    /// </summary>
    /// <returns>True if parent requirement is shown</returns>
    public async Task<bool> HasParentRequirementAsync()
    {
        return await _page.IsVisibleAsync("text=Parent Requirement");
    }
    
    /// <summary>
    /// Checks if the requirement has child requirements displayed
    /// </summary>
    /// <returns>True if child requirements are shown</returns>
    public async Task<bool> HasChildRequirementsAsync()
    {
        return await _page.IsVisibleAsync("text=Child Requirements");
    }
    
    /// <summary>
    /// Gets the requirement ID from the URL
    /// </summary>
    /// <returns>The requirement ID</returns>
    public async Task<string> GetRequirementIdFromUrlAsync()
    {
        var url = _page.Url;
        var parts = url.Split('/');
        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (parts[i] == "requirements" && int.TryParse(parts[i + 1], out _))
            {
                return parts[i + 1];
            }
        }
        return string.Empty;
    }
}
