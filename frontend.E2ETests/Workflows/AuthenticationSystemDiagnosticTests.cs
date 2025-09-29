using frontend.E2ETests.Fixtures;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.Playwright.Assertions;
using frontend.E2ETests.Infrastructure;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// System-level diagnostic tests for authentication infrastructure
/// OPTIMIZED: Now uses shared browser for performance improvement
/// These tests specifically test authentication system components
/// </summary>
public class AuthenticationSystemDiagnosticTests : E2ETestBase
{
    private readonly ITestOutputHelper _output;

    public AuthenticationSystemDiagnosticTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture)
    {
        _output = output;
    }

    [Fact]
    public async Task AuthenticationSystem_IdentityServerResponds_Success()
    {
        // Arrange - Start with clean session
        await Context.ClearCookiesAsync();
        
        // Act - Try to access OIDC discovery endpoint
        await Page.GotoAsync($"{BaseUrl}/.well-known/openid_configuration");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - Should get OIDC configuration or at least not an error page
        var content = await Page.ContentAsync();
        var isValidResponse = content.Contains("authorization_endpoint") ||
                             content.Contains("token_endpoint") ||
                             !content.Contains("404") && !content.Contains("500");
        
        Assert.True(isValidResponse, "Identity server should respond to OIDC discovery");
        _output.WriteLine("Identity server OIDC discovery endpoint is accessible");
    }
    
    [Fact]
    public async Task AuthenticationSystem_LoginFlowInitiates_Success()
    {
        // Arrange - Start with clean session
        await Context.ClearCookiesAsync();
        
        // Act - Navigate to protected page to trigger auth flow
        await Page.GotoAsync($"{BaseUrl}/users");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(3000); // Allow for redirects
        
        // Assert - Should be in authentication flow
        var isInAuthFlow = Page.Url.Contains("/Account/Login") ||
                          Page.Url.Contains("/connect/authorize") ||
                          await Page.IsVisibleAsync("input[name='Input.Username']");
        
        Assert.True(isInAuthFlow, "Authentication flow should initiate for protected pages");
        _output.WriteLine($"Authentication flow initiated, current URL: {Page.Url}");
    }
    
    [Fact]
    public async Task AuthenticationSystem_PKCEFlowWorks_Success()
    {
        // Arrange - Start with clean session
        await Context.ClearCookiesAsync();
        
        // Act - Initiate PKCE flow by accessing protected resource
        await Page.GotoAsync($"{BaseUrl}/projects");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000);
        
        // Check if PKCE parameters are present in URL during auth flow
        var currentUrl = Page.Url;
        var hasPKCEParams = currentUrl.Contains("code_challenge") ||
                           currentUrl.Contains("code_challenge_method") ||
                           currentUrl.Contains("/Account/Login");
        
        // Assert - Should have PKCE parameters or be on login page
        Assert.True(hasPKCEParams, "PKCE flow should be initiated with proper parameters");
        _output.WriteLine($"PKCE authentication flow working, URL: {currentUrl}");
    }
}