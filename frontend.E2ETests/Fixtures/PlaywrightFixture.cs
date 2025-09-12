using Microsoft.Playwright;
using Xunit;

namespace frontend.E2ETests.Fixtures;

/// <summary>
/// Manages singleton Playwright and Browser instances shared across test classes.
/// Dramatically reduces browser startup overhead by creating browser once per test collection.
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    private static readonly string[] BrowserArgs = {
        "--no-sandbox",
        "--disable-setuid-sandbox", 
        "--disable-dev-shm-usage", // Use /tmp instead of /dev/shm for shared memory
        "--disable-gpu",
        "--disable-web-security",
        "--ignore-certificate-errors", // Trust self-signed certificates
        "--ignore-ssl-errors", // Ignore SSL errors
        "--ignore-certificate-errors-spki-list", // Ignore certificate pinning
        "--ignore-certificate-errors-skip-list", // Skip certificate error list
        "--memory-pressure-off", // Disable memory pressure simulation
        "--max_old_space_size=512" // Limit V8 memory usage
    };
    
    public IPlaywright PlaywrightInstance { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    /// <summary>
    /// One-time browser setup for entire test collection.
    /// This replaces the expensive per-test browser creation.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Initialize Playwright with resource-optimized settings
        PlaywrightInstance = await Playwright.CreateAsync();
        Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true, // Always headless for resource efficiency (set to false for debugging)
            Args = BrowserArgs
        });
    }

    /// <summary>
    /// Cleanup browser resources at end of test collection.
    /// </summary>
    public async Task DisposeAsync()
    {
        try
        {
            if (Browser != null)
            {
                await Browser.CloseAsync();
            }
        }
        catch (Exception)
        {
            // Ignore cleanup errors
        }
        
        try
        {
            PlaywrightInstance?.Dispose();
        }
        catch (Exception)
        {
            // Ignore cleanup errors
        }
        
        // Force garbage collection to free memory immediately
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}