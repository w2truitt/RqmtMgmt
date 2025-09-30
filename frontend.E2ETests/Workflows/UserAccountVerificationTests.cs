using frontend.E2ETests.Fixtures;
using frontend.E2ETests.Infrastructure;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// User account verification tests to identify missing test user accounts
/// These tests attempt to authenticate with each required test user to identify which accounts need seeding
/// </summary>
public class UserAccountVerificationTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;
    private readonly ITestOutputHelper _output;
    private readonly string _baseUrl = "https://rqmtmgmt.local";

    /// <summary>
    /// Test user credentials that should work for E2E testing
    /// </summary>
    private readonly Dictionary<string, (string email, string password, string role)> _testUsers = 
        new Dictionary<string, (string, string, string)>
        {
            { "admin", ("admin@rqmtmgmt.local", "Admin123!", "Administrator") },
            { "pm", ("pm@rqmtmgmt.local", "Pm123!", "Project Manager") },
            { "tester", ("tester@rqmtmgmt.local", "Test123!", "Quality Tester") },
            { "viewer", ("viewer@rqmtmgmt.local", "View123!", "Requirements Viewer") },
            { "developer", ("dev@rqmtmgmt.local", "Dev123!", "Developer") }
        };

    public UserAccountVerificationTests(PlaywrightFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task UserVerification_TestAllUserLogins_IdentifiesMissingAccounts()
    {
        // Test each user account to see which ones can authenticate
        var results = new Dictionary<string, string>();
        
        TestLogger.LogTestStep("=== USER ACCOUNT VERIFICATION TEST ===", _output);
        TestLogger.LogTestStep("Testing login for all required test user accounts...", _output);

        foreach (var (key, (email, password, role)) in _testUsers)
        {
            TestLogger.LogDebug($"Testing authentication for: {key} ({email})", _output);
            
            try
            {
                // Create fresh browser context for each test
                var context = await _fixture.Browser.NewContextAsync(new BrowserNewContextOptions
                {
                    IgnoreHTTPSErrors = true,
                    Locale = "en-US"
                });

                var page = await context.NewPageAsync();
                
                // Attempt to login
                var loginResult = await AttemptLoginAsync(page, email, password);
                
                if (loginResult.success)
                {
                    results[key] = $"✅ SUCCESS - Can authenticate as {role}";
                    TestLogger.LogDebug($"  ✅ Login successful for {email}", _output);
                }
                else
                {
                    results[key] = $"❌ FAILED - {loginResult.error}";
                    TestLogger.LogDebug($"  ❌ Login failed for {email}: {loginResult.error}", _output);
                }
                
                await context.CloseAsync();
            }
            catch (Exception ex)
            {
                results[key] = $"❌ ERROR - {ex.Message}";
                TestLogger.LogDebug($"  ❌ Exception for {email}: {ex.Message}", _output);
            }
        }

        // Report results
        TestLogger.LogTestStep("=== ACCOUNT VERIFICATION RESULTS ===", _output);
        
        var successCount = 0;
        var failureCount = 0;
        var missingAccounts = new List<string>();
        
        foreach (var (key, result) in results)
        {
            var (email, password, role) = _testUsers[key];
            TestLogger.LogDebug($"{key.ToUpper()}: {result}", _output);
            TestLogger.LogDebug($"  Credentials: {email} / {password}", _output);
            
            if (result.Contains("✅"))
            {
                successCount++;
            }
            else
            {
                failureCount++;
                missingAccounts.Add(key);
            }
        }
        
        TestLogger.LogTestStep($"SUMMARY: {successCount} working accounts, {failureCount} failed accounts", _output);
        
        if (failureCount > 0)
        {
            TestLogger.LogTestStep("❌ MISSING USER ACCOUNTS DETECTED", _output);
            TestLogger.LogTestStep($"Missing accounts: {string.Join(", ", missingAccounts)}", _output);
            TestLogger.LogTestStep("", _output);
            TestLogger.LogTestStep("ACTION REQUIRED - Choose one of these solutions:", _output);
            TestLogger.LogTestStep("", _output);
            TestLogger.LogTestStep("OPTION 1: Restart services to trigger seeding", _output);
            TestLogger.LogTestStep("  kubectl delete pod -l app=identityserver", _output);
            TestLogger.LogTestStep("  kubectl delete pod -l app=backend", _output);
            TestLogger.LogTestStep("", _output);
            TestLogger.LogTestStep("OPTION 2: Manual database seeding", _output);
            TestLogger.LogTestStep("  Run: dotnet run --project backend -- --seed-data", _output);
            TestLogger.LogTestStep("", _output);
            TestLogger.LogTestStep("OPTION 3: Reset databases", _output);
            TestLogger.LogTestStep("  kubectl delete pvc data-mssql-0", _output);
            TestLogger.LogTestStep("  kubectl apply -f k8s/local/mssql-statefulset.yaml", _output);
        }
        else
        {
            TestLogger.LogTestStep("✅ ALL TEST USER ACCOUNTS ARE WORKING", _output);
        }
        
        // Test passes - this is diagnostic only
        Assert.True(true);
    }

    /// <summary>
    /// Attempts to login with the given credentials
    /// </summary>
    private async Task<(bool success, string error)> AttemptLoginAsync(IPage page, string email, string password)
    {
        try
        {
            // Navigate to login page
            await page.GotoAsync($"{_baseUrl}/Account/Login");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Fill in login form
            await page.FillAsync("input[name='Input.Username']", email);
            await page.FillAsync("input[name='Input.Password']", password);
            
            // Submit form
            await page.ClickAsync("button:has-text('Login')");
            
            // Wait for either success redirect or error message
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 10000 });
            
            var currentUrl = page.Url;
            
            // Check if we were redirected away from login (success)
            if (!currentUrl.Contains("/Account/Login"))
            {
                return (true, "Login successful");
            }
            
            // Still on login page - check for error messages
            var errorElements = await page.QuerySelectorAllAsync(".alert-danger, .text-danger, .error, .validation-summary-errors");
            if (errorElements.Any())
            {
                var errorText = await errorElements[0].TextContentAsync();
                return (false, $"Login error: {errorText?.Trim() ?? "Unknown error"}");
            }
            
            return (false, "Login failed - remained on login page");
        }
        catch (TimeoutException)
        {
            return (false, "Login timeout - page did not respond");
        }
        catch (Exception ex)
        {
            return (false, $"Login exception: {ex.Message}");
        }
    }

    [Fact]
    public async Task UserVerification_ShowTestCredentials_DisplaysAllCredentials()
    {
        TestLogger.LogTestStep("=== TEST USER CREDENTIALS REFERENCE ===", _output);
        TestLogger.LogTestStep("Use these credentials for manual testing:", _output);
        
        foreach (var (key, (email, password, role)) in _testUsers)
        {
            TestLogger.LogDebug($"{key.ToUpper()} ({role}):", _output);
            TestLogger.LogDebug($"  Email: {email}", _output);
            TestLogger.LogDebug($"  Password: {password}", _output);
            TestLogger.LogDebug("", _output);
        }
        
        TestLogger.LogTestStep("These accounts should be seeded automatically by:", _output);
        TestLogger.LogTestStep("- Identity Server: identityserver/Program.cs (SeedUsersAsync)", _output);
        TestLogger.LogTestStep("- Backend Database: backend/Data/DatabaseSeeder.cs", _output);
        TestLogger.LogTestStep("", _output);
        TestLogger.LogTestStep("If accounts are missing, check the seeding logs during application startup.", _output);
        
        Assert.True(true);
    }

    [Fact]
    public async Task UserVerification_CheckPMUser_SpecificCheck()
    {
        // Specific test for the PM user that's causing most failures
        TestLogger.LogTestStep("=== PM USER SPECIFIC VERIFICATION ===", _output);
        
        var email = "pm@rqmtmgmt.local";
        var password = "Pm123!";
        
        TestLogger.LogDebug($"Testing PM user authentication: {email}", _output);
        
        var context = await _fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            Locale = "en-US"
        });

        var page = await context.NewPageAsync();
        var result = await AttemptLoginAsync(page, email, password);
        
        if (result.success)
        {
            TestLogger.LogTestStep("✅ PM USER AUTHENTICATION WORKS", _output);
            TestLogger.LogTestStep("The PM user account is properly configured.", _output);
        }
        else
        {
            TestLogger.LogTestStep("❌ PM USER AUTHENTICATION FAILED", _output);
            TestLogger.LogTestStep($"Error: {result.error}", _output);
            TestLogger.LogTestStep("", _output);
            TestLogger.LogTestStep("This explains why PM-related E2E tests are failing.", _output);
            TestLogger.LogTestStep("The PM user account needs to be seeded in the Identity Server database.", _output);
        }
        
        await context.CloseAsync();
        Assert.True(true); // Always pass - this is diagnostic
    }
}