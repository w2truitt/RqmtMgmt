using frontend.E2ETests.Fixtures;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using RqmtMgmtShared;
using Xunit;
using Xunit.Abstractions;
using frontend.E2ETests.Infrastructure;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Debug Project Page Elements Tests
/// OPTIMIZED: Now uses shared browser and cached authentication for 4-10x performance improvement
/// Uses developer role for project page analysis
/// </summary>
public class DebugProjectPageElements : AuthenticatedE2ETestBase
{
    public DebugProjectPageElements(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        // Set developer user for project page analysis
        SetDeveloperUser();
    }

    [Fact]
    public async Task Debug_ProjectPageElements_Analysis()
    {
        // Arrange - Developer user already authenticated via base class
        
        TestLogger.LogDebug("Starting project page elements analysis", Output);
        
        // Navigate to projects page
        await Page.GotoAsync($"{BaseUrl}/projects");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        TestLogger.LogDebug($"Navigated to projects page: {Page.Url}", Output);
        
        // Analyze page elements
        var pageElements = await Page.EvaluateAsync<object>(@"
            () => {
                const elements = {
                    title: document.title,
                    headers: [],
                    buttons: [],
                    links: [],
                    tables: document.querySelectorAll('table').length,
                    cards: document.querySelectorAll('.card').length,
                    projectElements: []
                };
                
                // Get headers
                document.querySelectorAll('h1, h2, h3, h4, h5, h6').forEach(h => {
                    elements.headers.push(`${h.tagName}: ${h.textContent.trim()}`);
                });
                
                // Get buttons
                document.querySelectorAll('button').forEach(btn => {
                    elements.buttons.push(btn.textContent.trim());
                });
                
                // Get project-related links
                document.querySelectorAll('a[href*=""project""]').forEach(link => {
                    elements.links.push({
                        text: link.textContent.trim(),
                        href: link.getAttribute('href')
                    });
                });
                
                return elements;
            }
        ");
        
        TestLogger.LogDebug($"Project page elements: {pageElements}", Output);
        
        // Test navigation to a specific project if available
        var projectLinks = await Page.QuerySelectorAllAsync("a[href*='/projects/']");
        if (projectLinks.Count > 0)
        {
            TestLogger.LogDebug($"Found {projectLinks.Count} project links", Output);
            
            // Navigate to first project
            await projectLinks[0].ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            TestLogger.LogDebug($"Navigated to specific project: {Page.Url}", Output);
            
            // Analyze project detail page
            var projectDetailElements = await Page.EvaluateAsync<string[]>(@"
                () => {
                    const elements = [];
                    document.querySelectorAll('nav a, .nav-link, .sidebar a').forEach(link => {
                        elements.push(link.textContent.trim());
                    });
                    return elements;
                }
            ");
            
            Output.WriteLine($"Project navigation elements: {string.Join(", ", projectDetailElements)}");
        }
        else
        {
            TestLogger.LogDebug("No project links found for detailed analysis", Output);
        }
        
        TestLogger.LogDebug("Project page elements analysis completed", Output);
        
        // Assert test completed
        Assert.True(Page.Url.Contains("/projects"), "Should be on a projects-related page");
    }
}