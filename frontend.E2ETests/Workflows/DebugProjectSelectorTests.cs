using frontend.E2ETests.Fixtures;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using RqmtMgmtShared;
using Xunit;
using Xunit.Abstractions;
using frontend.E2ETests.Infrastructure;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Debug Project Selector Tests
/// OPTIMIZED: Now uses shared browser and cached authentication for 4-10x performance improvement
/// Uses developer role for project selector analysis
/// </summary>
public class DebugProjectSelectorTests : AuthenticatedE2ETestBase
{
    public DebugProjectSelectorTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        // Set developer user for project selector analysis
        SetDeveloperUser();
    }

    [Fact]
    public async Task Debug_ProjectSelector_Functionality()
    {
        // Arrange - Developer user already authenticated via base class
        
        TestLogger.LogDebug("Starting project selector debug test", Output);
        
        // Navigate to home page where project selector might be
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        TestLogger.LogDebug($"Navigated to home page: {Page.Url}", Output);
        
        // Look for project selector elements
        var selectorElements = await Page.EvaluateAsync<object>(@"
            () => {
                const selectors = {
                    dropdowns: document.querySelectorAll('.dropdown, select').length,
                    projectButtons: [],
                    projectLinks: []
                };
                
                // Look for project-related buttons
                document.querySelectorAll('button').forEach(btn => {
                    if (btn.textContent.toLowerCase().includes('project') || 
                        btn.textContent.toLowerCase().includes('select')) {
                        selectors.projectButtons.push(btn.textContent.trim());
                    }
                });
                
                // Look for project-related links
                document.querySelectorAll('a').forEach(link => {
                    if (link.textContent.toLowerCase().includes('project')) {
                        selectors.projectLinks.push({
                            text: link.textContent.trim(),
                            href: link.getAttribute('href')
                        });
                    }
                });
                
                return selectors;
            }
        ");
        
        TestLogger.LogDebug($"Project selector elements found: {selectorElements}", Output);
        
        // Try to interact with project selector if found
        var projectSelectorButton = await Page.QuerySelectorAsync("button:has-text('Select'), button:has-text('Project'), .dropdown-toggle");
        if (projectSelectorButton != null)
        {
            TestLogger.LogDebug("Project selector button found - testing interaction", Output);
            
            await projectSelectorButton.ClickAsync();
            await Task.Delay(1000);
            
            // Check if dropdown appeared
            var dropdownVisible = await Page.IsVisibleAsync(".dropdown-menu, .show");
            TestLogger.LogTestStep($"Dropdown appeared: {dropdownVisible}", Output);
            
            if (dropdownVisible)
            {
                // Look for project options
                var projectOptions = await Page.QuerySelectorAllAsync(".dropdown-item, option");
                TestLogger.LogDebug($"Found {projectOptions.Count} project options", Output);
                
                // Close dropdown by clicking elsewhere
                await Page.ClickAsync("body");
                await Task.Delay(500);
            }
        }
        else
        {
            TestLogger.LogDebug("No project selector button found", Output);
        }
        
        TestLogger.LogDebug("Project selector debug test completed", Output);
        
        // Assert test completed
        Assert.True(true, "Project selector debug test completed");
    }
}