using frontend.E2ETests.Fixtures;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Analysis test to understand project dropdown behavior
/// </summary>
public class ProjectDropdownAnalysisTest : AuthenticatedE2ETestBase
{
    public ProjectDropdownAnalysisTest(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        // Set project manager user
        SetProjectManagerUser();
    }

    [Fact]
    public async Task AnalyzeProjectDropdownContents()
    {
        // Navigate to home page
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        Output.WriteLine($"Navigated to: {Page.Url}");
        
        // Find and click the project selector
        var projectSelectorButton = await Page.QuerySelectorAsync("button:has-text('Select Project'), button:has(.bi-folder), .project-selector-btn");
        if (projectSelectorButton == null)
        {
            Output.WriteLine("No project selector button found");
            return;
        }
        
        Output.WriteLine("Found project selector button");
        await projectSelectorButton.ClickAsync();
        await Page.WaitForTimeoutAsync(1000);
        
        // Force Bootstrap dropdowns to be visible
        await Page.EvaluateAsync(@"
            const dropdowns = document.querySelectorAll('.dropdown-menu');
            dropdowns.forEach(dropdown => {
                dropdown.classList.add('show');
                dropdown.style.display = 'block';
                dropdown.style.position = 'static';
                dropdown.style.transform = 'none';
            });
        ");
        
        await Page.WaitForTimeoutAsync(500);
        
        // Get all dropdown items
        var dropdownItems = await Page.QuerySelectorAllAsync(".dropdown-item");
        Output.WriteLine($"Found {dropdownItems.Count} dropdown items");
        
        // Extract project names
        var projectNames = new List<string>();
        for (int i = 0; i < dropdownItems.Count; i++)
        {
            var text = await dropdownItems[i].TextContentAsync();
            if (!string.IsNullOrWhiteSpace(text) && 
                !text.Contains("View All Projects") && 
                !text.Contains("Clear Project Context") &&
                !text.Contains("Select Project") &&
                !text.Contains("Switch Project"))
            {
                projectNames.Add(text.Trim());
                Output.WriteLine($"Project {i}: {text.Trim()}");
            }
        }
        
        Output.WriteLine($"Total project names found: {projectNames.Count}");
        
        // Look specifically for our test project
        var targetProject = "E2E Test Project 3625e50c";
        var found = projectNames.Any(p => p.Contains(targetProject));
        Output.WriteLine($"Target project '{targetProject}' found: {found}");
        
        if (!found)
        {
            Output.WriteLine("Looking for partial matches:");
            var partialMatches = projectNames.Where(p => p.Contains("E2E Test Project") || p.Contains("3625e50c")).ToList();
            foreach (var match in partialMatches)
            {
                Output.WriteLine($"  Partial match: {match}");
            }
        }
    }
}
