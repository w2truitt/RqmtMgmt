using frontend.E2ETests.PageObjects;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Debug test to understand the current Users page structure
/// </summary>
public class DebugUsersPageStructure : AuthenticatedE2ETestBase
{
    public DebugUsersPageStructure(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task Debug_UsersPageStructure()
    {
        // Arrange - Login as admin
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Failed to login as admin");
        
        // Try to navigate to users page
        _output.WriteLine("=== USERS PAGE DEBUG ===");
        
        try
        {
            await Page.GotoAsync($"{BaseUrl}/users");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(3000); // Allow time for page to fully load
            
            _output.WriteLine($"Current URL: {Page.Url}");
            
            // Check page title
            var title = await Page.TitleAsync();
            _output.WriteLine($"Page title: {title}");
            
            // Check if we got redirected (might not have access)
            if (!Page.Url.Contains("/users"))
            {
                _output.WriteLine("❌ Redirected away from users page - might not have access");
            }
            else
            {
                _output.WriteLine("✅ Successfully on users page");
            }
            
            // Look for any buttons
            var allButtons = await Page.QuerySelectorAllAsync("button");
            _output.WriteLine($"Found {allButtons.Count} buttons on page");
            
            for (int i = 0; i < Math.Min(allButtons.Count, 10); i++) // Show first 10 buttons
            {
                var buttonText = await allButtons[i].TextContentAsync();
                var buttonClass = await allButtons[i].GetAttributeAsync("class");
                _output.WriteLine($"Button {i + 1}: '{buttonText?.Trim()}' (class: {buttonClass})");
            }
            
            // Look for create/add buttons specifically
            var createButtons = await Page.QuerySelectorAllAsync("button:has-text('Create'), button:has-text('Add'), button:has-text('New')");
            _output.WriteLine($"Found {createButtons.Count} create/add/new buttons");
            
            // Look for user-related content
            var userContent = await Page.QuerySelectorAllAsync("[data-testid*='user'], [class*='user'], [id*='user']");
            _output.WriteLine($"Found {userContent.Count} user-related elements");
            
            // Check page content for user-related text
            var bodyText = await Page.TextContentAsync("body");
            var hasUserText = bodyText?.ToLower().Contains("user") == true;
            var hasManageText = bodyText?.ToLower().Contains("manage") == true;
            _output.WriteLine($"Page contains 'user' text: {hasUserText}");
            _output.WriteLine($"Page contains 'manage' text: {hasManageText}");
            
            // Look for navigation elements that might lead to user management
            var navLinks = await Page.QuerySelectorAllAsync("nav a, .nav a, .sidebar a");
            _output.WriteLine($"Found {navLinks.Count} navigation links");
            
            for (int i = 0; i < Math.Min(navLinks.Count, 10); i++)
            {
                var linkText = await navLinks[i].TextContentAsync();
                var linkHref = await navLinks[i].GetAttributeAsync("href");
                _output.WriteLine($"Nav link {i + 1}: '{linkText?.Trim()}' -> {linkHref}");
            }
            
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Error accessing users page: {ex.Message}");
        }
        
        // Try alternative paths
        var alternativePaths = new[] { "/admin", "/admin/users", "/management", "/management/users", "/user-management" };
        
        foreach (var path in alternativePaths)
        {
            try
            {
                _output.WriteLine($"Trying alternative path: {path}");
                await Page.GotoAsync($"{BaseUrl}{path}");
                await Task.Delay(1000);
                
                if (Page.Url.Contains(path))
                {
                    _output.WriteLine($"✅ Successfully accessed {path}");
                    var pageText = await Page.TextContentAsync("body");
                    if (pageText?.ToLower().Contains("user") == true)
                    {
                        _output.WriteLine($"✅ {path} contains user-related content");
                    }
                }
                else
                {
                    _output.WriteLine($"❌ Redirected away from {path}");
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error accessing {path}: {ex.Message}");
            }
        }
        
        Assert.True(true, "Debug completed - check output for details");
    }
}