using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Example tests demonstrating authenticated E2E testing workflows.
/// These tests show how to use the AuthenticatedE2ETestBase for testing user scenarios.
/// </summary>
public class AuthenticatedWorkflowTests : AuthenticatedE2ETestBase
{
    public AuthenticatedWorkflowTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task AdminCanAccessProjectsPage()
    {
        // Arrange & Act
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Admin should be able to login");

        // Verify we can access the projects page
        var canAccessProjects = await NavigateToProtectedPageAsync("/projects");
        Assert.True(canAccessProjects, "Admin should be able to access projects page");

        // Wait for Blazor to load
        await WaitForBlazorAppAsync();

        // Verify page content
        var pageTitle = await Page.TitleAsync();
        _output.WriteLine($"Page title: {pageTitle}");
        
        // Should be able to see projects-related content
        var hasProjectsContent = await Page.IsVisibleAsync("h1:has-text('Projects')") || 
                                await Page.IsVisibleAsync("h3:has-text('Projects')") ||
                                await Page.GetByText("Projects").IsVisibleAsync();
        
        Assert.True(hasProjectsContent, "Projects page should display projects content");
    }

    [Fact]
    public async Task TesterCanAccessTestPlansPage()
    {
        // Arrange & Act
        var loginSuccess = await LoginAsTesterAsync();
        Assert.True(loginSuccess, "Tester should be able to login");

        // Verify we can access the test plans page
        var canAccessTestPlans = await NavigateToProtectedPageAsync("/testplans");
        Assert.True(canAccessTestPlans, "Tester should be able to access test plans page");

        // Wait for Blazor to load
        await WaitForBlazorAppAsync();

        // Verify we're on the correct page
        var currentUrl = Page.Url;
        Assert.Contains("/testplans", currentUrl);
    }

    [Fact]
    public async Task ViewerCanAccessRequirementsPage()
    {
        // Arrange & Act
        var loginSuccess = await LoginAsViewerAsync();
        Assert.True(loginSuccess, "Viewer should be able to login");

        // Verify we can access the requirements page
        var canAccessRequirements = await NavigateToProtectedPageAsync("/requirements");
        Assert.True(canAccessRequirements, "Viewer should be able to access requirements page");

        // Wait for Blazor to load
        await WaitForBlazorAppAsync();

        // Verify we're on the correct page
        var currentUrl = Page.Url;
        Assert.Contains("/requirements", currentUrl);
    }

    [Fact]
    public async Task UserCanLogoutSuccessfully()
    {
        // Arrange - Login first
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Should be able to login");

        // Act - Logout
        await LogoutAsync();

        // Verify - Try to access a protected page and ensure we get redirected to login
        await Page.GotoAsync($"{BaseUrl}/projects");
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle, new Microsoft.Playwright.PageWaitForLoadStateOptions { Timeout = 10000 });
        await Task.Delay(3000);

        var currentUrl = Page.Url;
        Assert.Contains("/Account/Login", currentUrl);
        _output.WriteLine($"After logout, accessing protected page redirected to: {currentUrl}");
    }

    [Fact]
    public async Task MultipleUserRolesCanAccessDashboard()
    {
        // Test different user roles accessing the dashboard
        var users = new (string RoleName, Func<Task<bool>> LoginMethod)[]
        {
            ("Admin", () => LoginAsAdminAsync()),
            ("Project Manager", () => LoginAsProjectManagerAsync()),
            ("Developer", () => LoginAsDeveloperAsync()),
            ("Tester", () => LoginAsTesterAsync()),
            ("Viewer", () => LoginAsViewerAsync())
        };

        foreach (var (roleName, loginMethod) in users)
        {
            _output.WriteLine($"Testing dashboard access for {roleName}");

            // Login as the user
            var loginSuccess = await loginMethod();
            Assert.True(loginSuccess, $"{roleName} should be able to login");

            // Navigate to dashboard (home page)
            var canAccessDashboard = await NavigateToProtectedPageAsync("/");
            Assert.True(canAccessDashboard, $"{roleName} should be able to access dashboard");

            // Wait for Blazor to load
            await WaitForBlazorAppAsync();

            // Verify we're on a valid page (not login)
            var currentUrl = Page.Url;
            Assert.DoesNotContain("/Account/Login", currentUrl);

            _output.WriteLine($"{roleName} successfully accessed dashboard at: {currentUrl}");

            // Logout before testing next user
            await LogoutAsync();
        }
    }
}
