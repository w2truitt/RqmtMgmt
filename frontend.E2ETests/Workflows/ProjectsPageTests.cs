using frontend.E2ETests.PageObjects;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using RqmtMgmtShared;
using Xunit;
using static Microsoft.Playwright.Assertions;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for the Projects page with authentication
/// </summary>
public class ProjectsPageTests : AuthenticatedE2ETestBase
{
    public ProjectsPageTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task Projects_NavigatesSuccessfully_AuthenticatedUser()
    {
        // Arrange - Login as admin (will skip if already authenticated)
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");

        // Act - Navigate to projects page
        var canAccessProjects = await NavigateToProtectedPageAsync("/projects");
        Assert.True(canAccessProjects, "Should be able to access projects page");
        
        await WaitForBlazorAppAsync();
        
        // Assert
        Assert.Contains("/projects", Page.Url);
        _output.WriteLine($"Successfully navigated to projects page: {Page.Url}");
    }
    
    [Fact]
    public async Task Projects_LoadsWithoutErrors_AuthenticatedUser()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        
        // Act
        var canAccessProjects = await NavigateToProtectedPageAsync("/projects");
        Assert.True(canAccessProjects, "Should be able to access projects page");
        
        await WaitForBlazorAppAsync();
        await projectsPage.WaitForPageLoadAsync();
        
        // Assert
        // Check that page loads without JavaScript errors
        var errors = await Page.EvaluateAsync<string[]>("() => window.errors || []");
        Assert.Empty(errors);
        
        // Check that we can access the page
        Assert.Contains("/projects", Page.Url);
        _output.WriteLine("Projects page loaded without errors");
    }
    
    [Fact]
    public async Task Projects_HasExpectedPageElements_AuthenticatedUser()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        
        // Act
        var canAccessProjects = await NavigateToProtectedPageAsync("/projects");
        Assert.True(canAccessProjects, "Should be able to access projects page");
        
        await WaitForBlazorAppAsync();
        await projectsPage.WaitForPageLoadAsync();
        
        // Assert
        var title = await Page.TitleAsync();
        Assert.NotNull(title);
        
        // Verify main page elements are visible
        await Expect(Page.Locator("[data-testid='create-project-button']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='search-input']")).ToBeVisibleAsync();
        await Expect(Page.Locator("h3:has-text('Projects')")).ToBeVisibleAsync();
        
        _output.WriteLine("All expected page elements are present");
    }
    
    [Fact]
    public async Task Projects_CanCreateNewProject_AuthenticatedAdmin()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
        var testId = CreateTestId();
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var project = TestDataFactory.CreateProject(testId);
        
        // Act
        var canAccessProjects = await NavigateToProtectedPageAsync("/projects");
        Assert.True(canAccessProjects, "Should be able to access projects page");
        
        await WaitForBlazorAppAsync();
        await projectsPage.WaitForPageLoadAsync();
        await projectsPage.ClickCreateProjectAsync();
        await projectsPage.WaitForFormModalAsync();
        await projectsPage.FillProjectFormAsync(
            project.Name, 
            project.Code, 
            project.Description ?? "", 
            project.Status.ToString(), 
            project.OwnerId
        );
        await projectsPage.SaveProjectAsync();
        
        // Wait for operation to complete
        await Page.WaitForTimeoutAsync(3000);
        
        // Verify project was created
        var isVisible = await projectsPage.IsProjectVisibleAsync(project.Name);
        Assert.True(isVisible);
        
        // Assert
        Assert.NotNull(project);
        Assert.Contains(testId, project.Name);
        Assert.Contains("/projects", Page.Url);
        
        _output.WriteLine($"Successfully created project: {project.Name}");
    }
    
    [Fact]
    public async Task Projects_CanSearchProjects_AuthenticatedUser()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
        var testId = CreateTestId();
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        
        // Act
        var canAccessProjects = await NavigateToProtectedPageAsync("/projects");
        Assert.True(canAccessProjects, "Should be able to access projects page");
        
        await WaitForBlazorAppAsync();
        await projectsPage.WaitForPageLoadAsync();
        await projectsPage.SearchProjectsAsync("Test");
        
        // Wait for search to complete
        await Page.WaitForTimeoutAsync(1000);
        
        // Assert - we should see some projects in search results or the search should execute without error
        Assert.Contains("/projects", Page.Url);
        
        // Verify the search input has the expected value
        var searchValue = await Page.InputValueAsync("[data-testid='search-input']");
        Assert.Equal("Test", searchValue);
        
        _output.WriteLine("Search functionality works correctly");
    }
    
    [Fact]
    public async Task Projects_CanOpenAndCancelForm_AuthenticatedUser()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        
        // Act
        var canAccessProjects = await NavigateToProtectedPageAsync("/projects");
        Assert.True(canAccessProjects, "Should be able to access projects page");
        
        await WaitForBlazorAppAsync();
        await projectsPage.WaitForPageLoadAsync();
        await projectsPage.ClickCreateProjectAsync();
        await projectsPage.WaitForFormModalAsync();
        
        // Verify form is visible
        await Expect(Page.Locator(".modal.show")).ToBeVisibleAsync();
        
        // Cancel the form
        await projectsPage.CancelFormAsync();
        await projectsPage.WaitForFormModalToHideAsync();
        
        // Assert
        // Form should be hidden
        await Expect(Page.Locator(".modal.show")).Not.ToBeVisibleAsync();
        
        _output.WriteLine("Form modal can be opened and cancelled successfully");
    }
    
    [Fact]
    public async Task Projects_FormValidatesRequiredFields_AuthenticatedUser()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        
        // Act
        var canAccessProjects = await NavigateToProtectedPageAsync("/projects");
        Assert.True(canAccessProjects, "Should be able to access projects page");
        
        await WaitForBlazorAppAsync();
        await projectsPage.WaitForPageLoadAsync();
        await projectsPage.ClickCreateProjectAsync();
        await projectsPage.WaitForFormModalAsync();
        
        // Try to save without filling required fields
        await projectsPage.SaveProjectAsync();
        
        // Wait a moment for validation
        await Page.WaitForTimeoutAsync(1000);
        
        // Assert
        // Form should still be visible (not saved due to validation)
        await Expect(Page.Locator(".modal.show")).ToBeVisibleAsync();
        
        _output.WriteLine("Form validation works correctly for required fields");
    }
    
    [Fact]
    public async Task Projects_CanEditExistingProject_AuthenticatedAdmin()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
        var testId = CreateTestId();
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var project = TestDataFactory.CreateProject(testId);
        
        // First create a project
        var canAccessProjects = await NavigateToProtectedPageAsync("/projects");
        Assert.True(canAccessProjects, "Should be able to access projects page");
        
        await WaitForBlazorAppAsync();
        await projectsPage.WaitForPageLoadAsync();
        await projectsPage.ClickCreateProjectAsync();
        await projectsPage.WaitForFormModalAsync();
        await projectsPage.FillProjectFormAsync(
            project.Name, 
            project.Code, 
            project.Description ?? "", 
            project.Status.ToString(), 
            project.OwnerId
        );
        await projectsPage.SaveProjectAsync();
        
        // Wait for creation to complete
        await Page.WaitForTimeoutAsync(3000);
        
        // Now edit the project
        await projectsPage.EditProjectAsync(project.Name);
        await projectsPage.WaitForFormModalAsync();
        
        // Verify form is populated with existing data
        var currentName = await projectsPage.GetProjectNameInputValueAsync();
        Assert.Equal(project.Name, currentName);
        
        // Update the project name
        var updatedName = $"Updated {project.Name}";
        await Page.FillAsync("[data-testid='name-input']", updatedName);
        await projectsPage.SaveProjectAsync();
        
        // Wait for update to complete
        await Page.WaitForTimeoutAsync(3000);
        
        // Assert
        var isUpdatedVisible = await projectsPage.IsProjectVisibleAsync(updatedName);
        Assert.True(isUpdatedVisible);
        
        _output.WriteLine($"Successfully edited project from {project.Name} to {updatedName}");
    }
    
    [Fact]
    public async Task Projects_CanDeleteProject_AuthenticatedAdmin()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
        var testId = CreateTestId();
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var project = TestDataFactory.CreateProject(testId);
        
        // First create a project
        var canAccessProjects = await NavigateToProtectedPageAsync("/projects");
        Assert.True(canAccessProjects, "Should be able to access projects page");
        
        await WaitForBlazorAppAsync();
        await projectsPage.WaitForPageLoadAsync();
        await projectsPage.ClickCreateProjectAsync();
        await projectsPage.WaitForFormModalAsync();
        await projectsPage.FillProjectFormAsync(
            project.Name, 
            project.Code, 
            project.Description ?? "", 
            project.Status.ToString(), 
            project.OwnerId
        );
        await projectsPage.SaveProjectAsync();
        
        // Wait for creation to complete
        await Page.WaitForTimeoutAsync(3000);
        
        // Set up dialog handler for confirmation
        Page.Dialog += async (_, dialog) =>
        {
            Assert.Equal("confirm", dialog.Type);
            await dialog.AcceptAsync();
        };
        
        // Delete the project
        await projectsPage.DeleteProjectAsync(project.Name);
        
        // Wait for deletion to complete
        await Page.WaitForTimeoutAsync(3000);
        
        // Assert
        var isDeletedVisible = await projectsPage.IsProjectVisibleAsync(project.Name);
        Assert.False(isDeletedVisible);
        
        _output.WriteLine($"Successfully deleted project: {project.Name}");
    }
    
    [Fact]
    public async Task Projects_CanPerformFullCrudWorkflow_AuthenticatedAdmin()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
        var testId = CreateTestId();
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var project = TestDataFactory.CreateProject(testId);
        
        // Act & Assert: Create
        var canAccessProjects = await NavigateToProtectedPageAsync("/projects");
        Assert.True(canAccessProjects, "Should be able to access projects page");
        
        await WaitForBlazorAppAsync();
        await projectsPage.WaitForPageLoadAsync();
        await projectsPage.ClickCreateProjectAsync();
        await projectsPage.WaitForFormModalAsync();
        await projectsPage.FillProjectFormAsync(
            project.Name, 
            project.Code, 
            project.Description ?? "", 
            project.Status.ToString(), 
            project.OwnerId
        );
        await projectsPage.SaveProjectAsync();
        
        // Wait for creation
        await Page.WaitForTimeoutAsync(3000);
        
        // Verify creation
        var isVisible = await projectsPage.IsProjectVisibleAsync(project.Name);
        Assert.True(isVisible, "Project should be visible after creation");
        
        // Act & Assert: Update
        await projectsPage.EditProjectAsync(project.Name);
        await projectsPage.WaitForFormModalAsync();
        var updatedName = $"Updated {project.Name}";
        await Page.FillAsync("[data-testid='name-input']", updatedName);
        await projectsPage.SaveProjectAsync();
        
        // Wait for update
        await Page.WaitForTimeoutAsync(3000);
        
        // Verify update
        var isUpdatedVisible = await projectsPage.IsProjectVisibleAsync(updatedName);
        Assert.True(isUpdatedVisible, "Updated project should be visible after edit");
        
        // Act & Assert: Delete
        Page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
        await projectsPage.DeleteProjectAsync(updatedName);
        
        // Wait for deletion
        await Page.WaitForTimeoutAsync(3000);
        
        // Verify deletion
        var isDeletedVisible = await projectsPage.IsProjectVisibleAsync(updatedName);
        Assert.False(isDeletedVisible, "Project should not be visible after deletion");
        
        _output.WriteLine($"Successfully completed full CRUD workflow for project: {project.Name}");
    }
    
    [Fact]
    public async Task Projects_ShowsProjectCounts_AuthenticatedUser()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login as admin");
        
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        
        // Act
        var canAccessProjects = await NavigateToProtectedPageAsync("/projects");
        Assert.True(canAccessProjects, "Should be able to access projects page");
        
        await WaitForBlazorAppAsync();
        await projectsPage.WaitForPageLoadAsync();
        
        // Assert
        // Check that project count elements are present
        await Expect(Page.Locator("text=/Showing \\d+ of \\d+ project/")).ToBeVisibleAsync();
        
        _output.WriteLine("Project counts are displayed correctly");
    }
}
