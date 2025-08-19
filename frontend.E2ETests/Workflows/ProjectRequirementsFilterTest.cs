using Microsoft.Playwright;
using Xunit;

namespace frontend.E2ETests.Workflows;

public class ProjectRequirementsFilterTest : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });
        _page = await _browser.NewPageAsync();
        
        await _page.GotoAsync("http://localhost:8080");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task DisposeAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [Fact]
    public async Task ProjectRequirements_ShouldShowCorrectCount_WhenProjectSelected()
    {
        // Wait for application to load
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000);

        // Navigate to project 1 requirements (which has 13 requirements)
        await _page.GotoAsync("http://localhost:8080/projects/1/requirements");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(3000);

        // Check the page title contains "Requirements"
        var pageTitle = await _page.TitleAsync();
        Console.WriteLine($"Page title: {pageTitle}");
        Assert.Contains("Requirements", pageTitle);

        // Look for the requirements count in the UI
        var requirementsCountText = await _page.TextContentAsync(".project-context-header p");
        Console.WriteLine($"Requirements count text: {requirementsCountText}");
        
        // Should show "13 requirement" or "13 requirements"
        Assert.Contains("13 requirement", requirementsCountText ?? "");

        // Navigate to a project with no requirements (like project 3004)
        await _page.GotoAsync("http://localhost:8080/projects/3004/requirements");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(3000);

        // Check that this shows 0 requirements
        var emptyCountText = await _page.TextContentAsync(".project-context-header p");
        Console.WriteLine($"Empty project count text: {emptyCountText}");
        
        // Should show "0 requirement" or "0 requirements"
        Assert.Contains("0 requirement", emptyCountText ?? "");
    }
}
