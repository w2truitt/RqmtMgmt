using frontend.E2ETests.Fixtures;
using frontend.E2ETests.TestData;
using frontend.E2ETests.Services;
using frontend.E2ETests.Infrastructure;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;
using RqmtMgmtShared;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Diagnostic tests for ensuring required test data exists in the system.
/// These tests can be used to seed missing data or verify data integrity.
/// Run these first when setting up a new database environment.
/// </summary>
public class DataSeedingDiagnosticTests : E2ETestBase
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _httpClient;
    private readonly TestDataSeeder _seeder;

    public DataSeedingDiagnosticTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture)
    {
        _output = output;
        _httpClient = new HttpClient { BaseAddress = new Uri("https://rqmtmgmt.local") };
        _seeder = new TestDataSeeder(_httpClient);
    }

    [Fact]
    public async Task DataSeeding_VerifyRequiredProjects_ReportsStatus()
    {
        TestLogger.LogTestStep("=== VERIFYING REQUIRED STATIC PROJECTS ===", _output);
        
        // Define expected projects (must match TestDataFactory.GetStaticProject)
        var expectedProjects = new[]
        {
            "Legacy Requirements",
            "Performance Test Project 638944753472742469",
            "Requirements Test Project 638944753477280014", 
            "Test Project"
        };

        var missingProjects = new List<string>();
        var existingProjects = new List<string>();

        foreach (var projectName in expectedProjects)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Project?search={Uri.EscapeDataString(projectName)}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (content.Contains(projectName))
                    {
                        existingProjects.Add(projectName);
                        TestLogger.LogTestStep($"✅ Found project: {projectName}", _output);
                    }
                    else
                    {
                        missingProjects.Add(projectName);
                        TestLogger.LogTestStep($"❌ Missing project: {projectName}", _output);
                    }
                }
                else
                {
                    missingProjects.Add(projectName);
                    TestLogger.LogTestStep($"❌ Cannot verify project: {projectName} (HTTP {response.StatusCode})", _output);
                }
            }
            catch (Exception ex)
            {
                missingProjects.Add(projectName);
                TestLogger.LogTestStep($"❌ Error checking project '{projectName}': {ex.Message}", _output);
            }
        }

        TestLogger.LogTestStep($"=== PROJECT VERIFICATION RESULTS ===", _output);
        TestLogger.LogTestStep($"Existing projects: {existingProjects.Count}", _output);
        TestLogger.LogTestStep($"Missing projects: {missingProjects.Count}", _output);

        if (missingProjects.Any())
        {
            TestLogger.LogTestStep("❌ SOME REQUIRED PROJECTS ARE MISSING", _output);
            TestLogger.LogTestStep("Run DataSeeding_SeedRequiredProjects_CreatesStaticProjects to fix", _output);
        }
        else
        {
            TestLogger.LogTestStep("✅ ALL REQUIRED PROJECTS EXIST", _output);
        }
    }

    [Fact(Skip = "SeedRequiredTestProjectsAsync method not yet implemented in TestDataSeeder")]
    public async Task DataSeeding_SeedRequiredProjects_CreatesStaticProjects()
    {
        TestLogger.LogTestStep("=== SEEDING REQUIRED STATIC PROJECTS ===", _output);
        
        try
        {
            // TODO: Implement SeedRequiredTestProjectsAsync in TestDataSeeder
            // var results = await _seeder.SeedRequiredTestProjectsAsync();
            var results = new Dictionary<string, string>(); // Placeholder
            
            var created = 0;
            var existing = 0;
            var errors = 0;

            foreach (var (projectCode, result) in results)
            {
                TestLogger.LogTestStep($"{projectCode}: {result}", _output);
                
                if (result.Contains("Created"))
                    created++;
                else if (result.Contains("already exists"))
                    existing++;
                else
                    errors++;
            }

            TestLogger.LogTestStep("=== SEEDING RESULTS ===", _output);
            TestLogger.LogTestStep($"CREATED: {created} projects", _output);
            TestLogger.LogTestStep($"EXISTING: {existing} projects", _output);
            TestLogger.LogTestStep($"ERRORS: {errors} projects", _output);

            if (errors > 0)
            {
                throw new Exception($"Failed to seed {errors} projects. Check backend API connectivity and logs.");
            }

            TestLogger.LogTestStep("✅ PROJECT SEEDING COMPLETED SUCCESSFULLY", _output);
        }
        catch (Exception ex)
        {
            TestLogger.LogTestStep($"❌ PROJECT SEEDING FAILED: {ex.Message}", _output);
            throw;
        }
    }

    [Fact]
    public async Task DataSeeding_VerifyIdentityServerUsers_ChecksAuthentication()
    {
        TestLogger.LogTestStep("=== VERIFYING IDENTITY SERVER USERS ===", _output);
        TestLogger.LogTestStep("Note: This test checks if users can authenticate, indicating they exist in Identity Server", _output);
        
        var authResults = new Dictionary<string, bool>();

        foreach (var (key, (email, password, role, name)) in TestDataSeeder.RequiredTestUsers)
        {
            try
            {
                // Try to get an authentication context (this will fail if user doesn't exist in Identity Server)
                await using var context = await AuthenticationService.GetAuthenticatedContextAsync(
                    Fixture, email, password);
                
                authResults[key] = true;
                TestLogger.LogTestStep($"✅ {key} ({email}): Authentication successful", _output);
            }
            catch (Exception ex)
            {
                authResults[key] = false;
                TestLogger.LogTestStep($"❌ {key} ({email}): Authentication failed - {ex.Message}", _output);
            }
        }

        var workingUsers = authResults.Count(kv => kv.Value);
        var failingUsers = authResults.Count(kv => !kv.Value);

        TestLogger.LogTestStep("=== AUTHENTICATION VERIFICATION RESULTS ===", _output);
        TestLogger.LogTestStep($"Working users: {workingUsers}", _output);
        TestLogger.LogTestStep($"Failing users: {failingUsers}", _output);

        if (failingUsers > 0)
        {
            TestLogger.LogTestStep("❌ SOME USERS CANNOT AUTHENTICATE", _output);
            TestLogger.LogTestStep("This indicates missing users in Identity Server database", _output);
            TestLogger.LogTestStep("SOLUTION: Restart Identity Server pod to trigger user seeding:", _output);
            TestLogger.LogTestStep("  kubectl delete pod -l app=identityserver", _output);
            TestLogger.LogTestStep("  kubectl delete pod -l app=backend", _output);
        }
        else
        {
            TestLogger.LogTestStep("✅ ALL USERS CAN AUTHENTICATE SUCCESSFULLY", _output);
        }
    }

    [Fact]
    public async Task DataSeeding_ShowRecoveryInstructions_DisplaysActions()
    {
        TestLogger.LogTestStep("=== DATA SEEDING RECOVERY INSTRUCTIONS ===", _output);
        TestLogger.LogTestStep("", _output);
        TestLogger.LogTestStep("If E2E tests are failing due to missing data, follow these steps:", _output);
        TestLogger.LogTestStep("", _output);
        TestLogger.LogTestStep("1. VERIFY PROJECTS:", _output);
        TestLogger.LogTestStep("   Run: DataSeeding_VerifyRequiredProjects_ReportsStatus", _output);
        TestLogger.LogTestStep("", _output);
        TestLogger.LogTestStep("2. SEED MISSING PROJECTS:", _output);
        TestLogger.LogTestStep("   Run: DataSeeding_SeedRequiredProjects_CreatesStaticProjects", _output);
        TestLogger.LogTestStep("", _output);
        TestLogger.LogTestStep("3. VERIFY IDENTITY SERVER USERS:", _output);
        TestLogger.LogTestStep("   Run: DataSeeding_VerifyIdentityServerUsers_ChecksAuthentication", _output);
        TestLogger.LogTestStep("", _output);
        TestLogger.LogTestStep("4. FIX MISSING IDENTITY SERVER USERS:", _output);
        TestLogger.LogTestStep("   kubectl delete pod -l app=identityserver", _output);
        TestLogger.LogTestStep("   kubectl delete pod -l app=backend", _output);
        TestLogger.LogTestStep("", _output);
        TestLogger.LogTestStep("5. ALTERNATIVE - MANUAL DATABASE SEEDING:", _output);
        TestLogger.LogTestStep("   dotnet run --project backend -- --seed-data", _output);
        TestLogger.LogTestStep("", _output);
        TestLogger.LogTestStep("6. VERIFY AFTER FIXES:", _output);
        TestLogger.LogTestStep("   Re-run the verification tests to confirm all data is present", _output);
        TestLogger.LogTestStep("", _output);
        TestLogger.LogTestStep("=== REQUIRED TEST USERS ===", _output);
        
        foreach (var (key, (email, password, role, name)) in TestDataSeeder.RequiredTestUsers)
        {
            TestLogger.LogTestStep($"{key}: {email} / {password} ({role})", _output);
        }
        
        TestLogger.LogTestStep("", _output);
        TestLogger.LogTestStep("=== REQUIRED STATIC PROJECTS ===", _output);
        TestLogger.LogTestStep("1. Legacy Requirements (LEG)", _output);
        TestLogger.LogTestStep("2. Performance Test Project 638944753472742469 (PTP3474)", _output);
        TestLogger.LogTestStep("3. Requirements Test Project 638944753477280014 (RTP198)", _output);
        TestLogger.LogTestStep("4. Test Project (TST)", _output);
    }

    public override Task DisposeAsync()
    {
        _httpClient?.Dispose();
        return base.DisposeAsync();
    }
}