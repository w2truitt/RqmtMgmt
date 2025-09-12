using frontend.E2ETests.Fixtures;
using frontend.E2ETests.Services;
using Microsoft.Playwright;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Base class for E2E tests that require a pre-authenticated user.
/// Uses AuthenticationService for cached session management, eliminating repeated login flows.
/// 
/// Performance improvement: ~12-15 seconds saved per test by reusing authentication sessions.
/// 
/// Usage Pattern:
/// 1. Call SetUser() or use helper methods (SetAdminUser(), etc.) in constructor
/// 2. Tests automatically start with user already authenticated
/// 3. No need for explicit login calls in test methods
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
    }

    /// <summary>
    /// Sets the user for this test class. Must be called in constructor.
    /// All tests in the class will run as this user with cached authentication.
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="password">User password</param>
    protected void SetUser(string email, string password)
    {
        _userEmail = email;
        _userPassword = password;
    }

    /// <summary>
    /// Creates pre-authenticated browser context using cached session.
    /// This replaces the expensive login flow with instant session restoration.
    /// </summary>
    public override async Task InitializeAsync()
    {
        if (string.IsNullOrEmpty(_userEmail) || string.IsNullOrEmpty(_userPassword))
        {
            throw new InvalidOperationException(
                "SetUser must be called in constructor to specify the user role. " +
                "Example: SetAdminUser() or SetUser(\"admin@rqmtmgmt.local\", \"Admin123!\")");
        }

        Output.WriteLine($"Creating authenticated context for: {_userEmail}");

        // Get cached session (login only happens once per user role across entire test suite)
        var storageState = await AuthenticationService.GetStorageStateAsync(
            Fixture, _userEmail, _userPassword);

        // Create context with pre-authenticated session
        Context = await Fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageState = storageState,
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 }
        });

        Page = await Context.NewPageAsync();
        Page.SetDefaultTimeout(30000);
        Page.SetDefaultNavigationTimeout(30000);

        Output.WriteLine($"Authenticated context ready for: {_userEmail}");
    }

    // ========================================
    // Session Management Helpers
    // ========================================

    /// <summary>
    /// Clears the current user's cached session and re-authenticates.
    /// Useful for testing authentication edge cases or session expiration.
    /// </summary>
    protected async Task ClearSessionAndReauthenticate()
    {
        if (string.IsNullOrEmpty(_userEmail))
        {
            throw new InvalidOperationException("No user set for session clearing");
        }

        Output.WriteLine($"Clearing session and re-authenticating: {_userEmail}");
        
        AuthenticationService.ClearUserSession(_userEmail);
        await DisposeAsync();
        await InitializeAsync();
        
        Output.WriteLine($"Re-authentication complete for: {_userEmail}");
    }

    /// <summary>
    /// Switches to a different user role within the same test.
    /// Clears current context and creates new authenticated context for different user.
    /// </summary>
    /// <param name="email">New user email</param>
    /// <param name="password">New user password</param>
    protected async Task SwitchToUser(string email, string password)
    {
        Output.WriteLine($"Switching from {_userEmail} to {email}");
        
        await DisposeAsync();
        SetUser(email, password);
        await InitializeAsync();
        
        Output.WriteLine($"User switch complete, now authenticated as: {email}");
    }

    // ========================================
    // User Role Convenience Methods
    // ========================================

    /// <summary>
    /// Sets the test class to run as admin user.
    /// Call this in the constructor for admin-focused test classes.
    /// </summary>
    protected void SetAdminUser() => SetUser("admin@rqmtmgmt.local", "Admin123!");

    /// <summary>
    /// Sets the test class to run as project manager user.
    /// Call this in the constructor for PM-focused test classes.
    /// </summary>
    protected void SetProjectManagerUser() => SetUser("pm@rqmtmgmt.local", "Pm123!");

    /// <summary>
    /// Sets the test class to run as developer user.
    /// Call this in the constructor for developer-focused test classes.
    /// </summary>
    protected void SetDeveloperUser() => SetUser("dev@rqmtmgmt.local", "Dev123!");

    /// <summary>
    /// Sets the test class to run as tester user.
    /// Call this in the constructor for tester-focused test classes.
    /// </summary>
    protected void SetTesterUser() => SetUser("tester@rqmtmgmt.local", "Test123!");

    /// <summary>
    /// Sets the test class to run as viewer user.
    /// Call this in the constructor for viewer-focused test classes.
    /// </summary>
    protected void SetViewerUser() => SetUser("viewer@rqmtmgmt.local", "View123!");

    // ========================================
    // Legacy Compatibility Methods
    // ========================================
    // These methods are kept for backward compatibility but are no longer needed
    // for normal test execution since authentication is handled automatically.

    /// <summary>
    /// [DEPRECATED] Authentication is handled automatically in InitializeAsync.
    /// This method always returns true for backward compatibility.
    /// </summary>
    [Obsolete("Authentication is handled automatically. Remove this call from tests.")]
    protected Task<bool> LoginAsAdminAsync() => Task.FromResult(true);

    /// <summary>
    /// [DEPRECATED] Authentication is handled automatically in InitializeAsync.
    /// This method always returns true for backward compatibility.
    /// </summary>
    [Obsolete("Authentication is handled automatically. Remove this call from tests.")]
    protected Task<bool> LoginAsProjectManagerAsync() => Task.FromResult(true);

    /// <summary>
    /// [DEPRECATED] Authentication is handled automatically in InitializeAsync.
    /// This method always returns true for backward compatibility.
    /// </summary>
    [Obsolete("Authentication is handled automatically. Remove this call from tests.")]
    protected Task<bool> LoginAsDeveloperAsync() => Task.FromResult(true);

    /// <summary>
    /// [DEPRECATED] Authentication is handled automatically in InitializeAsync.
    /// This method always returns true for backward compatibility.
    /// </summary>
    [Obsolete("Authentication is handled automatically. Remove this call from tests.")]
    protected Task<bool> LoginAsTesterAsync() => Task.FromResult(true);

    /// <summary>
    /// [DEPRECATED] Authentication is handled automatically in InitializeAsync.
    /// This method always returns true for backward compatibility.
    /// </summary>
    [Obsolete("Authentication is handled automatically. Remove this call from tests.")]
    protected Task<bool> LoginAsViewerAsync() => Task.FromResult(true);
}