using frontend.E2ETests.Fixtures;
using frontend.E2ETests.Infrastructure;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Diagnostic test to investigate the projects page UI behavior and pagination
/// </summary>
public class ProjectsPageUIDiagnosticTests : AuthenticatedE2ETestBase
{
    public ProjectsPageUIDiagnosticTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        SetAdminUser();
    }

    [Fact]
    public async Task DiagnoseProjectsPageUI_PaginationAndEditBehavior()
    {
        TestLogger.LogTestStep("=== PROJECTS PAGE UI DIAGNOSTIC ===", Output);
        
        // Navigate to projects page
        await Page.GotoAsync($"{BaseUrl}/projects");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        TestLogger.LogTestStep("Analyzing projects page structure...", Output);
        
        // Take a screenshot for visual reference
        await Page.ScreenshotAsync(new PageScreenshotOptions 
        { 
            Path = "/tmp/projects-page-diagnostic.png",
            FullPage = true 
        });
        TestLogger.LogDebug("Screenshot saved to /tmp/projects-page-diagnostic.png", Output);
        
        // 1. Check for pagination elements
        TestLogger.LogTestStep("--- PAGINATION ANALYSIS ---", Output);
        
        var paginationSelectors = new[]
        {
            ".pagination",
            ".page-link",
            ".page-item",
            "[aria-label*='pagination']",
            "[aria-label*='page']",
            "button:has-text('Next')",
            "button:has-text('Previous')",
            "a:has-text('Next')",
            "a:has-text('Previous')",
            ".btn:has-text('Next')",
            ".btn:has-text('Previous')",
            "[data-testid*='page']",
            "[data-testid*='pagination']"
        };
        
        bool hasPagination = false;
        foreach (var selector in paginationSelectors)
        {
            var count = await Page.Locator(selector).CountAsync();
            if (count > 0)
            {
                TestLogger.LogDebug($"Found pagination element: {selector} (count: {count})", Output);
                hasPagination = true;
            }
        }
        
        if (!hasPagination)
        {
            TestLogger.LogTestStep("❌ NO PAGINATION ELEMENTS FOUND", Output);
        }
        else
        {
            TestLogger.LogTestStep("✅ PAGINATION ELEMENTS DETECTED", Output);
        }
        
        // 2. Count total projects visible
        TestLogger.LogTestStep("--- PROJECT COUNT ANALYSIS ---", Output);
        
        var projectSelectors = new[]
        {
            "table tbody tr",
            ".project-item",
            ".project-card",
            "[data-testid='project-row']",
            "tr:has(td)" // Generic table rows with cells
        };
        
        int totalProjectsVisible = 0;
        string workingSelector = "";
        
        foreach (var selector in projectSelectors)
        {
            var count = await Page.Locator(selector).CountAsync();
            if (count > totalProjectsVisible)
            {
                totalProjectsVisible = count;
                workingSelector = selector;
            }
        }
        
        TestLogger.LogTestStep($"Total projects visible: {totalProjectsVisible} (using selector: {workingSelector})", Output);
        
        // 3. List all visible project names
        TestLogger.LogTestStep("--- VISIBLE PROJECTS ---", Output);
        
        if (totalProjectsVisible > 0)
        {
            var projectRows = await Page.Locator(workingSelector).AllAsync();
            for (int i = 0; i < Math.Min(projectRows.Count, 10); i++) // Limit to first 10
            {
                var rowText = await projectRows[i].TextContentAsync();
                TestLogger.LogDebug($"Project {i + 1}: {rowText?.Trim()}", Output);
            }
            
            if (projectRows.Count > 10)
            {
                TestLogger.LogDebug($"... and {projectRows.Count - 10} more projects", Output);
            }
        }
        
        // 4. Check for search functionality
        TestLogger.LogTestStep("--- SEARCH FUNCTIONALITY ---", Output);
        
        var searchSelectors = new[]
        {
            "[data-testid='search-input']",
            "input[placeholder*='search']",
            "input[placeholder*='Search']",
            "input[type='search']",
            ".search-input",
            "input[name*='search']"
        };
        
        bool hasSearch = false;
        string searchSelector = "";
        
        foreach (var selector in searchSelectors)
        {
            if (await Page.IsVisibleAsync(selector))
            {
                hasSearch = true;
                searchSelector = selector;
                TestLogger.LogDebug($"Found search input: {selector}", Output);
                break;
            }
        }
        
        if (!hasSearch)
        {
            TestLogger.LogTestStep("❌ NO SEARCH FUNCTIONALITY FOUND", Output);
        }
        else
        {
            TestLogger.LogTestStep($"✅ SEARCH FUNCTIONALITY FOUND: {searchSelector}", Output);
        }
        
        // 5. Test project creation and immediate visibility
        TestLogger.LogTestStep("--- PROJECT CREATION & VISIBILITY TEST ---", Output);
        
        var testId = CreateTestId();
        var projectName = $"UI Diagnostic Test Project {testId}";
        
        // Create a project
        TestLogger.LogTestStep($"Creating test project: {projectName}", Output);
        
        await Page.ClickAsync("button:has-text('Add Project'), button:has-text('Create Project'), button:has-text('New Project')");
        await Page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { Timeout = 5000 });
        
        // Fill form
        await Page.FillAsync("[data-testid='name-input'], input[name*='name'], input[placeholder*='name']", projectName);
        await Page.FillAsync("[data-testid='code-input'], input[name*='code'], input[placeholder*='code']", $"DIAG{testId}");
        await Page.FillAsync("[data-testid='description-input'], textarea[name*='description'], textarea[placeholder*='description']", "UI diagnostic test project");
        
        // Save
        await Page.ClickAsync("button:has-text('Save'), button:has-text('Create'), [data-testid='save-button']");
        
        // Wait for modal to close
        await Page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = 10000 });
        
        TestLogger.LogTestStep("Project created, analyzing visibility...", Output);
        
        // 6. Check immediate visibility
        await Task.Delay(1000); // Brief wait
        
        var isImmediatelyVisible = await Page.IsVisibleAsync($"text={projectName}");
        TestLogger.LogTestStep($"Immediately visible after creation: {isImmediatelyVisible}", Output);
        
        if (!isImmediatelyVisible)
        {
            TestLogger.LogTestStep("Project not immediately visible, checking for pagination/search needs...", Output);
            
            // Check if we need to search
            if (hasSearch)
            {
                TestLogger.LogTestStep("Trying search to find the project...", Output);
                await Page.FillAsync(searchSelector, projectName);
                await Page.PressAsync(searchSelector, "Enter");
                await Task.Delay(2000);
                
                var visibleAfterSearch = await Page.IsVisibleAsync($"text={projectName}");
                TestLogger.LogTestStep($"Visible after search: {visibleAfterSearch}", Output);
            }
            
            // Check if we need to navigate pages
            if (hasPagination)
            {
                TestLogger.LogTestStep("Checking pagination for the project...", Output);
                
                // Try to find "Next" button and navigate
                var nextButton = Page.Locator("button:has-text('Next'), a:has-text('Next'), .page-link:has-text('Next')");
                var nextCount = await nextButton.CountAsync();
                
                if (nextCount > 0)
                {
                    TestLogger.LogTestStep("Found Next button, checking subsequent pages...", Output);
                    
                    for (int pageNum = 2; pageNum <= 5; pageNum++) // Check up to 5 pages
                    {
                        try
                        {
                            await nextButton.First.ClickAsync();
                            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                            await Task.Delay(1000);
                            
                            var visibleOnPage = await Page.IsVisibleAsync($"text={projectName}");
                            TestLogger.LogTestStep($"Page {pageNum}: Project visible = {visibleOnPage}", Output);
                            
                            if (visibleOnPage)
                            {
                                break;
                            }
                            
                            // Check if Next button is still available
                            var stillHasNext = await nextButton.CountAsync() > 0 && await nextButton.First.IsEnabledAsync();
                            if (!stillHasNext)
                            {
                                TestLogger.LogTestStep("No more pages available", Output);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            TestLogger.LogDebug($"Error navigating to page {pageNum}: {ex.Message}", Output);
                            break;
                        }
                    }
                }
            }
        }
        
        // 7. Test project editing behavior
        TestLogger.LogTestStep("--- PROJECT EDIT BEHAVIOR TEST ---", Output);
        
        // First, make sure we can see the project (search for it if needed)
        if (hasSearch && !await Page.IsVisibleAsync($"text={projectName}"))
        {
            await Page.FillAsync(searchSelector, projectName);
            await Page.PressAsync(searchSelector, "Enter");
            await Task.Delay(2000);
        }
        
        var isVisibleForEdit = await Page.IsVisibleAsync($"text={projectName}");
        TestLogger.LogTestStep($"Project visible for editing: {isVisibleForEdit}", Output);
        
        if (isVisibleForEdit)
        {
            var updatedName = $"UPDATED {projectName}";
            
            // Find and click edit button
            var editSelectors = new[]
            {
                "button[title*='Edit']",
                ".btn:has(i.bi-pencil)",
                "[data-testid='edit-project']",
                "button:has-text('Edit')"
            };
            
            bool editClicked = false;
            foreach (var editSelector in editSelectors)
            {
                if (await Page.IsVisibleAsync(editSelector))
                {
                    await Page.ClickAsync(editSelector);
                    editClicked = true;
                    TestLogger.LogDebug($"Clicked edit using selector: {editSelector}", Output);
                    break;
                }
            }
            
            if (editClicked)
            {
                await Page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { Timeout = 5000 });
                
                // Update the name
                await Page.FillAsync("[data-testid='name-input'], input[name*='name'], input[placeholder*='name']", updatedName);
                
                // Save
                await Page.ClickAsync("button:has-text('Save'), button:has-text('Update'), [data-testid='save-button']");
                
                // Wait for modal to close
                await Page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = 10000 });
                
                TestLogger.LogTestStep("Edit completed, checking visibility of updated name...", Output);
                
                // Check visibility at different intervals
                var intervals = new[] { 500, 1000, 2000, 3000, 5000 };
                bool foundUpdated = false;
                
                foreach (var interval in intervals)
                {
                    await Task.Delay(interval);
                    var visible = await Page.IsVisibleAsync($"text={updatedName}");
                    TestLogger.LogTestStep($"After {interval}ms: Updated name visible = {visible}", Output);
                    
                    if (visible)
                    {
                        foundUpdated = true;
                        break;
                    }
                    
                    // Also check if we need to refresh the search
                    if (hasSearch && interval >= 2000)
                    {
                        TestLogger.LogDebug("Trying search refresh...", Output);
                        await Page.FillAsync(searchSelector, updatedName);
                        await Page.PressAsync(searchSelector, "Enter");
                        await Task.Delay(1000);
                        
                        var visibleAfterSearch = await Page.IsVisibleAsync($"text={updatedName}");
                        TestLogger.LogTestStep($"After search refresh: Updated name visible = {visibleAfterSearch}", Output);
                        
                        if (visibleAfterSearch)
                        {
                            foundUpdated = true;
                            break;
                        }
                    }
                }
                
                if (!foundUpdated)
                {
                    TestLogger.LogTestStep("❌ UPDATED PROJECT NAME NOT FOUND - This confirms the race condition issue", Output);
                    
                    // Take another screenshot to see current state
                    await Page.ScreenshotAsync(new PageScreenshotOptions 
                    { 
                        Path = "/tmp/projects-page-after-edit.png",
                        FullPage = true 
                    });
                    TestLogger.LogDebug("Post-edit screenshot saved to /tmp/projects-page-after-edit.png", Output);
                }
                else
                {
                    TestLogger.LogTestStep("✅ UPDATED PROJECT NAME FOUND", Output);
                }
            }
            else
            {
                TestLogger.LogTestStep("❌ Could not find edit button", Output);
            }
        }
        
        TestLogger.LogTestStep("=== DIAGNOSTIC COMPLETE ===", Output);
    }
}