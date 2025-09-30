using frontend.E2ETests.Fixtures;
using frontend.E2ETests.Services;
using Microsoft.Playwright;
using Xunit.Abstractions;
using frontend.E2ETests.Infrastructure;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Enhanced base class for E2E tests that require a pre-authenticated user.
/// Uses browser state detection instead of file caching for more reliable authentication.
/// 
/// Key improvements:
/// - Creates fresh browser contexts for complete test isolation
/// - Detects current browser authentication state
/// - Automatically handles user switching
/// - No stale cache issues
/// - Real-time validation of authentication status
/// - Enables parallel test execution through isolation
/// 
/// Usage Pattern:
/// 1. Call SetUser() or use helper methods (SetAdminUser(), etc.) in constructor
/// 2. Tests automatically start with user already authenticated
/// 3. Use SwitchToUser() within tests to change authenticated user
/// </summary>
public abstract class AuthenticatedE2ETestBase : E2ETestBase
{
    protected readonly ITestOutputHelper Output;
    private string? _userEmail;
    private string? _userPassword;

    protected AuthenticatedE2ETestBase(PlaywrightFixture fixture, ITestOutputHelper output)
        : base(fixture)
    {
        Output = output;
        // Initialize logging system
        TestLogger.Initialize();
    }

    /// <summary>
    /// Sets the user for this test class. Must be called in constructor.
    /// All tests in the class will run as this user with fresh authentication verification.
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="password">User password</param>
    protected void SetUser(string email, string password)
    {
        _userEmail = email;
        _userPassword = password;
    }

    /// <summary>
    /// Creates and verifies authenticated browser context using browser state detection.
    /// This ensures fresh, valid authentication every time with complete test isolation.
    /// </summary>
    public override async Task InitializeAsync()
    {
        if (string.IsNullOrEmpty(_userEmail) || string.IsNullOrEmpty(_userPassword))
        {
            throw new InvalidOperationException(
                "SetUser must be called in constructor to specify the user role. " +
                "Example: SetAdminUser() or SetUser(\"admin@rqmtmgmt.local\", \"Admin123!\")");
        }

        TestLogger.LogTestStep($"Creating authenticated context for: {_userEmail}", Output);

        // Get authenticated context using fresh browser context approach
        Context = await AuthenticationService.GetAuthenticatedContextAsync(
            Fixture, _userEmail, _userPassword);

        Page = await Context.NewPageAsync();
        Page.SetDefaultTimeout(30000);
        Page.SetDefaultNavigationTimeout(30000);

        // Verify authentication is working
        var isAuthenticated = await AuthenticationService.VerifyAuthenticationAsync(Page, _userEmail);
        if (!isAuthenticated)
        {
            throw new Exception($"Failed to establish authenticated session for: {_userEmail}");
        }

        TestLogger.LogTestStep($"Authenticated context verified for: {_userEmail}", Output);
    }

    // ========================================
    // Session Management Helpers
    // ========================================

    /// <summary>
    /// Switches to a different user within the same test.
    /// Logs out current user and logs in as the specified user.
    /// </summary>
    /// <param name="email">New user email</param>
    /// <param name="password">New user password</param>
    protected async Task SwitchToUser(string email, string password)
    {
        if (Context == null || Page == null)
        {
            throw new InvalidOperationException("Test context not initialized");
        }

        TestLogger.LogTestStep($"Switching from {_userEmail} to: {email}", Output);

        await AuthenticationService.SwitchUserAsync(Context, email, password);

        // Verify the switch was successful
        var isAuthenticated = await AuthenticationService.VerifyAuthenticationAsync(Page, email);
        if (!isAuthenticated)
        {
            throw new Exception($"Failed to switch to user: {email}");
        }

        // Update current user tracking
        _userEmail = email;
        _userPassword = password;

        TestLogger.LogTestStep($"Successfully switched to: {email}", Output);
    }

    /// <summary>
    /// Verifies that the current browser session is authenticated as the expected user.
    /// Useful for debugging authentication issues in tests.
    /// </summary>
    protected async Task<bool> VerifyCurrentAuthentication()
    {
        if (Page == null || string.IsNullOrEmpty(_userEmail))
        {
            return false;
        }

        return await AuthenticationService.VerifyAuthenticationAsync(Page, _userEmail);
    }

    /// <summary>
    /// Logs out the current user and clears authentication state.
    /// Useful for testing unauthenticated scenarios.
    /// </summary>
    protected async Task Logout()
    {
        if (Context == null)
        {
            return;
        }

        TestLogger.LogTestStep($"Logging out: {_userEmail}", Output);
        await AuthenticationService.ClearAuthenticationAsync(Context);
        TestLogger.LogTestStep("Logout completed", Output);
    }

    // ========================================
    // User Role Convenience Methods
    // ========================================

    /// <summary>
    /// Convenience method: Sets admin user as the default for this test class.
    /// </summary>
    protected void SetAdminUser()
    {
        SetUser("admin@rqmtmgmt.local", "Admin123!");
    }

    /// <summary>
    /// Convenience method: Sets project manager user as the default for this test class.
    /// </summary>
    protected void SetProjectManagerUser()
    {
        SetUser("pm@rqmtmgmt.local", "PM123!");
    }

    /// <summary>
    /// Convenience method: Sets developer user as the default for this test class.
    /// </summary>
    protected void SetDeveloperUser()
    {
        SetUser("pm@rqmtmgmt.local", "Pm123!"); // Use PM as developer equivalent
    }

    /// <summary>
    /// Convenience method: Sets tester user as the default for this test class.
    /// </summary>
    protected void SetTesterUser()
    {
        SetUser("tester@rqmtmgmt.local", "Test123!");
    }

    /// <summary>
    /// Convenience method: Sets analyst user as the default for this test class.
    /// </summary>
    protected void SetAnalystUser()
    {
        SetUser("analyst@rqmtmgmt.local", "Analyst123!");
    }

    // ========================================
    // Convenience Methods for Switching Users in Tests
    // ========================================

    /// <summary>
    /// Convenience method: Switches to admin user within a test.
    /// </summary>
    protected async Task SwitchToAdminUser()
    {
        await SwitchToUser("admin@rqmtmgmt.local", "Admin123!");
    }

    /// <summary>
    /// Convenience method: Switches to project manager user within a test.
    /// </summary>
    protected async Task SwitchToProjectManagerUser()
    {
        await SwitchToUser("pm@rqmtmgmt.local", "PM123!");
    }

    /// <summary>
    /// Convenience method: Switches to developer user within a test.
    /// </summary>
    protected async Task SwitchToDeveloperUser()
    {
        await SwitchToUser("pm@rqmtmgmt.local", "Pm123!"); // Use PM as developer equivalent
    }

    /// <summary>
    /// Convenience method: Switches to tester user within a test.
    /// </summary>
    protected async Task SwitchToTesterUser()
    {
        await SwitchToUser("tester@rqmtmgmt.local", "Test123!");
    }

    /// <summary>
    /// Convenience method: Switches to analyst user within a test.
    /// </summary>
    protected async Task SwitchToAnalystUser()
    {
        await SwitchToUser("analyst@rqmtmgmt.local", "Analyst123!");
    }

    // ========================================
    // Diagnostics and Debugging
    // ========================================

    /// <summary>
    /// Gets diagnostic information about the current authentication state.
    /// Useful for debugging test failures.
    /// </summary>
    protected async Task<string> GetAuthenticationDiagnostics()
    {
        if (Page == null)
        {
            return "Page not initialized";
        }

        try
        {
            var currentUrl = Page.Url;
            var isOnLoginPage = currentUrl.Contains("/Account/Login");
            var isAuthenticated = await VerifyCurrentAuthentication();

            return $"Current URL: {currentUrl}, Expected User: {_userEmail}, " +
                   $"On Login Page: {isOnLoginPage}, Authenticated: {isAuthenticated}";
        }
        catch (Exception ex)
        {
            return $"Error getting diagnostics: {ex.Message}";
        }
    }
}