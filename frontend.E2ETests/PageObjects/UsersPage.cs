using Microsoft.Playwright;

namespace frontend.E2ETests.PageObjects;

/// <summary>
/// Page object for the Users page
/// FIXED: Updated selectors to match actual page structure
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
    /// FIXED: Page uses "Add User" button, not data-testid
    /// </summary>
    public async Task ClickCreateUserAsync()
    {
        // Try multiple selectors to find the create button
        var selectors = new[]
        {
            "button:has-text('Add User')",
            ".btn-success:has-text('Add User')",
            "[data-testid='create-user-button']"
        };
        
        foreach (var selector in selectors)
        {
            if (await _page.IsVisibleAsync(selector))
            {
                await _page.ClickAsync(selector);
                return;
            }
        }
        
        throw new Exception("Create user button not found with any expected selector");
    }
    
    /// <summary>
    /// Fills the user form
    /// </summary>
    /// <param name="userName">User name</param>
    /// <param name="email">User email</param>
    /// <param name="roles">User roles</param>
    public async Task FillUserFormAsync(string userName, string email, string[] roles)
    {
        // Try multiple selectors for username input
        var usernameSelectors = new[]
        {
            "[data-testid='username-input']",
            "input[name*='username']",
            "input[placeholder*='username']"
        };
        
        foreach (var selector in usernameSelectors)
        {
            if (await _page.IsVisibleAsync(selector))
            {
                await _page.FillAsync(selector, userName);
                break;
            }
        }
        
        // Try multiple selectors for email input
        var emailSelectors = new[]
        {
            "[data-testid='email-input']",
            "input[name*='email']",
            "input[placeholder*='email']"
        };
        
        foreach (var selector in emailSelectors)
        {
            if (await _page.IsVisibleAsync(selector))
            {
                await _page.FillAsync(selector, email);
                break;
            }
        }
        
        // Select roles using checkboxes (updated implementation)
        foreach (var role in roles)
        {
            var roleSelectors = new[]
            {
                $"#role_{role}",
                $"input[value='{role}']",
                $"input[name*='role'][value='{role}']"
            };
            
            foreach (var selector in roleSelectors)
            {
                if (await _page.IsVisibleAsync(selector))
                {
                    await _page.CheckAsync(selector);
                    break;
                }
            }
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
            var selectors = new[] { $"#role_{role}", $"input[value='{role}']" };
            foreach (var selector in selectors)
            {
                if (await _page.IsVisibleAsync(selector))
                {
                    await _page.CheckAsync(selector);
                    break;
                }
            }
        }
        
        // Remove roles
        foreach (var role in rolesToRemove)
        {
            var selectors = new[] { $"#role_{role}", $"input[value='{role}']" };
            foreach (var selector in selectors)
            {
                if (await _page.IsVisibleAsync(selector))
                {
                    await _page.UncheckAsync(selector);
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Checks if a role checkbox is selected for a user
    /// </summary>
    /// <param name="roleName">Role name</param>
    /// <returns>True if the role checkbox is checked</returns>
    public async Task<bool> IsRoleSelectedAsync(string roleName)
    {
        var selectors = new[] { $"#role_{roleName}", $"input[value='{roleName}']" };
        
        foreach (var selector in selectors)
        {
            if (await _page.IsVisibleAsync(selector))
            {
                return await _page.IsCheckedAsync(selector);
            }
        }
        
        return false;
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
        var selectors = new[]
        {
            "[data-testid='save-button']",
            "button:has-text('Save')",
            "button:has-text('Create')",
            "button:has-text('Submit')"
        };
        
        foreach (var selector in selectors)
        {
            if (await _page.IsVisibleAsync(selector))
            {
                await _page.ClickAsync(selector);
                return;
            }
        }
    }
    
    /// <summary>
    /// Searches for users
    /// FIXED: Use flexible search input selector
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    public async Task SearchUsersAsync(string searchTerm)
    {
        var searchSelectors = new[]
        {
            "[data-testid='search-input']",
            "input[placeholder*='Search users']",
            "input[type='text']"
        };
        
        foreach (var selector in searchSelectors)
        {
            if (await _page.IsVisibleAsync(selector))
            {
                await _page.FillAsync(selector, searchTerm);
                
                // Try to find and click search button
                var searchButtonSelectors = new[]
                {
                    "button:has-text('Search')",
                    ".btn:has(i.bi-search)"
                };
                
                foreach (var btnSelector in searchButtonSelectors)
                {
                    if (await _page.IsVisibleAsync(btnSelector))
                    {
                        await _page.ClickAsync(btnSelector);
                        return;
                    }
                }
                
                // Fallback: press Enter
                await _page.PressAsync(selector, "Enter");
                return;
            }
        }
    }
    
    /// <summary>
    /// Gets the count of visible user rows
    /// </summary>
    /// <returns>Number of user rows</returns>
    public async Task<int> GetUserCountAsync()
    {
        var selectors = new[]
        {
            "[data-testid='user-row']",
            "table tbody tr"
        };
        
        foreach (var selector in selectors)
        {
            var rows = await _page.QuerySelectorAllAsync(selector);
            if (rows.Count > 0)
            {
                return rows.Count;
            }
        }
        
        return 0;
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
        var selectors = new[]
        {
            $"[data-testid='edit-{userName}']",
            "[data-testid='edit-user']",
            "button[title*='Edit']",
            ".btn:has(i.bi-pencil)"
        };
        
        foreach (var selector in selectors)
        {
            if (await _page.IsVisibleAsync(selector))
            {
                await _page.ClickAsync(selector);
                return;
            }
        }
    }
    
    /// <summary>
    /// Clicks the delete button for a user
    /// </summary>
    /// <param name="userName">User name</param>
    public async Task DeleteUserAsync(string userName)
    {
        var selectors = new[]
        {
            $"[data-testid='delete-{userName}']",
            "[data-testid='delete-user']",
            "button[title*='Delete']",
            ".btn-outline-danger:has(i.bi-trash)"
        };
        
        foreach (var selector in selectors)
        {
            if (await _page.IsVisibleAsync(selector))
            {
                await _page.ClickAsync(selector);
                return;
            }
        }
    }
    
    /// <summary>
    /// Confirms deletion in the confirmation dialog
    /// </summary>
    public async Task ConfirmDeleteAsync()
    {
        var selectors = new[]
        {
            "[data-testid='confirm-delete']",
            "button:has-text('Delete')",
            "button:has-text('Confirm')",
            ".btn-danger"
        };
        
        foreach (var selector in selectors)
        {
            if (await _page.IsVisibleAsync(selector))
            {
                await _page.ClickAsync(selector);
                return;
            }
        }
    }
}