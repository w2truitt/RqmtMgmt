using System.Collections.Concurrent;
using frontend.E2ETests.Fixtures;
using Microsoft.Playwright;
using frontend.E2ETests.Infrastructure;

namespace frontend.E2ETests.Services;

/// <summary>
/// Handles user authentication and caches the StorageState to avoid repeated logins.
/// This service performs UI login only once per user role for the entire test suite,
/// then reuses the cached session for subsequent tests.
/// </summary>
public static class AuthenticationService
{
    private static readonly ConcurrentDictionary<string, string> StorageStateFileCache = new();
    private static readonly SemaphoreSlim LoginLock = new(1, 1);

    /// <summary>
    /// Gets the expected file path for a user's storage state
    /// </summary>
    private static string GetStorageStateFilePath(string email)
    {
        return Path.Combine(Path.GetTempPath(), $"playwright-auth-{email.Replace("@", "-").Replace(".", "-")}.json");
    }

    /// <summary>
    /// Gets the authentication storage state file path for a user.
    /// If not cached, performs a UI login and caches the result.
    /// Subsequent calls for the same user return the cached session file path instantly.
    /// </summary>
    /// <param name="fixture">Shared browser fixture</param>
    /// <param name="email">User email</param>
    /// <param name="password">User password</param>
    /// <returns>Path to the storage state file for creating authenticated contexts</returns>
    public static async Task<string> GetStorageStateAsync(
        PlaywrightFixture fixture, 
        string email, 
        string password)
    {
        var expectedFilePath = GetStorageStateFilePath(email);

        // Check if we have a cached file path and the file exists
        if (StorageStateFileCache.TryGetValue(email, out var cachedFilePath) && File.Exists(cachedFilePath))
        {
            TestLogger.LogAuthentication($"Using cached authentication file for: {email}");
            return cachedFilePath;
        }

        // Check if the expected file exists even if not in memory cache (handles test runner restarts)
        if (File.Exists(expectedFilePath))
        {
            TestLogger.LogAuthentication($"Found existing authentication file for: {email}");
            StorageStateFileCache[email] = expectedFilePath; // Update cache
            return expectedFilePath;
        }

        // Thread-safe login for first-time users
        await LoginLock.WaitAsync();
        try
        {
            // Double-check pattern - another thread might have logged in while we waited
            if (File.Exists(expectedFilePath))
            {
                TestLogger.LogAuthentication($"Found authentication file created by another thread for: {email}");
                StorageStateFileCache[email] = expectedFilePath;
                return expectedFilePath;
            }

            // Perform actual login and cache the result
            var newFilePath = await PerformLoginAndCaptureState(fixture, email, password);
            StorageStateFileCache[email] = newFilePath;
            
            TestLogger.LogAuthentication($"Cached new authentication file for: {email}");
            TestLogger.LogAuthentication($"Total cached sessions: {StorageStateFileCache.Count}");
            
            return newFilePath;
        }
        finally
        {
            LoginLock.Release();
        }
    }

    /// <summary>
    /// Performs the actual UI login flow and captures the authentication state.
    /// Uses a temporary context to avoid affecting other tests.
    /// </summary>
    private static async Task<string> PerformLoginAndCaptureState(
        PlaywrightFixture fixture, 
        string email, 
        string password)
    {
        TestLogger.LogAuthentication($"Performing fresh login for: {email}");
        var loginStartTime = DateTime.UtcNow;
        
        // Use the expected file path pattern
        var stateFile = GetStorageStateFilePath(email);
        
        // Use temporary, isolated context for login with no cached state
        await using var context = await fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        { 
            IgnoreHTTPSErrors = true,
            // Ensure no cached authentication state
            StorageState = null
        });
        
        var page = await context.NewPageAsync();

        try
        {
            TestLogger.LogAuthentication($"Creating authenticated context for: {email}");
            
            // Navigate to a protected page first, which will redirect to login with proper ReturnUrl
            // This mimics the real authentication flow
            await page.GotoAsync("https://rqmtmgmt.local/");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Wait for potential redirect to login page
            await Task.Delay(2000);
            
            // Check if we're on login page, if not, try to navigate to a protected page
            if (!page.Url.Contains("/Account/Login"))
            {
                // Try navigating to a protected page to trigger authentication
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

            // Debug: Check what page we're actually on
            TestLogger.LogDebug($"Current URL: {page.Url}");
            TestLogger.LogDebug($"Page title: {await page.TitleAsync()}");
            
            // Debug: Check if we can find any form elements
            var inputCount = await page.Locator("input").CountAsync();
            TestLogger.LogDebug($"Found {inputCount} input elements on page");
            
            if (inputCount > 0)
            {
                // List all input elements for debugging
                var inputs = await page.Locator("input").AllAsync();
                for (int i = 0; i < inputs.Count; i++)
                {
                    var input = inputs[i];
                    var name = await input.GetAttributeAsync("name") ?? "";
                    var id = await input.GetAttributeAsync("id") ?? "";
                    var type = await input.GetAttributeAsync("type") ?? "";
                    var placeholder = await input.GetAttributeAsync("placeholder") ?? "";
                    TestLogger.LogVerbose($"Input {i}: name='{name}', id='{id}', type='{type}', placeholder='{placeholder}'");
                }
            }

            // Wait for the login form to be ready and fill it using name attributes (like the diagnostic tests)
            await page.WaitForSelectorAsync("input[name='Input.Username']", new PageWaitForSelectorOptions { Timeout = 10000 });
            
            // Fill the login form using the name attributes (same as diagnostic tests)
            await page.FillAsync("input[name='Input.Username']", email);
            await page.FillAsync("input[name='Input.Password']", password);
            
            // Click the login button using the same selector as diagnostic tests
            await page.ClickAsync("button:has-text('Login')");
            
            // Wait for successful authentication (should redirect to home page)
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Wait additional time for OIDC flow (same as diagnostic tests)
            await Task.Delay(3000);
            
            var currentUrl = page.Url;
            TestLogger.LogDebug($"Current URL after login attempt: {currentUrl}");
            
            // Check if login was successful (same logic as diagnostic tests)
            var isAuthenticated = !currentUrl.Contains("/Account/Login") && 
                                 !currentUrl.Contains("/connect/authorize");
            
            if (!isAuthenticated)
            {
                // Check for error messages on login page
                var errorElements = await page.Locator(".text-danger, .alert-danger, .validation-summary-errors").AllAsync();
                if (errorElements.Count > 0)
                {
                    foreach (var error in errorElements)
                    {
                        var errorText = await error.TextContentAsync();
                        TestLogger.LogError($"Error message found: {errorText}");
                    }
                }
                
                throw new Exception($"Login failed for {email} - still on: {currentUrl}");
            }
            
            var loginDuration = DateTime.UtcNow - loginStartTime;
            TestLogger.LogAuthentication($"Login successful for {email}, redirected to: {currentUrl} (took {loginDuration.TotalSeconds:F1}s)");

            // Save the storage state directly to the persistent file
            await context.StorageStateAsync(new BrowserContextStorageStateOptions 
            { 
                Path = stateFile 
            });
            
            TestLogger.LogAuthentication($"Saved authentication state to: {stateFile}");
            
            return stateFile;
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// Clears cached session for a specific user.
    /// Useful for authentication-specific tests that need fresh login flows.
    /// </summary>
    /// <param name="email">User email to clear</param>
    public static void ClearUserSession(string email)
    {
        var expectedFilePath = GetStorageStateFilePath(email);
        
        if (StorageStateFileCache.TryRemove(email, out var filePath))
        {
            TestLogger.LogAuthentication($"Cleared cached session for: {email}");
        }
        
        // Delete the file if it exists
        if (File.Exists(expectedFilePath))
        {
            try
            {
                File.Delete(expectedFilePath);
                TestLogger.LogAuthentication($"Deleted cached file: {expectedFilePath}");
            }
            catch
            {
                // Ignore file deletion errors
            }
        }
    }

    /// <summary>
    /// Clears all cached sessions.
    /// Useful for test cleanup or when testing authentication edge cases.
    /// </summary>
    public static void ClearAllSessions()
    {
        var count = StorageStateFileCache.Count;
        
        // Delete all cached files
        foreach (var filePath in StorageStateFileCache.Values)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch
                {
                    // Ignore file deletion errors
                }
            }
        }
        
        // Also delete any files that match our pattern
        var tempDir = Path.GetTempPath();
        var authFiles = Directory.GetFiles(tempDir, "playwright-auth-*.json");
        foreach (var file in authFiles)
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
                // Ignore file deletion errors
            }
        }
        
        StorageStateFileCache.Clear();
        TestLogger.LogAuthentication($"Cleared all {count} cached sessions and {authFiles.Length} cached files");
    }

    /// <summary>
    /// Gets the count of cached sessions (for diagnostics/testing)
    /// </summary>
    public static int CachedSessionCount => StorageStateFileCache.Count;

    /// <summary>
    /// Checks if a user session is cached (for diagnostics/testing)
    /// </summary>
    public static bool IsUserSessionCached(string email) => StorageStateFileCache.ContainsKey(email);
    
    /// <summary>
    /// Gets diagnostic information about the cache state
    /// </summary>
    public static string GetCacheStatus()
    {
        var users = string.Join(", ", StorageStateFileCache.Keys);
        return $"Cached sessions ({StorageStateFileCache.Count}): [{users}]";
    }
}