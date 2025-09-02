using Microsoft.Playwright;

namespace frontend.E2ETests.PageObjects;

/// <summary>
/// Page object for the Users page
/// </summary>
public class UsersPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    
    public UsersPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }
    
    /// <summary>
    /// Navigates to the users page
    /// </summary>
    public async Task NavigateToAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/users");
    }
    
    /// <summary>
    /// Clicks the create user button
    /// </summary>
    public async Task ClickCreateUserAsync()
    {
        await _page.ClickAsync("[data-testid='create-user-button']");
    }
    
    /// <summary>
    /// Fills the user form
    /// </summary>
    /// <param name="userName">User name</param>
    /// <param name="email">User email</param>
    /// <param name="roles">User roles</param>
    public async Task FillUserFormAsync(string userName, string email, string[] roles)
    {
        await _page.FillAsync("[data-testid='username-input']", userName);
        await _page.FillAsync("[data-testid='email-input']", email);
        
        // Select roles using checkboxes (updated implementation)
        foreach (var role in roles)
        {
            await _page.CheckAsync($"#role_{role}");
        }
    }
    
    /// <summary>
    /// Updates user roles by selecting/deselecting checkboxes
    /// </summary>
    /// <param name="rolesToAdd">Roles to add (check)</param>
    /// <param name="rolesToRemove">Roles to remove (uncheck)</param>
    public async Task UpdateUserRolesAsync(string[] rolesToAdd, string[] rolesToRemove)
    {
        // Add new roles
        foreach (var role in rolesToAdd)
        {
            await _page.CheckAsync($"#role_{role}");
        }
        
        // Remove roles
        foreach (var role in rolesToRemove)
        {
            await _page.UncheckAsync($"#role_{role}");
        }
    }
    
    /// <summary>
    /// Checks if a role checkbox is selected for a user
    /// </summary>
    /// <param name="roleName">Role name</param>
    /// <returns>True if the role checkbox is checked</returns>
    public async Task<bool> IsRoleSelectedAsync(string roleName)
    {
        return await _page.IsCheckedAsync($"#role_{roleName}");
    }
    
    /// <summary>
    /// Waits for the user form modal to appear
    /// </summary>
    public async Task WaitForFormModalAsync()
    {
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { Timeout = 10000 });
    }
    
    /// <summary>
    /// Waits for the user form modal to disappear
    /// </summary>
    public async Task WaitForFormModalToHideAsync()
    {
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = 10000 });
    }
    
    /// <summary>
    /// Saves the user form
    /// </summary>
    public async Task SaveUserAsync()
    {
        await _page.ClickAsync("[data-testid='save-button']");
    }
    
    /// <summary>
    /// Searches for users
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    public async Task SearchUsersAsync(string searchTerm)
    {
        await _page.FillAsync("[data-testid='search-input']", searchTerm);
        await _page.PressAsync("[data-testid='search-input']", "Enter");
    }
    
    /// <summary>
    /// Gets the count of visible user rows
    /// </summary>
    /// <returns>Number of user rows</returns>
    public async Task<int> GetUserCountAsync()
    {
        var rows = await _page.QuerySelectorAllAsync("[data-testid='user-row']");
        return rows.Count;
    }
    
    /// <summary>
    /// Checks if a user is visible in the list
    /// </summary>
    /// <param name="userName">User name to look for</param>
    /// <returns>True if user is visible</returns>
    public async Task<bool> IsUserVisibleAsync(string userName)
    {
        return await _page.IsVisibleAsync($"text={userName}");
    }
    
    /// <summary>
    /// Clicks the edit button for a user
    /// </summary>
    /// <param name="userName">User name</param>
    public async Task EditUserAsync(string userName)
    {
        await _page.ClickAsync($"[data-testid='edit-{userName}']");
    }
    
    /// <summary>
    /// Clicks the delete button for a user
    /// </summary>
    /// <param name="userName">User name</param>
    public async Task DeleteUserAsync(string userName)
    {
        await _page.ClickAsync($"[data-testid='delete-{userName}']");
    }
    
    /// <summary>
    /// Confirms deletion in the confirmation dialog
    /// </summary>
    public async Task ConfirmDeleteAsync()
    {
        await _page.ClickAsync("[data-testid='confirm-delete']");
    }
}