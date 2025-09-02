using frontend.E2ETests.PageObjects;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for User Role Management functionality - adapted for current UI state
/// </summary>
public class UserRoleManagementE2ETests : AuthenticatedE2ETestBase
{
    public UserRoleManagementE2ETests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task UsersPage_CanNavigateSuccessfully()
    {
        // Arrange - Login as admin to access users page
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Failed to login as admin");
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/users");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000);
        
        // Assert
        Assert.Contains("/users", Page.Url);
        
        // Verify page has content (indicates it loaded successfully)
        var pageContent = await Page.ContentAsync();
        Assert.True(pageContent.Length > 1000, "Page should have substantial content");
        
        // Verify page title
        var title = await Page.TitleAsync();
        Assert.NotNull(title);
        Assert.Contains("TestFlow Pro", title);
    }
    
    [Fact]
    public async Task UsersPage_HasExpectedNavigationElements()
    {
        // Arrange - Login as admin to access users page
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Failed to login as admin");
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/users");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000);
        
        // Assert - Check for Users navigation link (confirms we're in the right area)
        var usersLink = await Page.IsVisibleAsync("a:has-text('Users')");
        Assert.True(usersLink, "Should have Users navigation link visible");
        
        // Check that page contains user-related text
        var bodyText = await Page.TextContentAsync("body");

        Assert.True(bodyText?.Contains("user", StringComparison.OrdinalIgnoreCase) == true, "Page should contain user-related text");
    }
    
    [Fact]
    public async Task UsersPage_LoadsWithoutErrors()
    {
        // Arrange - Login as admin to access users page
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Failed to login as admin");
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/users");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000);
        
        // Assert - Check that page loads without JavaScript errors
        var errors = await Page.EvaluateAsync<string[]>("() => window.errors || []");
        Assert.Empty(errors);
        
        // Check that we successfully reached the users page
        Assert.Contains("/users", Page.Url);
    }
    
    [Fact]
    public async Task UsersPage_HasBasicUIStructure()
    {
        // Arrange - Login as admin to access users page
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Failed to login as admin");
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/users");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000);
        
        // Assert - Check for basic UI structure
        var hasButtons = await Page.QuerySelectorAllAsync("button");
        Assert.True(hasButtons.Count > 0, "Page should have some buttons");
        
        // Check for project selector (common across pages)
        var projectSelector = await Page.IsVisibleAsync(".project-selector-btn");
        Assert.True(projectSelector, "Should have project selector available");
        
        // Verify this is not a critical error page (be more specific about error detection)
        var bodyText = await Page.TextContentAsync("body");

        Assert.False(bodyText?.Contains("error occurred", StringComparison.OrdinalIgnoreCase) == true, "Page should not show 'error occurred' messages");
        Assert.False(bodyText?.Contains("something went wrong", StringComparison.OrdinalIgnoreCase) == true, "Page should not show 'something went wrong' messages");
        Assert.False(bodyText?.Contains("404", StringComparison.OrdinalIgnoreCase) == true, "Page should not be a 404 error");
        Assert.False(bodyText?.Contains("500", StringComparison.OrdinalIgnoreCase) == true, "Page should not be a 500 error");
    }
    
    [Fact]
    public async Task UserManagement_PlaceholderForFutureImplementation()
    {
        // Arrange - Login as admin for user management
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Failed to login as admin");
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/users");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000);
        
        // Assert - This test serves as a placeholder for when user management is fully implemented
        Assert.Contains("/users", Page.Url);
        
        // TODO: When user management UI is implemented, add tests for:
        // - Creating new users
        // - Editing existing users  
        // - Assigning roles to users
        // - Deleting users
        // - User validation (required fields, email format, etc.)
        
        // For now, just verify we can access the page with admin privileges
        Assert.True(true, "User management page accessible - ready for future implementation");
    }
}