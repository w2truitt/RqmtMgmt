using frontend.E2ETests.Fixtures;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using RqmtMgmtShared;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Debug Users Page Structure Tests
/// OPTIMIZED: Now uses shared browser and cached authentication for 4-10x performance improvement
/// Uses admin user for comprehensive users page analysis
/// </summary>
public class DebugUsersPageStructure : AuthenticatedE2ETestBase
{
    public DebugUsersPageStructure(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        // Set admin user for comprehensive users page analysis
        SetAdminUser();
    }

    [Fact]
    public async Task Debug_UsersPageStructure_Analysis()
    {
        // Arrange - Admin user already authenticated via base class
        
        Output.WriteLine("Starting users page structure analysis");
        
        // Navigate to users page
        await Page.GotoAsync($"{BaseUrl}/users");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        Output.WriteLine($"Navigated to users page: {Page.Url}");
        
        // Analyze page structure
        var pageInfo = await Page.EvaluateAsync<object>(@"
            () => {
                const info = {
                    title: document.title,
                    url: window.location.href,
                    headers: [],
                    buttons: [],
                    tables: document.querySelectorAll('table').length,
                    forms: document.querySelectorAll('form').length,
                    modals: document.querySelectorAll('.modal').length
                };
                
                // Get headers
                document.querySelectorAll('h1, h2, h3, h4, h5, h6').forEach(h => {
                    info.headers.push(`${h.tagName}: ${h.textContent.trim()}`);
                });
                
                // Get buttons
                document.querySelectorAll('button').forEach(btn => {
                    info.buttons.push(btn.textContent.trim());
                });
                
                return info;
            }
        ");
        
        Output.WriteLine($"Page analysis results: {pageInfo}");
        
        // Check for specific user management elements
        var hasUserTable = await Page.IsVisibleAsync("table");
        var hasCreateButton = await Page.IsVisibleAsync("button:has-text('Create'), button:has-text('Add')");
        var hasSearchInput = await Page.IsVisibleAsync("input[type='search'], input[placeholder*='search']");
        
        Output.WriteLine($"User management elements:");
        Output.WriteLine($"- User table: {hasUserTable}");
        Output.WriteLine($"- Create button: {hasCreateButton}");
        Output.WriteLine($"- Search input: {hasSearchInput}");
        
        // Test basic interactions
        if (hasCreateButton)
        {
            Output.WriteLine("Testing create button interaction");
            var createButton = await Page.QuerySelectorAsync("button:has-text('Create'), button:has-text('Add')");
            if (createButton != null)
            {
                await createButton.ClickAsync();
                await Task.Delay(1000);
                
                var modalAppeared = await Page.IsVisibleAsync(".modal, form, [data-testid*='form']");
                Output.WriteLine($"Modal/form appeared after create click: {modalAppeared}");
                
                if (modalAppeared)
                {
                    // Close the modal
                    var closeButton = await Page.QuerySelectorAsync("button:has-text('Cancel'), button:has-text('Close'), .btn-close");
                    if (closeButton != null)
                    {
                        await closeButton.ClickAsync();
                        Output.WriteLine("Modal closed successfully");
                    }
                }
            }
        }
        
        Output.WriteLine("Users page structure analysis completed");
        
        // Assert test completed
        Assert.Contains("/users", Page.Url);
    }
}