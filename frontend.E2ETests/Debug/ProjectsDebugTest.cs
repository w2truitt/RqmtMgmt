using Microsoft.Playwright;
using Xunit;

namespace frontend.E2ETests.Debug;

[Collection("E2E Tests")]
public class ProjectsDebugTest : E2ETestBase
{
    [Fact]
    public async Task DebugProjectsList()
    {
        // Navigate to the projects page
        await Page.GotoAsync($"{BaseUrl}/projects");
        
        // Wait for the page to load
        await Page.WaitForTimeoutAsync(3000);
        
        // Get the page title
        var title = await Page.TitleAsync();
        Console.WriteLine($"Projects page title: {title}");
        
        // Get the URL
        Console.WriteLine($"Current URL: {Page.Url}");
        
        // Look for project cards or items
        var projectCards = await Page.QuerySelectorAllAsync(".card, .project-card, .list-group-item, [data-testid*='project']");
        Console.WriteLine($"Found {projectCards.Count} project cards/items");
        
        // Get project names/text from each card
        for (int i = 0; i < projectCards.Count && i < 10; i++)
        {
            var text = await projectCards[i].TextContentAsync();
            Console.WriteLine($"  Project {i + 1}: {text?.Trim().Substring(0, Math.Min(100, text?.Trim().Length ?? 0))}");
        }
        
        // Look for any text that might indicate project IDs
        var bodyText = await Page.Locator("body").TextContentAsync();
        if (bodyText != null)
        {
            // Look for patterns that might be project IDs or links
            var lines = bodyText.Split('\n').Where(line => line.Contains("Project") || line.Contains("ID") || line.Contains("/projects/")).Take(10);
            Console.WriteLine("Lines containing project info:");
            foreach (var line in lines)
            {
                Console.WriteLine($"  {line.Trim()}");
            }
        }
        
        // Check for project links
        var projectLinks = await Page.QuerySelectorAllAsync("a[href*='/projects/']");
        Console.WriteLine($"Found {projectLinks.Count} project links:");
        for (int i = 0; i < projectLinks.Count && i < 10; i++)
        {
            var href = await projectLinks[i].GetAttributeAsync("href");
            var text = await projectLinks[i].TextContentAsync();
            Console.WriteLine($"  Link {i + 1}: {href} - {text?.Trim()}");
        }
        
        Assert.True(true);
    }
}
