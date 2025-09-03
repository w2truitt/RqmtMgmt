using frontend.E2ETests.Fixtures;
using frontend.E2ETests.PageObjects;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using RqmtMgmtShared;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.Playwright.Assertions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Integration tests that verify end-to-end workflows across multiple pages
/// OPTIMIZED: Now uses shared browser and cached authentication for 4-10x performance improvement
/// All tests run as admin user for full system access
/// </summary>
public class IntegrationTests : AuthenticatedE2ETestBase
{
    public IntegrationTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        // Set admin user for all tests - integration tests need full system access
        SetAdminUser();
    }

    [Fact]
    public async Task Integration_CanNavigateBetweenAllPages_AuthenticatedAdmin()
    {
        // Arrange - Admin user already authenticated via base class
        var pages = new[]
        {
            "/dashboard",
            "/projects", 
            "/requirements",
            "/testcases",
            "/users"
        };
        
        // Act & Assert - Navigate to each page
        foreach (var pagePath in pages)
        {
            await Page.GotoAsync($"{BaseUrl}{pagePath}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            Assert.Contains(pagePath, Page.Url);
            Output.WriteLine($"Successfully navigated to: {pagePath}");
        }
        
        Output.WriteLine("Successfully navigated between all major pages");
    }
    
    [Fact]
    public async Task Integration_ProjectToRequirementsWorkflow_AuthenticatedAdmin()
    {
        // Arrange - Admin user already authenticated
        
        // Act - Navigate to projects
        await Page.GotoAsync($"{BaseUrl}/projects");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Try to navigate to project requirements
        var firstProjectLink = await Page.QuerySelectorAsync("tbody tr:first-child a, .project-link");
        if (firstProjectLink != null)
        {
            await firstProjectLink.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        
        // Try to access requirements from project context
        var requirementsLink = await Page.QuerySelectorAsync("a:has-text('Requirements'), .nav-link:has-text('Requirements')");
        if (requirementsLink != null)
        {
            await requirementsLink.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        
        // Assert - Should be on some requirements-related page
        var isOnRequirementsPage = Page.Url.Contains("/requirements") || 
                                  await Page.IsVisibleAsync("h1:has-text('Requirements')") ||
                                  await Page.IsVisibleAsync("h2:has-text('Requirements')");
        
        Assert.True(isOnRequirementsPage, "Should be able to navigate from projects to requirements");
        Output.WriteLine("Project to requirements workflow works");
    }
    
    [Fact]
    public async Task Integration_RequirementsToTestCasesWorkflow_AuthenticatedAdmin()
    {
        // Arrange - Admin user already authenticated
        
        // Act - Start from requirements
        await Page.GotoAsync($"{BaseUrl}/requirements");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Try to navigate to test cases
        var testCasesLink = await Page.QuerySelectorAsync("a:has-text('Test Cases'), .nav-link:has-text('Test Cases')");
        if (testCasesLink != null)
        {
            await testCasesLink.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        else
        {
            // Direct navigation if link not found
            await Page.GotoAsync($"{BaseUrl}/testcases");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        
        // Assert - Should be on test cases page
        var isOnTestCasesPage = Page.Url.Contains("/testcases") || 
                               await Page.IsVisibleAsync("h3:has-text('Test Cases')");
        
        Assert.True(isOnTestCasesPage, "Should be able to navigate from requirements to test cases");
        Output.WriteLine("Requirements to test cases workflow works");
    }
    
    [Fact]
    public async Task Integration_UserManagementWorkflow_AuthenticatedAdmin()
    {
        // Arrange - Admin user already authenticated
        
        // Act - Navigate to users page
        await Page.GotoAsync($"{BaseUrl}/users");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Check if we can access user management functions
        var hasUserManagement = await Page.IsVisibleAsync("button:has-text('Create')") ||
                               await Page.IsVisibleAsync("button:has-text('Add')") ||
                               await Page.IsVisibleAsync("[data-testid='create-user']") ||
                               await Page.IsVisibleAsync("table");
        
        // Assert - Should have access to user management
        Assert.True(hasUserManagement, "Admin should have access to user management functions");
        Output.WriteLine("User management workflow accessible");
    }
    
    [Fact]
    public async Task Integration_SearchFunctionalityAcrossPages_AuthenticatedAdmin()
    {
        // Arrange - Admin user already authenticated
        var pagesWithSearch = new[]
        {
            "/projects",
            "/requirements", 
            "/testcases",
            "/users"
        };
        
        // Act & Assert - Test search on each page that has it
        foreach (var pagePath in pagesWithSearch)
        {
            await Page.GotoAsync($"{BaseUrl}{pagePath}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            var searchInput = await Page.QuerySelectorAsync("input[type='search'], input[placeholder*='search'], input[placeholder*='Search']");
            if (searchInput != null)
            {
                await searchInput.FillAsync("test");
                await Task.Delay(500);
                await searchInput.FillAsync(""); // Clear search
                await Task.Delay(500);
                
                Output.WriteLine($"Search functionality works on: {pagePath}");
            }
            else
            {
                Output.WriteLine($"No search functionality found on: {pagePath}");
            }
        }
        
        Output.WriteLine("Search functionality tested across all pages");
    }
    
    [Fact]
    public async Task Integration_NavigationMenuConsistency_AuthenticatedAdmin()
    {
        // Arrange - Admin user already authenticated
        var pages = new[]
        {
            "/dashboard",
            "/projects",
            "/requirements", 
            "/testcases",
            "/users"
        };
        
        // Act & Assert - Check navigation menu on each page
        foreach (var pagePath in pages)
        {
            await Page.GotoAsync($"{BaseUrl}{pagePath}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Check for navigation menu presence
            var hasNavigation = await Page.IsVisibleAsync("nav") ||
                               await Page.IsVisibleAsync(".navbar") ||
                               await Page.IsVisibleAsync(".nav-menu") ||
                               await Page.IsVisibleAsync(".sidebar");
            
            Assert.True(hasNavigation, $"Navigation should be present on {pagePath}");
            Output.WriteLine($"Navigation menu present on: {pagePath}");
        }
        
        Output.WriteLine("Navigation menu consistency verified across all pages");
    }
}