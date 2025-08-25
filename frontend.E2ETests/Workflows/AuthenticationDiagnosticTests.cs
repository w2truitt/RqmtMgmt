using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Diagnostic test to understand the authentication flow
/// </summary>
public class AuthenticationDiagnosticTests : E2ETestBase
{
    private readonly ITestOutputHelper _output;

    public AuthenticationDiagnosticTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task DiagnoseHomepageRedirectBehavior()
    {
        _output.WriteLine("Starting homepage navigation diagnostic...");
        
        try
        {
            // Navigate to homepage and capture what happens
            _output.WriteLine($"Navigating to: {BaseUrl}");
            var response = await Page.GotoAsync(BaseUrl);
            
            _output.WriteLine($"Initial response status: {response?.Status}");
            _output.WriteLine($"Initial response URL: {response?.Url}");
            
            // Wait for any redirects to complete
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 10000 });
            await Task.Delay(2000);
            
            // Capture current state
            var currentUrl = Page.Url;
            var title = await Page.TitleAsync();
            var bodyVisible = await Page.IsVisibleAsync("body");
            var appVisible = await Page.IsVisibleAsync("#app");
            
            _output.WriteLine($"Final URL: {currentUrl}");
            _output.WriteLine($"Page title: {title}");
            _output.WriteLine($"Body visible: {bodyVisible}");
            _output.WriteLine($"#app visible: {appVisible}");
            
            // Check if we're on a login page
            var isLoginPage = currentUrl.Contains("login") || currentUrl.Contains("auth") || 
                            await Page.IsVisibleAsync("input[type='password']") ||
                            await Page.IsVisibleAsync("button:has-text('Login')") ||
                            await Page.IsVisibleAsync("button:has-text('Sign in')");
            
            _output.WriteLine($"Appears to be login page: {isLoginPage}");
            
            // Capture page content for analysis
            var pageContent = await Page.ContentAsync();
            _output.WriteLine("=== PAGE CONTENT (first 1000 chars) ===");
            _output.WriteLine(pageContent.Length > 1000 ? pageContent.Substring(0, 1000) + "..." : pageContent);
            
            // Check for specific authentication-related elements
            var authElements = new[]
            {
                "form[action*='login']",
                "input[name='username']", 
                "input[name='email']",
                "input[name='password']",
                ".login-form",
                ".auth-form",
                "button:has-text('Login')",
                "button:has-text('Sign in')",
                "a:has-text('Login')",
                "a:has-text('Sign in')"
            };
            
            _output.WriteLine("=== AUTHENTICATION ELEMENTS ===");
            foreach (var selector in authElements)
            {
                var isVisible = await Page.IsVisibleAsync(selector);
                if (isVisible)
                {
                    _output.WriteLine($"Found: {selector}");
                }
            }
            
            // Check for error messages
            var errorSelectors = new[]
            {
                ".error",
                ".alert-danger", 
                ".text-danger",
                "[role='alert']",
                ".blazor-error-boundary"
            };
            
            _output.WriteLine("=== ERROR ELEMENTS ===");
            foreach (var selector in errorSelectors)
            {
                var isVisible = await Page.IsVisibleAsync(selector);
                if (isVisible)
                {
                    var errorText = await Page.TextContentAsync(selector);
                    _output.WriteLine($"Error found ({selector}): {errorText}");
                }
            }
            
            // Always fail this test so we can see the output
            Assert.True(false, "Diagnostic test - check output for authentication flow details");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Exception occurred: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [Fact]
    public async Task DiagnoseProjectsPageRedirectBehavior()
    {
        _output.WriteLine("Starting projects page navigation diagnostic...");
        
        try
        {
            // Navigate directly to projects page
            var projectsUrl = $"{BaseUrl}/projects";
            _output.WriteLine($"Navigating to: {projectsUrl}");
            
            var response = await Page.GotoAsync(projectsUrl);
            _output.WriteLine($"Response status: {response?.Status}");
            _output.WriteLine($"Response URL: {response?.Url}");
            
            // Wait for redirects
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 10000 });
            await Task.Delay(2000);
            
            var currentUrl = Page.Url;
            var title = await Page.TitleAsync();
            
            _output.WriteLine($"Final URL: {currentUrl}");
            _output.WriteLine($"Page title: {title}");
            
            // Check if we ended up on the projects page or got redirected
            var onProjectsPage = currentUrl.Contains("/projects") && !currentUrl.Contains("login") && !currentUrl.Contains("auth");
            var onLoginPage = currentUrl.Contains("login") || currentUrl.Contains("auth");
            
            _output.WriteLine($"On projects page: {onProjectsPage}");
            _output.WriteLine($"On login page: {onLoginPage}");
            
            // Look for projects page elements
            var projectsElements = new[]
            {
                "h1:has-text('Projects')",
                "h3:has-text('Projects')",
                "[data-testid='create-project-button']",
                ".projects-page",
                "table",
                ".mud-table"
            };
            
            _output.WriteLine("=== PROJECTS PAGE ELEMENTS ===");
            foreach (var selector in projectsElements)
            {
                var isVisible = await Page.IsVisibleAsync(selector);
                if (isVisible)
                {
                    _output.WriteLine($"Found: {selector}");
                }
            }
            
            Assert.True(false, "Diagnostic test - check output for projects page redirect details");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Exception occurred: {ex.Message}");
            throw;
        }
    }
    
    [Fact]
    public async Task DiagnoseIdentityServerEndpoint()
    {
        _output.WriteLine("Testing identity server endpoint accessibility...");
        
        try
        {
            // Test the identity server discovery endpoint
            var discoveryUrl = $"{BaseUrl}/.well-known/openid-configuration";
            _output.WriteLine($"Testing discovery endpoint: {discoveryUrl}");
            
            var response = await Page.GotoAsync(discoveryUrl);
            _output.WriteLine($"Discovery response status: {response?.Status}");
            
            if (response?.Status == 200)
            {
                var content = await Page.ContentAsync();
                _output.WriteLine("Discovery endpoint accessible - content preview:");
                _output.WriteLine(content.Length > 500 ? content.Substring(0, 500) + "..." : content);
            }
            
            // Test the authorization endpoint by trying to navigate there
            var authUrl = $"{BaseUrl}/connect/authorize?client_id=rqmtmgmt-frontend&response_type=code&scope=openid%20profile%20email%20role%20rqmtmgmt.api&redirect_uri={BaseUrl}/authentication/login-callback";
            _output.WriteLine($"Testing authorization endpoint: {authUrl}");
            
            var authResponse = await Page.GotoAsync(authUrl);
            _output.WriteLine($"Authorization response status: {authResponse?.Status}");
            _output.WriteLine($"Authorization final URL: {Page.Url}");
            
            // Check if we get a login form
            await Task.Delay(2000);
            var hasLoginForm = await Page.IsVisibleAsync("form") || 
                              await Page.IsVisibleAsync("input[type='password']") ||
                              await Page.IsVisibleAsync("button:has-text('Login')");
            
            _output.WriteLine($"Login form detected: {hasLoginForm}");
            
            if (hasLoginForm)
            {
                // Look for username/email and password fields
                var usernameField = await Page.IsVisibleAsync("input[name='username']") || 
                                   await Page.IsVisibleAsync("input[name='email']") ||
                                   await Page.IsVisibleAsync("input[type='email']");
                var passwordField = await Page.IsVisibleAsync("input[name='password']") || 
                                   await Page.IsVisibleAsync("input[type='password']");
                
                _output.WriteLine($"Username/email field found: {usernameField}");
                _output.WriteLine($"Password field found: {passwordField}");
                
                // Capture form content
                var formContent = await Page.ContentAsync();
                _output.WriteLine("=== LOGIN FORM CONTENT (first 1000 chars) ===");
                _output.WriteLine(formContent.Length > 1000 ? formContent.Substring(0, 1000) + "..." : formContent);
            }
            
            Assert.True(false, "Diagnostic test - check output for identity server endpoint details");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Exception occurred: {ex.Message}");
            throw;
        }
    }
    
    [Fact]
    public async Task TestCompleteLoginFlow()
    {
        _output.WriteLine("Testing complete login flow...");
        
        try
        {
            // Clear any existing sessions/cookies first
            await Page.Context.ClearCookiesAsync();
            await Task.Delay(1000);
            
            // Navigate to homepage, which should redirect to login
            _output.WriteLine($"Navigating to: {BaseUrl}");
            await Page.GotoAsync(BaseUrl);
            
            // Wait longer for redirect to login page
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 15000 });
            await Task.Delay(3000); // Give more time for authentication to process
            
            var currentUrl = Page.Url;
            _output.WriteLine($"After redirect, current URL: {currentUrl}");
            
            // Check if we got redirected at all - if not, there might be an issue
            if (!currentUrl.Contains("/Account/Login"))
            {
                _output.WriteLine("No redirect to login detected. Checking page content...");
                var pageContent = await Page.ContentAsync();
                _output.WriteLine($"Page content (first 500 chars): {pageContent.Substring(0, Math.Min(500, pageContent.Length))}");
                
                // Check if we're actually on a Blazor app page
                var hasBlazorApp = await Page.IsVisibleAsync("#app");
                var hasAuthState = await Page.EvaluateAsync<bool>("() => window.localStorage.getItem('oidc.user') !== null");
                
                _output.WriteLine($"Has Blazor app element: {hasBlazorApp}");
                _output.WriteLine($"Has authentication state: {hasAuthState}");
                
                // Maybe we're already authenticated? Check if user info is present
                var userInfo = await Page.EvaluateAsync<string>("() => window.localStorage.getItem('oidc.user')");
                if (userInfo != null)
                {
                    _output.WriteLine($"Found existing user authentication: {userInfo.Substring(0, Math.Min(200, userInfo.Length))}...");
                }
                
                Assert.True(false, $"Expected redirect to login page, but stayed on: {currentUrl}");
            }
            
            // Verify we're on the login page
            var isLoginPage = currentUrl.Contains("/Account/Login");
            Assert.True(isLoginPage, $"Should be redirected to login page, but got: {currentUrl}");
            
            _output.WriteLine("Successfully redirected to login page!");
            
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Exception occurred: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}
