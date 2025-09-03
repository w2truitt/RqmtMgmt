using frontend.E2ETests.Fixtures;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using RqmtMgmtShared;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Debug Test Plans Page Tests
/// OPTIMIZED: Now uses shared browser and cached authentication for 4-10x performance improvement
/// Uses tester role for test plans debugging
/// </summary>
public class DebugTestPlansPageTests : AuthenticatedE2ETestBase
{
    public DebugTestPlansPageTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        // Set tester user for test plans debugging
        SetTesterUser();
    }

    [Fact]
    public async Task Debug_TestPlansPage_Structure()
    {
        // Arrange - Tester user already authenticated via base class
        
        // Navigate to test plans page
        await Page.GotoAsync($"{BaseUrl}/testplans");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Debug the page structure
        var pageTitle = await Page.TitleAsync();
        var url = Page.Url;
        
        Output.WriteLine($"Test Plans Page Debug:");
        Output.WriteLine($"Title: {pageTitle}");
        Output.WriteLine($"URL: {url}");
        
        // Check for common page elements
        var hasTable = await Page.IsVisibleAsync("table");
        var hasCreateButton = await Page.IsVisibleAsync("button:has-text('Create'), button:has-text('Add'), button:has-text('New')");
        var hasHeader = await Page.IsVisibleAsync("h1, h2, h3");
        
        Output.WriteLine($"Has table: {hasTable}");
        Output.WriteLine($"Has create button: {hasCreateButton}");
        Output.WriteLine($"Has header: {hasHeader}");
        
        // Get page content for debugging
        var headers = await Page.EvaluateAsync<string[]>(@"
            () => {
                const headers = [];
                document.querySelectorAll('h1, h2, h3, h4, h5, h6').forEach(h => {
                    headers.push(`${h.tagName}: ${h.textContent.trim()}`);
                });
                return headers;
            }
        ");
        
        Output.WriteLine($"Headers found: {string.Join(", ", headers)}");
        
        // Assert test completed
        Assert.Contains("/testplans", Page.Url);
        Output.WriteLine("Test Plans page debug completed");
    }
}