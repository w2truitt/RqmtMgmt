using frontend.E2ETests.Fixtures;
using frontend.E2ETests.PageObjects;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using RqmtMgmtShared;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.Playwright.Assertions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for role assignment validation
/// OPTIMIZED: Now uses shared browser and cached authentication for 4-10x performance improvement
/// All tests run as admin user since role assignment requires admin privileges
/// </summary>
public class RoleAssignmentValidationTests : AuthenticatedE2ETestBase
{
    public RoleAssignmentValidationTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture, output)
    {
        // Set admin user for all tests - role assignment validation requires admin privileges
        SetAdminUser();
    }

    [Fact]
    public async Task RoleAssignment_AdminCanAccessAllFeatures_Success()
    {
        // Arrange - Admin user already authenticated via base class
        var adminFeatures = new[]
        {
            ("/users", "User Management"),
            ("/projects", "Project Management"),
            ("/requirements", "Requirements"),
            ("/testcases", "Test Cases"),
            ("/dashboard", "Dashboard")
        };
        
        // Act & Assert - Admin should access all features
        foreach (var (page, featureName) in adminFeatures)
        {
            await Page.GotoAsync($"{BaseUrl}{page}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            Assert.DoesNotContain("/Account/Login", Page.Url);
            Assert.Contains(page, Page.Url);
            
            Output.WriteLine($"Admin successfully accessed {featureName}: {page}");
        }
        
        Output.WriteLine("Admin role has access to all system features");
    }
    
    [Fact]
    public async Task RoleAssignment_ProjectManagerHasProjectAccess_Success()
    {
        // Arrange - Switch to project manager user
        await SwitchToUser("pm@rqmtmgmt.local", "Pm123!");
        
        var pmFeatures = new[]
        {
            ("/dashboard", "Dashboard"),
            ("/projects", "Project Management"),
            ("/requirements", "Requirements")
        };
        
        // Act & Assert - PM should access project-related features
        foreach (var (page, featureName) in pmFeatures)
        {
            await Page.GotoAsync($"{BaseUrl}{page}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            Assert.DoesNotContain("/Account/Login", Page.Url);
            
            Output.WriteLine($"Project Manager successfully accessed {featureName}: {page}");
        }
        
        Output.WriteLine("Project Manager role has appropriate project access");
    }
    
    [Fact]
    public async Task RoleAssignment_TesterHasTestingAccess_Success()
    {
        // Arrange - Switch to tester user
        await SwitchToUser("tester@rqmtmgmt.local", "Test123!");
        
        var testerFeatures = new[]
        {
            ("/dashboard", "Dashboard"),
            ("/testcases", "Test Cases"),
            ("/requirements", "Requirements") // Testers need to see requirements
        };
        
        // Act & Assert - Tester should access testing-related features
        foreach (var (page, featureName) in testerFeatures)
        {
            await Page.GotoAsync($"{BaseUrl}{page}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            Assert.DoesNotContain("/Account/Login", Page.Url);
            
            Output.WriteLine($"Tester successfully accessed {featureName}: {page}");
        }
        
        Output.WriteLine("Tester role has appropriate testing access");
    }
}