using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Quick test to verify PM password fix
/// </summary>
public class PMPasswordValidationTests : AuthenticatedE2ETestBase
{
    public PMPasswordValidationTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task PMLogin_WorksWithCorrectedPassword()
    {
        // Test that PM user can log in with the corrected password "Pm123!"
        var loginSuccess = await LoginAsProjectManagerAsync();
        Assert.True(loginSuccess, "PM user should be able to login with corrected password PM123!");
        
        _output.WriteLine("✅ PM user successfully logged in with corrected password PM123!");
    }
}
