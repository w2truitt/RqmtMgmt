using frontend.E2ETests.Fixtures;
using frontend.E2ETests.TestData;
using frontend.E2ETests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Diagnostic tests for checking and seeding missing test data
/// Use these tests to verify and fix missing test users that cause authentication failures
/// </summary>
public class TestDataSeederDiagnosticTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;
    private readonly ITestOutputHelper _output;

    public TestDataSeederDiagnosticTests(PlaywrightFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Diagnostic_CheckRequiredTestUsersStatus_ReportsCurrentState()
    {
        // Arrange
        using var httpClient = new HttpClient { BaseAddress = new Uri("https://rqmtmgmt.local") };
        var seeder = new TestDataSeeder(httpClient);

        // Act - Check status of all required test users
        var status = await seeder.GetTestUserStatusAsync();

        // Assert & Report
        TestLogger.LogTestStep("=== TEST USER STATUS REPORT ===", _output);
        
        var missingCount = 0;
        var existingCount = 0;
        
        foreach (var (key, statusMsg) in status)
        {
            var userInfo = TestDataSeeder.RequiredTestUsers[key];
            TestLogger.LogDebug($"{key.ToUpper()}: {statusMsg}", _output);
            TestLogger.LogDebug($"  Expected: {userInfo.email} | {userInfo.role} | Password: {userInfo.password}", _output);
            
            if (statusMsg.Contains("❌"))
            {
                missingCount++;
            }
            else if (statusMsg.Contains("✅"))
            {
                existingCount++;
            }
        }
        
        TestLogger.LogTestStep($"SUMMARY: {existingCount} users exist, {missingCount} users missing", _output);
        
        if (missingCount > 0)
        {
            TestLogger.LogTestStep("❌ MISSING USERS DETECTED - Run SeedMissingTestUsers test to fix", _output);
        }
        else
        {
            TestLogger.LogTestStep("✅ ALL REQUIRED TEST USERS EXIST", _output);
        }
        
        // This test always passes - it's just for reporting
        Assert.True(true);
    }

    [Fact] 
    public async Task Diagnostic_SeedMissingTestUsers_CreatesApplicationUsers()
    {
        // Arrange
        using var httpClient = new HttpClient { BaseAddress = new Uri("https://rqmtmgmt.local") };
        var seeder = new TestDataSeeder(httpClient);

        // Act - Attempt to seed missing users
        TestLogger.LogTestStep("=== SEEDING MISSING TEST USERS ===", _output);
        
        var results = await seeder.SeedMissingTestUsersAsync();

        // Assert & Report
        var createdCount = 0;
        var existingCount = 0;
        var errorCount = 0;
        
        foreach (var (key, result) in results)
        {
            var userInfo = TestDataSeeder.RequiredTestUsers[key];
            TestLogger.LogDebug($"{key.ToUpper()}: {result}", _output);
            
            if (result.Contains("Created successfully"))
            {
                createdCount++;
                TestLogger.LogDebug($"  ✅ Created: {userInfo.email}", _output);
            }
            else if (result.Contains("Already exists"))
            {
                existingCount++;
            }
            else
            {
                errorCount++;
                TestLogger.LogDebug($"  ❌ Error for: {userInfo.email}", _output);
            }
        }
        
        TestLogger.LogTestStep($"SEEDING RESULTS: {createdCount} created, {existingCount} existing, {errorCount} errors", _output);
        
        if (createdCount > 0)
        {
            TestLogger.LogTestStep("⚠️  NOTE: Users created in APPLICATION database only", _output);
            TestLogger.LogTestStep("⚠️  For full authentication, users must also exist in IDENTITY SERVER", _output);
            TestLogger.LogTestStep("⚠️  If login still fails, check Identity Server seeding", _output);
        }
        
        // This test passes if no critical errors occurred
        Assert.True(errorCount == 0, $"Failed to seed {errorCount} users due to errors");
    }

    [Fact]
    public async Task Diagnostic_VerifyRequiredUsers_ListsMissingUsers()
    {
        // Arrange
        using var httpClient = new HttpClient { BaseAddress = new Uri("https://rqmtmgmt.local") };
        var seeder = new TestDataSeeder(httpClient);

        // Act
        var missingUsers = await seeder.VerifyRequiredTestUsersAsync();

        // Assert & Report
        TestLogger.LogTestStep("=== MISSING USERS VERIFICATION ===", _output);
        
        if (missingUsers.Any())
        {
            TestLogger.LogTestStep($"❌ Found {missingUsers.Count} missing users:", _output);
            foreach (var missing in missingUsers)
            {
                TestLogger.LogDebug($"  • {missing}", _output);
            }
        }
        else
        {
            TestLogger.LogTestStep("✅ All required test users are present", _output);
        }
        
        // This test always passes - it's for reporting
        Assert.True(true);
    }
    
    [Fact]
    public async Task Diagnostic_ShowTestUserCredentials_DisplaysAllCredentials()
    {
        // This test just displays the expected test user credentials for reference
        TestLogger.LogTestStep("=== TEST USER CREDENTIALS REFERENCE ===", _output);
        
        foreach (var (key, (email, password, role, name)) in TestDataSeeder.RequiredTestUsers)
        {
            TestLogger.LogDebug($"{key.ToUpper()}:", _output);
            TestLogger.LogDebug($"  Email: {email}", _output);
            TestLogger.LogDebug($"  Password: {password}", _output);
            TestLogger.LogDebug($"  Role: {role}", _output);
            TestLogger.LogDebug($"  Name: {name}", _output);
            TestLogger.LogDebug("", _output);
        }
        
        TestLogger.LogTestStep("Use these credentials for manual testing or debugging", _output);
        Assert.True(true);
    }
}