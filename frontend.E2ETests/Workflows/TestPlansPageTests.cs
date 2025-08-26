using frontend.E2ETests.PageObjects;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.Playwright.Assertions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for the Test Plans page
/// </summary>
public class TestPlansPageTests : AuthenticatedE2ETestBase
{
    public TestPlansPageTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task TestPlans_NavigatesSuccessfully()
    {
        // Arrange - Login as tester to access test plans
        var loginSuccess = await LoginAsTesterAsync();
        Assert.True(loginSuccess, "Failed to login as tester");
        
        // Arrange
        var testPlansPage = new TestPlansPage(Page, BaseUrl);
        
        // Act
        await testPlansPage.NavigateToAsync();
        
        // Assert
        Assert.Contains("/testplans", Page.Url);
    }
    
    [Fact]
    public async Task TestPlans_LoadsWithoutErrors()
    {
        // Arrange - Login as tester to access test plans
        var loginSuccess = await LoginAsTesterAsync();
        Assert.True(loginSuccess, "Failed to login as tester");
        
        // Arrange
        var testPlansPage = new TestPlansPage(Page, BaseUrl);
        
        // Act
        await testPlansPage.NavigateToAsync();
        
        // Assert
        // Check that page loads without JavaScript errors
        var errors = await Page.EvaluateAsync<string[]>("() => window.errors || []");
        Assert.Empty(errors);
        
        // Check that we can access the page
        Assert.Contains("/testplans", Page.Url);
    }
}