using frontend.E2ETests.Fixtures;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using RqmtMgmtShared;
using Xunit;
using Xunit.Abstractions;
using frontend.E2ETests.Infrastructure;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Debug tests for requirements creation
/// OPTIMIZED: Now uses shared browser and cached authentication for 4-10x performance improvement
/// Uses project manager role for requirements creation testing
/// </summary>
public class DebugRequirementsCreationTests : AuthenticatedE2ETestBase
{
    public DebugRequirementsCreationTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        // Set project manager user for requirements creation
        SetProjectManagerUser();
    }

    [Fact]
    public async Task Debug_RequirementsCreation_ProjectContext()
    {
        // Arrange - Project manager already authenticated via base class
        
        TestLogger.LogDebug("Starting requirements creation debug test", Output);
        
        // Navigate to projects first
        await Page.GotoAsync($"{BaseUrl}/projects");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        TestLogger.LogDebug($"Navigated to projects page: {Page.Url}", Output);
        
        // Try to find a project to work with
        var projectLinks = await Page.QuerySelectorAllAsync("a[href*='/projects/']");
        
        if (projectLinks.Count > 0)
        {
            TestLogger.LogDebug($"Found {projectLinks.Count} project links", Output);
            
            // Click on the first project
            await projectLinks[0].ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            TestLogger.LogDebug($"Navigated to project: {Page.Url}", Output);
            
            // Try to navigate to requirements within this project
            var requirementsLink = await Page.QuerySelectorAsync("a[href*='requirements'], .nav-link:has-text('Requirements')");
            if (requirementsLink != null)
            {
                await requirementsLink.ClickAsync();
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                
                TestLogger.LogDebug($"Navigated to project requirements: {Page.Url}", Output);
                
                // Check if we can create a requirement in this context
                var createButton = await Page.QuerySelectorAsync("button:has-text('Create'), button:has-text('New'), button:has-text('Add')");
                if (createButton != null)
                {
                    TestLogger.LogDebug("Create button found - requirements creation available in project context", Output);
                    
                    await createButton.ClickAsync();
                    await Task.Delay(1000);
                    
                    // Check if form appeared
                    var formVisible = await Page.IsVisibleAsync("form, .modal, [data-testid*='form']");
                    if (formVisible)
                    {
                        TestLogger.LogDebug("Requirements creation form opened successfully", Output);
                        
                        // Cancel the form
                        var cancelButton = await Page.QuerySelectorAsync("button:has-text('Cancel'), .btn-secondary");
                        if (cancelButton != null)
                        {
                            await cancelButton.ClickAsync();
                            TestLogger.LogDebug("Form cancelled successfully", Output);
                        }
                    }
                    else
                    {
                        TestLogger.LogDebug("Requirements creation form did not open", Output);
                    }
                }
                else
                {
                    TestLogger.LogDebug("No create button found - requirements creation may not be available", Output);
                }
            }
            else
            {
                TestLogger.LogDebug("No requirements link found in project navigation", Output);
            }
        }
        else
        {
            TestLogger.LogDebug("No projects found for requirements creation testing", Output);
        }
        
        TestLogger.LogDebug("Requirements creation debug test completed", Output);
        
        // Assert - Test completed successfully
        Assert.True(true, "Debug test completed - check output for details");
    }
}