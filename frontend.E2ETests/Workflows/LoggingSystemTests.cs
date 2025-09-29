using frontend.E2ETests.Fixtures;
using frontend.E2ETests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Tests to verify the new logging system functionality.
/// These tests demonstrate different logging levels and categories.
/// </summary>
public class LoggingSystemTests : AuthenticatedE2ETestBase
{
    public LoggingSystemTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        SetAdminUser();
    }

    [Fact]
    public async Task LoggingSystem_DemonstrateLogLevels_Success()
    {
        TestLogger.LogInfo("=== Logging System Demonstration ===", Output);
        
        TestLogger.LogTestStep("Starting logging level demonstration", Output);
        
        // These will appear at different log levels
        TestLogger.LogAuthentication("This is an authentication message (Normal+)", Output);
        TestLogger.LogTestStep("This is a test step message (Minimal+)", Output);
        TestLogger.LogDebug("This is debug information (Detailed+)", Output);
        TestLogger.LogVerbose("This is verbose internal information (Verbose only)", Output);
        TestLogger.LogError("This is an error message (always visible unless Silent)", Output);
        TestLogger.LogInfo("This is general information (Minimal+)", Output);
        
        // Navigate to a page to demonstrate authentication caching
        await Page.GotoAsync($"{BaseUrl}/dashboard");
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
        
        TestLogger.LogTestStep("Successfully navigated to dashboard", Output);
        
        // Check current log level
        var currentLevel = TestLogger.GetCurrentLevel();
        TestLogger.LogInfo($"Current log level: {currentLevel}", Output);
        
        // Conditional logging example
        if (TestLogger.IsEnabled(TestLogger.LogLevel.Detailed))
        {
            TestLogger.LogDebug("Detailed logging is enabled - showing extra information", Output);
            TestLogger.LogDebug($"Page URL: {Page.Url}", Output);
            TestLogger.LogDebug($"Page title: {await Page.TitleAsync()}", Output);
        }
        
        TestLogger.LogTestStep("Logging demonstration completed", Output);
        TestLogger.LogInfo("=== End Demonstration ===", Output);
    }

    [Fact]
    public async Task LoggingSystem_VerifyAuthenticationLogging_Success()
    {
        TestLogger.LogTestStep("Testing authentication logging integration", Output);
        
        // This test will show how authentication messages are logged
        // The authentication should already be cached, so we'll see cache messages
        
        await Page.GotoAsync($"{BaseUrl}/users");
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
        
        // Verify we're authenticated
        Assert.DoesNotContain("/Account/Login", Page.Url);
        TestLogger.LogTestStep("Authentication verified - user is logged in", Output);
        
        // Navigate to another page to test session persistence
        await Page.GotoAsync($"{BaseUrl}/projects");
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
        
        Assert.DoesNotContain("/Account/Login", Page.Url);
        TestLogger.LogTestStep("Session persistence verified", Output);
    }
}