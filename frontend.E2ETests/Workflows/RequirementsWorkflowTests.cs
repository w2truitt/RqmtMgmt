using frontend.E2ETests.PageObjects;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using RqmtMgmtShared;
using Xunit;
using static Microsoft.Playwright.Assertions;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for the Requirements page with authentication
/// </summary>
public class RequirementsWorkflowTests : AuthenticatedE2ETestBase
{
    public RequirementsWorkflowTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task Requirements_NavigatesSuccessfully_AuthenticatedUser()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
        var requirementsPage = new RequirementsPage(Page, BaseUrl);
        
        // Act
        await requirementsPage.NavigateToAsync();
        
        // Assert
        Assert.Contains("/requirements", Page.Url);
        _output.WriteLine($"Successfully navigated to requirements page: {Page.Url}");
    }
    
    [Fact]
    public async Task Requirements_LoadsWithoutErrors_AuthenticatedUser()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
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
        _output.WriteLine("Requirements page loaded without errors");
    }
    
    [Fact]
    public async Task Requirements_HasExpectedPageElements_AuthenticatedUser()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
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
        
        _output.WriteLine("All expected page elements are present");
    }
    
    [Fact]
    public async Task Requirements_CanCreateNewRequirement_AuthenticatedAdmin()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
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
        
        // Additional wait to ensure form binding is complete before saving
        await Page.WaitForTimeoutAsync(1000);
        
        // Debug: Check form values right before save
        var titleValueBeforeSave = await Page.InputValueAsync("[data-testid='title-input']");
        var descValueBeforeSave = await Page.InputValueAsync("[data-testid='description-input']");
        _output.WriteLine($"Form values before save - Title: '{titleValueBeforeSave}', Description: '{descValueBeforeSave}'");
        
        await requirementsPage.SaveRequirementAsync();
        
        // Wait for operation to complete
        await Page.WaitForTimeoutAsync(3000);
        
        // Debug: Check for any error messages on the page
        var errorElement = await Page.QuerySelectorAsync(".alert-danger");
        var errorMessage = errorElement != null ? await errorElement.TextContentAsync() : "No error message";
        _output.WriteLine($"Error message after save: '{errorMessage}'");
        
        // Debug: Check the page URL after save
        _output.WriteLine($"Page URL after save: {Page.Url}");
        
        // Debug: Check if modal is still visible (which would indicate validation errors)
        var modalVisible = await Page.IsVisibleAsync(".modal.show.d-block");
        _output.WriteLine($"Modal still visible after save: {modalVisible}");
        
        // Debug: Check form input values if modal is still visible
        if (modalVisible)
        {
            var titleValue = await Page.InputValueAsync("[data-testid='title-input']");
            var descValue = await Page.InputValueAsync("[data-testid='description-input']");
            var typeValue = await Page.InputValueAsync("[data-testid='type-select']");
            var statusValue = await Page.InputValueAsync("[data-testid='status-select']");
            _output.WriteLine($"Form values - Title: '{titleValue}', Description: '{descValue}', Type: '{typeValue}', Status: '{statusValue}'");
            
            // Check for any validation messages
            var validationMessages = await Page.QuerySelectorAllAsync(".text-danger, .field-validation-error, .validation-message");
            if (validationMessages.Count > 0)
            {
                _output.WriteLine($"Found {validationMessages.Count} validation messages:");
                foreach (var msg in validationMessages)
                {
                    var text = await msg.TextContentAsync();
                    _output.WriteLine($"  - {text}");
                }
            }
        }
        else 
        {
            // Modal closed, so save was successful. Check search term
            var searchInput = await Page.InputValueAsync("input[placeholder*='Search requirements']");
            _output.WriteLine($"Search input value after save: '{searchInput}'");
        }
        
        // Debug: Get table row count before checking visibility
        var rowCount = await Page.QuerySelectorAllAsync("[data-testid='requirement-row']");
        _output.WriteLine($"Number of requirement rows visible: {rowCount.Count}");
        
        // Debug: Get all visible text content that contains our test title
        var pageText = await Page.TextContentAsync("body");
        var containsTitle = pageText?.Contains(requirement.Title) ?? false;
        _output.WriteLine($"Page contains requirement title '{requirement.Title}': {containsTitle}");
        
        // Verify requirement was created
        var isVisible = await requirementsPage.IsRequirementVisibleAsync(requirement.Title);
        Assert.True(isVisible);
        
        // Assert
        Assert.NotNull(requirement);
        Assert.Contains(testId, requirement.Title);
        Assert.Contains("/requirements", Page.Url);
        
        _output.WriteLine($"Successfully created requirement: {requirement.Title}");
    }
    
    [Fact]
    public async Task Requirements_CanSearchRequirements_AuthenticatedUser()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
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
        
        _output.WriteLine("Search functionality works correctly");
    }
    
    [Fact]
    public async Task Requirements_CanOpenAndCancelForm_AuthenticatedUser()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
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
        
        _output.WriteLine("Form modal can be opened and cancelled successfully");
    }
    
    [Fact]
    public async Task Requirements_FormValidatesRequiredFields_AuthenticatedUser()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
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
        
        _output.WriteLine("Form validation works correctly for required fields");
    }
    
    [Fact]
    public async Task Requirements_CanEditExistingRequirement_AuthenticatedAdmin()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
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
        
        _output.WriteLine($"Successfully edited requirement to: {updatedTitle}");
    }
    
    [Fact]
    public async Task Requirements_CanDeleteRequirement_AuthenticatedAdmin()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
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
        
        _output.WriteLine($"Successfully deleted requirement: {requirement.Title}");
    }
    
    [Fact]
    public async Task Requirements_CanPerformFullCrudWorkflow_AuthenticatedAdmin()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
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
        
        _output.WriteLine($"Successfully completed full CRUD workflow for requirement: {requirement.Title}");
    }
}