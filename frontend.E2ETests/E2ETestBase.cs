using frontend.E2ETests.Fixtures;
using Microsoft.Playwright;
using Xunit;

namespace frontend.E2ETests;

/// <summary>
/// Optimized base class for E2E tests using shared browser with isolated contexts per test.
/// Creates lightweight BrowserContext + Page instead of expensive full browser creation.
/// 
/// Performance improvement: ~2-3 seconds saved per test by reusing browser instance.
/// </summary>
[Collection("Playwright")]
public abstract class E2ETestBase : IAsyncLifetime
{
    protected readonly PlaywrightFixture Fixture;
    public IBrowserContext Context { get; protected set; } = null!;
    public IPage Page { get; protected set; } = null!;
    protected string BaseUrl { get; } = "https://rqmtmgmt.local";

    protected E2ETestBase(PlaywrightFixture fixture)
    {
        Fixture = fixture;
    }

    /// <summary>
    /// Creates isolated browser context and page per test.
    /// This is a fast operation (~100ms) compared to browser creation (~2-3s).
    /// Provides complete test isolation while sharing the expensive browser instance.
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        Context = await Fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 }
        });

        Page = await Context.NewPageAsync();
        Page.SetDefaultTimeout(30000);
        Page.SetDefaultNavigationTimeout(30000);
    }

    /// <summary>
    /// Cleanup context and page after each test.
    /// Fast operation that maintains test isolation.
    /// </summary>
    public virtual async Task DisposeAsync()
    {
        try
        {
            if (Page != null && !Page.IsClosed)
            {
                await Page.CloseAsync();
            }
        }
        catch (Exception)
        {
            // Ignore cleanup errors
        }
        
        try
        {
            if (Context != null)
            {
                await Context.DisposeAsync();
            }
        }
        catch (Exception)
        {
            // Ignore cleanup errors
        }
    }

    /// <summary>
    /// Creates a unique test identifier for test isolation
    /// </summary>
    /// <returns>Unique test ID</returns>
    protected string CreateTestId()
    {
        return Guid.NewGuid().ToString()[..8];
    }
    
    /// <summary>
    /// Waits for an element to be visible
    /// </summary>
    /// <param name="selector">Element selector</param>
    /// <param name="timeout">Timeout in milliseconds</param>
    protected async Task WaitForElementAsync(string selector, int timeout = 5000)
    {
        await Page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
        {
            Timeout = timeout
        });
    }

    /// <summary>
    /// Ensures the navigation menu is expanded and visible for link interaction
    /// </summary>
    protected async Task EnsureNavigationMenuVisible()
    {
        await Page.EvaluateAsync(@"
            // Force the navigation menu to expand by removing the collapse class
            const navMenu = document.querySelector('.nav-scrollable');
            if (navMenu) {
                navMenu.classList.remove('collapse');
                navMenu.style.display = 'block';
            }
        ");
        
        // Wait for changes to take effect
        await Page.WaitForTimeoutAsync(500);
    }

    /// <summary>
    /// Selects an existing static project instead of creating a new one.
    /// Updated to handle the recent projects functionality by navigating to projects page if needed.
    /// </summary>
    /// <param name="projectIndex">Index of the static project (0-3)</param>
    protected async Task SelectExistingProject(int projectIndex = 0)
    {
        var project = TestData.TestDataFactory.GetStaticProject(projectIndex);
        
        // Ensure we start from home page in clean state
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Try to select project from dropdown first (recent projects)
        if (await TrySelectProjectFromDropdown(project.Name))
        {
            return;
        }
        
        // If not found in recent projects, navigate to projects page and select from there
        await NavigateToProjectsPageAndSelect(project.Name);
    }

    /// <summary>
    /// Attempts to select a project from the project selector dropdown
    /// </summary>
    /// <param name="projectName">Name of the project to select</param>
    /// <returns>True if project was found and selected, false otherwise</returns>
    private async Task<bool> TrySelectProjectFromDropdown(string projectName)
    {
        try
        {
            // Click the project selector to open dropdown
            await ClickProjectSelector();
            
            // Wait for dropdown to appear
            await Page.WaitForTimeoutAsync(1000);
            
            // Force Bootstrap dropdowns to be visible using JavaScript
            await Page.EvaluateAsync(@"
                const dropdowns = document.querySelectorAll('.dropdown-menu');
                dropdowns.forEach(dropdown => {
                    dropdown.classList.add('show');
                    dropdown.style.display = 'block';
                    dropdown.style.position = 'static';
                    dropdown.style.transform = 'none';
                });
            ");
            
            // Wait for changes to take effect
            await Page.WaitForTimeoutAsync(500);
            
            // Try to find and select the project
            return await TrySelectProjectFromCurrentDropdown(projectName);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Navigates to the projects page and selects the project from there
    /// </summary>
    /// <param name="projectName">Name of the project to select</param>
    private async Task NavigateToProjectsPageAndSelect(string projectName)
    {
        // Navigate to projects page
        await Page.GotoAsync($"{BaseUrl}/projects");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for projects to load
        await Page.WaitForTimeoutAsync(2000);
        
        // Try to find and click the project
        var projectStrategies = new[]
        {
            $"a:has-text('{projectName}')",
            $"button:has-text('{projectName}')",
            $".project-card:has-text('{projectName}')",
            $".project-item:has-text('{projectName}')",
            $"[data-project-name='{projectName}']"
        };

        foreach (var strategy in projectStrategies)
        {
            try
            {
                var projectElement = Page.Locator(strategy);
                if (await projectElement.CountAsync() > 0)
                {
                    await projectElement.First.ClickAsync();
                    await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    return;
                }
            }
            catch (Exception)
            {
                continue;
            }
        }
        
        throw new Exception($"Could not find project '{projectName}' on the projects page");
    }

    /// <summary>
    /// Clicks the project selector dropdown button
    /// </summary>
    private async Task ClickProjectSelector()
    {
        // Try different possible selectors for the project selector
        var selectors = new[]
        {
            ".project-selector-container button",
            "button:has-text('Select Project')",
            ".dropdown-toggle.project-selector-btn",
            "[data-testid='project-selector']",
            "button:has(.bi-folder)",
            "button:has(.bi-folder-plus)"
        };

        foreach (var selector in selectors)
        {
            var element = Page.Locator(selector).First;
            if (await element.CountAsync() > 0)
            {
                await element.ClickAsync();
                await Page.WaitForTimeoutAsync(500); // Give time for dropdown to appear
                return;
            }
        }
        
        throw new Exception("Could not find project selector button");
    }

    /// <summary>
    /// Attempts to select a project from the currently visible dropdown items
    /// </summary>
    /// <param name="projectName">Name of the project to select</param>
    /// <returns>True if project was found and selected, false otherwise</returns>
    private async Task<bool> TrySelectProjectFromCurrentDropdown(string projectName)
    {
        // Try multiple strategies to find the project
        var strategies = new[]
        {
            // Strategy 1: Look for dropdown items directly by text
            $".dropdown-item:has-text('{projectName}')",
            
            // Strategy 2: Look within project dropdown specifically
            $".project-dropdown .dropdown-item:has-text('{projectName}')",
            
            // Strategy 3: Look for any button containing the project name
            $"button:has-text('{projectName}')",
            
            // Strategy 4: Look within dropdown menu
            $".dropdown-menu .dropdown-item:has-text('{projectName}')"
        };

        foreach (var strategy in strategies)
        {
            try
            {
                var projectItem = Page.Locator(strategy);
                var count = await projectItem.CountAsync();
                
                if (count > 0)
                {
                    // Try regular click first, then force with JavaScript
                    try
                    {
                        await projectItem.First.ClickAsync();
                    }
                    catch
                    {
                        // Fallback: force click with JavaScript
                        await projectItem.First.EvaluateAsync("element => element.click()");
                    }
                    await Page.WaitForTimeoutAsync(1000);
                    return true;
                }
            }
            catch (Exception)
            {
                continue;
            }
        }
        
        return false;
    }
}