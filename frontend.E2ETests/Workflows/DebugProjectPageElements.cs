using Microsoft.Playwright;
using Xunit;

namespace frontend.E2ETests.Workflows;

public class DebugProjectPageElements : IAsyncLifetime
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
    public async Task Debug_ProjectPageElements()
    {
        // Wait for application to load
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000); // Allow time for components to initialize

        // Click the project selector button
        await _page.ClickAsync(".project-selector-container button");
        await Task.Delay(1000);

        // Force dropdown visibility
        await _page.EvaluateAsync(@"
            document.querySelectorAll('.dropdown-menu').forEach(menu => {
                menu.style.display = 'block';
                menu.classList.add('show');
            });
        ");

        // Select "Legacy Requirements" project
        await _page.ClickAsync(".dropdown-item:has-text('Legacy Requirements')");
        
        // Wait for navigation
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000);

        // Debug: Print current URL
        var currentUrl = _page.Url;
        Console.WriteLine($"Current URL: {currentUrl}");

        // Debug: Print page title
        var pageTitle = await _page.TitleAsync();
        Console.WriteLine($"Page title: {pageTitle}");

        // Debug: Print all elements with data-testid
        var testIdElements = await _page.EvaluateAsync<string[]>(@"
            Array.from(document.querySelectorAll('[data-testid]')).map(el => el.getAttribute('data-testid'))
        ");
        Console.WriteLine($"Elements with data-testid: {string.Join(", ", testIdElements)}");

        // Debug: Print main navigation elements
        var navElements = await _page.EvaluateAsync<string[]>(@"
            Array.from(document.querySelectorAll('nav a, .navbar a, .nav-link')).map(el => el.textContent?.trim() || el.getAttribute('href') || 'no text')
        ");
        Console.WriteLine($"Navigation elements: {string.Join(", ", navElements)}");

        // Debug: Check for breadcrumb-like elements
        var breadcrumbElements = await _page.EvaluateAsync<string[]>(@"
            Array.from(document.querySelectorAll('.breadcrumb, .breadcrumb-item, [class*=""breadcrumb""]')).map(el => el.textContent?.trim() || el.className)
        ");
        Console.WriteLine($"Breadcrumb elements: {string.Join(", ", breadcrumbElements)}");

        // Debug: Check for project-related elements
        var projectElements = await _page.EvaluateAsync<string[]>(@"
            Array.from(document.querySelectorAll('[class*=""project""], [data-testid*=""project""]')).map(el => el.className + ' | ' + (el.getAttribute('data-testid') || 'no testid'))
        ");
        Console.WriteLine($"Project elements: {string.Join(", ", projectElements)}");

        // Success if we made it this far
        Assert.True(true);
    }
}
