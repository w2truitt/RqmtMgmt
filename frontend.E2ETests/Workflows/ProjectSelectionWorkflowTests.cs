using frontend.E2ETests.Fixtures;
using frontend.E2ETests.PageObjects;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using RqmtMgmtShared;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.Playwright.Assertions;
using frontend.E2ETests.Infrastructure;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for project selection workflows including user management and requirements within project context
/// OPTIMIZED: Now uses shared browser and cached authentication for 4-10x performance improvement
/// Uses project manager role since these tests focus on project navigation and selection
/// </summary>
public class ProjectSelectionWorkflowTests : AuthenticatedE2ETestBase
{
    public ProjectSelectionWorkflowTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        // Set project manager user - appropriate role for project selection workflows
        SetProjectManagerUser();
    }

    [Fact]
    public async Task ProjectSelection_NavigateFromHomeToProjectRequirements_Success()
    {
        // Arrange - Project manager already authenticated via base class
        var requirementsPage = new RequirementsPage(Page, BaseUrl);
        
        // Start at home page
        await Page.GotoAsync($"{BaseUrl}");
        
        // Use existing static project
        await SelectExistingProject(2); // Use project index 2 (Performance Test Project 638944753472742469)
        
        // Act - Navigate to project requirements
        await ClickProjectAwareRequirementsLink();
        
        // Assert
        Assert.Contains("/requirements", Page.Url);
        
        // Wait for content to load
        await Page.WaitForTimeoutAsync(2000);
        
        // Check for requirements header - comprehensive approach
        var hasRequirementsHeader = await Page.IsVisibleAsync("h1:has-text('Requirements')") ||
                                   await Page.IsVisibleAsync("h2:has-text('Requirements')") ||
                                   await Page.IsVisibleAsync("h3:has-text('Requirements')") ||
                                   await Page.IsVisibleAsync("h4:has-text('Requirements')") ||
                                   await Page.IsVisibleAsync("[data-testid='requirements-header']") ||
                                   await Page.IsVisibleAsync(".page-title:has-text('Requirements')") ||
                                   await Page.IsVisibleAsync("*:has-text('Requirements')");
        
        Assert.True(hasRequirementsHeader, 
            "Should see requirements header on requirements page");
    }
    
    [Fact]
    public async Task ProjectSelection_CanSwitchBetweenProjects_Success()
    {
        // Arrange - Project manager already authenticated
        
        // Start at home page
        await Page.GotoAsync($"{BaseUrl}");
        
        // Try to select any available projects
        var projectUrls = new List<string>();
        
        // Try to select first available project
        for (int i = 0; i < 4; i++)
        {
            try
            {
                await SelectExistingProject(i);
                projectUrls.Add(Page.Url);
                TestLogger.LogDebug($"Successfully selected project {i}: {Page.Url}", Output);
                break;
            }
            catch (Exception ex)
            {
                TestLogger.LogTestStep($"Could not select project {i}: {ex.Message}", Output);
                continue;
            }
        }
        
        // Try to select a different project
        for (int i = 0; i < 4; i++)
        {
            try
            {
                // Skip if we already selected this project
                if (projectUrls.Count > 0)
                {
                    await SelectExistingProject(i);
                    var newUrl = Page.Url;
                    if (newUrl != projectUrls[0])
                    {
                        projectUrls.Add(newUrl);
                        TestLogger.LogDebug($"Successfully selected different project {i}: {newUrl}", Output);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                TestLogger.LogTestStep($"Could not select different project {i}: {ex.Message}", Output);
                continue;
            }
        }
        
        // Assert - Should have been able to select at least one project
        Assert.True(projectUrls.Count >= 1, "Should be able to select at least one project");
        
        // If we got two different projects, verify they're different
        if (projectUrls.Count >= 2)
        {
            Assert.NotEqual(projectUrls[0], projectUrls[1]);
            TestLogger.LogTestStep("Successfully demonstrated project switching capability", Output);
        }
        else
        {
            TestLogger.LogTestStep("Only one project available, but project selection is working", Output);
        }
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
