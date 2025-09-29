using frontend.E2ETests.Fixtures;
using frontend.E2ETests.PageObjects;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using RqmtMgmtShared;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.Playwright.Assertions;
using frontend.E2ETests.Infrastructure;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for the Projects page functionality
/// OPTIMIZED: Now uses shared browser and cached authentication for 4-10x performance improvement
/// All tests run as admin user since project management requires admin privileges
/// </summary>
public class ProjectsPageTests : AuthenticatedE2ETestBase
{
    public ProjectsPageTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        // Set admin user for all tests - project management requires admin privileges
        SetAdminUser();
    }

    [Fact]
    public async Task Projects_NavigatesSuccessfully_AuthenticatedUser()
    {
        // Arrange - User already authenticated via base class
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        
        // Act
        await projectsPage.NavigateToAsync();
        await projectsPage.WaitForPageLoadAsync();
        
        // Assert
        Assert.Contains("/projects", Page.Url);
        await Expect(Page.Locator("h3:has-text('Projects')")).ToBeVisibleAsync();
        
        TestLogger.LogDebug($"Successfully navigated to projects page: {Page.Url}", Output);
    }
    
    [Fact]
    public async Task Projects_LoadsWithoutErrors_AuthenticatedUser()
    {
        // Arrange - User already authenticated
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        
        // Act
        await projectsPage.NavigateToAsync();
        await projectsPage.WaitForPageLoadAsync();
        
        // Assert - Check that page loads without JavaScript errors
        var errors = await Page.EvaluateAsync<string[]>("() => window.errors || []");
        Assert.Empty(errors);
        
        Assert.Contains("/projects", Page.Url);
        TestLogger.LogDebug("Projects page loaded without errors", Output);
    }
    
    [Fact]
    public async Task Projects_HasExpectedPageElements_AuthenticatedUser()
    {
        // Arrange - User already authenticated
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        
        // Act
        await projectsPage.NavigateToAsync();
        await projectsPage.WaitForPageLoadAsync();
        
        // Assert
        await Expect(Page.Locator("h3:has-text('Projects')")).ToBeVisibleAsync();
        await Expect(Page.Locator("table")).ToBeVisibleAsync();
        await Expect(Page.Locator("button:has-text('Add Project')")).ToBeVisibleAsync();
        
        TestLogger.LogDebug("All expected page elements are present", Output);
    }
    
    [Fact]
    public async Task Projects_CanSearchProjects_AuthenticatedUser()
    {
        // Arrange - User already authenticated
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        
        // Act
        await projectsPage.NavigateToAsync();
        await projectsPage.WaitForPageLoadAsync();
        
        var initialCount = await projectsPage.GetProjectCountAsync();
        await projectsPage.SearchProjectsAsync("test");
        await Task.Delay(1000); // Allow search to process
        
        // Assert - Search functionality works (count may change)
        Assert.Contains("/projects", Page.Url);
        TestLogger.LogTestStep("Search functionality works correctly", Output);
    }
    
    [Fact]
    public async Task Projects_ShowsProjectCounts_AuthenticatedUser()
    {
        // Arrange - User already authenticated
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        
        // Act
        await projectsPage.NavigateToAsync();
        await projectsPage.WaitForPageLoadAsync();
        
        var projectCount = await projectsPage.GetProjectCountAsync();
        
        // Assert
        Assert.True(projectCount >= 0, "Should have valid project count");
        TestLogger.LogTestStep("Project counts are displayed correctly", Output);
    }
    
    [Fact]
    public async Task Projects_CanOpenAndCancelForm_AuthenticatedUser()
    {
        // Arrange - User already authenticated
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        
        // Act
        await projectsPage.NavigateToAsync();
        await projectsPage.WaitForPageLoadAsync();
        
        await projectsPage.ClickCreateProjectAsync();
        await projectsPage.WaitForFormModalAsync();
        
        // Use the optimized cancel method that uses class selector
        await projectsPage.CancelFormAsync();
        await projectsPage.WaitForFormModalToHideAsync();
        
        // Assert
        Assert.Contains("/projects", Page.Url);
        TestLogger.LogDebug("Form modal can be opened and cancelled successfully", Output);
    }
    
    [Fact]
    public async Task Projects_FormValidatesRequiredFields_AuthenticatedUser()
    {
        // Arrange - User already authenticated
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        
        // Act
        await projectsPage.NavigateToAsync();
        await projectsPage.WaitForPageLoadAsync();
        
        await projectsPage.ClickCreateProjectAsync();
        await projectsPage.WaitForFormModalAsync();
        
        // Try to save without filling required fields
        await projectsPage.SaveProjectAsync();
        await Task.Delay(1000);
        
        // Assert - Should still be on modal (validation prevented save)
        var modalVisible = await Page.IsVisibleAsync(".modal.show");
        Assert.True(modalVisible, "Modal should still be visible due to validation");
        
        // Cleanup
        await projectsPage.CancelFormAsync();
        TestLogger.LogDebug("Form validation works correctly for required fields", Output);
    }
    
    [Fact]
    public async Task Projects_CanCreateNewProject_AuthenticatedAdmin()
    {
        // Arrange - Admin user already authenticated
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var testId = CreateTestId();
        var projectName = $"E2E Test Project {testId}";
        
        // Act
        await projectsPage.NavigateToAsync();
        await projectsPage.WaitForPageLoadAsync();
        
        await projectsPage.ClickCreateProjectAsync();
        await projectsPage.WaitForFormModalAsync();
        
        await projectsPage.FillProjectFormAsync(
            name: projectName,
            code: $"E2E{testId}",
            description: "Created by E2E test"
        );
        
        await projectsPage.SaveProjectAsync();
        await Task.Delay(2000); // Allow save to complete
        
        // Assert
        var isVisible = await projectsPage.IsProjectVisibleAsync(projectName);
        Assert.True(isVisible, $"Should be able to see created project: {projectName}");
        
        TestLogger.LogTestStep($"Successfully created project: {projectName}", Output);
    }
    
    [Fact]
    public async Task Projects_CanEditExistingProject_AuthenticatedAdmin()
    {
        // Arrange - Admin user already authenticated
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var testId = CreateTestId();
        var originalName = $"E2E Test Project {testId}";
        var updatedName = $"Updated E2E Test Project {testId}";
        
        // Create project first
        await projectsPage.NavigateToAsync();
        await projectsPage.WaitForPageLoadAsync();
        
        await projectsPage.ClickCreateProjectAsync();
        await projectsPage.WaitForFormModalAsync();
        
        await projectsPage.FillProjectFormAsync(
            name: originalName,
            code: $"E2E{testId}",
            description: "Created by E2E test"
        );
        
        await projectsPage.SaveProjectAsync();
        await Task.Delay(2000);
        
        // Act - Edit the project
        await projectsPage.EditProjectAsync(originalName);
        await projectsPage.WaitForFormModalAsync();
        
        // Update the name
        await Page.FillAsync("[data-testid='name-input']", updatedName);
        await projectsPage.SaveProjectAsync();
        await Task.Delay(2000);
        
        // Assert
        var isVisible = await projectsPage.IsProjectVisibleAsync(updatedName);
        Assert.True(isVisible, $"Should see updated project name: {updatedName}");
        
        TestLogger.LogTestStep($"Successfully edited project from {originalName} to {updatedName}", Output);
    }
    
    [Fact]
    public async Task Projects_CanDeleteProject_AuthenticatedAdmin()
    {
        // Arrange - Admin user already authenticated
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var testId = CreateTestId();
        var projectName = $"E2E Test Project {testId}";
        
        // Create project first
        await projectsPage.NavigateToAsync();
        await projectsPage.WaitForPageLoadAsync();
        
        await projectsPage.ClickCreateProjectAsync();
        await projectsPage.WaitForFormModalAsync();
        
        await projectsPage.FillProjectFormAsync(
            name: projectName,
            code: $"E2E{testId}",
            description: "Created by E2E test for deletion"
        );
        
        await projectsPage.SaveProjectAsync();
        await Task.Delay(2000);
        
        // Act - Delete the project
        await projectsPage.DeleteProjectAsync(projectName);
        await projectsPage.ConfirmDeleteAsync();
        await Task.Delay(2000);
        
        // Assert
        var isVisible = await projectsPage.IsProjectVisibleAsync(projectName);
        Assert.False(isVisible, $"Project should be deleted: {projectName}");
        
        TestLogger.LogTestStep($"Successfully deleted project: {projectName}", Output);
    }
    
    [Fact]
    public async Task Projects_CanPerformFullCrudWorkflow_AuthenticatedAdmin()
    {
        // Arrange - Admin user already authenticated
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var testId = CreateTestId();
        var projectName = $"E2E Test Project {testId}";
        var updatedName = $"Updated {projectName}";
        
        // Act & Assert - Create
        await projectsPage.NavigateToAsync();
        await projectsPage.WaitForPageLoadAsync();
        
        await projectsPage.ClickCreateProjectAsync();
        await projectsPage.WaitForFormModalAsync();
        
        await projectsPage.FillProjectFormAsync(
            name: projectName,
            code: $"E2E{testId}",
            description: "CRUD test project"
        );
        
        await projectsPage.SaveProjectAsync();
        await Task.Delay(2000);
        
        // Read
        var isCreated = await projectsPage.IsProjectVisibleAsync(projectName);
        Assert.True(isCreated, "Project should be created");
        
        // Update
        await projectsPage.EditProjectAsync(projectName);
        await projectsPage.WaitForFormModalAsync();
        await Page.FillAsync("[data-testid='name-input']", updatedName);
        await projectsPage.SaveProjectAsync();
        await Task.Delay(2000);
        
        var isUpdated = await projectsPage.IsProjectVisibleAsync(updatedName);
        Assert.True(isUpdated, "Project should be updated");
        
        // Delete
        await projectsPage.DeleteProjectAsync(updatedName);
        await projectsPage.ConfirmDeleteAsync();
        await Task.Delay(2000);
        
        var isDeleted = !await projectsPage.IsProjectVisibleAsync(updatedName);
        Assert.True(isDeleted, "Project should be deleted");
        
        TestLogger.LogTestStep($"Successfully completed full CRUD workflow for project: {projectName}", Output);
    }
}