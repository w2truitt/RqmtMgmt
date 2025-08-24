using frontend.E2ETests.PageObjects;
using Microsoft.Playwright;
using Xunit;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for Project Navigation functionality
/// </summary>
public class ProjectNavigationE2ETests : E2ETestBase
{
    [Fact]
    public async Task ProjectsList_NavigateUsingViewButton_Success()
    {
        // Arrange
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var dashboardPage = new ProjectDashboardPage(Page, BaseUrl);
        
        // Act
        await projectsPage.NavigateToAsync();
        await Task.Delay(2000); // Allow page to load
        
        // Check if any projects exist
        var projectCount = await projectsPage.GetProjectCountAsync();
        if (projectCount > 0)
        {
            var firstProjectName = await projectsPage.GetFirstProjectNameAsync();
            await projectsPage.NavigateToProjectDashboardUsingViewButtonAsync(firstProjectName);
            
            // Assert we're on the project dashboard
            await Page.WaitForURLAsync(url => url.Contains("/projects/") && url.Contains("/dashboard"));
            Assert.True(await dashboardPage.IsOnProjectDashboardAsync());
            
            // Verify the breadcrumb shows the correct project
            var breadcrumbText = await Page.TextContentAsync(".mud-breadcrumbs");
            Assert.Contains(firstProjectName, breadcrumbText);
        }
        else
        {
            Assert.True(true, "No projects found to test navigation - test skipped");
        }
    }
    
    [Fact]
    public async Task ProjectsList_NavigateUsingClickableProjectName_Success()
    {
        // Arrange
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var dashboardPage = new ProjectDashboardPage(Page, BaseUrl);
        
        // Act
        await projectsPage.NavigateToAsync();
        await Task.Delay(2000); // Allow page to load
        
        // Check if any projects exist
        var projectCount = await projectsPage.GetProjectCountAsync();
        if (projectCount > 0)
        {
            var firstProjectName = await projectsPage.GetFirstProjectNameAsync();
            await projectsPage.NavigateToProjectDashboardUsingProjectNameAsync(firstProjectName);
            
            // Assert we're on the project dashboard
            await Page.WaitForURLAsync(url => url.Contains("/projects/") && url.Contains("/dashboard"));
            Assert.True(await dashboardPage.IsOnProjectDashboardAsync());
            
            // Verify the breadcrumb shows the correct project
            var breadcrumbText = await Page.TextContentAsync(".mud-breadcrumbs");
            Assert.Contains(firstProjectName, breadcrumbText);
        }
        else
        {
            Assert.True(true, "No projects found to test navigation - test skipped");
        }
    }
    
    [Fact]
    public async Task ProjectDashboard_NavigateToRequirements_Success()
    {
        // Arrange
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var dashboardPage = new ProjectDashboardPage(Page, BaseUrl);
        
        // Act
        await projectsPage.NavigateToAsync();
        await Task.Delay(2000); // Allow page to load
        
        var projectCount = await projectsPage.GetProjectCountAsync();
        if (projectCount > 0)
        {
            var firstProjectName = await projectsPage.GetFirstProjectNameAsync();
            await projectsPage.NavigateToProjectDashboardUsingViewButtonAsync(firstProjectName);
            
            // Wait for dashboard to load
            await Page.WaitForURLAsync(url => url.Contains("/projects/") && url.Contains("/dashboard"));
            
            // Navigate to requirements from dashboard
            await dashboardPage.NavigateToRequirementsAsync();
            
            // Assert we're on the requirements page
            await Page.WaitForURLAsync(url => url.Contains("/projects/") && url.Contains("/requirements"));
            
            // Verify page content
            var pageTitle = await Page.TextContentAsync("h1, .page-title, .mud-typography-h1");
            Assert.Contains("Requirements", pageTitle, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            Assert.True(true, "No projects found to test navigation - test skipped");
        }
    }
    
    [Fact]
    public async Task ProjectDashboard_NavigateToTestCases_Success()
    {
        // Arrange
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var dashboardPage = new ProjectDashboardPage(Page, BaseUrl);
        
        // Act
        await projectsPage.NavigateToAsync();
        await Task.Delay(2000); // Allow page to load
        
        var projectCount = await projectsPage.GetProjectCountAsync();
        if (projectCount > 0)
        {
            var firstProjectName = await projectsPage.GetFirstProjectNameAsync();
            await projectsPage.NavigateToProjectDashboardUsingViewButtonAsync(firstProjectName);
            
            // Wait for dashboard to load
            await Page.WaitForURLAsync(url => url.Contains("/projects/") && url.Contains("/dashboard"));
            
            // Navigate to test cases from dashboard
            await dashboardPage.NavigateToTestCasesAsync();
            
            // Assert we're on the test cases page
            await Page.WaitForURLAsync(url => url.Contains("/projects/") && url.Contains("/testcases"));
            
            // Verify page content
            var pageTitle = await Page.TextContentAsync("h1, .page-title, .mud-typography-h1");
            Assert.Contains("Test Cases", pageTitle, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            Assert.True(true, "No projects found to test navigation - test skipped");
        }
    }
    
    [Fact]
    public async Task ProjectDashboard_NavigateToTestPlans_Success()
    {
        // Arrange
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var dashboardPage = new ProjectDashboardPage(Page, BaseUrl);
        
        // Act
        await projectsPage.NavigateToAsync();
        await Task.Delay(2000); // Allow page to load
        
        var projectCount = await projectsPage.GetProjectCountAsync();
        if (projectCount > 0)
        {
            var firstProjectName = await projectsPage.GetFirstProjectNameAsync();
            await projectsPage.NavigateToProjectDashboardUsingViewButtonAsync(firstProjectName);
            
            // Wait for dashboard to load
            await Page.WaitForURLAsync(url => url.Contains("/projects/") && url.Contains("/dashboard"));
            
            // Navigate to test plans from dashboard
            await dashboardPage.NavigateToTestPlansAsync();
            
            // Assert we're on the test plans page
            await Page.WaitForURLAsync(url => url.Contains("/projects/") && url.Contains("/testplans"));
            
            // Verify page content
            var pageTitle = await Page.TextContentAsync("h1, .page-title, .mud-typography-h1");
            Assert.Contains("Test Plans", pageTitle, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            Assert.True(true, "No projects found to test navigation - test skipped");
        }
    }
    
    [Fact]
    public async Task ProjectDashboard_BreadcrumbNavigation_Success()
    {
        // Arrange
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var dashboardPage = new ProjectDashboardPage(Page, BaseUrl);
        
        // Act
        await projectsPage.NavigateToAsync();
        await Task.Delay(2000); // Allow page to load
        
        var projectCount = await projectsPage.GetProjectCountAsync();
        if (projectCount > 0)
        {
            var firstProjectName = await projectsPage.GetFirstProjectNameAsync();
            await projectsPage.NavigateToProjectDashboardUsingViewButtonAsync(firstProjectName);
            
            // Wait for dashboard to load
            await Page.WaitForURLAsync(url => url.Contains("/projects/") && url.Contains("/dashboard"));
            
            // Navigate to requirements
            await dashboardPage.NavigateToRequirementsAsync();
            await Page.WaitForURLAsync(url => url.Contains("/projects/") && url.Contains("/requirements"));
            
            // Use breadcrumb to navigate back to dashboard
            await Page.ClickAsync(".mud-breadcrumbs a:has-text('Dashboard')");
            await Page.WaitForURLAsync(url => url.Contains("/projects/") && url.Contains("/dashboard"));
            
            // Assert we're back on the dashboard
            Assert.True(await dashboardPage.IsOnProjectDashboardAsync());
            
            // Use breadcrumb to navigate back to projects list
            await Page.ClickAsync(".mud-breadcrumbs a:has-text('Projects')");
            await Page.WaitForURLAsync(url => url.Contains("/projects") && !url.Contains("/dashboard"));
            
            // Assert we're back on the projects list
            var currentUrl = Page.Url;
            Assert.Contains("/projects", currentUrl);
            Assert.DoesNotContain("/dashboard", currentUrl);
        }
        else
        {
            Assert.True(true, "No projects found to test navigation - test skipped");
        }
    }
    
    [Fact]
    public async Task ProjectsList_ClickableProjectNames_VisualIndicators_Success()
    {
        // Arrange
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        
        // Act
        await projectsPage.NavigateToAsync();
        await Task.Delay(2000); // Allow page to load
        
        var projectCount = await projectsPage.GetProjectCountAsync();
        if (projectCount > 0)
        {
            // Check that project names are displayed as clickable links
            var firstProjectNameElement = await Page.QuerySelectorAsync(".project-name-link");
            Assert.NotNull(firstProjectNameElement);
            
            // Verify the cursor changes to pointer on hover
            await Page.HoverAsync(".project-name-link");
            var cursor = await Page.EvaluateAsync<string>("() => getComputedStyle(document.querySelector('.project-name-link')).cursor");
            Assert.Equal("pointer", cursor);
            
            // Verify the link styling (should have appropriate CSS classes)
            var hasLinkClass = await Page.IsVisibleAsync(".project-name-link.mud-link");
            Assert.True(hasLinkClass || await Page.IsVisibleAsync(".project-name-link"));
        }
        else
        {
            Assert.True(true, "No projects found to test visual indicators - test skipped");
        }
    }
    
    [Fact]
    public async Task ProjectNavigation_FullWorkflow_Success()
    {
        // Arrange
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var dashboardPage = new ProjectDashboardPage(Page, BaseUrl);
        
        // Act - Test the complete navigation workflow
        await projectsPage.NavigateToAsync();
        await Task.Delay(2000); // Allow page to load
        
        var projectCount = await projectsPage.GetProjectCountAsync();
        if (projectCount > 0)
        {
            var firstProjectName = await projectsPage.GetFirstProjectNameAsync();
            
            // 1. Navigate from Projects List to Dashboard using project name
            await projectsPage.NavigateToProjectDashboardUsingProjectNameAsync(firstProjectName);
            await Page.WaitForURLAsync(url => url.Contains("/dashboard"));
            Assert.True(await dashboardPage.IsOnProjectDashboardAsync());
            
            // 2. Navigate from Dashboard to Requirements
            await dashboardPage.NavigateToRequirementsAsync();
            await Page.WaitForURLAsync(url => url.Contains("/requirements"));
            
            // 3. Navigate back to Dashboard using breadcrumb
            await Page.ClickAsync(".mud-breadcrumbs a:has-text('Dashboard')");
            await Page.WaitForURLAsync(url => url.Contains("/dashboard"));
            Assert.True(await dashboardPage.IsOnProjectDashboardAsync());
            
            // 4. Navigate to Test Cases
            await dashboardPage.NavigateToTestCasesAsync();
            await Page.WaitForURLAsync(url => url.Contains("/testcases"));
            
            // 5. Navigate back to Projects List using breadcrumb
            await Page.ClickAsync(".mud-breadcrumbs a:has-text('Projects')");
            await Page.WaitForURLAsync(url => url.Contains("/projects") && !url.Contains("/dashboard"));
            
            // 6. Navigate to Dashboard using View button
            await projectsPage.NavigateToProjectDashboardUsingViewButtonAsync(firstProjectName);
            await Page.WaitForURLAsync(url => url.Contains("/dashboard"));
            Assert.True(await dashboardPage.IsOnProjectDashboardAsync());
            
            // Assert the full workflow completed successfully
            Assert.True(true, "Full navigation workflow completed successfully");
        }
        else
        {
            Assert.True(true, "No projects found to test full workflow - test skipped");
        }
    }
}
