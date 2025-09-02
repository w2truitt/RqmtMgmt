using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Debug test for exploring project page elements and structure
/// </summary>
public class DebugProjectPageElements : AuthenticatedE2ETestBase
{
    public DebugProjectPageElements(ITestOutputHelper output) : base(output)
    {
    }

    /// <summary>
    /// Debug test to explore and document project page elements
    /// </summary>
    [Fact]
    public async Task Debug_ProjectPageElements()
    {
        // Arrange - Login as developer for debugging access
        var loginSuccess = await LoginAsDeveloperAsync();
        Assert.True(loginSuccess, "Failed to login as developer");
        
        // Navigate to the homepage first
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000); // Allow time for components to initialize

        // Click the project selector button
        await Page.ClickAsync(".project-selector-container button");
        await Task.Delay(1000);

        // Force dropdown visibility
        await Page.EvaluateAsync(@"
            document.querySelectorAll('.dropdown-menu').forEach(menu => {
                menu.style.display = 'block';
                menu.classList.add('show');
            });
        ");

        // Get all project options
        var projectOptions = await Page.QuerySelectorAllAsync(".dropdown-item");
        Console.WriteLine($"Found {projectOptions.Count} project options");

        foreach (var option in projectOptions)
        {
            var text = await option.TextContentAsync();
            var href = await option.GetAttributeAsync("href");
            Console.WriteLine($"Project option: '{text}' -> {href}");
        }

        // Assert that we found some debug information
        Assert.True(true, "Debug test completed - check console output for project elements");
    }
}