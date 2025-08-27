using Microsoft.Playwright;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Base class for E2E tests that require authentication.
/// Provides helper methods for logging in and managing authenticated sessions.
/// </summary>
public abstract class AuthenticatedE2ETestBase : E2ETestBase
{
    protected readonly ITestOutputHelper _output;

    protected AuthenticatedE2ETestBase(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Checks if the current session is authenticated with the specified user.
    /// </summary>
    /// <param name="expectedEmail">The email of the user we expect to be logged in as</param>
    /// <returns>True if already authenticated as the specified user</returns>
    protected async Task<bool> IsAuthenticatedAsAsync(string expectedEmail)
    {
        try
        {
            _output.WriteLine($"Checking if already authenticated as: {expectedEmail}");
            
            // Navigate to a protected page to check authentication status
            await Page.GotoAsync($"{BaseUrl}/projects");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 15000 });
            await Task.Delay(3000); // Give time for OIDC redirects
            
            var currentUrl = Page.Url;
            
            // If we're redirected to login or OIDC auth flow, we're not authenticated
            if (currentUrl.Contains("/Account/Login") || currentUrl.Contains("/connect/authorize"))
            {
                _output.WriteLine($"Not authenticated - redirected to auth flow: {currentUrl}");
                return false;
            }
            
            // If we're on the projects page, we're likely authenticated, but let's be conservative
            // and only return true if we can find clear evidence of the specific user
            if (currentUrl.Contains("/projects"))
            {
                _output.WriteLine($"On protected page, but need to verify user identity: {currentUrl}");
                
                // Try to find user info in the page (this might be in a user menu or profile area)
                var userInfoSelectors = new[]
                {
                    "[data-testid='user-email']",
                    ".user-email",
                    ".current-user",
                    "[data-user-email]",
                    "text=" + expectedEmail
                };
                
                foreach (var selector in userInfoSelectors)
                {
                    try
                    {
                        var element = await Page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions { Timeout = 2000 });
                        if (element != null)
                        {
                            var text = await element.TextContentAsync();
                            if (text?.Contains(expectedEmail) == true)
                            {
                                _output.WriteLine($"Already authenticated as {expectedEmail}");
                                return true;
                            }
                        }
                    }
                    catch
                    {
                        // Continue to next selector
                    }
                }
                
                // Alternative approach: Try to get user info from JavaScript if available
                try
                {
                    var userInfo = await Page.EvaluateAsync<string>("() => { " +
                        "const userEmail = document.querySelector('[data-user-email]')?.getAttribute('data-user-email') || " +
                        "document.querySelector('.user-email')?.textContent || " +
                        "window.currentUser?.email || " +
                        "localStorage.getItem('currentUserEmail') || " +
                        "sessionStorage.getItem('currentUserEmail'); " +
                        "return userInfo; " +
                    "}");
                    
                    if (!string.IsNullOrEmpty(userInfo) && userInfo.Contains(expectedEmail))
                    {
                        _output.WriteLine($"Already authenticated as {expectedEmail} (from JS)");
                        return true;
                    }
                }
                catch
                {
                    // JavaScript approach failed, continue
                }
                
                // We're on a protected page but can't verify the specific user
                // For safety, assume we need to re-authenticate
                _output.WriteLine($"On protected page but cannot verify user identity - assuming re-auth needed");
                return false;
            }
            
            _output.WriteLine($"Authentication status unclear - URL: {currentUrl}");
            return false;
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Authentication check failed with exception: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Ensures authentication with the specified user, skipping login if already authenticated.
    /// </summary>
    /// <param name="email">User email address</param>
    /// <param name="password">User password</param>
    /// <returns>True if authentication was successful</returns>
    protected async Task<bool> EnsureAuthenticatedAsync(string email, string password)
    {
        // First check if we're already authenticated as this user
        if (await IsAuthenticatedAsAsync(email))
        {
            _output.WriteLine($"Already authenticated as {email}, skipping login");
            return true;
        }
        
        // Not authenticated or authenticated as different user, perform login
        _output.WriteLine($"Not authenticated as {email}, performing login");
        return await LoginAsync(email, password);
    }

    /// <summary>
    /// Logs in with the specified test user credentials.
    /// </summary>
    /// <param name="email">User email address</param>
    /// <param name="password">User password</param>
    /// <returns>True if login was successful</returns>
    protected async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            _output.WriteLine($"Attempting login with user: {email}");
            
            // Clear any existing sessions first
            await Page.Context.ClearCookiesAsync();
            await Task.Delay(1000);
            
            // Navigate to a protected page to trigger authentication
            await Page.GotoAsync($"{BaseUrl}/projects");
            
            // Wait for redirect to login page
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 15000 });
            await Task.Delay(2000);
            
            var currentUrl = Page.Url;
            if (!currentUrl.Contains("/Account/Login"))
            {
                _output.WriteLine($"Expected redirect to login page, but got: {currentUrl}");
                return false;
            }
            
            // Fill in login form
            var usernameField = await Page.WaitForSelectorAsync("input[name='Input.Username']", new PageWaitForSelectorOptions { Timeout = 5000 });
            var passwordField = await Page.WaitForSelectorAsync("input[name='Input.Password']", new PageWaitForSelectorOptions { Timeout = 5000 });
            var loginButton = await Page.WaitForSelectorAsync("button:has-text('Login')", new PageWaitForSelectorOptions { Timeout = 5000 });
            
            if (usernameField == null || passwordField == null || loginButton == null)
            {
                _output.WriteLine("Login form elements not found");
                return false;
            }
            
            await usernameField.FillAsync(email);
            await passwordField.FillAsync(password);
            
            // Submit login form
            await loginButton.ClickAsync();
            
            // Wait for authentication to complete
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 20000 });
            await Task.Delay(3000);
            
            var finalUrl = Page.Url;
            var isAuthenticated = !finalUrl.Contains("/Account/Login") && finalUrl.Contains(BaseUrl);
            
            _output.WriteLine($"Login {(isAuthenticated ? "successful" : "failed")}. Final URL: {finalUrl}");
            return isAuthenticated;
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Login failed with exception: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Logs in as the admin test user, skipping login if already authenticated.
    /// </summary>
    /// <returns>True if login was successful</returns>
    protected async Task<bool> LoginAsAdminAsync()
    {
        return await EnsureAuthenticatedAsync("admin@rqmtmgmt.local", "Admin123!");
    }

    /// <summary>
    /// Logs in as the project manager test user, skipping login if already authenticated.
    /// </summary>
    /// <returns>True if login was successful</returns>
    protected async Task<bool> LoginAsProjectManagerAsync()
    {
        return await EnsureAuthenticatedAsync("pm@rqmtmgmt.local", "Pm123!");
    }

    /// <summary>
    /// Logs in as the developer test user, skipping login if already authenticated.
    /// </summary>
    /// <returns>True if login was successful</returns>
    protected async Task<bool> LoginAsDeveloperAsync()
    {
        return await EnsureAuthenticatedAsync("dev@rqmtmgmt.local", "Dev123!");
    }

    /// <summary>
    /// Logs in as the tester test user, skipping login if already authenticated.
    /// </summary>
    /// <returns>True if login was successful</returns>
    protected async Task<bool> LoginAsTesterAsync()
    {
        return await EnsureAuthenticatedAsync("tester@rqmtmgmt.local", "Test123!");
    }

    /// <summary>
    /// Logs in as the viewer test user, skipping login if already authenticated.
    /// </summary>
    /// <returns>True if login was successful</returns>
    protected async Task<bool> LoginAsViewerAsync()
    {
        return await EnsureAuthenticatedAsync("viewer@rqmtmgmt.local", "View123!");
    }

    /// <summary>
    /// Logs out the current user by clearing cookies and navigating to logout.
    /// </summary>
    protected async Task LogoutAsync()
    {
        try
        {
            _output.WriteLine("Logging out current user");
            
            // Try to navigate to logout endpoint first
            await Page.GotoAsync($"{BaseUrl}/Account/Logout");
            await Task.Delay(2000);
            
            // Clear cookies to ensure complete logout
            await Page.Context.ClearCookiesAsync();
            await Task.Delay(1000);
            
            _output.WriteLine("Logout completed");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Logout failed with exception: {ex.Message}");
            // Clear cookies anyway as fallback
            await Page.Context.ClearCookiesAsync();
        }
    }

    /// <summary>
    /// Navigates to a protected page and verifies the user is authenticated.
    /// </summary>
    /// <param name="relativePath">The relative path to navigate to (e.g., "/projects")</param>
    /// <returns>True if successfully navigated to the protected page without redirect</returns>
    protected async Task<bool> NavigateToProtectedPageAsync(string relativePath)
    {
        try
        {
            var fullUrl = $"{BaseUrl}{relativePath}";
            _output.WriteLine($"Navigating to protected page: {fullUrl}");
            
            await Page.GotoAsync(fullUrl);
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 15000 });
            await Task.Delay(3000); // Give more time for potential OIDC redirects
            
            var currentUrl = Page.Url;
            
            // Check if we're on the target page
            if (currentUrl.Contains(relativePath) && !currentUrl.Contains("/Account/Login"))
            {
                _output.WriteLine($"Navigation successful. Current URL: {currentUrl}");
                return true;
            }
            
            // If we're redirected to login, the authentication may have expired
            if (currentUrl.Contains("/Account/Login"))
            {
                _output.WriteLine($"Redirected to login page, authentication may have expired: {currentUrl}");
                return false;
            }
            
            // If we're on some other OIDC redirect, wait a bit more
            if (currentUrl.Contains("/connect/authorize") || currentUrl.Contains("/authentication/"))
            {
                _output.WriteLine($"OIDC redirect detected, waiting for completion: {currentUrl}");
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 10000 });
                await Task.Delay(3000);
                
                var finalUrl = Page.Url;
                var isFinalSuccess = finalUrl.Contains(relativePath) && !finalUrl.Contains("/Account/Login");
                _output.WriteLine($"After OIDC redirect - Navigation {(isFinalSuccess ? "successful" : "failed")}. Final URL: {finalUrl}");
                return isFinalSuccess;
            }
            
            _output.WriteLine($"Navigation failed. Current URL: {currentUrl}");
            return false;
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Navigation failed with exception: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Waits for the Blazor application to fully load on the current page.
    /// </summary>
    protected async Task WaitForBlazorAppAsync()
    {
        try
        {
            // Wait for the Blazor app div to be visible
            await Page.WaitForSelectorAsync("#app", new PageWaitForSelectorOptions { Timeout = 10000 });
            
            // Give additional time for Blazor to initialize
            await Task.Delay(3000);
            
            _output.WriteLine("Blazor app loaded successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Failed to wait for Blazor app: {ex.Message}");
        }
    }
}
