using frontend.E2ETests.PageObjects;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Debug tests for the Test Plans page to understand what's being rendered
/// </summary>
public class DebugTestPlansPageTests : AuthenticatedE2ETestBase
{
    public DebugTestPlansPageTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task Debug_TestPlansPage_ShowPageContent()
    {
        // Arrange - Login as tester to access test plans debug
        var loginSuccess = await LoginAsTesterAsync();
        Assert.True(loginSuccess, "Failed to login as tester");
        
        // Arrange
        var testPlansPage = new TestPlansPage(Page, BaseUrl);
        
        // Act
        await testPlansPage.NavigateToAsync();
        
        // Wait for page to fully load
        await Task.Delay(3000);
        
        // Assert
        Assert.Contains("/testplans", Page.Url);
    }
}