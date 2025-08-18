using Microsoft.Playwright;
using Xunit;

namespace frontend.E2ETests.Workflows;

public class SimpleProjectsPageTests : E2ETestBase
{
    [Fact]
    public async Task Projects_SimplePageLoadTest()
    {
        // Navigate to projects page
        await Page.GotoAsync($"{BaseUrl}/projects");
        
        // Wait for basic page structure
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        
        // Check if we're on the right URL
        Assert.Contains("/projects", Page.Url);
        
        // Try to find the h3 element with a shorter timeout
        var h3Element = Page.Locator("h3:has-text('Projects')");
        
        // Check if element exists at all
        var elementCount = await h3Element.CountAsync();
        Assert.True(elementCount > 0, $"Expected h3 'Projects' element to exist, but found {elementCount} elements");
        
        // Try to wait for it to be visible with shorter timeout
        await h3Element.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
    }
    
    [Fact]
    public async Task Projects_CheckPageContentDebug()
    {
        // Navigate to projects page
        await Page.GotoAsync($"{BaseUrl}/projects");
        
        // Wait a bit for page to load
        await Page.WaitForTimeoutAsync(3000);
        
        // Get page content
        var pageContent = await Page.ContentAsync();
        var bodyContent = await Page.Locator("body").InnerHTMLAsync();
        
        // Check what's actually on the page
        Assert.False(string.IsNullOrEmpty(pageContent), "Page content should not be empty");
        Assert.Contains("Projects", bodyContent, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public async Task Projects_CheckForErrors()
    {
        var consoleMessages = new List<string>();
        var errors = new List<string>();
        
        Page.Console += (_, e) => consoleMessages.Add($"{e.Type}: {e.Text}");
        Page.PageError += (_, e) => errors.Add(e.ToString());
        
        // Navigate to projects page
        await Page.GotoAsync($"{BaseUrl}/projects");
        
        // Wait for page to settle
        await Page.WaitForTimeoutAsync(5000);
        
        // Check for errors
        var errorMessages = errors.Where(e => !e.Contains("favicon")).ToList();
        var criticalConsoleMessages = consoleMessages.Where(m => 
            m.Contains("error", StringComparison.OrdinalIgnoreCase) && 
            !m.Contains("favicon", StringComparison.OrdinalIgnoreCase)).ToList();
        
        Assert.Empty(errorMessages);
        Assert.Empty(criticalConsoleMessages);
    }
}
