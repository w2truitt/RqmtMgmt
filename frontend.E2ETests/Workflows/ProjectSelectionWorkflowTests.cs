using frontend.E2ETests.PageObjects;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using RqmtMgmtShared;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.Playwright.Assertions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for project selection workflows including user management and requirements within project context
/// </summary>
public class ProjectSelectionWorkflowTests : AuthenticatedE2ETestBase
{
    public ProjectSelectionWorkflowTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task ProjectSelection_NavigateFromHomeToProjectRequirements_Success()
    {
        // Arrange - Login as project manager to select and navigate projects
        var loginSuccess = await LoginAsProjectManagerAsync();
        Assert.True(loginSuccess, "Failed to login as project manager");
        
        // Arrange
        var requirementsPage = new RequirementsPage(Page, BaseUrl);
        
        // Start at home page
        await Page.GotoAsync($"{BaseUrl}");
        
        // Use existing static project
        await SelectExistingProject(1); // Use project index 1 (E2E Test Project 3625e50c)
        
        // Act - Navigate to project requirements
        await ClickProjectAwareRequirementsLink();
        
        // Assert
        Assert.Contains("/requirements", Page.Url);
        
        // Check for either global requirements (h1) or project-specific requirements (h2)
        var hasGlobalHeader = await Page.IsVisibleAsync("h1:has-text('Requirements')");
        var hasProjectHeader = await Page.IsVisibleAsync("h2:has-text('Requirements')");
        
        Assert.True(hasGlobalHeader || hasProjectHeader, 
            "Should have either global requirements header (h1) or project-specific requirements header (h2)");
    }
    
    [Fact]
    public async Task ProjectSelection_CanSwitchBetweenProjects_Success()
    {
        // Arrange - Login as project manager to switch between projects
        var loginSuccess = await LoginAsProjectManagerAsync();
        Assert.True(loginSuccess, "Failed to login as project manager");
        
        // Start at home page
        await Page.GotoAsync($"{BaseUrl}");
        
        // Select first project
        await SelectExistingProject(0);
        var firstProjectUrl = Page.Url;
        
        // Act - Switch to second project
        await SelectExistingProject(1);
        var secondProjectUrl = Page.Url;
        
        // Assert - URLs should be different (different project contexts)
        Assert.NotEqual(firstProjectUrl, secondProjectUrl);
    }

    /// <summary>
    /// Helper method to click the project-aware requirements link
    /// </summary>
    private async Task ClickProjectAwareRequirementsLink()
    {
        // Ensure navigation menu is visible
        await EnsureNavigationMenuVisible();
        
        // Try multiple strategies to find the requirements link
        var strategies = new[]
        {
            "nav a:has-text('Requirements')",
            ".nav-link:has-text('Requirements')",
            "a[href*='/requirements']",
            ".sidebar a:has-text('Requirements')"
        };

        foreach (var strategy in strategies)
        {
            try
            {
                var element = Page.Locator(strategy);
                if (await element.CountAsync() > 0)
                {
                    await element.First.ClickAsync();
                    await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    return;
                }
            }
            catch (Exception)
            {
                continue;
            }
        }
        
        throw new Exception("Could not find requirements navigation link");
    }
}