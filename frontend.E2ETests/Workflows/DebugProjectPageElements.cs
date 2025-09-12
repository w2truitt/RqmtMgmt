using frontend.E2ETests.Fixtures;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using RqmtMgmtShared;
using Xunit;
using Xunit.Abstractions;

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
        
        Output.WriteLine("Starting project page elements analysis");
        
        // Navigate to projects page
        await Page.GotoAsync($"{BaseUrl}/projects");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        Output.WriteLine($"Navigated to projects page: {Page.Url}");
        
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
        
        Output.WriteLine($"Project page elements: {pageElements}");
        
        // Test navigation to a specific project if available
        var projectLinks = await Page.QuerySelectorAllAsync("a[href*='/projects/']");
        if (projectLinks.Count > 0)
        {
            Output.WriteLine($"Found {projectLinks.Count} project links");
            
            // Navigate to first project
            await projectLinks[0].ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            Output.WriteLine($"Navigated to specific project: {Page.Url}");
            
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
            Output.WriteLine("No project links found for detailed analysis");
        }
        
        Output.WriteLine("Project page elements analysis completed");
        
        // Assert test completed
        Assert.True(Page.Url.Contains("/projects"), "Should be on a projects-related page");
    }
}