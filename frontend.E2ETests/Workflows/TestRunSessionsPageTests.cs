using frontend.E2ETests.PageObjects;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for the Test Run Sessions page
/// </summary>
public class TestRunSessionsPageTests : AuthenticatedE2ETestBase
{
    public TestRunSessionsPageTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task TestRunSessions_NavigatesSuccessfully()
    {
        // Arrange - Login as tester to access test run sessions
        var loginSuccess = await LoginAsTesterAsync();
        Assert.True(loginSuccess, "Failed to login as tester");
        
        // Arrange
        var testRunSessionsPage = new TestRunSessionsPage(Page, BaseUrl);
        
        // Act
        await testRunSessionsPage.NavigateToAsync();
        
        // Assert
        Assert.Contains("/testrunsessions", Page.Url);
    }
    
    [Fact]
    public async Task TestRunSessions_LoadsWithoutErrors()
    {
        // Arrange - Login as tester to access test run sessions
        var loginSuccess = await LoginAsTesterAsync();
        Assert.True(loginSuccess, "Failed to login as tester");
        
        // Arrange
        var testRunSessionsPage = new TestRunSessionsPage(Page, BaseUrl);
        
        // Act
        await testRunSessionsPage.NavigateToAsync();
        
        // Assert
        // Check that page loads without JavaScript errors
        var errors = await Page.EvaluateAsync<string[]>("() => window.errors || []");
        Assert.Empty(errors);
        
        // Check that we can access the page
        Assert.Contains("/testrunsessions", Page.Url);
    }
}