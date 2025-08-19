using Microsoft.Playwright;

namespace frontend.E2ETests.PageObjects;

/// <summary>
/// Page object for the Project Team page
/// </summary>
public class ProjectTeamPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    
    public ProjectTeamPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }
    
    /// <summary>
    /// Navigates to the project team page
    /// </summary>
    /// <param name="projectId">Project ID</param>
    public async Task NavigateToAsync(int projectId)
    {
        await _page.GotoAsync($"{_baseUrl}/projects/{projectId}/team");
    }
    
    /// <summary>
    /// Waits for the page to load
    /// </summary>
    public async Task WaitForPageLoadAsync()
    {
        await _page.WaitForSelectorAsync("h3:has-text('Project Team')", new PageWaitForSelectorOptions { Timeout = 30000 });
    }
    
    /// <summary>
    /// Clicks the add team member button
    /// </summary>
    public async Task ClickAddTeamMemberAsync()
    {
        await _page.ClickAsync("[data-testid='add-team-member-button']");
    }
    
    /// <summary>
    /// Waits for the team member form modal to appear
    /// </summary>
    public async Task WaitForFormModalAsync()
    {
        await _page.WaitForSelectorAsync("[data-testid='team-member-modal']", new PageWaitForSelectorOptions { Timeout = 10000 });
    }
    
    /// <summary>
    /// Waits for the form modal to hide
    /// </summary>
    public async Task WaitForFormModalToHideAsync()
    {
        await _page.WaitForSelectorAsync("[data-testid='team-member-modal']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = 10000 });
    }
    
    /// <summary>
    /// Fills the team member form
    /// </summary>
    /// <param name="userId">User ID to add</param>
    /// <param name="role">Role to assign</param>
    public async Task FillTeamMemberFormAsync(string userId, string role)
    {
        await _page.SelectOptionAsync("[data-testid='user-select']", userId);
        await _page.SelectOptionAsync("[data-testid='role-select']", role);
    }
    
    /// <summary>
    /// Saves the team member form
    /// </summary>
    public async Task SaveTeamMemberAsync()
    {
        await _page.ClickAsync("[data-testid='save-team-member-button']");
    }
    
    /// <summary>
    /// Cancels the team member form
    /// </summary>
    public async Task CancelTeamMemberAsync()
    {
        await _page.ClickAsync("[data-testid='cancel-team-member-button']");
    }
    
    /// <summary>
    /// Checks if a team member is visible in the list
    /// </summary>
    /// <param name="userName">User name to look for</param>
    /// <returns>True if team member is visible</returns>
    public async Task<bool> IsTeamMemberVisibleAsync(string userName)
    {
        return await _page.IsVisibleAsync($"text={userName}");
    }
    
    /// <summary>
    /// Clicks the edit button for a team member
    /// </summary>
    /// <param name="userName">User name</param>
    public async Task EditTeamMemberAsync(string userName)
    {
        await _page.ClickAsync($"[data-testid='edit-{userName}']");
    }
    
    /// <summary>
    /// Clicks the remove button for a team member
    /// </summary>
    /// <param name="userName">User name</param>
    public async Task RemoveTeamMemberAsync(string userName)
    {
        await _page.ClickAsync($"[data-testid='remove-{userName}']");
    }
    
    /// <summary>
    /// Confirms removal in the confirmation dialog
    /// </summary>
    public async Task ConfirmRemoveAsync()
    {
        await _page.ClickAsync("[data-testid='confirm-remove']");
    }
    
    /// <summary>
    /// Gets the count of visible team member rows
    /// </summary>
    /// <returns>Number of team member rows</returns>
    public async Task<int> GetTeamMemberCountAsync()
    {
        var rows = await _page.QuerySelectorAllAsync("[data-testid='team-member-row']");
        return rows.Count;
    }
    
    /// <summary>
    /// Gets the role of a specific team member
    /// </summary>
    /// <param name="userName">User name</param>
    /// <returns>Role name</returns>
    public async Task<string> GetTeamMemberRoleAsync(string userName)
    {
        var roleElement = await _page.QuerySelectorAsync($"[data-testid='team-member-row']:has-text('{userName}') [data-testid='member-role']");
        if (roleElement != null)
        {
            return await roleElement.TextContentAsync() ?? "";
        }
        return "";
    }
    
    /// <summary>
    /// Searches for team members
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    public async Task SearchTeamMembersAsync(string searchTerm)
    {
        await _page.FillAsync("input[placeholder*='Search team members']", searchTerm);
        await _page.ClickAsync("text=Search");
    }
}
