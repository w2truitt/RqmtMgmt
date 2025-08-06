using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Xunit;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace frontend.E2ETests;

/// <summary>
/// Base class for end-to-end tests using Playwright
/// </summary>
public abstract class E2ETestBase : IAsyncLifetime
{
    protected WebApplicationFactory<Program> Factory { get; private set; } = null!;
    protected IPlaywright PlaywrightInstance { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;
    
    /// <summary>
    /// Initialize test setup
    /// </summary>
    public async Task InitializeAsync()
    {
        // Create web application factory
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Remove all existing DbContext-related services
                    services.RemoveAll(typeof(DbContextOptions));
                    services.RemoveAll(typeof(DbContextOptions<RqmtMgmtDbContext>));
                    services.RemoveAll(typeof(RqmtMgmtDbContext));
                    
                    // Add in-memory database for testing
                    services.AddDbContext<RqmtMgmtDbContext>(options =>
                    {
                        options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                    });
                });
            });
        
        // Initialize Playwright
        PlaywrightInstance = await Playwright.CreateAsync();
        Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true // Set to false for debugging
        });
        
        // Create a new page for each test
        Page = await Browser.NewPageAsync();
        
        // Set base URL to the test server
        var baseUrl = Factory.Server.BaseAddress.ToString();
        await Page.GotoAsync(baseUrl);
    }
    
    /// <summary>
    /// Cleanup test resources
    /// </summary>
    public async Task DisposeAsync()
    {
        await Page?.CloseAsync()!;
        await Browser?.CloseAsync()!;
        PlaywrightInstance?.Dispose();
        Factory?.Dispose();
    }
    
    /// <summary>
    /// Creates a unique test identifier for test isolation
    /// </summary>
    /// <returns>Unique test ID</returns>
    protected string CreateTestId()
    {
        return Guid.NewGuid().ToString()[..8];
    }
    
    /// <summary>
    /// Waits for an element to be visible
    /// </summary>
    /// <param name="selector">Element selector</param>
    /// <param name="timeout">Timeout in milliseconds</param>
    protected async Task WaitForElementAsync(string selector, int timeout = 5000)
    {
        await Page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
        {
            Timeout = timeout
        });
    }
}