using frontend.E2ETests.PageObjects;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using RqmtMgmtShared;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Debug tests for the Projects page to investigate API issues
/// </summary>
public class DebugProjectsPageTests : E2ETestBase
{
    [Fact]
    public async Task Debug_ProjectsPage_ShowPageContent()
    {
        // Arrange
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        
        // Act
        await projectsPage.NavigateToAsync();
        await projectsPage.WaitForPageLoadAsync();
        
        // Get page content for debugging
        var content = await Page.ContentAsync();
        var pageLength = content.Length;
        
        // Get all buttons for debugging
        var buttons = await Page.QuerySelectorAllAsync("button");
        var buttonInfo = new List<string>();
        foreach (var button in buttons)
        {
            var text = await button.TextContentAsync();
            var testId = await button.GetAttributeAsync("data-testid");
            buttonInfo.Add($"Text: '{text?.Trim()}', TestId: '{testId}'");
        }
        
        // Get all elements with data-testid
        var testIdElements = await Page.QuerySelectorAllAsync("[data-testid]");
        var testIds = new List<string>();
        foreach (var element in testIdElements)
        {
            var testId = await element.GetAttributeAsync("data-testid");
            var tagName = await element.EvaluateAsync<string>("el => el.tagName");
            testIds.Add($"{tagName}: {testId}");
        }
        
        // Log everything for debugging
        throw new Exception($"Debug Info - Buttons: [{string.Join(", ", buttonInfo)}], TestIds: [{string.Join(", ", testIds)}], PageLength: {pageLength}, Content: {content}");
    }
    
    [Fact]
    public async Task Debug_ProjectsPage_TestFormSubmission()
    {
        // Arrange
        var testId = CreateTestId();
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var project = TestDataFactory.CreateProject(testId);
        
        // Set up console logging to capture API calls
        var consoleMessages = new List<string>();
        Page.Console += (_, e) => consoleMessages.Add($"Console: {e.Text}");
        
        // Set up request/response logging
        var apiRequests = new List<string>();
        Page.Request += (_, e) => 
        {
            if (e.Url.Contains("/api/"))
                apiRequests.Add($"Request: {e.Method} {e.Url}");
        };
        
        var apiResponses = new List<string>();
        Page.Response += (_, e) => 
        {
            if (e.Url.Contains("/api/"))
                apiResponses.Add($"Response: {e.Status} {e.Url}");
        };
        
        // Act
        await projectsPage.NavigateToAsync();
        await projectsPage.WaitForPageLoadAsync();
        await projectsPage.ClickCreateProjectAsync();
        await projectsPage.WaitForFormModalAsync();
        
        // Fill form and try to save
        await projectsPage.FillProjectFormAsync(
            project.Name, 
            project.Code, 
            project.Description ?? "", 
            project.Status.ToString(), 
            project.OwnerId
        );
        await projectsPage.SaveProjectAsync();
        
        // Wait for any API calls to complete
        await Page.WaitForTimeoutAsync(5000);
        
        // Check if modal is still visible (indicates save failed)
        var modalStillVisible = await Page.IsVisibleAsync(".modal.show");
        
        // Get any error messages
        var errorMessage = "";
        try
        {
            errorMessage = await Page.TextContentAsync(".alert-danger") ?? "";
        }
        catch { }
        
        // Log everything for debugging
        throw new Exception($"Debug Info - API Requests: [{string.Join("; ", apiRequests)}], API Responses: [{string.Join("; ", apiResponses)}], Console Messages: [{string.Join("; ", consoleMessages)}], Modal still visible: {modalStillVisible}, Error Message: '{errorMessage}'");
    }
}
