using frontend.E2ETests.PageObjects;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.Playwright.Assertions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for the Test Suites page
/// </summary>
public class TestSuitesPageTests : AuthenticatedE2ETestBase
{
    public TestSuitesPageTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task TestSuites_NavigatesSuccessfully()
    {
        // Arrange - Login as tester to access test suites
        var loginSuccess = await LoginAsTesterAsync();
        Assert.True(loginSuccess, "Failed to login as tester");
        
        // Arrange
        var testSuitesPage = new TestSuitesPage(Page, BaseUrl);
        
        // Act
        await testSuitesPage.NavigateToAsync();
        
        // Assert
        Assert.Contains("/testsuites", Page.Url);
    }
    
    [Fact]
    public async Task TestSuites_LoadsWithoutErrors()
    {
        // Arrange - Login as tester to access test suites
        var loginSuccess = await LoginAsTesterAsync();
        Assert.True(loginSuccess, "Failed to login as tester");
        
        // Arrange
        var testSuitesPage = new TestSuitesPage(Page, BaseUrl);
        
        // Act
        await testSuitesPage.NavigateToAsync();
        
        // Assert
        // Check that page loads without JavaScript errors
        var errors = await Page.EvaluateAsync<string[]>("() => window.errors || []");
        Assert.Empty(errors);
        
        // Check that we can access the page
        Assert.Contains("/testsuites", Page.Url);
    }
    
    [Fact]
    public async Task TestSuites_HasExpectedPageElements()
    {
        // Arrange - Login as tester to access test suites
        var loginSuccess = await LoginAsTesterAsync();
        Assert.True(loginSuccess, "Failed to login as tester");
        
        // Arrange
        var testSuitesPage = new TestSuitesPage(Page, BaseUrl);
        
        // Act
        await testSuitesPage.NavigateToAsync();
        
        // Assert
        var title = await Page.TitleAsync();
        Assert.NotNull(title);
        // TODO: Add more specific element checks when frontend is implemented
    }
}