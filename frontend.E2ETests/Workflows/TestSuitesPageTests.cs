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
/// E2E tests for the Test Suites page functionality
/// OPTIMIZED: Now uses shared browser and cached authentication for 4-10x performance improvement
/// All tests run as tester user since test suite management is typically a tester responsibility
/// </summary>
public class TestSuitesPageTests : AuthenticatedE2ETestBase
{
    public TestSuitesPageTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        // Set tester user for all tests - appropriate role for test suite management
        SetTesterUser();
    }

    [Fact]
    public async Task TestSuites_NavigatesSuccessfully_AuthenticatedUser()
    {
        // Arrange - Tester user already authenticated via base class
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/testsuites");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert
        Assert.Contains("/testsuites", Page.Url);
        await Expect(Page.Locator("h3:has-text('Test Suites')")).ToBeVisibleAsync();
        
        Output.WriteLine($"Successfully navigated to test suites page: {Page.Url}");
    }
    
    [Fact]
    public async Task TestSuites_LoadsWithoutErrors_AuthenticatedUser()
    {
        // Arrange - Tester user already authenticated
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/testsuites");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - Check that page loads without JavaScript errors
        var errors = await Page.EvaluateAsync<string[]>("() => window.errors || []");
        Assert.Empty(errors);
        
        Assert.Contains("/testsuites", Page.Url);
        Output.WriteLine("Test suites page loaded without errors");
    }
    
    [Fact]
    public async Task TestSuites_HasExpectedPageElements_AuthenticatedUser()
    {
        // Arrange - Tester user already authenticated
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/testsuites");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert
        await Expect(Page.Locator("h3:has-text('Test Suites')")).ToBeVisibleAsync();
        
        // Check for test suites table or list
        var hasTestSuitesDisplay = await Page.IsVisibleAsync("table") || 
                                  await Page.IsVisibleAsync(".testsuites-list") ||
                                  await Page.IsVisibleAsync("[data-testid='testsuites-table']");
        Assert.True(hasTestSuitesDisplay, "Should have some form of test suites display");
        
        Output.WriteLine("All expected page elements are present");
    }
}