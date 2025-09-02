using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Diagnostic tests to understand the current authentication system behavior
/// </summary>
public class AuthenticationSystemDiagnosticTests : E2ETestBase
{
    private readonly ITestOutputHelper _output;

    public AuthenticationSystemDiagnosticTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task DiagnoseAuthenticationFlow_Homepage()
    {
        _output.WriteLine("=== Diagnosing Homepage Access ===");
        
        // Clear any existing cookies
        await Page.Context.ClearCookiesAsync();
        
        // Navigate to homepage
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 15000 });
        await Task.Delay(2000);
        
        var currentUrl = Page.Url;
        _output.WriteLine($"Homepage URL: {currentUrl}");
        
        // Check if we can see the page content
        var bodyVisible = await Page.IsVisibleAsync("body");
        var appVisible = await Page.IsVisibleAsync("#app");
        
        _output.WriteLine($"Body visible: {bodyVisible}");
        _output.WriteLine($"App visible: {appVisible}");
        
        // Check for any authentication-related elements
        var loginButton = await Page.IsVisibleAsync("text=Login");
        var logoutButton = await Page.IsVisibleAsync("text=Logout");
        var userInfo = await Page.IsVisibleAsync("text=Hello,");
        
        _output.WriteLine($"Login button visible: {loginButton}");
        _output.WriteLine($"Logout button visible: {logoutButton}");
        _output.WriteLine($"User info visible: {userInfo}");
        
        // Check page title
        var title = await Page.TitleAsync();
        _output.WriteLine($"Page title: {title}");
        
        // Get page content for analysis
        var pageContent = await Page.ContentAsync();
        var hasBlazorApp = pageContent.Contains("blazor");
        var hasOidcAuth = pageContent.Contains("oidc") || pageContent.Contains("authentication");
        
        _output.WriteLine($"Has Blazor app: {hasBlazorApp}");
        _output.WriteLine($"Has OIDC auth references: {hasOidcAuth}");
        
        Assert.True(bodyVisible, "Homepage should load and show body");
    }

    [Fact]
    public async Task DiagnoseAuthenticationFlow_ProjectsPage()
    {
        _output.WriteLine("=== Diagnosing Projects Page Access ===");
        
        // Clear any existing cookies
        await Page.Context.ClearCookiesAsync();
        
        // Navigate to projects page (should be protected)
        await Page.GotoAsync($"{BaseUrl}/projects");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 15000 });
        await Task.Delay(3000);
        
        var currentUrl = Page.Url;
        _output.WriteLine($"Projects page URL: {currentUrl}");
        
        // Check if we were redirected
        var wasRedirected = !currentUrl.Contains("/projects");
        _output.WriteLine($"Was redirected away from projects: {wasRedirected}");
        
        if (wasRedirected)
        {
            _output.WriteLine($"Redirected to: {currentUrl}");
            
            // Check if it's an authentication page
            var isAuthPage = currentUrl.Contains("/Account/Login") || 
                           currentUrl.Contains("/authentication") || 
                           currentUrl.Contains("/connect/authorize");
            _output.WriteLine($"Redirected to auth page: {isAuthPage}");
        }
        
        // Check page content
        var bodyVisible = await Page.IsVisibleAsync("body");
        var appVisible = await Page.IsVisibleAsync("#app");
        
        _output.WriteLine($"Body visible: {bodyVisible}");
        _output.WriteLine($"App visible: {appVisible}");
        
        // Look for authentication-related elements
        var loginForm = await Page.IsVisibleAsync("form");
        var usernameField = await Page.IsVisibleAsync("input[type='text'], input[type='email']");
        var passwordField = await Page.IsVisibleAsync("input[type='password']");
        
        _output.WriteLine($"Login form visible: {loginForm}");
        _output.WriteLine($"Username field visible: {usernameField}");
        _output.WriteLine($"Password field visible: {passwordField}");
        
        // Check for any error messages
        var errorMessages = await Page.QuerySelectorAllAsync(".alert, .error, [class*='error']");
        _output.WriteLine($"Error elements found: {errorMessages.Count}");
        
        foreach (var error in errorMessages)
        {
            var errorText = await error.TextContentAsync();
            _output.WriteLine($"Error text: {errorText}");
        }
        
        Assert.True(bodyVisible, "Page should load and show body");
    }

    [Fact]
    public async Task DiagnoseBlazorAppInitialization()
    {
        _output.WriteLine("=== Diagnosing Blazor App Initialization ===");
        
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 15000 });
        
        // Wait for Blazor to initialize
        await Task.Delay(5000);
        
        // Check if Blazor has loaded
        var blazorStarted = await Page.EvaluateAsync<bool>(@"
            () => {
                return window.Blazor !== undefined;
            }
        ");
        
        _output.WriteLine($"Blazor started: {blazorStarted}");
        
        // Check for authentication state
        var authState = await Page.EvaluateAsync<string>(@"
            () => {
                try {
                    const authService = window.DotNet;
                    return authService ? 'AuthService available' : 'AuthService not available';
                } catch (e) {
                    return 'Error checking auth: ' + e.message;
                }
            }
        ");
        
        _output.WriteLine($"Auth state: {authState}");
        
        // Check console for any errors
        var consoleMessages = new List<string>();
        Page.Console += (_, e) => consoleMessages.Add($"{e.Type}: {e.Text}");
        
        await Task.Delay(2000);
        
        _output.WriteLine($"Console messages: {consoleMessages.Count}");
        foreach (var msg in consoleMessages.Take(10))
        {
            _output.WriteLine($"Console: {msg}");
        }
        
        Assert.True(blazorStarted, "Blazor should initialize successfully");
    }
}