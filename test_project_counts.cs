using Microsoft.Playwright;
using Xunit;

namespace ProjectCountTest;

public class ProjectCountVerification
{
    [Fact]
    public async Task Project_Shows_Correct_Counts()
    {
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });
        var page = await browser.NewPageAsync();

        // Navigate to projects page
        await page.GotoAsync("http://localhost:8080/projects");
        
        // Wait for page to load
        await page.WaitForSelectorAsync("h3:has-text('Projects')");
        
        // Take a screenshot to see what we got
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = "projects_page.png" });
        
        // Check if we can find project counts (look for REQ, TS, TP indicators)
        var projectRows = await page.QuerySelectorAllAsync("[data-testid='project-row']");
        
        Console.WriteLine($"Found {projectRows.Count} project rows");
        
        if (projectRows.Count > 0)
        {
            // Check the first project's counts
            var firstRow = projectRows[0];
            var rowText = await firstRow.TextContentAsync();
            Console.WriteLine($"First project row text: {rowText}");
            
            // Look for REQ, TS, TP counts
            var hasReqCount = rowText?.Contains("REQ") == true;
            var hasTsCount = rowText?.Contains("TS") == true;
            var hasTpCount = rowText?.Contains("TP") == true;
            
            Console.WriteLine($"Has REQ count: {hasReqCount}");
            Console.WriteLine($"Has TS count: {hasTsCount}");
            Console.WriteLine($"Has TP count: {hasTpCount}");
        }
        
        await browser.CloseAsync();
    }
}
