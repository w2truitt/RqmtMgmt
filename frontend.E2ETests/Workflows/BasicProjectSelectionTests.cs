using frontend.E2ETests.PageObjects;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using RqmtMgmtShared;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.Playwright.Assertions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Simple E2E tests for basic project selection workflows
/// </summary>
public class BasicProjectSelectionTests : AuthenticatedE2ETestBase
{
    public BasicProjectSelectionTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task ProjectSelection_CanNavigateToProjectsPage_Success()
    {
        // Arrange - Login as project manager to access projects
        var loginSuccess = await LoginAsProjectManagerAsync();
        Assert.True(loginSuccess, "Failed to login as project manager");
        
        // Arrange & Act
        await Page.GotoAsync($"{BaseUrl}/projects");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert
        Assert.Contains("/projects", Page.Url);
        await Expect(Page.Locator("h3:has-text('Projects')")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ProjectSelection_CanAccessProjectRequirementsDirectly_Success()
    {
        // Arrange - Login as project manager to access project requirements
        var loginSuccess = await LoginAsProjectManagerAsync();
        Assert.True(loginSuccess, "Failed to login as project manager");
        
        // Arrange
        var projectId = 1; // Use a known project ID
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/projects/{projectId}/requirements");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert
        Assert.Contains($"/projects/{projectId}/requirements", Page.Url);
        await Expect(Page.Locator("h3:has-text('Requirements')")).ToBeVisibleAsync();
    }
    
    [Fact]
    public async Task ProjectSelection_CanAccessProjectDashboard_Success()
    {
        // Arrange - Login as project manager to access project dashboard
        var loginSuccess = await LoginAsProjectManagerAsync();
        Assert.True(loginSuccess, "Failed to login as project manager");
        
        // Arrange
        var projectId = 1; // Use a known project ID
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/projects/{projectId}/dashboard");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert
        Assert.Contains($"/projects/{projectId}/dashboard", Page.Url);
    }
}