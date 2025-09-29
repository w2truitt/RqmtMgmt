using frontend.E2ETests.Fixtures;
using frontend.E2ETests.PageObjects;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using RqmtMgmtShared;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.Playwright.Assertions;
using frontend.E2ETests.Infrastructure;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Simple E2E tests for basic project selection workflows
/// OPTIMIZED: Now uses shared browser and cached authentication for 4-10x performance improvement
/// Uses project manager role since these tests focus on project access and navigation
/// </summary>
public class BasicProjectSelectionTests : AuthenticatedE2ETestBase
{
    public BasicProjectSelectionTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        // Set project manager user - appropriate role for project selection and access
        SetProjectManagerUser();
    }

    [Fact]
    public async Task ProjectSelection_CanNavigateToProjectsPage_Success()
    {
        // Arrange - Project manager already authenticated via base class
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/projects");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert
        Assert.Contains("/projects", Page.Url);
        await Expect(Page.Locator("h3:has-text('Projects')")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ProjectSelection_CanAccessProjectRequirementsDirectly_Success()
    {
        // Arrange - Project manager already authenticated
        var projectId = 1; // Use a known project ID
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/projects/{projectId}/requirements");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert
        Assert.Contains($"/projects/{projectId}/requirements", Page.Url);
        // Project-specific requirements page uses h2, not h1
        await Expect(Page.Locator("h2:has-text('Requirements')")).ToBeVisibleAsync();
    }
    
    [Fact]
    public async Task ProjectSelection_CanAccessProjectDashboard_Success()
    {
        // Arrange - Project manager already authenticated
        var projectId = 1; // Use a known project ID
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/projects/{projectId}/dashboard");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert
        Assert.Contains($"/projects/{projectId}/dashboard", Page.Url);
    }
}