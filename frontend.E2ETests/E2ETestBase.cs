using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Xunit;

namespace frontend.E2ETests;

/// <summary>
/// Base class for end-to-end tests using Playwright
/// </summary>
public abstract class E2ETestBase : IAsyncLifetime
{
    protected WebApplicationFactory<Program> Factory { get; private set; } = null!;
    protected IPlaywright PlaywrightInstance { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;
    protected string BaseUrl { get; private set; } = null!;
    
    /// <summary>
    /// Initialize test setup
    /// </summary>
    public async Task InitializeAsync()
    {
        // Initialize Playwright
        PlaywrightInstance = await Playwright.CreateAsync();
        Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true // Set to false for debugging
        });
        
        // Create a new page for each test with explicit viewport for desktop navigation
        Page = await Browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = 1280,
                Height = 720
            }
        });
        
        // Use the actual running frontend URL (Docker containers with nginx proxy)
        BaseUrl = "http://localhost:8080";
        
        // Create a minimal factory just for cleanup purposes (some tests might reference it)
        Factory = new WebApplicationFactory<Program>();
    }
    
    /// <summary>
    /// Cleanup test resources
    /// </summary>
    public async Task DisposeAsync()
    {
        await Page?.CloseAsync()!;
        await Browser?.CloseAsync()!;
        PlaywrightInstance?.Dispose();
        Factory?.Dispose();
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
    /// Selects an existing static project instead of creating a new one
    /// </summary>
    /// <param name="projectIndex">Index of the static project (0-3)</param>
    protected async Task SelectExistingProject(int projectIndex = 0)
    {
        var project = TestData.TestDataFactory.GetStaticProject(projectIndex);
        
        // Ensure we start from home page in clean state
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
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
        
        // Select the project by name
        await SelectProjectByName(project.Name);
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
    /// Selects a project by name from the dropdown
    /// </summary>
    /// <param name="projectName">Name of the project to select</param>
    private async Task SelectProjectByName(string projectName)
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
                    return;
                }
            }
            catch (Exception)
            {
                continue;
            }
        }
        
        throw new Exception($"Could not find project '{projectName}' in dropdown");
    }
}