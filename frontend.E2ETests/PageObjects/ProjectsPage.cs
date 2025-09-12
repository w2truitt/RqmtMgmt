using Microsoft.Playwright;

namespace frontend.E2ETests.PageObjects;

/// <summary>
/// Page object for the Projects page
/// FIXED: Updated to handle JavaScript confirm dialogs properly
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
    /// Waits for the page to load
    /// </summary>
    public async Task WaitForPageLoadAsync()
    {
        // Wait for any projects header or the projects page to be ready
        var selectors = new[]
        {
            "h1:has-text('Projects')",
            "h2:has-text('Projects')",
            "h3:has-text('Projects')",
            "[data-testid='projects-header']",
            ".page-title:has-text('Projects')",
            "table", // Projects table
            ".btn:has-text('New Project')", // New project button
            ".btn:has-text('Create Project')" // Create project button
        };
        
        foreach (var selector in selectors)
        {
            try
            {
                await _page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions { Timeout = 5000 });
                return; // Found one, page is ready
            }
            catch (TimeoutException)
            {
                continue; // Try next selector
            }
        }
        
        // If none found, just wait for network idle as fallback
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
    
    /// <summary>
    /// Clicks the create project button
    /// </summary>
    public async Task ClickCreateProjectAsync()
    {
        var selectors = new[]
        {
            "button:has-text('Create Project')",
            "button:has-text('Add Project')",
            "button:has-text('New Project')",
            "[data-testid='create-project-button']",
            ".btn-success:has-text('Create')"
        };
        
        foreach (var selector in selectors)
        {
            if (await _page.IsVisibleAsync(selector))
            {
                await _page.ClickAsync(selector);
                return;
            }
        }
        
        throw new Exception("Create project button not found with any expected selector");
    }
    
    /// <summary>
    /// Waits for the form modal to appear
    /// </summary>
    public async Task WaitForFormModalAsync()
    {
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { Timeout = 10000 });
    }
    
    /// <summary>
    /// Waits for the form modal to hide
    /// </summary>
    public async Task WaitForFormModalToHideAsync()
    {
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = 10000 });
    }
    
    /// <summary>
    /// Fills the project form with flexible selectors
    /// </summary>
    /// <param name="name">Project name</param>
    /// <param name="code">Project code</param>
    /// <param name="description">Project description</param>
    public async Task FillProjectFormAsync(string name, string code, string description)
    {
        // Try multiple selectors for name input
        var nameSelectors = new[]
        {
            "[data-testid='name-input']",
            "input[name*='name']",
            "input[placeholder*='name']"
        };
        
        foreach (var selector in nameSelectors)
        {
            if (await _page.IsVisibleAsync(selector))
            {
                await _page.FillAsync(selector, name);
                break;
            }
        }
        
        // Try multiple selectors for code input
        var codeSelectors = new[]
        {
            "[data-testid='code-input']",
            "input[name*='code']",
            "input[placeholder*='code']"
        };
        
        foreach (var selector in codeSelectors)
        {
            if (await _page.IsVisibleAsync(selector))
            {
                await _page.FillAsync(selector, code);
                break;
            }
        }
        
        // Try multiple selectors for description input
        var descriptionSelectors = new[]
        {
            "[data-testid='description-input']",
            "textarea[name*='description']",
            "textarea[placeholder*='description']"
        };
        
        foreach (var selector in descriptionSelectors)
        {
            if (await _page.IsVisibleAsync(selector))
            {
                await _page.FillAsync(selector, description);
                break;
            }
        }
    }
    
    /// <summary>
    /// Saves the project form
    /// </summary>
    public async Task SaveProjectAsync()
    {
        var selectors = new[]
        {
            "[data-testid='save-button']",
            "button:has-text('Save')",
            "button:has-text('Create')",
            "button:has-text('Submit')",
            ".btn-primary:has-text('Save')"
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
    /// Cancels the project form
    /// </summary>
    public async Task CancelFormAsync()
    {
        var selectors = new[]
        {
            "[data-testid='cancel-button']",
            "button:has-text('Cancel')",
            ".btn-secondary:has-text('Cancel')",
            "button.btn-secondary"
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
    /// Gets the current value of the project name input
    /// </summary>
    public async Task<string> GetProjectNameInputValueAsync()
    {
        var selectors = new[]
        {
            "[data-testid='name-input']",
            "input[name*='name']",
            "input[placeholder*='name']"
        };
        
        foreach (var selector in selectors)
        {
            if (await _page.IsVisibleAsync(selector))
            {
                return await _page.InputValueAsync(selector);
            }
        }
        
        return string.Empty;
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
        var selectors = new[]
        {
            $"[data-testid='edit-{name}']",
            "[data-testid='edit-project']",
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
    /// Clicks the delete button for a project
    /// FIXED: Now handles JavaScript confirm dialogs properly
    /// </summary>
    /// <param name="name">Project name</param>
    public async Task DeleteProjectAsync(string name)
    {
        // Set up dialog handler for JavaScript confirm
        _page.Dialog += async (_, dialog) =>
        {
            if (dialog.Type == DialogType.Confirm)
            {
                await dialog.AcceptAsync();
            }
        };
        
        var selectors = new[]
        {
            $"[data-testid='delete-{name}']",
            "[data-testid='delete-project']",
            "button[title*='Delete']",
            ".btn-outline-danger:has(i.bi-trash)",
            ".btn-danger:has-text('Delete')"
        };
        
        foreach (var selector in selectors)
        {
            if (await _page.IsVisibleAsync(selector))
            {
                await _page.ClickAsync(selector);
                // Wait a moment for the dialog to appear and be handled
                await Task.Delay(1000);
                return;
            }
        }
    }
    
    /// <summary>
    /// Confirms deletion in the confirmation dialog
    /// FIXED: Now handles JavaScript confirm dialogs instead of HTML elements
    /// </summary>
    public async Task ConfirmDeleteAsync()
    {
        // Handle both HTML-based confirmation dialogs and JavaScript confirm dialogs
        
        // Wait a moment for any dialog to appear
        await Task.Delay(500);
        
        // Try to handle HTML-based confirmation dialogs with improved selectors
        var selectors = new[]
        {
            "[data-testid='confirm-delete']",
            "[data-testid='delete-confirm']", 
            "button:has-text('Delete')",
            "button:has-text('Confirm')",
            "button:has-text('Yes')",
            "button:has-text('OK')",
            ".btn-danger",
            ".modal button:has-text('Delete')",
            ".modal .btn-danger",
            ".modal-footer button:has-text('Delete')",
            ".modal-footer .btn-danger"
        };
        
        foreach (var selector in selectors)
        {
            try
            {
                if (await _page.IsVisibleAsync(selector))
                {
                    await _page.ClickAsync(selector);
                    await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    return;
                }
            }
            catch (TimeoutException)
            {
                // Continue to next selector
                continue;
            }
        }
        
        // If no HTML confirmation dialog found, assume JavaScript confirm was handled
        // or deletion completed without confirmation dialog
        await Task.Delay(1000); // Allow time for any async operations to complete
    }
    
    /// <summary>
    /// Searches for projects
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    public async Task SearchProjectsAsync(string searchTerm)
    {
        var searchSelectors = new[]
        {
            "[data-testid='search-input']",
            "input[placeholder*='Search projects']",
            "input[placeholder*='search']",
            "input[type='search']",
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
                    ".btn:has(i.bi-search)",
                    "[data-testid='search-button']"
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
    /// Gets the count of visible project rows
    /// </summary>
    /// <returns>Number of project rows</returns>
    public async Task<int> GetProjectCountAsync()
    {
        var selectors = new[]
        {
            "[data-testid='project-row']",
            "table tbody tr",
            ".project-item"
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
}