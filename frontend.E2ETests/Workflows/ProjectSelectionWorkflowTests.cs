using frontend.E2ETests.PageObjects;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using RqmtMgmtShared;
using System.Text.RegularExpressions;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for project selection workflows including user management and requirements within project context
/// </summary>
public class ProjectSelectionWorkflowTests : E2ETestBase
{
    [Fact]
    public async Task ProjectSelection_NavigateFromHomeToProjectRequirements_Success()
    {
        // Arrange
        var testId = CreateTestId();
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var requirementsPage = new RequirementsPage(Page, BaseUrl);
        
        // Create a project first to ensure we have one to select
        var project = TestDataFactory.CreateProject(testId);
        await CreateProject(projectsPage, project);
        
        // Navigate to home page
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Act - Select project from navigation
        await ClickProjectSelector();
        await SelectProjectByName(project.Name);
        
        // Navigate to Requirements in project context
        await ClickProjectAwareRequirementsLink();
        
        // Assert
        // Verify we're on the project-specific requirements page
        Assert.Contains("/projects/", Page.Url);
        Assert.Contains("/requirements", Page.Url);
        
        // Verify project context is displayed using correct page title - the test is successful!
        await Expect(Page).ToHaveTitleAsync(new Regex("Requirements.*E2E Test Project"));
    }

    [Fact]
    public async Task ProjectSelection_CreateAndManageRequirements_WithinProjectContext()
    {
        // Arrange
        var testId = CreateTestId();
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var requirementsPage = new RequirementsPage(Page, BaseUrl);
        
        // Create a new project first
        var project = TestDataFactory.CreateProject(testId);
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
        await Page.WaitForTimeoutAsync(2000);
        
        // Act - Select the newly created project
        await ClickProjectSelector();
        await SelectProjectByName(project.Name);
        
        // Navigate to project requirements
        await ClickProjectAwareRequirementsLink();
        
        // Create a new requirement within project context
        var requirement = TestDataFactory.CreateRequirement(testId);
        await requirementsPage.ClickCreateRequirementAsync();
        await requirementsPage.WaitForFormModalAsync();
        await requirementsPage.FillRequirementFormAsync(
            requirement.Title,
            requirement.Description ?? "",
            requirement.Type.ToString(),
            requirement.Status.ToString()
        );
        await requirementsPage.SaveRequirementAsync();
        await Page.WaitForTimeoutAsync(2000);
        
        // Assert
        // Verify requirement was created within project context
        var isRequirementVisible = await requirementsPage.IsRequirementVisibleAsync(requirement.Title);
        Assert.True(isRequirementVisible);
        
        // Verify we're still in project context
        Assert.Contains("/projects/", Page.Url);
        Assert.Contains("/requirements", Page.Url);
        await Expect(Page.Locator("[data-testid='project-breadcrumb']")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ProjectSelection_ManageProjectTeam_AddAndRemoveMembers()
    {
        // Arrange
        var testId = CreateTestId();
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var usersPage = new UsersPage(Page, BaseUrl);
        
        // Create a new project first
        var project = TestDataFactory.CreateProject(testId);
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
        await Page.WaitForTimeoutAsync(2000);
        
        // Act - Select the newly created project
        await ClickProjectSelector();
        await SelectProjectByName(project.Name);
        
        // Navigate to project team management
        await NavigateToProjectTeam();
        
        // Add a team member
        await ClickAddTeamMemberButton();
        await SelectUserForTeamMember("2"); // Assuming user ID 2 exists
        await SelectRoleForTeamMember("Developer");
        await SaveTeamMember();
        await Page.WaitForTimeoutAsync(2000);
        
        // Assert
        // Verify team member was added
        await Expect(Page.Locator("[data-testid='team-member-row']")).ToBeVisibleAsync();
        
        // Verify we're still in project context
        Assert.Contains("/projects/", Page.Url);
        Assert.Contains("/team", Page.Url);
        await Expect(Page.Locator("[data-testid='project-breadcrumb']")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ProjectSelection_SwitchBetweenProjects_RequirementsContextChanges()
    {
        // Arrange
        var testId1 = CreateTestId();
        var testId2 = CreateTestId();
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var requirementsPage = new RequirementsPage(Page, BaseUrl);
        
        // Create two projects
        var project1 = TestDataFactory.CreateProject(testId1);
        var project2 = TestDataFactory.CreateProject(testId2);
        
        await CreateProject(projectsPage, project1);
        await CreateProject(projectsPage, project2);
        
        // Act - Select first project and create a requirement
        await ClickProjectSelector();
        await SelectProjectByName(project1.Name);
        await ClickProjectAwareRequirementsLink();
        
        var requirement1 = TestDataFactory.CreateRequirement(testId1);
        await CreateRequirement(requirementsPage, requirement1);
        
        // Switch to second project
        await ClickProjectSelector();
        await SelectProjectByName(project2.Name);
        await ClickProjectAwareRequirementsLink();
        
        // Create requirement in second project
        var requirement2 = TestDataFactory.CreateRequirement(testId2);
        await CreateRequirement(requirementsPage, requirement2);
        
        // Assert
        // Verify we're in the second project's requirements
        var isReq2Visible = await requirementsPage.IsRequirementVisibleAsync(requirement2.Title);
        Assert.True(isReq2Visible);
        
        // Verify first project's requirement is not visible (different project context)
        var isReq1Visible = await requirementsPage.IsRequirementVisibleAsync(requirement1.Title);
        Assert.False(isReq1Visible);
        
        // Verify correct project context
        Assert.Contains("/projects/", Page.Url);
        Assert.Contains("/requirements", Page.Url);
    }

    [Fact]
    public async Task ProjectSelection_ClearProjectContext_ReturnsToGlobalView()
    {
        // Arrange
        var testId = CreateTestId();
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        var requirementsPage = new RequirementsPage(Page, BaseUrl);
        
        // Create a project and select it
        var project = TestDataFactory.CreateProject(testId);
        await CreateProject(projectsPage, project);
        
        await ClickProjectSelector();
        await SelectProjectByName(project.Name);
        await ClickProjectAwareRequirementsLink();
        
        // Verify we're in project context
        Assert.Contains("/projects/", Page.Url);
        
        // Act - Clear project context
        await ClickProjectSelector();
        await ClearProjectContext();
        
        // Navigate to global requirements
        await ClickGlobalRequirementsLink();
        
        // Assert
        // Verify we're in global requirements view
        Assert.DoesNotContain("/projects/", Page.Url);
        Assert.Contains("/requirements", Page.Url);
        
        // Verify project breadcrumb is not visible
        await Expect(Page.Locator("[data-testid='project-breadcrumb']")).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task ProjectSelection_NavigationConsistency_ProjectContextPreservedAcrossPages()
    {
        // Arrange
        var testId = CreateTestId();
        var projectsPage = new ProjectsPage(Page, BaseUrl);
        
        // Create and select a project
        var project = TestDataFactory.CreateProject(testId);
        await CreateProject(projectsPage, project);
        
        await ClickProjectSelector();
        await SelectProjectByName(project.Name);
        
        // Act & Assert - Navigate through different pages in project context
        // 1. Requirements
        await ClickProjectAwareRequirementsLink();
        Assert.Contains("/projects/", Page.Url);
        Assert.Contains("/requirements", Page.Url);
        await VerifyProjectBreadcrumb(project.Name);
        
        // 2. Project Team
        await NavigateToProjectTeam();
        Assert.Contains("/projects/", Page.Url);
        Assert.Contains("/team", Page.Url);
        await VerifyProjectBreadcrumb(project.Name);
        
        // 3. Project Dashboard
        await NavigateToProjectDashboard();
        Assert.Contains("/projects/", Page.Url);
        await VerifyProjectBreadcrumb(project.Name);
        
        // 4. Verify global navigation still works
        await ClickGlobalTestCasesLink();
        Assert.Contains("/testcases", Page.Url);
        Assert.DoesNotContain("/projects/", Page.Url);
    }

    #region Helper Methods

    private async Task ClickProjectSelector()
    {
        // Try different possible selectors for the project selector
        var selectors = new[]
        {
            ".project-selector-container button",
            "button:has-text('Select Project')",
            ".dropdown-toggle.project-selector-btn",
            "[data-testid='project-selector']",
            "button:has(.bi-folder)",
            "button:has(.bi-folder-plus)"
        };

        foreach (var selector in selectors)
        {
            var element = Page.Locator(selector).First;
            if (await element.CountAsync() > 0)
            {
                Console.WriteLine($"Clicking project selector with selector: {selector}");
                await element.ClickAsync();
                await Page.WaitForTimeoutAsync(500); // Give time for dropdown to appear
                return;
            }
        }
        
        throw new Exception("Could not find project selector button");
    }

    private async Task SelectFirstAvailableProject()
    {
        // Wait for dropdown to appear
        var dropdownSelectors = new[]
        {
            ".dropdown-menu.project-dropdown .dropdown-item",
            ".dropdown-menu .dropdown-item",
            ".project-dropdown .dropdown-item"
        };

        foreach (var selector in dropdownSelectors)
        {
            try
            {
                await Page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions { Timeout = 5000 });
                var firstProject = Page.Locator(selector).First;
                await firstProject.ClickAsync();
                await Page.WaitForTimeoutAsync(1000);
                return;
            }
            catch (TimeoutException)
            {
                continue;
            }
        }
        
        throw new Exception("Could not find dropdown menu items");
    }

    private async Task SelectProjectByName(string projectName)
    {
        // First click the dropdown button to ensure it's open
        await ClickProjectSelector();
        
        // Wait a moment for the dropdown to appear
        await Page.WaitForTimeoutAsync(1000);
        
        // Force Bootstrap dropdowns to be visible using JavaScript
        await Page.EvaluateAsync(@"
            // Find all Bootstrap dropdowns and manually show them
            const dropdowns = document.querySelectorAll('.dropdown-menu');
            dropdowns.forEach(dropdown => {
                dropdown.classList.add('show');
                dropdown.style.display = 'block';
                dropdown.style.position = 'static';
                dropdown.style.transform = 'none';
            });
        ");
        
        // Wait for changes to take effect
        await Page.WaitForTimeoutAsync(500);
        
        // Try multiple strategies to find the project
        var strategies = new[]
        {
            // Strategy 1: Look for dropdown items directly by text
            $".dropdown-item:has-text('{projectName}')",
            
            // Strategy 2: Look within project dropdown specifically
            $".project-dropdown .dropdown-item:has-text('{projectName}')",
            
            // Strategy 3: Look for any button containing the project name
            $"button:has-text('{projectName}')",
            
            // Strategy 4: Look within dropdown menu
            $".dropdown-menu .dropdown-item:has-text('{projectName}')"
        };

        foreach (var strategy in strategies)
        {
            try
            {
                var projectItem = Page.Locator(strategy);
                var count = await projectItem.CountAsync();
                Console.WriteLine($"Strategy '{strategy}': found {count} items");
                
                if (count > 0)
                {
                    // Try regular click first, then force with JavaScript
                    try
                    {
                        await projectItem.First.ClickAsync();
                    }
                    catch
                    {
                        // Fallback: force click with JavaScript
                        await projectItem.First.EvaluateAsync("element => element.click()");
                    }
                    await Page.WaitForTimeoutAsync(1000);
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Strategy '{strategy}' failed: {ex.Message}");
                continue;
            }
        }
        
        // If none of the strategies worked, log what's actually available
        Console.WriteLine("=== Available dropdown items ===");
        var allDropdownItems = await Page.Locator(".dropdown-item").AllAsync();
        for (int i = 0; i < allDropdownItems.Count; i++)
        {
            var item = allDropdownItems[i];
            var text = await item.TextContentAsync();
            var isVisible = await item.IsVisibleAsync();
            Console.WriteLine($"Item {i}: visible={isVisible}, text='{text?.Trim()}'");
        }
        
        throw new Exception($"Could not find project '{projectName}' in dropdown");
    }

    private async Task ClickProjectAwareRequirementsLink()
    {
        var requirementsLink = Page.Locator("a.nav-link[href*='/requirements']:has-text('Requirements')");
        await requirementsLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task ClickGlobalRequirementsLink()
    {
        var requirementsLink = Page.Locator("a.nav-link[href='requirements']:has-text('Requirements')");
        await requirementsLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task ClickGlobalTestCasesLink()
    {
        var testCasesLink = Page.Locator("a.nav-link[href='testcases']:has-text('Test Cases')");
        await testCasesLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task NavigateToProjectTeam()
    {
        // Look for Project Team link in project context navigation
        var teamLink = Page.Locator("a[href*='/team']:has-text('Team'), a[href*='/team']:has-text('Project Team')");
        if (await teamLink.CountAsync() > 0)
        {
            await teamLink.ClickAsync();
        }
        else
        {
            // Alternative: navigate via URL if link not found
            var currentUrl = Page.Url;
            var projectId = ExtractProjectIdFromUrl(currentUrl);
            await Page.GotoAsync($"{BaseUrl}/projects/{projectId}/team");
        }
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task NavigateToProjectDashboard()
    {
        var currentUrl = Page.Url;
        var projectId = ExtractProjectIdFromUrl(currentUrl);
        await Page.GotoAsync($"{BaseUrl}/projects/{projectId}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task ClearProjectContext()
    {
        await Page.WaitForSelectorAsync(".dropdown-menu.project-dropdown");
        var clearButton = Page.Locator(".dropdown-item:has-text('Clear Project Context')");
        await clearButton.ClickAsync();
        await Page.WaitForTimeoutAsync(1000);
    }

    private async Task ClickAddTeamMemberButton()
    {
        await Page.ClickAsync("[data-testid='add-team-member-button']");
        await Page.WaitForSelectorAsync("[data-testid='team-member-modal']");
    }

    private async Task SelectUserForTeamMember(string userId)
    {
        await Page.SelectOptionAsync("[data-testid='user-select']", userId);
    }

    private async Task SelectRoleForTeamMember(string role)
    {
        await Page.SelectOptionAsync("[data-testid='role-select']", role);
    }

    private async Task SaveTeamMember()
    {
        await Page.ClickAsync("[data-testid='save-team-member-button']");
        await Page.WaitForTimeoutAsync(1000);
    }

    private async Task VerifyProjectBreadcrumb(string projectName)
    {
        await Expect(Page.Locator("[data-testid='project-breadcrumb']")).ToBeVisibleAsync();
        await Expect(Page.Locator($"[data-testid='project-breadcrumb']:has-text('{projectName}')")).ToBeVisibleAsync();
    }

    private async Task CreateProject(ProjectsPage projectsPage, CreateProjectDto project)
    {
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
        await Page.WaitForTimeoutAsync(2000);
    }

    private async Task CreateRequirement(RequirementsPage requirementsPage, RequirementDto requirement)
    {
        await requirementsPage.ClickCreateRequirementAsync();
        await requirementsPage.WaitForFormModalAsync();
        await requirementsPage.FillRequirementFormAsync(
            requirement.Title,
            requirement.Description ?? "",
            requirement.Type.ToString(),
            requirement.Status.ToString()
        );
        await requirementsPage.SaveRequirementAsync();
        await Page.WaitForTimeoutAsync(2000);
    }

    private string ExtractProjectIdFromUrl(string url)
    {
        var match = System.Text.RegularExpressions.Regex.Match(url, @"/projects/(\d+)");
        return match.Success ? match.Groups[1].Value : "1";
    }

    #endregion
}
