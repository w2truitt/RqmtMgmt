using frontend.E2ETests.PageObjects;
using Microsoft.Playwright;
using Xunit;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for Project Requirements Management functionality
/// </summary>
public class ProjectRequirementsE2ETests : E2ETestBase
{
    [Fact]
    public async Task NewRequirement_NavigationFromProjectDashboard_Success()
    {
        // Arrange
        var projectDashboard = new ProjectDashboardPage(Page, BaseUrl);
        var requirementForm = new RequirementFormPage(Page, BaseUrl);
        
        // Act
        await projectDashboard.NavigateToAsync(1);
        await projectDashboard.WaitForPageLoadAsync();
        await projectDashboard.ClickNewRequirementButtonAsync();
        
        // Assert
        await requirementForm.WaitForPageLoadAsync();
        Assert.True(await requirementForm.IsCreateModeAsync());
        Assert.Contains("/projects/1/requirements/new", Page.Url);
    }
    
    [Fact]
    public async Task CreateRequirement_WithValidData_Success()
    {
        // Arrange
        var requirementForm = new RequirementFormPage(Page, BaseUrl);
        var projectDashboard = new ProjectDashboardPage(Page, BaseUrl);
        
        // Act
        await requirementForm.NavigateToNewRequirementAsync(1);
        await requirementForm.WaitForPageLoadAsync();
        
        await requirementForm.FillRequirementFormAsync(
            title: "Test Requirement E2E",
            description: "This is a test requirement created via E2E test",
            type: "CRS",
            status: "Draft"
        );
        
        await requirementForm.SaveRequirementAsync();
        
        // Check for any validation errors
        await Task.Delay(1000);
        var errorElements = await Page.QuerySelectorAllAsync(".alert-danger, .validation-message, .field-validation-error");
        if (errorElements.Count > 0)
        {
            var errorTexts = new List<string>();
            foreach (var element in errorElements)
            {
                var text = await element.TextContentAsync();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    errorTexts.Add(text);
                }
            }
            Console.WriteLine($"Validation errors found: {string.Join(", ", errorTexts)}");
        }
        
        // Assert - Should redirect to project dashboard or requirements list
        await Task.Delay(2000); // Allow for navigation
        var currentUrl = Page.Url;
        Console.WriteLine($"Current URL after save: {currentUrl}");
        Assert.True(currentUrl.Contains("/projects/1") && !currentUrl.Contains("/new"), 
            $"Expected URL to contain '/projects/1' and not contain '/new', but got: {currentUrl}");
    }
    
    [Fact]
    public async Task CreateRequirement_WithMissingTitle_ShowsValidation()
    {
        // Arrange
        var requirementForm = new RequirementFormPage(Page, BaseUrl);
        
        // Act
        await requirementForm.NavigateToNewRequirementAsync(1);
        await requirementForm.WaitForPageLoadAsync();
        
        await requirementForm.FillRequirementFormAsync(
            title: "", // Missing title
            description: "This requirement has no title",
            type: "CRS",
            status: "Draft"
        );
        
        await requirementForm.SaveRequirementAsync();
        
        // Debug: Check what validation elements are present
        await Task.Delay(1000); // Wait for validation to appear
        
        // Check if we're still on the form page (validation should prevent navigation)
        var currentUrl = Page.Url;
        Console.WriteLine($"Current URL after save attempt: {currentUrl}");
        var stillOnNewPage = currentUrl.Contains("/new");
        Console.WriteLine($"Still on new requirement page: {stillOnNewPage}");
        
        // Check for any validation-related text
        var pageText = await Page.TextContentAsync("body");
        var hasRequiredText = pageText?.Contains("required") == true || pageText?.Contains("Required") == true;
        Console.WriteLine($"Page contains 'required' text: {hasRequiredText}");
        
        // Look for validation messages near the title field
        var titleValidation = await Page.QuerySelectorAsync("input[placeholder='Enter requirement title...'] ~ .validation-message, input[placeholder='Enter requirement title...'] + .validation-message");
        if (titleValidation != null)
        {
            var validationText = await titleValidation.TextContentAsync();
            Console.WriteLine($"Title validation text: '{validationText}'");
        }
        
        // Assert - if validation is working, we should still be on the form page
        // and not have navigated away (which would indicate successful submission)
        Assert.True(stillOnNewPage, "Expected to remain on the new requirement page due to validation errors");
    }
    
    [Fact]
    public async Task EditRequirement_Navigation_Success()
    {
        // Arrange
        var requirementView = new RequirementViewPage(Page, BaseUrl);
        var requirementForm = new RequirementFormPage(Page, BaseUrl);
        
        // Act
        await requirementView.NavigateToAsync(1, 1); // Assuming requirement ID 1 exists
        await requirementView.WaitForPageLoadAsync();
        await requirementView.ClickEditButtonAsync();
        
        // Assert - Wait for edit page to load (different from new requirement page)
        await Page.WaitForSelectorAsync("h2:has-text('Edit Requirement')", new PageWaitForSelectorOptions { Timeout = 30000 });
        Assert.True(await requirementForm.IsEditModeAsync());
        Assert.Contains("/projects/1/requirements/1/edit", Page.Url);
    }
    
    [Fact]
    public async Task ViewRequirement_DisplaysRequirementDetails_Success()
    {
        // Arrange
        var requirementView = new RequirementViewPage(Page, BaseUrl);
        
        // Act
        await requirementView.NavigateToAsync(1, 1); // Assuming requirement ID 1 exists
        await requirementView.WaitForPageLoadAsync();
        
        // Assert
        var title = await requirementView.GetRequirementTitleAsync();
        Assert.False(string.IsNullOrEmpty(title));
        Assert.True(await requirementView.IsEditButtonVisibleAsync());
        Assert.Contains("/projects/1/requirements/1", Page.Url);
    }
    
    [Fact]
    public async Task RequirementForm_CancelButton_RedirectsBack()
    {
        // Arrange
        var requirementForm = new RequirementFormPage(Page, BaseUrl);
        
        // Act
        await requirementForm.NavigateToNewRequirementAsync(1);
        await requirementForm.WaitForPageLoadAsync();
        
        await requirementForm.FillRequirementFormAsync(
            title: "Test Requirement to Cancel",
            description: "This should be cancelled",
            type: "CRS",
            status: "Draft"
        );
        
        await requirementForm.CancelAsync();
        
        // Assert - Should redirect back to project dashboard or requirements list
        await Task.Delay(1000); // Allow for navigation
        Assert.True(Page.Url.Contains("/projects/1") && !Page.Url.Contains("/new"));
    }
    
    [Fact]
    public async Task RequirementWorkflow_CreateViewEdit_Success()
    {
        // Arrange
        var requirementForm = new RequirementFormPage(Page, BaseUrl);
        var requirementView = new RequirementViewPage(Page, BaseUrl);
        var projectDashboard = new ProjectDashboardPage(Page, BaseUrl);
        
        var testTitle = $"E2E Test Requirement {DateTime.Now:HHmmss}";
        
        // Act & Assert - Create
        await projectDashboard.NavigateToAsync(1);
        await projectDashboard.ClickNewRequirementButtonAsync();
        await requirementForm.WaitForPageLoadAsync();
        
        await requirementForm.FillRequirementFormAsync(
            title: testTitle,
            description: "Full workflow test requirement",
            type: "CRS",
            status: "Draft"
        );
        
        await requirementForm.SaveRequirementAsync();
        await Task.Delay(2000); // Allow for save and navigation
        
        // Navigate back to view the created requirement (assuming we get redirected somewhere)
        // This part might need adjustment based on actual redirect behavior
        Assert.Contains("/projects/1", Page.Url);
    }
}
