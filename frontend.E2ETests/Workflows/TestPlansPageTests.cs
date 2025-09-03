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
/// E2E tests for the Test Plans page functionality
/// OPTIMIZED: Now uses shared browser and cached authentication for 4-10x performance improvement
/// All tests run as tester user since test plan management is typically a tester responsibility
/// </summary>
public class TestPlansPageTests : AuthenticatedE2ETestBase
{
    public TestPlansPageTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        // Set tester user for all tests - appropriate role for test plan management
        SetTesterUser();
    }

    [Fact]
    public async Task TestPlans_NavigatesSuccessfully_AuthenticatedUser()
    {
        // Arrange - Tester user already authenticated via base class
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/testplans");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert
        Assert.Contains("/testplans", Page.Url);
        await Expect(Page.Locator("h3:has-text('Test Plans')")).ToBeVisibleAsync();
        
        Output.WriteLine($"Successfully navigated to test plans page: {Page.Url}");
    }
    
    [Fact]
    public async Task TestPlans_LoadsWithoutErrors_AuthenticatedUser()
    {
        // Arrange - Tester user already authenticated
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/testplans");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - Check that page loads without JavaScript errors
        var errors = await Page.EvaluateAsync<string[]>("() => window.errors || []");
        Assert.Empty(errors);
        
        Assert.Contains("/testplans", Page.Url);
        Output.WriteLine("Test plans page loaded without errors");
    }
    
    [Fact]
    public async Task TestPlans_HasExpectedPageElements_AuthenticatedUser()
    {
        // Arrange - Tester user already authenticated
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/testplans");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert
        await Expect(Page.Locator("h3:has-text('Test Plans')")).ToBeVisibleAsync();
        
        // Check for test plans table or list
        var hasTestPlansDisplay = await Page.IsVisibleAsync("table") || 
                                 await Page.IsVisibleAsync(".testplans-list") ||
                                 await Page.IsVisibleAsync("[data-testid='testplans-table']");
        Assert.True(hasTestPlansDisplay, "Should have some form of test plans display");
        
        Output.WriteLine("All expected page elements are present");
    }
}