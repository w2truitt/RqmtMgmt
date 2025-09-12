using System.Collections.Concurrent;
using frontend.E2ETests.Fixtures;
using Microsoft.Playwright;

namespace frontend.E2ETests.Services;

/// <summary>
/// Handles user authentication and caches the StorageState to avoid repeated logins.
/// This service performs UI login only once per user role for the entire test suite,
/// then reuses the cached session for subsequent tests.
/// </summary>
public static class AuthenticationService
{
    private static readonly ConcurrentDictionary<string, string> StorageStateCache = new();
    private static readonly SemaphoreSlim LoginLock = new(1, 1);

    /// <summary>
    /// Gets the authentication storage state for a user.
    /// If not cached, performs a UI login and caches the result.
    /// Subsequent calls for the same user return the cached session instantly.
    /// </summary>
    /// <param name="fixture">Shared browser fixture</param>
    /// <param name="email">User email</param>
    /// <param name="password">User password</param>
    /// <returns>Playwright StorageState JSON for creating authenticated contexts</returns>
    public static async Task<string> GetStorageStateAsync(
        PlaywrightFixture fixture, 
        string email, 
        string password)
    {
        // Return cached session if available (fast path)
        if (StorageStateCache.TryGetValue(email, out var cachedState))
        {
            return cachedState;
        }

        // Thread-safe login for first-time users
        await LoginLock.WaitAsync();
        try
        {
            // Double-check pattern - another thread might have logged in while we waited
            if (StorageStateCache.TryGetValue(email, out var recheckState))
            {
                return recheckState;
            }

            // Perform actual login and cache the result
            var newState = await PerformLoginAndCaptureState(fixture, email, password);
            StorageStateCache[email] = newState;
            return newState;
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
        // Use temporary, isolated context for login
        await using var context = await fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        { 
            IgnoreHTTPSErrors = true 
        });
        
        var page = await context.NewPageAsync();

        try
        {
            // Navigate to protected page to trigger authentication flow
            await page.GotoAsync("https://rqmtmgmt.local/projects");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Fill login form
            await page.Locator("input[name='Input.Username']").FillAsync(email);
            await page.Locator("input[name='Input.Password']").FillAsync(password);
            await page.Locator("button:has-text('Login')").ClickAsync();
            
            // Wait for successful authentication (redirected to projects page)
            await page.WaitForURLAsync("**/projects", new PageWaitForURLOptions { Timeout = 20000 });

            // Capture and return the authentication state (cookies, localStorage, etc.)
            return await context.StorageStateAsync();
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
        StorageStateCache.TryRemove(email, out _);
    }

    /// <summary>
    /// Clears all cached sessions.
    /// Useful for test cleanup or when testing authentication edge cases.
    /// </summary>
    public static void ClearAllSessions()
    {
        StorageStateCache.Clear();
    }

    /// <summary>
    /// Gets the count of cached sessions (for diagnostics/testing)
    /// </summary>
    public static int CachedSessionCount => StorageStateCache.Count;

    /// <summary>
    /// Checks if a user session is cached (for diagnostics/testing)
    /// </summary>
    public static bool IsUserSessionCached(string email) => StorageStateCache.ContainsKey(email);
}