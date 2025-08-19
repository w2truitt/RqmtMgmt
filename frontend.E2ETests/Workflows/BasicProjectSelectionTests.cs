using frontend.E2ETests.PageObjects;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using RqmtMgmtShared;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Simple E2E tests for basic project selection workflows
/// </summary>
public class BasicProjectSelectionTests : E2ETestBase
{
    [Fact]
    public async Task ProjectSelection_CanNavigateToProjectsPage_Success()
    {
        // Arrange & Act
        await Page.GotoAsync($"{BaseUrl}/projects");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert
        Assert.Contains("/projects", Page.Url);
        await Expect(Page.Locator("h3:has-text('Projects')")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ProjectSelection_CanAccessProjectRequirementsDirectly_Success()
    {
        // Arrange
        var projectId = 1; // Use a known project ID
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/projects/{projectId}/requirements");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert
        Assert.Contains($"/projects/{projectId}/requirements", Page.Url);
        
        // Check if we get content or are redirected to login/projects
        var hasRequirementsHeader = await Page.Locator("h3:has-text('Requirements')").CountAsync() > 0;
        var hasProjectsHeader = await Page.Locator("h3:has-text('Projects')").CountAsync() > 0;
        var hasLoginForm = await Page.Locator("form, input[type='password']").CountAsync() > 0;
        
        // Should have either requirements page or be redirected to projects/login
        Assert.True(hasRequirementsHeader || hasProjectsHeader || hasLoginForm);
    }

    [Fact]
    public async Task ProjectSelection_ProjectSelectorExists_Success()
    {
        // Arrange & Act
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - Check that project selector exists in navigation
        var projectSelectorExists = await Page.Locator("button:has-text('Select Project')").CountAsync() > 0 ||
                                   await Page.Locator(".project-selector-container").CountAsync() > 0 ||
                                   await Page.Locator("button:has(.bi-folder)").CountAsync() > 0;
        
        Assert.True(projectSelectorExists, "Project selector should be visible in navigation");
    }

    [Fact]
    public async Task ProjectSelection_RequirementsLinkExists_Success()
    {
        // Arrange & Act
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - Check that requirements link exists in navigation
        var requirementsLinkExists = await Page.Locator("a.nav-link:has-text('Requirements')").CountAsync() > 0;
        
        Assert.True(requirementsLinkExists, "Requirements link should be visible in navigation");
    }

    [Fact]
    public async Task ProjectSelection_GlobalRequirementsAccessible_Success()
    {
        // Arrange & Act
        await Page.GotoAsync($"{BaseUrl}/requirements");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert
        Assert.Contains("/requirements", Page.Url);
        Assert.DoesNotContain("/projects/", Page.Url); // Should be global requirements, not project-specific
        
        // Should have requirements content
        var hasRequirementsContent = await Page.Locator("h1:has-text('Requirements'), h3:has-text('Requirements')").CountAsync() > 0;
        Assert.True(hasRequirementsContent, "Should display requirements page content");
    }

    [Fact]
    public async Task ProjectSelection_CreateProjectAndVerifyInList_Success()
    {
        // Arrange
        var testId = CreateTestId();
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var project = TestDataFactory.CreateProject(testId);
        
        // Act
        await projectsPage.NavigateToAsync();
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
        await Page.WaitForTimeoutAsync(3000); // Wait for project to be created
        
        // Assert
        var isProjectVisible = await projectsPage.IsProjectVisibleAsync(project.Name);
        Assert.True(isProjectVisible, $"Project '{project.Name}' should be visible in the projects list");
    }

    [Fact]
    public async Task ProjectSelection_NavigationLinksWork_Success()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Act & Assert - Test various navigation links
        
        // Test Projects link
        var projectsLink = Page.Locator("a.nav-link:has-text('Projects')");
        if (await projectsLink.CountAsync() > 0)
        {
            await projectsLink.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            Assert.Contains("/projects", Page.Url);
        }
        
        // Test Requirements link
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var requirementsLink = Page.Locator("a.nav-link:has-text('Requirements')");
        if (await requirementsLink.CountAsync() > 0)
        {
            await requirementsLink.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            Assert.Contains("/requirements", Page.Url);
        }
        
        // Test Home link
        var homeLink = Page.Locator("a.nav-link[href='/'], a[href='/']:has-text('Home')");
        if (await homeLink.CountAsync() > 0)
        {
            await homeLink.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            Assert.Equal($"{BaseUrl}/", Page.Url);
        }
    }
}
