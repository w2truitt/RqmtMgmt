using Microsoft.Playwright;
using Xunit;
using frontend.E2ETests.TestData;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Tests for project requirements filtering functionality
/// </summary>
public class ProjectRequirementsFilterTest : E2ETestBase
{
    /// <summary>
    /// Test that project requirements are shown with correct count when viewing specific project requirements
    /// </summary>
    [Fact]
    public async Task ProjectRequirements_ShouldShowCorrectCount_WhenProjectSelected()
    {
        // Get stable test project from factory (Legacy Requirements project)
        var testProject = TestDataFactory.GetStaticProject(0); // First static project
        
        // Navigate to the project requirements page
        await Page.GotoAsync($"{BaseUrl}/projects/{testProject.Id}/requirements");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for the project context header to be visible
        await Page.WaitForSelectorAsync(".project-context-header");

        // Check the page title contains "Requirements"
        var pageTitle = await Page.TitleAsync();
        Console.WriteLine($"Page title: {pageTitle}");
        Assert.Contains("Requirements", pageTitle);

        // Look for the requirements count in the UI
        var requirementsCountText = await Page.TextContentAsync(".project-context-header p");
        Console.WriteLine($"Requirements count text for {testProject.Name}: {requirementsCountText}");
        
        // The Legacy Requirements project should have 70 requirements based on our API verification
        Assert.Contains("70 requirement", requirementsCountText ?? "");

        // Test with a different project that has fewer requirements
        var secondProject = TestDataFactory.GetStaticProject(1); // Second static project
        await Page.GotoAsync($"{BaseUrl}/projects/{secondProject.Id}/requirements");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for the project context header to be visible
        await Page.WaitForSelectorAsync(".project-context-header");

        // Check that this shows a different count (validating the filter works)
        var secondCountText = await Page.TextContentAsync(".project-context-header p");
        Console.WriteLine($"Requirements count text for {secondProject.Name}: {secondCountText}");
        
        // Should show a different count than 70, proving the filter is working
        Assert.DoesNotContain("70 requirement", secondCountText ?? "");
    }
}
