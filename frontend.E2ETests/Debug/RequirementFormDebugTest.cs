using Microsoft.Playwright;
using Xunit;

namespace frontend.E2ETests.Debug;

[Collection("E2E Tests")]
public class RequirementFormDebugTest : E2ETestBase
{
    [Fact]
    public async Task DebugRequirementFormPage()
    {
        // Navigate to the requirement form
        await Page.GotoAsync($"{BaseUrl}/projects/1/requirements/new");
        
        // Wait a bit for any loading to happen
        await Page.WaitForTimeoutAsync(5000);
        
        // Take a screenshot to see what's on the page
        await Page.ScreenshotAsync(new() { Path = "/tmp/requirement-form-debug.png", FullPage = true });
        
        // Get the page title
        var title = await Page.TitleAsync();
        Console.WriteLine($"Page title: {title}");
        
        // Get the URL
        Console.WriteLine($"Current URL: {Page.Url}");
        
        // Check if the test ID element exists
        var testIdElement = await Page.QuerySelectorAsync("[data-testid='requirement-form-page']");
        Console.WriteLine($"Test ID element exists: {testIdElement != null}");
        
        // Check for any error messages
        var errorElements = await Page.QuerySelectorAllAsync(".alert-danger");
        if (errorElements.Any())
        {
            Console.WriteLine($"Found {errorElements.Count} error message(s):");
            foreach (var errorElement in errorElements)
            {
                var text = await errorElement.TextContentAsync();
                Console.WriteLine($"  Error: {text}");
            }
        }
        
        // Check for loading spinner
        var loadingSpinner = await Page.QuerySelectorAsync(".spinner-border");
        Console.WriteLine($"Loading spinner exists: {loadingSpinner != null}");
        
        // Check if the h2 heading exists
        var h2Element = await Page.QuerySelectorAsync("h2:has-text('New Requirement')");
        Console.WriteLine($"H2 'New Requirement' exists: {h2Element != null}");
        
        // Check if the title input field exists
        var titleInput = await Page.QuerySelectorAsync("input[placeholder='Enter requirement title...']");
        Console.WriteLine($"Title input field exists: {titleInput != null}");
        
        // Get all text content to see what's on the page
        var bodyText = await Page.Locator("body").TextContentAsync();
        Console.WriteLine($"Page body text: {bodyText?.Substring(0, Math.Min(500, bodyText?.Length ?? 0))}...");
        
        // Check if we're on a 404 page
        var notFoundText = await Page.QuerySelectorAsync("h1:has-text('404'), h2:has-text('Not Found'), div:has-text('Page not found')");
        Console.WriteLine($"404/Not Found element exists: {notFoundText != null}");
        
        // Check what page we actually landed on
        var pageHeadings = await Page.QuerySelectorAllAsync("h1, h2, h3");
        Console.WriteLine($"Found {pageHeadings.Count} headings:");
        foreach (var heading in pageHeadings)
        {
            var headingText = await heading.TextContentAsync();
            Console.WriteLine($"  Heading: {headingText}");
        }
        
        // Check for any console errors
        var consoleMessages = new List<string>();
        Page.Console += (_, msg) => {
            if (msg.Type == "error")
            {
                consoleMessages.Add($"Console error: {msg.Text}");
            }
        };
        
        // Wait a bit more to capture any console errors
        await Page.WaitForTimeoutAsync(2000);
        
        foreach (var message in consoleMessages)
        {
            Console.WriteLine(message);
        }
        
        // The test always passes - we're just debugging
        Assert.True(true);
    }
}
