using frontend.E2ETests.PageObjects;
using Microsoft.Playwright;
using Xunit;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Debug tests for the Test Plans page to understand what's being rendered
/// </summary>
public class DebugTestPlansPageTests : E2ETestBase
{
    [Fact]
    public async Task Debug_TestPlansPage_ShowPageContent()
    {
        // Arrange
        var testPlansPage = new TestPlansPage(Page, BaseUrl);
        
        // Act
        await testPlansPage.NavigateToAsync();
        
        // Wait for page to fully load
        await Page.WaitForTimeoutAsync(3000);
        
        // Take a screenshot for debugging
        await Page.ScreenshotAsync(new() { Path = "test-plans-debug.png" });
        
        // Get page content to see what's actually rendered
        var pageContent = await Page.ContentAsync();
        
        // Get all buttons to see what buttons exist
        var buttons = await Page.Locator("button").AllAsync();
        var buttonTexts = new List<string>();
        foreach (var button in buttons)
        {
            var text = await button.TextContentAsync();
            var testId = await button.GetAttributeAsync("data-testid");
            buttonTexts.Add($"Text: '{text}', TestId: '{testId}'");
        }
        
        // Get all elements with data-testid
        var testIdElements = await Page.Locator("[data-testid]").AllAsync();
        var testIds = new List<string>();
        foreach (var element in testIdElements)
        {
            var testId = await element.GetAttributeAsync("data-testid");
            var tagName = await element.EvaluateAsync<string>("el => el.tagName");
            testIds.Add($"{tagName}: {testId}");
        }
        
        // Assert and output debug info
        Assert.Contains("/testplans", Page.Url);
        
        // This will show in test output for debugging
        throw new Exception($"Debug Info - Buttons: [{string.Join(", ", buttonTexts)}], TestIds: [{string.Join(", ", testIds)}], PageLength: {pageContent.Length}, Content: {pageContent}");
    }
}
