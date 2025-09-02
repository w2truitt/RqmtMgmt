using frontend.E2ETests.PageObjects;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Basic smoke tests to verify the application is working with authentication
/// </summary>
public class SmokeTests : AuthenticatedE2ETestBase
{
    public SmokeTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task Homepage_LoadsSuccessfullyForAuthenticatedUser()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");

        // Act - Navigate to homepage
        await Page.GotoAsync(BaseUrl);
        
        // Wait for the application to load
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitForBlazorAppAsync();
        
        // Assert that we can see basic UI elements and are authenticated
        var isVisible = await Page.IsVisibleAsync("body") && 
                       await Page.IsVisibleAsync("#app");
        
        var currentUrl = Page.Url;
        var isNotLoginPage = !currentUrl.Contains("/Account/Login");
        
        Assert.True(isVisible, "Homepage should load successfully");
        Assert.True(isNotLoginPage, "Should not be redirected to login page when authenticated");
        
        _output.WriteLine($"Homepage loaded successfully at: {currentUrl}");
    }
    
    [Fact]
    public async Task ProjectsPage_LoadsSuccessfullyForAuthenticatedUser()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
        // Act - Navigate to projects page
        var canAccessProjects = await NavigateToProtectedPageAsync("/projects");
        Assert.True(canAccessProjects, "Should be able to access projects page when authenticated");
        
        await WaitForBlazorAppAsync();
        
        // Assert basic page elements are present
        var isLoaded = await Page.IsVisibleAsync("body");
        var currentUrl = Page.Url;
        var hasProjectsUrl = currentUrl.Contains("/projects");
        
        Assert.True(isLoaded, "Projects page should load successfully");
        Assert.True(hasProjectsUrl, "Should be on projects page");
        
        _output.WriteLine($"Projects page loaded successfully at: {currentUrl}");
    }
    
    [Fact]
    public async Task UsersPage_LoadsSuccessfullyForAuthenticatedAdmin()
    {
        // Arrange - Login as admin (who should have access to users page)
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
        // Act - Navigate to users page
        var canAccessUsers = await NavigateToProtectedPageAsync("/users");
        Assert.True(canAccessUsers, "Admin should be able to access users page");
        
        await WaitForBlazorAppAsync();
        
        // Assert basic page elements are present
        var isLoaded = await Page.IsVisibleAsync("body");
        var currentUrl = Page.Url;
        var hasUsersUrl = currentUrl.Contains("/users");
        
        Assert.True(isLoaded, "Users page should load successfully");
        Assert.True(hasUsersUrl, "Should be on users page");
        
        _output.WriteLine($"Users page loaded successfully at: {currentUrl}");
    }

    [Fact]
    public async Task UnauthenticatedUser_RedirectsToLogin()
    {
        // Arrange - Ensure no authentication
        await LogoutAsync();
        
        // Act - Try to access protected page
        await Page.GotoAsync($"{BaseUrl}/projects");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 10000 });
        await Task.Delay(3000);
        
        // Assert - Should be redirected to login
        var currentUrl = Page.Url;
        var isOnLoginPage = currentUrl.Contains("/Account/Login");
        
        Assert.True(isOnLoginPage, "Unauthenticated user should be redirected to login page");
        
        _output.WriteLine($"Unauthenticated user correctly redirected to: {currentUrl}");
    }
}
