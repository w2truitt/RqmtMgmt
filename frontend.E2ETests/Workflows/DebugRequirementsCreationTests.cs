using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Debug test to investigate Requirements creation with user identity
/// </summary>
public class DebugRequirementsCreationTests : AuthenticatedE2ETestBase
{
    public DebugRequirementsCreationTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task Debug_RequirementsCreation_InvestigateUserIdentityIssues()
    {
        // Arrange - Login as project manager to create requirements
        var loginSuccess = await LoginAsProjectManagerAsync();
        Assert.True(loginSuccess, "Failed to login as project manager");
        
        _output.WriteLine("=== DEBUGGING REQUIREMENTS CREATION WITH USER IDENTITY ===");
        
        // Navigate to project requirements page
        await Page.GotoAsync($"{BaseUrl}/projects/1/requirements");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(3000);
        
        _output.WriteLine($"Current URL: {Page.Url}");
        
        // Check if we can access the requirements page
        var pageTitle = await Page.TitleAsync();
        _output.WriteLine($"Page title: {pageTitle}");
        
        // Look for create requirement button
        var createButtons = await Page.QuerySelectorAllAsync("button");
        _output.WriteLine($"Found {createButtons.Count} buttons on requirements page");
        
        var createRequirementButton = await Page.QuerySelectorAsync("button:has-text('Create'), button:has-text('New'), button:has-text('Add')");
        if (createRequirementButton != null)
        {
            var buttonText = await createRequirementButton.TextContentAsync();
            _output.WriteLine($"Found create button: '{buttonText}'");
            
            // Click the create button
            await createRequirementButton.ClickAsync();
            await Task.Delay(2000);
            
            _output.WriteLine($"URL after clicking create: {Page.Url}");
            
            // Check if we're on the new requirement page or modal opened
            var isOnNewPage = Page.Url.Contains("/new");
            var modalVisible = await Page.IsVisibleAsync(".modal.show");
            
            _output.WriteLine($"On new requirement page: {isOnNewPage}");
            _output.WriteLine($"Modal visible: {modalVisible}");
            
            if (isOnNewPage || modalVisible)
            {
                // Try to fill out the form
                var titleField = await Page.QuerySelectorAsync("input[placeholder*='title'], input[name*='title'], #title");
                var descField = await Page.QuerySelectorAsync("textarea[placeholder*='description'], textarea[name*='description'], #description");
                
                if (titleField != null && descField != null)
                {
                    var testTitle = $"Debug Test Requirement {DateTime.Now:HHmmss}";
                    await titleField.FillAsync(testTitle);
                    await descField.FillAsync("Debug test description with user identity");
                    
                    _output.WriteLine($"Filled form with title: '{testTitle}'");
                    
                    // Look for save button
                    var saveButton = await Page.QuerySelectorAsync("button:has-text('Save'), button:has-text('Create'), button[type='submit']");
                    if (saveButton != null)
                    {
                        var saveButtonText = await saveButton.TextContentAsync();
                        _output.WriteLine($"Found save button: '{saveButtonText}'");
                        
                        // Click save
                        await saveButton.ClickAsync();
                        await Task.Delay(3000);
                        
                        _output.WriteLine($"URL after save: {Page.Url}");
                        
                        // Check for any error messages
                        var errorMessages = await Page.QuerySelectorAllAsync(".alert-danger, .error, .validation-message");
                        if (errorMessages.Count > 0)
                        {
                            _output.WriteLine("Found error messages:");
                            foreach (var error in errorMessages)
                            {
                                var errorText = await error.TextContentAsync();
                                _output.WriteLine($"  - {errorText}");
                            }
                        }
                        else
                        {
                            _output.WriteLine("No error messages found");
                        }
                        
                        // Check if modal is still visible (indicates save didn't complete)
                        var modalStillVisible = await Page.IsVisibleAsync(".modal.show");
                        _output.WriteLine($"Modal still visible after save: {modalStillVisible}");
                        
                        // Check if we're back on requirements list
                        var backOnList = Page.Url.Contains("/requirements") && !Page.Url.Contains("/new");
                        _output.WriteLine($"Back on requirements list: {backOnList}");
                        
                        if (backOnList)
                        {
                            // Look for the created requirement
                            await Task.Delay(2000); // Allow for page refresh
                            var pageContent = await Page.TextContentAsync("body");
                            var requirementVisible = pageContent?.Contains(testTitle) == true;
                            _output.WriteLine($"Created requirement visible in list: {requirementVisible}");
                            
                            if (!requirementVisible)
                            {
                                // Check how many requirements are shown
                                var requirementRows = await Page.QuerySelectorAllAsync("tr, .requirement-item, .list-item");
                                _output.WriteLine($"Total requirement rows/items visible: {requirementRows.Count}");
                                
                                // Check if there's pagination or filtering
                                var paginationElements = await Page.QuerySelectorAllAsync(".pagination, .page-link");
                                _output.WriteLine($"Pagination elements found: {paginationElements.Count}");
                            }
                        }
                    }
                    else
                    {
                        _output.WriteLine("⚠️ Save button not found");
                    }
                }
                else
                {
                    _output.WriteLine("⚠️ Title or description fields not found");
                    _output.WriteLine($"Title field found: {titleField != null}");
                    _output.WriteLine($"Description field found: {descField != null}");
                }
            }
            else
            {
                _output.WriteLine("⚠️ Neither new requirement page nor modal appeared after clicking create");
            }
        }
        else
        {
            _output.WriteLine("⚠️ Create requirement button not found");
            
            // List all buttons to see what's available
            for (int i = 0; i < Math.Min(createButtons.Count, 5); i++)
            {
                var buttonText = await createButtons[i].TextContentAsync();
                var buttonClass = await createButtons[i].GetAttributeAsync("class");
                _output.WriteLine($"Button {i}: '{buttonText}' (class: {buttonClass})");
            }
        }
        
        // Assert that we completed the debug investigation
        Assert.True(true, "Debug investigation completed - check output for details");
    }
}