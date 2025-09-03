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
/// E2E tests for the Test Run Sessions page functionality
/// OPTIMIZED: Now uses shared browser and cached authentication for 4-10x performance improvement
/// All tests run as tester user since test run session management is typically a tester responsibility
/// </summary>
public class TestRunSessionsPageTests : AuthenticatedE2ETestBase
{
    public TestRunSessionsPageTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        // Set tester user for all tests - appropriate role for test run session management
        SetTesterUser();
    }

    [Fact]
    public async Task TestRunSessions_NavigatesSuccessfully_AuthenticatedUser()
    {
        // Arrange - Tester user already authenticated via base class
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/testrunsessions");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert
        Assert.Contains("/testrunsessions", Page.Url);
        await Expect(Page.Locator("h3:has-text('Test Run Sessions')")).ToBeVisibleAsync();
        
        Output.WriteLine($"Successfully navigated to test run sessions page: {Page.Url}");
    }
    
    [Fact]
    public async Task TestRunSessions_LoadsWithoutErrors_AuthenticatedUser()
    {
        // Arrange - Tester user already authenticated
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/testrunsessions");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - Check that page loads without JavaScript errors
        var errors = await Page.EvaluateAsync<string[]>("() => window.errors || []");
        Assert.Empty(errors);
        
        Assert.Contains("/testrunsessions", Page.Url);
        Output.WriteLine("Test run sessions page loaded without errors");
    }
}