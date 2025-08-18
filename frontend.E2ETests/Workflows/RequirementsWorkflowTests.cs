using frontend.E2ETests.PageObjects;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using RqmtMgmtShared;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for the Requirements page
/// </summary>
public class RequirementsWorkflowTests : E2ETestBase
{
    [Fact]
    public async Task Requirements_NavigatesSuccessfully()
    {
        // Arrange
        var requirementsPage = new RequirementsPage(Page, BaseUrl);
        
        // Act
        await requirementsPage.NavigateToAsync();
        
        // Assert
        Assert.Contains("/requirements", Page.Url);
    }
    
    [Fact]
    public async Task Requirements_LoadsWithoutErrors()
    {
        // Arrange
        var requirementsPage = new RequirementsPage(Page, BaseUrl);
        
        // Act
        await requirementsPage.NavigateToAsync();
        await requirementsPage.WaitForPageLoadAsync();
        
        // Assert
        // Check that page loads without JavaScript errors
        var errors = await Page.EvaluateAsync<string[]>("() => window.errors || []");
        Assert.Empty(errors);
        
        // Check that we can access the page
        Assert.Contains("/requirements", Page.Url);
    }
    
    [Fact]
    public async Task Requirements_HasExpectedPageElements()
    {
        // Arrange
        var requirementsPage = new RequirementsPage(Page, BaseUrl);
        
        // Act
        await requirementsPage.NavigateToAsync();
        await requirementsPage.WaitForPageLoadAsync();
        
        // Assert
        var title = await Page.TitleAsync();
        Assert.NotNull(title);
        
        // Verify main page elements are visible
        await Expect(Page.Locator("text=Add Requirement")).ToBeVisibleAsync();
        await Expect(Page.Locator("input[placeholder*='Search requirements']")).ToBeVisibleAsync();
        await Expect(Page.Locator("h1:has-text('Requirements')")).ToBeVisibleAsync();
    }
    
    [Fact]
    public async Task Requirements_CanCreateNewRequirement()
    {
        // Arrange
        var testId = CreateTestId();
        var requirementsPage = new RequirementsPage(Page, BaseUrl);
        var requirement = TestDataFactory.CreateRequirement(testId);
        
        // Act
        await requirementsPage.NavigateToAsync();
        await requirementsPage.WaitForPageLoadAsync();
        await requirementsPage.ClickCreateRequirementAsync();
        await requirementsPage.WaitForFormModalAsync();
        await requirementsPage.FillRequirementFormAsync(
            requirement.Title, 
            requirement.Description ?? "", 
            requirement.Type.ToString(), 
            requirement.Status.ToString()
        );
        await requirementsPage.SaveRequirementAsync();
        
        // Wait for operation to complete
        await Page.WaitForTimeoutAsync(3000);
        
        // Verify requirement was created
        var isVisible = await requirementsPage.IsRequirementVisibleAsync(requirement.Title);
        Assert.True(isVisible);
        
        // Assert
        Assert.NotNull(requirement);
        Assert.Contains(testId, requirement.Title);
        Assert.Contains("/requirements", Page.Url);
    }
    
    [Fact]
    public async Task Requirements_CanSearchRequirements()
    {
        // Arrange
        var testId = CreateTestId();
        var requirementsPage = new RequirementsPage(Page, BaseUrl);
        
        // Act
        await requirementsPage.NavigateToAsync();
        await requirementsPage.WaitForPageLoadAsync();
        await requirementsPage.SearchRequirementsAsync("Test");
        
        // Wait for search to complete
        await Page.WaitForTimeoutAsync(1000);
        
        // Assert - we should see some requirements in search results or the search should execute without error
        Assert.Contains("/requirements", Page.Url);
        
        // Verify the search input has the expected value
        var searchValue = await Page.InputValueAsync("input[placeholder*='Search requirements']");
        Assert.Equal("Test", searchValue);
    }
    
    [Fact]
    public async Task Requirements_CanOpenAndCancelForm()
    {
        // Arrange
        var requirementsPage = new RequirementsPage(Page, BaseUrl);
        
        // Act
        await requirementsPage.NavigateToAsync();
        await requirementsPage.WaitForPageLoadAsync();
        await requirementsPage.ClickCreateRequirementAsync();
        await requirementsPage.WaitForFormModalAsync();
        
        // Verify form is visible
        await Expect(Page.Locator(".modal.show.d-block")).ToBeVisibleAsync();
        
        // Cancel the form
        await requirementsPage.CancelRequirementAsync();
        await requirementsPage.WaitForFormModalToHideAsync();
        
        // Assert
        // Form should be hidden
        await Expect(Page.Locator(".modal.show.d-block")).Not.ToBeVisibleAsync();
    }
    
    [Fact]
    public async Task Requirements_FormValidatesRequiredFields()
    {
        // Arrange
        var requirementsPage = new RequirementsPage(Page, BaseUrl);
        
        // Act
        await requirementsPage.NavigateToAsync();
        await requirementsPage.WaitForPageLoadAsync();
        await requirementsPage.ClickCreateRequirementAsync();
        await requirementsPage.WaitForFormModalAsync();
        
        // Try to save without filling required fields
        await requirementsPage.SaveRequirementAsync();
        
        // Wait a moment for validation
        await Page.WaitForTimeoutAsync(1000);
        
        // Assert
        // Form should still be visible (not saved due to validation)
        await Expect(Page.Locator(".modal.show.d-block")).ToBeVisibleAsync();
    }
    
    [Fact]
    public async Task Requirements_CanEditExistingRequirement()
    {
        // Arrange
        var testId = CreateTestId();
        var requirementsPage = new RequirementsPage(Page, BaseUrl);
        var requirement = TestDataFactory.CreateRequirement(testId);
        
        // First create a requirement
        await requirementsPage.NavigateToAsync();
        await requirementsPage.WaitForPageLoadAsync();
        await requirementsPage.ClickCreateRequirementAsync();
        await requirementsPage.WaitForFormModalAsync();
        await requirementsPage.FillRequirementFormAsync(
            requirement.Title, 
            requirement.Description ?? "", 
            requirement.Type.ToString(), 
            requirement.Status.ToString()
        );
        await requirementsPage.SaveRequirementAsync();
        
        // Wait for creation to complete
        await Page.WaitForTimeoutAsync(3000);
        
        // Now edit the requirement
        await requirementsPage.EditRequirementAsync(requirement.Title);
        await requirementsPage.WaitForFormModalAsync();
        
        // Verify form is populated with existing data
        var currentTitle = await requirementsPage.GetRequirementTitleInputValueAsync();
        Assert.Equal(requirement.Title, currentTitle);
        
        // Update the requirement title
        var updatedTitle = $"Updated {requirement.Title}";
        await Page.FillAsync("[data-testid='title-input']", updatedTitle);
        await requirementsPage.SaveRequirementAsync();
        
        // Wait for update to complete
        await Page.WaitForTimeoutAsync(3000);
        
        // Assert
        var isUpdatedVisible = await requirementsPage.IsRequirementVisibleAsync(updatedTitle);
        Assert.True(isUpdatedVisible);
    }
    
    [Fact]
    public async Task Requirements_CanDeleteRequirement()
    {
        // Arrange
        var testId = CreateTestId();
        var requirementsPage = new RequirementsPage(Page, BaseUrl);
        var requirement = TestDataFactory.CreateRequirement(testId);
        
        // First create a requirement
        await requirementsPage.NavigateToAsync();
        await requirementsPage.WaitForPageLoadAsync();
        await requirementsPage.ClickCreateRequirementAsync();
        await requirementsPage.WaitForFormModalAsync();
        await requirementsPage.FillRequirementFormAsync(
            requirement.Title, 
            requirement.Description ?? "", 
            requirement.Type.ToString(), 
            requirement.Status.ToString()
        );
        await requirementsPage.SaveRequirementAsync();
        
        // Wait for creation to complete
        await Page.WaitForTimeoutAsync(3000);
        
        // Set up dialog handler for confirmation
        Page.Dialog += async (_, dialog) =>
        {
            Assert.Equal("confirm", dialog.Type);
            await dialog.AcceptAsync();
        };
        
        // Delete the requirement
        await requirementsPage.DeleteRequirementAsync(requirement.Title);
        
        // Wait for deletion to complete
        await Page.WaitForTimeoutAsync(3000);
        
        // Assert
        var isDeletedVisible = await requirementsPage.IsRequirementVisibleAsync(requirement.Title);
        Assert.False(isDeletedVisible);
    }
    
    [Fact]
    public async Task Requirements_CanPerformFullCrudWorkflow()
    {
        // Arrange
        var testId = CreateTestId();
        var requirementsPage = new RequirementsPage(Page, BaseUrl);
        var requirement = TestDataFactory.CreateRequirement(testId);
        
        // Act & Assert: Create
        await requirementsPage.NavigateToAsync();
        await requirementsPage.WaitForPageLoadAsync();
        await requirementsPage.ClickCreateRequirementAsync();
        await requirementsPage.WaitForFormModalAsync();
        await requirementsPage.FillRequirementFormAsync(
            requirement.Title, 
            requirement.Description ?? "", 
            requirement.Type.ToString(), 
            requirement.Status.ToString()
        );
        await requirementsPage.SaveRequirementAsync();
        
        // Wait for creation
        await Page.WaitForTimeoutAsync(3000);
        
        // Verify creation
        var isVisible = await requirementsPage.IsRequirementVisibleAsync(requirement.Title);
        Assert.True(isVisible, "Requirement should be visible after creation");
        
        // Act & Assert: Update
        await requirementsPage.EditRequirementAsync(requirement.Title);
        await requirementsPage.WaitForFormModalAsync();
        var updatedTitle = $"Updated {requirement.Title}";
        await Page.FillAsync("[data-testid='title-input']", updatedTitle);
        await requirementsPage.SaveRequirementAsync();
        
        // Wait for update
        await Page.WaitForTimeoutAsync(3000);
        
        // Verify update
        var isUpdatedVisible = await requirementsPage.IsRequirementVisibleAsync(updatedTitle);
        Assert.True(isUpdatedVisible, "Updated requirement should be visible after edit");
        
        // Act & Assert: Delete
        Page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
        await requirementsPage.DeleteRequirementAsync(updatedTitle);
        
        // Wait for deletion
        await Page.WaitForTimeoutAsync(3000);
        
        // Verify deletion
        var isDeletedVisible = await requirementsPage.IsRequirementVisibleAsync(updatedTitle);
        Assert.False(isDeletedVisible, "Requirement should not be visible after deletion");
    }
}