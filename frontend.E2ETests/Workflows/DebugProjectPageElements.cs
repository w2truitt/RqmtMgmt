using Microsoft.Playwright;
using Xunit;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Debug test for exploring project page elements and structure
/// </summary>
public class DebugProjectPageElements : E2ETestBase
{
    /// <summary>
    /// Debug test to explore and document project page elements
    /// </summary>
    [Fact]
    public async Task Debug_ProjectPageElements()
    {
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

        // Select "Legacy Requirements" project
        await Page.ClickAsync(".dropdown-item:has-text('Legacy Requirements')");
        
        // Wait for navigation
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000);

        // Debug: Print current URL
        var currentUrl = Page.Url;
        Console.WriteLine($"Current URL: {currentUrl}");

        // Debug: Print page title
        var pageTitle = await Page.TitleAsync();
        Console.WriteLine($"Page title: {pageTitle}");

        // Debug: Print all elements with data-testid
        var testIdElements = await Page.EvaluateAsync<string[]>(@"
            Array.from(document.querySelectorAll('[data-testid]')).map(el => el.getAttribute('data-testid'))
        ");
        Console.WriteLine($"Elements with data-testid: {string.Join(", ", testIdElements)}");

        // Debug: Print main navigation elements
        var navElements = await Page.EvaluateAsync<string[]>(@"
            Array.from(document.querySelectorAll('nav a, .navbar a, .nav-link')).map(el => el.textContent?.trim() || el.getAttribute('href') || 'no text')
        ");
        Console.WriteLine($"Navigation elements: {string.Join(", ", navElements)}");

        // Debug: Check for breadcrumb-like elements
        var breadcrumbElements = await Page.EvaluateAsync<string[]>(@"
            Array.from(document.querySelectorAll('.breadcrumb, .breadcrumb-item, [class*=""breadcrumb""]')).map(el => el.textContent?.trim() || el.className)
        ");
        Console.WriteLine($"Breadcrumb elements: {string.Join(", ", breadcrumbElements)}");

        // Debug: Check for project-related elements
        var projectElements = await Page.EvaluateAsync<string[]>(@"
            Array.from(document.querySelectorAll('[class*=""project""], [data-testid*=""project""]')).map(el => el.className + ' | ' + (el.getAttribute('data-testid') || 'no testid'))
        ");
        Console.WriteLine($"Project elements: {string.Join(", ", projectElements)}");

        // Success if we made it this far
        Assert.True(true);
    }
}
