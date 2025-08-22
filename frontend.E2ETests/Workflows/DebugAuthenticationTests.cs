using Microsoft.Playwright;
using Xunit;

namespace frontend.E2ETests.Workflows;

public class DebugAuthenticationTests : E2ETestBase
{
    [Fact]
    public async Task Debug_CheckWhatPageLoads()
    {
        // Listen for console errors
        var consoleMessages = new List<string>();
        Page.Console += (_, e) => {
            if (e.Type == "error") {
                consoleMessages.Add($"Console Error: {e.Text}");
            }
        };

        // Listen for page errors
        var pageErrors = new List<string>();
        Page.PageError += (_, e) => {
            pageErrors.Add($"Page Error: {e}");
        };

        // First navigate to home page
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Get home page info
        var homeUrl = Page.Url;
        var homeTitle = await Page.TitleAsync();
        Console.WriteLine($"Home URL: {homeUrl}");
        Console.WriteLine($"Home title: {homeTitle}");
        
        // Check if home page has login form
        var homeLoginElements = await Page.Locator("input[type='email'], input[type='password'], button:has-text('Login'), button:has-text('Sign in')").CountAsync();
        Console.WriteLine($"Home login elements: {homeLoginElements}");
        
        // Now navigate to projects page
        await Page.GotoAsync($"{BaseUrl}/projects");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait a bit for any async JavaScript to complete
        await Page.WaitForTimeoutAsync(3000);
        
        // Get current URL
        var currentUrl = Page.Url;
        Console.WriteLine($"Projects URL: {currentUrl}");
        
        // Get page title
        var title = await Page.TitleAsync();
        Console.WriteLine($"Projects page title: {title}");
        
        // Get page content preview
        var bodyText = await Page.Locator("body").TextContentAsync();
        var truncatedText = bodyText?.Substring(0, Math.Min(500, bodyText?.Length ?? 0));
        Console.WriteLine($"Projects page content preview: {truncatedText}");
        
        // Check if there's a login form
        var loginElements = await Page.Locator("input[type='email'], input[type='password'], button:has-text('Login'), button:has-text('Sign in')").CountAsync();
        Console.WriteLine($"Projects login elements found: {loginElements}");
        
        // Check for specific elements
        var projectsHeading = await Page.Locator("h3:has-text('Projects')").CountAsync();
        Console.WriteLine($"Projects heading found: {projectsHeading}");
        
        // Report any errors
        Console.WriteLine($"Console errors: {consoleMessages.Count}");
        foreach (var error in consoleMessages) {
            Console.WriteLine(error);
        }
        
        Console.WriteLine($"Page errors: {pageErrors.Count}");
        foreach (var error in pageErrors) {
            Console.WriteLine(error);
        }
        
        // Always pass this test - it's just for debugging
        Assert.True(true);
    }
}
