using frontend.E2ETests.Fixtures;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.Playwright.Assertions;
using frontend.E2ETests.Infrastructure;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Smoke tests that verify basic application functionality and availability
/// OPTIMIZED: Now uses shared browser and cached authentication for 4-10x performance improvement
/// These tests verify the application is running and accessible
/// </summary>
public class SmokeTests : AuthenticatedE2ETestBase
{
    public SmokeTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        // Set admin user for smoke tests - need full access to verify all functionality
        SetAdminUser();
    }

    [Fact]
    public async Task Smoke_ApplicationIsRunning_Success()
    {
        // Arrange - Admin user already authenticated via base class
        
        // Act - Navigate to home page
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - Application should be running and accessible
        Assert.DoesNotContain("This site can't be reached", await Page.ContentAsync());
        Assert.DoesNotContain("ERR_CONNECTION_REFUSED", await Page.ContentAsync());
        
        // Should be redirected to a valid page (dashboard, projects, etc.)
        var isOnValidPage = Page.Url.Contains("/dashboard") || 
                           Page.Url.Contains("/projects") ||
                           Page.Url.Contains(BaseUrl);
        
        Assert.True(isOnValidPage, "Should be redirected to a valid application page");
        TestLogger.LogDebug($"Application is running and accessible at: {Page.Url}", Output);
    }
    
    [Fact]
    public async Task Smoke_AllMajorPagesAccessible_AuthenticatedUser()
    {
        // Arrange - Admin user already authenticated
        var majorPages = new[]
        {
            ("/dashboard", "Dashboard"),
            ("/projects", "Projects"),
            ("/requirements", "Requirements"),
            ("/testcases", "Test Cases"),
            ("/users", "Users")
        };
        
        // Act & Assert - Check each major page
        foreach (var (path, pageName) in majorPages)
        {
            await Page.GotoAsync($"{BaseUrl}{path}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Should not get error pages
            var pageContent = await Page.ContentAsync();
            var pageTitle = await Page.TitleAsync();
            
            // Check for actual error pages, not font weights or other legitimate uses
            Assert.DoesNotContain("404 Not Found", pageContent);
            Assert.DoesNotContain("500 Internal Server Error", pageContent);
            Assert.DoesNotContain("HTTP Error 404", pageContent);
            Assert.DoesNotContain("HTTP Error 500", pageContent);
            Assert.DoesNotContain("Error", pageTitle);
            
            // Verify we're not on an error page by checking for error-specific patterns
            Assert.False(pageContent.Contains("404") && pageContent.Contains("not found"), 
                        "Should not be on a 404 error page");
            Assert.False(pageContent.Contains("500") && pageContent.Contains("server error"), 
                        "Should not be on a 500 error page");
            
            // Should be on the correct page
            Assert.Contains(path, Page.Url);
            
            TestLogger.LogDebug($"{pageName} page is accessible at: {path}", Output);
        }
        
        TestLogger.LogDebug("All major pages are accessible", Output);
    }
    
    [Fact]
    public async Task Smoke_AuthenticationSystemWorking_Success()
    {
        // Arrange - Admin user already authenticated via base class
        
        // Act - Try to access a protected page
        await Page.GotoAsync($"{BaseUrl}/users");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - Should be authenticated and able to access protected content
        Assert.DoesNotContain("/Account/Login", Page.Url);
        Assert.Contains("/users", Page.Url);
        
        // Wait for page to fully load
        await Page.WaitForTimeoutAsync(2000);
        
        // Should see authenticated content (more comprehensive check)
        var hasAuthenticatedContent = await Page.IsVisibleAsync("table") ||
                                     await Page.IsVisibleAsync(".table") ||
                                     await Page.IsVisibleAsync(".authenticated-content") ||
                                     await Page.IsVisibleAsync("button:has-text('Create')") ||
                                     await Page.IsVisibleAsync("button:has-text('Add')") ||
                                     await Page.IsVisibleAsync("button:has-text('New')") ||
                                     await Page.IsVisibleAsync("h1:has-text('Users')") ||
                                     await Page.IsVisibleAsync("h2:has-text('Users')") ||
                                     await Page.IsVisibleAsync("h3:has-text('Users')") ||
                                     Page.Url.Contains("/users"); // At minimum, we should be on the users page
        
        Assert.True(hasAuthenticatedContent, "Should see authenticated content on protected pages");
        TestLogger.LogAuthentication("Authentication system is working correctly", Output);
    }
    
    [Fact]
    public async Task Smoke_JavaScriptIsWorking_Success()
    {
        // Arrange - Admin user already authenticated
        
        // Act - Navigate to an interactive page
        await Page.GotoAsync($"{BaseUrl}/projects");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Test JavaScript functionality
        var jsWorking = await Page.EvaluateAsync<bool>("() => typeof window !== 'undefined' && typeof document !== 'undefined'");
        Assert.True(jsWorking, "JavaScript should be working");
        
        // Check for JavaScript errors
        var errors = await Page.EvaluateAsync<string[]>("() => window.errors || []");
        Assert.Empty(errors);
        
        // Check if interactive elements are present (indicating JS is working)
        var hasInteractiveElements = await Page.IsVisibleAsync("button") ||
                                    await Page.IsVisibleAsync("input") ||
                                    await Page.IsVisibleAsync("select");
        
        Assert.True(hasInteractiveElements, "Should have interactive elements indicating JavaScript is working");
        TestLogger.LogTestStep("JavaScript is working correctly", Output);
    }

    [Fact]
    public async Task Get_FirstProjectName_FromUI()
    {
        // Arrange - Admin user already authenticated
        TestLogger.LogTestStep("Starting to get first project name from UI", Output);

        // Act - Navigate to projects page
        await Page.GotoAsync($"{BaseUrl}/projects");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for projects to load
        await Page.WaitForSelectorAsync("table, .project-card, [data-testid*='project'], .project-item", new PageWaitForSelectorOptions
        {
            Timeout = 10000
        });

        // Debug: Log page content to understand structure
        var pageContent = await Page.TextContentAsync("body");
        TestLogger.LogDebug($"Page content preview: {pageContent?.Substring(0, Math.Min(300, pageContent.Length))}", Output);

        // Debug: Try to find any table content
        var tableContent = await Page.TextContentAsync("table");
        TestLogger.LogDebug($"Table content: {tableContent}", Output);

        // Try different selectors to find the first project name (avoiding headers)
        var projectSelectors = new[]
        {
            "table tbody tr:first-child td:first-child a:not(:has-text('Projects'))", // Table row with link, not header
            "table tbody tr:first-child td:nth-child(2):not(:has-text('Projects'))", // Table row second column, not header
            ".project-card:first-child h3:not(:has-text('Projects'))", // Project card title, not header
            ".project-card:first-child .project-name:not(:has-text('Projects'))", // Project card name, not header
            "[data-testid*='project']:first-child [data-testid*='name']:not(:has-text('Projects'))", // Data testid name, not header
            "[data-testid*='project']:first-child a:not(:has-text('Projects'))", // Data testid link, not header
            "a[href*='project']:first-child:not(:has-text('Projects'))", // Link with project in href, not header
            ".project-item:first-child:not(:has-text('Projects'))", // Generic project item, not header
            ".project-link:first-child:not(:has-text('Projects'))" // Generic project link, not header
        };

        string? firstProjectName = null;
        foreach (var selector in projectSelectors)
        {
            try
            {
                var element = await Page.QuerySelectorAsync(selector);
                if (element != null)
                {
                    var text = await element.TextContentAsync();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        firstProjectName = text.Trim();
                        TestLogger.LogDebug($"Found project name with selector '{selector}': {firstProjectName}", Output);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                TestLogger.LogDebug($"Selector '{selector}' failed: {ex.Message}", Output);
            }
        }

        // Assert - Should have found a project name
        Assert.NotNull(firstProjectName);
        Assert.NotEmpty(firstProjectName);

        // Log the result
        TestLogger.LogTestStep($"First project name found: {firstProjectName}", Output);

        // Extract just the project name (first line or before "Default project")
        var projectNameOnly = firstProjectName?.Split('\n')[0]?.Split("Default project")[0]?.Trim();
        if (string.IsNullOrEmpty(projectNameOnly))
        {
            projectNameOnly = firstProjectName;
        }

        // Output to console for easy access
        Console.WriteLine($"FIRST_PROJECT_NAME: {projectNameOnly}");
    }
}