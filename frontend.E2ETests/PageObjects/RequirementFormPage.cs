using Microsoft.Playwright;

namespace frontend.E2ETests.PageObjects;

/// <summary>
/// Page object for the Requirement Form page (Create/Edit)
/// </summary>
public class RequirementFormPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    
    public RequirementFormPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }
    
    /// <summary>
    /// Navigates to the new requirement form within a project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    public async Task NavigateToNewRequirementAsync(int projectId)
    {
        await _page.GotoAsync($"{_baseUrl}/projects/{projectId}/requirements/new");
    }
    
    /// <summary>
    /// Navigates to the edit requirement form within a project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="requirementId">Requirement ID</param>
    public async Task NavigateToEditRequirementAsync(int projectId, int requirementId)
    {
        await _page.GotoAsync($"{_baseUrl}/projects/{projectId}/requirements/{requirementId}/edit");
    }
    
    /// <summary>
    /// Waits for the requirement form page to load
    /// </summary>
    public async Task WaitForPageLoadAsync()
    {
        await _page.WaitForSelectorAsync("h3", new PageWaitForSelectorOptions { Timeout = 30000 });
    }
    
    /// <summary>
    /// Fills the requirement form
    /// </summary>
    /// <param name="title">Requirement title</param>
    /// <param name="description">Requirement description</param>
    /// <param name="type">Requirement type</param>
    /// <param name="status">Requirement status</param>
    /// <param name="parentRequirementId">Parent requirement ID (optional)</param>
    public async Task FillRequirementFormAsync(string title, string description, string type, string status, int? parentRequirementId = null)
    {
        await _page.FillAsync("input[placeholder='Enter requirement title']", title);
        await _page.FillAsync("textarea[placeholder='Enter detailed description']", description);
        
        if (!string.IsNullOrEmpty(type))
        {
            await _page.SelectOptionAsync("select:has-text('Type')", type);
        }
        
        if (!string.IsNullOrEmpty(status))
        {
            await _page.SelectOptionAsync("select:has-text('Status')", status);
        }
        
        if (parentRequirementId.HasValue && parentRequirementId.Value > 0)
        {
            await _page.SelectOptionAsync("select:has-text('Parent Requirement')", parentRequirementId.Value.ToString());
        }
    }
    
    /// <summary>
    /// Clicks the save requirement button
    /// </summary>
    public async Task SaveRequirementAsync()
    {
        await _page.ClickAsync("button:has-text('Save Requirement')");
    }
    
    /// <summary>
    /// Clicks the cancel button
    /// </summary>
    public async Task CancelAsync()
    {
        await _page.ClickAsync("button:has-text('Cancel')");
    }
    
    /// <summary>
    /// Checks if a validation error message is displayed
    /// </summary>
    /// <param name="message">Expected error message</param>
    /// <returns>True if error message is visible</returns>
    public async Task<bool> IsValidationErrorVisibleAsync(string message)
    {
        return await _page.IsVisibleAsync($".text-danger:has-text('{message}')");
    }
    
    /// <summary>
    /// Gets the current requirement ID from the form (for edit scenarios)
    /// </summary>
    /// <returns>The requirement ID</returns>
    public async Task<string> GetRequirementIdAsync()
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
    
    /// <summary>
    /// Checks if the form is in edit mode
    /// </summary>
    /// <returns>True if in edit mode</returns>
    public async Task<bool> IsEditModeAsync()
    {
        return await _page.IsVisibleAsync("h3:has-text('Edit Requirement')");
    }
    
    /// <summary>
    /// Checks if the form is in create mode
    /// </summary>
    /// <returns>True if in create mode</returns>
    public async Task<bool> IsCreateModeAsync()
    {
        return await _page.IsVisibleAsync("h3:has-text('New Requirement')");
    }
}
