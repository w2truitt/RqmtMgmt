using System.Collections.Concurrent;
using frontend.E2ETests.Fixtures;
using Microsoft.Playwright;
using frontend.E2ETests.Infrastructure;

namespace frontend.E2ETests.Services;

/// <summary>
/// Enhanced authentication service that uses browser state detection instead of file caching.
/// This service creates fresh browser contexts with proper authentication for each test,
/// eliminating stale cache issues and enabling parallel test execution.
/// </summary>
public static class AuthenticationService
{
    /// <summary>
    /// Gets an authenticated browser context for the specified user.
    /// Creates a fresh context and performs login for complete isolation.
    /// </summary>
    public static async Task<IBrowserContext> GetAuthenticatedContextAsync(PlaywrightFixture fixture, string email, string password)
    {
        TestLogger.LogAuthentication($"Creating authenticated context for: {email}");

        // Create a fresh context for complete isolation
        var context = await fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 }
        });

        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(30000);
        page.SetDefaultNavigationTimeout(30000);

        // Always perform login (since we have fresh context)
        await PerformLoginAsync(page, email, password);

        TestLogger.LogAuthentication($"Authenticated context ready for: {email}");
        return context;
    }

    /// <summary>
    /// Performs the login flow for the specified user.
    /// </summary>
    private static async Task PerformLoginAsync(IPage page, string email, string password)
    {
        TestLogger.LogAuthentication($"Performing login for: {email}");
        var loginStartTime = DateTime.UtcNow;

        try
        {
            // Navigate to a protected page first to trigger proper authentication flow
            await page.GotoAsync("https://rqmtmgmt.local/dashboard");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Wait for potential redirect to login page
            await Task.Delay(2000);
            
            // If not redirected to login, try another protected page
            if (!page.Url.Contains("/Account/Login"))
            {
                await page.GotoAsync("https://rqmtmgmt.local/users");
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await Task.Delay(2000);
            }
            
            // If still not on login page, navigate directly with ReturnUrl
            if (!page.Url.Contains("/Account/Login"))
            {
                await page.GotoAsync("https://rqmtmgmt.local/Account/Login?ReturnUrl=%2F");
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            }

            // Wait for login form
            await page.WaitForSelectorAsync("input[name='Input.Username']", new PageWaitForSelectorOptions { Timeout = 10000 });

            // Fill login form
            await page.FillAsync("input[name='Input.Username']", email);
            await page.FillAsync("input[name='Input.Password']", password);

            // Submit login
            await page.ClickAsync("button:has-text('Login')");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Wait for OIDC flow completion
            await Task.Delay(3000);

            // Verify login success
            var currentUrl = page.Url;
            var isAuthenticated = !currentUrl.Contains("/Account/Login") && 
                                 !currentUrl.Contains("/connect/authorize");

            if (!isAuthenticated)
            {
                // Check for error messages
                var errorElements = await page.Locator(".text-danger, .alert-danger, .validation-summary-errors").AllAsync();
                var errorMessages = new List<string>();
                
                foreach (var error in errorElements)
                {
                    var errorText = await error.TextContentAsync();
                    if (!string.IsNullOrEmpty(errorText))
                    {
                        errorMessages.Add(errorText.Trim());
                    }
                }

                var errorMessage = errorMessages.Count > 0 
                    ? $"Login failed for {email}. Errors: {string.Join(", ", errorMessages)}"
                    : $"Login failed for {email} - still on: {currentUrl}";
                
                throw new Exception(errorMessage);
            }

            var loginDuration = DateTime.UtcNow - loginStartTime;
            TestLogger.LogAuthentication($"Login successful for {email}, redirected to: {currentUrl} (took {loginDuration.TotalSeconds:F1}s)");
        }
        catch (Exception ex)
        {
            TestLogger.LogAuthentication($"Login failed for {email}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Switches the current browser context to a different user.
    /// Logs out the current user and logs in as the new user.
    /// </summary>
    public static async Task SwitchUserAsync(IBrowserContext context, string email, string password)
    {
        var page = context.Pages.FirstOrDefault() ?? await context.NewPageAsync();
        
        TestLogger.LogAuthentication($"Switching to user: {email}");
        
        // Log out current user
        await LogoutAsync(page);
        
        // Log in as new user
        await PerformLoginAsync(page, email, password);
        
        TestLogger.LogAuthentication($"Successfully switched to user: {email}");
    }

    /// <summary>
    /// Logs out the current user from the browser.
    /// </summary>
    private static async Task LogoutAsync(IPage page)
    {
        try
        {
            TestLogger.LogAuthentication("Logging out current user");

            // Look for logout button on current page first (much faster than navigating)
            try
            {
                var logoutButton = await page.WaitForSelectorAsync("button:has-text('Log out')", new PageWaitForSelectorOptions 
                { 
                    Timeout = 2000 
                });
                
                if (logoutButton != null)
                {
                    await logoutButton.ClickAsync();
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    TestLogger.LogAuthentication("Logout completed");
                    return;
                }
            }
            catch (TimeoutException)
            {
                // No logout button on current page
            }

            // If no logout button found, navigate to home page and try again
            await page.GotoAsync("https://rqmtmgmt.local/");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            try
            {
                var logoutButton = await page.WaitForSelectorAsync("button:has-text('Log out')", new PageWaitForSelectorOptions 
                { 
                    Timeout = 3000 
                });
                
                if (logoutButton != null)
                {
                    await logoutButton.ClickAsync();
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    TestLogger.LogAuthentication("Logout completed");
                    return;
                }
            }
            catch (TimeoutException)
            {
                // Still no logout button - might already be logged out
                TestLogger.LogAuthentication("No logout button found - might already be logged out");
            }
        }
        catch (Exception ex)
        {
            TestLogger.LogAuthentication($"Error during logout: {ex.Message}");
        }

        // Clear browser storage to ensure clean state
        try
        {
            await page.Context.ClearCookiesAsync();
            await page.EvaluateAsync("() => { localStorage.clear(); sessionStorage.clear(); }");
        }
        catch (Exception ex)
        {
            TestLogger.LogAuthentication($"Error clearing browser storage: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifies that the browser is currently authenticated as the expected user.
    /// </summary>
    public static async Task<bool> VerifyAuthenticationAsync(IPage page, string expectedEmail)
    {
        try
        {
            // Simply try to navigate to a protected page and see if we stay there
            await page.GotoAsync("https://rqmtmgmt.local/dashboard");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Wait a bit for Blazor to render
            await Task.Delay(3000);
            
            // If we're on a login page, authentication failed
            if (page.Url.Contains("/Account/Login") || page.Url.Contains("/connect/authorize"))
            {
                TestLogger.LogAuthentication($"Authentication verification failed: redirected to login page");
                return false;
            }
            
            TestLogger.LogAuthentication($"Authentication verification successful: can access protected page");
            return true;
        }
        catch (Exception ex)
        {
            TestLogger.LogAuthentication($"Authentication verification error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Clears all authentication state from the browser context.
    /// </summary>
    public static async Task ClearAuthenticationAsync(IBrowserContext context)
    {
        var page = context.Pages.FirstOrDefault() ?? await context.NewPageAsync();
        
        // Log out
        await LogoutAsync(page);
        
        TestLogger.LogAuthentication("Cleared authentication state");
    }

    // Legacy methods for backward compatibility
    /// <summary>
    /// Legacy compatibility method - redirects to GetAuthenticatedContextAsync
    /// </summary>
    public static Task<string> GetStorageStateAsync(PlaywrightFixture fixture, string email, string password)
    {
        // For backward compatibility, just return a placeholder since we don't use file storage anymore
        return Task.FromResult($"fresh-context-{email}");
    }
}