using frontend.E2ETests.PageObjects;
using Microsoft.Playwright;
using Xunit;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Basic smoke tests to verify the application is working
/// </summary>
public class SmokeTests : E2ETestBase
{
    [Fact]
    public async Task Homepage_LoadsSuccessfully()
    {
        // Act
        await Page.GotoAsync(BaseUrl);
        
        // Wait for the application to load
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(3000); // Allow Blazor to initialize
        
        // Assert that we can see some basic UI elements
        var isVisible = await Page.IsVisibleAsync("body") && 
                       await Page.IsVisibleAsync("#app");
        
        Assert.True(isVisible, "Homepage should load successfully");
    }
    
    [Fact]
    public async Task ProjectsPage_LoadsSuccessfully()
    {
        // Arrange
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        
        // Act
        await projectsPage.NavigateToAsync();
        await Task.Delay(5000); // Allow page to load
        
        // Assert basic page elements are present
        var isLoaded = await Page.IsVisibleAsync("body");
        Assert.True(isLoaded, "Projects page should load successfully");
    }
    
    [Fact]
    public async Task UsersPage_LoadsSuccessfully()
    {
        // Arrange
        var usersPage = new UsersPage(Page, BaseUrl);
        
        // Act
        await usersPage.NavigateToAsync();
        await Task.Delay(5000); // Allow page to load
        
        // Assert basic page elements are present
        var isLoaded = await Page.IsVisibleAsync("body");
        Assert.True(isLoaded, "Users page should load successfully");
    }
}
