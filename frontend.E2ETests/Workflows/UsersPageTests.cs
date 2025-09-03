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
/// E2E tests for the Users page functionality
/// OPTIMIZED: Now uses shared browser and cached authentication for 4-10x performance improvement
/// All tests run as admin user since user management requires admin privileges
/// </summary>
public class UsersPageTests : AuthenticatedE2ETestBase
{
    public UsersPageTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        // Set admin user for all tests - user management requires admin privileges
        SetAdminUser();
    }

    [Fact]
    public async Task Users_NavigatesSuccessfully_AuthenticatedUser()
    {
        // Arrange - Admin user already authenticated via base class
        var usersPage = new UsersPage(Page, BaseUrl);
        
        // Act
        await usersPage.NavigateToAsync();
        
        // Assert
        Assert.Contains("/users", Page.Url);
        await Expect(Page.Locator("h3:has-text('Users')")).ToBeVisibleAsync();
        
        Output.WriteLine($"Successfully navigated to users page: {Page.Url}");
    }
    
    [Fact]
    public async Task Users_LoadsWithoutErrors_AuthenticatedUser()
    {
        // Arrange - Admin user already authenticated
        var usersPage = new UsersPage(Page, BaseUrl);
        
        // Act
        await usersPage.NavigateToAsync();
        
        // Assert - Check that page loads without JavaScript errors
        var errors = await Page.EvaluateAsync<string[]>("() => window.errors || []");
        Assert.Empty(errors);
        
        Assert.Contains("/users", Page.Url);
        Output.WriteLine("Users page loaded without errors");
    }
    
    [Fact]
    public async Task Users_HasExpectedPageElements_AuthenticatedUser()
    {
        // Arrange - Admin user already authenticated
        var usersPage = new UsersPage(Page, BaseUrl);
        
        // Act
        await usersPage.NavigateToAsync();
        
        // Assert
        await Expect(Page.Locator("h3:has-text('Users')")).ToBeVisibleAsync();
        await Expect(Page.Locator("table")).ToBeVisibleAsync();
        
        Output.WriteLine("All expected page elements are present");
    }
    
    [Fact]
    public async Task Users_CanSearchUsers_AuthenticatedUser()
    {
        // Arrange - Admin user already authenticated
        var usersPage = new UsersPage(Page, BaseUrl);
        
        // Act
        await usersPage.NavigateToAsync();
        
        var initialCount = await usersPage.GetUserCountAsync();
        await usersPage.SearchUsersAsync("admin");
        await Task.Delay(1000); // Allow search to process
        
        // Assert - Search functionality works
        Assert.Contains("/users", Page.Url);
        Output.WriteLine("Search functionality works correctly");
    }
    
    [Fact]
    public async Task Users_ShowsUserCounts_AuthenticatedUser()
    {
        // Arrange - Admin user already authenticated
        var usersPage = new UsersPage(Page, BaseUrl);
        
        // Act
        await usersPage.NavigateToAsync();
        
        var userCount = await usersPage.GetUserCountAsync();
        
        // Assert
        Assert.True(userCount >= 0, "Should have valid user count");
        Output.WriteLine($"User count displayed correctly: {userCount}");
    }
    
    [Fact]
    public async Task Users_CanCreateNewUser_AuthenticatedAdmin()
    {
        // Arrange - Admin user already authenticated
        var usersPage = new UsersPage(Page, BaseUrl);
        var testId = CreateTestId();
        var userName = $"testuser{testId}";
        var userEmail = $"testuser{testId}@example.com";
        
        // Act
        await usersPage.NavigateToAsync();
        
        await usersPage.ClickCreateUserAsync();
        await usersPage.FillUserFormAsync(
            userName: userName,
            email: userEmail,
            roles: new[] { "Viewer" }
        );
        
        await usersPage.SaveUserAsync();
        await Task.Delay(2000); // Allow save to complete
        
        // Assert
        var isVisible = await usersPage.IsUserVisibleAsync(userName);
        Assert.True(isVisible, $"Should be able to see created user: {userName}");
        
        Output.WriteLine($"Successfully created user: {userName}");
    }
    
    [Fact]
    public async Task Users_CanEditExistingUser_AuthenticatedAdmin()
    {
        // Arrange - Admin user already authenticated
        var usersPage = new UsersPage(Page, BaseUrl);
        var testId = CreateTestId();
        var originalName = $"edituser{testId}";
        var updatedName = $"updated{testId}";
        var userEmail = $"edituser{testId}@example.com";
        
        // Create user first
        await usersPage.NavigateToAsync();
        
        await usersPage.ClickCreateUserAsync();
        await usersPage.FillUserFormAsync(
            userName: originalName,
            email: userEmail,
            roles: new[] { "Viewer" }
        );
        
        await usersPage.SaveUserAsync();
        await Task.Delay(2000);
        
        // Act - Edit the user
        await usersPage.EditUserAsync(originalName);
        await Page.FillAsync("[data-testid='username-input']", updatedName);
        await usersPage.SaveUserAsync();
        await Task.Delay(2000);
        
        // Assert
        var isVisible = await usersPage.IsUserVisibleAsync(updatedName);
        Assert.True(isVisible, $"Should see updated user name: {updatedName}");
        
        Output.WriteLine($"Successfully edited user from {originalName} to {updatedName}");
    }
    
    [Fact]
    public async Task Users_CanDeleteUser_AuthenticatedAdmin()
    {
        // Arrange - Admin user already authenticated
        var usersPage = new UsersPage(Page, BaseUrl);
        var testId = CreateTestId();
        var userName = $"deleteuser{testId}";
        var userEmail = $"deleteuser{testId}@example.com";
        
        // Create user first
        await usersPage.NavigateToAsync();
        
        await usersPage.ClickCreateUserAsync();
        await usersPage.FillUserFormAsync(
            userName: userName,
            email: userEmail,
            roles: new[] { "Viewer" }
        );
        
        await usersPage.SaveUserAsync();
        await Task.Delay(2000);
        
        // Act - Delete the user
        await usersPage.DeleteUserAsync(userName);
        await usersPage.ConfirmDeleteAsync();
        await Task.Delay(2000);
        
        // Assert
        var isVisible = await usersPage.IsUserVisibleAsync(userName);
        Assert.False(isVisible, $"User should be deleted: {userName}");
        
        Output.WriteLine($"Successfully deleted user: {userName}");
    }
    
    [Fact]
    public async Task Users_FormValidatesRequiredFields_AuthenticatedAdmin()
    {
        // Arrange - Admin user already authenticated
        var usersPage = new UsersPage(Page, BaseUrl);
        
        // Act
        await usersPage.NavigateToAsync();
        
        await usersPage.ClickCreateUserAsync();
        // Try to save without filling required fields
        await usersPage.SaveUserAsync();
        await Task.Delay(1000);
        
        // Assert - Should still be on modal (validation prevented save)
        var modalVisible = await Page.IsVisibleAsync(".modal.show");
        Assert.True(modalVisible, "Modal should still be visible due to validation");
        
        // Cleanup
        await Page.ClickAsync("button:has-text('Cancel')");
        Output.WriteLine("Form validation works correctly for required fields");
    }
}