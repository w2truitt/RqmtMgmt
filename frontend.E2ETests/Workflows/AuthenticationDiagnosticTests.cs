using frontend.E2ETests.Fixtures;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.Playwright.Assertions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// Diagnostic tests for authentication system functionality
/// OPTIMIZED: Now uses shared browser for performance improvement
/// These tests specifically test authentication flows, so they use E2ETestBase (not AuthenticatedE2ETestBase)
/// </summary>
public class AuthenticationDiagnosticTests : E2ETestBase
{
    private readonly ITestOutputHelper _output;

    public AuthenticationDiagnosticTests(PlaywrightFixture fixture, ITestOutputHelper output) 
        : base(fixture)
    {
        _output = output;
    }

    [Fact]
    public async Task Authentication_LoginPageAccessible_Success()
    {
        // Arrange - Start with clean session
        await Context.ClearCookiesAsync();
        
        // Act - Navigate to login page
        await Page.GotoAsync($"{BaseUrl}/Account/Login");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - Should be on login page
        Assert.Contains("/Account/Login", Page.Url);
        
        // Should have login form elements
        var hasLoginForm = await Page.IsVisibleAsync("input[name='Input.Username']") &&
                          await Page.IsVisibleAsync("input[name='Input.Password']") &&
                          await Page.IsVisibleAsync("button:has-text('Login')");
        
        Assert.True(hasLoginForm, "Login page should have username, password, and login button");
        _output.WriteLine("Login page is accessible and has required form elements");
    }
    
    [Fact]
    public async Task Authentication_ValidLoginWorks_Success()
    {
        // Arrange - Start with clean session
        await Context.ClearCookiesAsync();
        
        // Act - Perform login
        await Page.GotoAsync($"{BaseUrl}/Account/Login");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        await Page.FillAsync("input[name='Input.Username']", "admin@rqmtmgmt.local");
        await Page.FillAsync("input[name='Input.Password']", "Admin123!");
        await Page.ClickAsync("button:has-text('Login')");
        
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(3000); // Allow for OIDC redirects
        
        // Assert - Should be redirected away from login page
        Assert.DoesNotContain("/Account/Login", Page.Url);
        
        // Should be on a protected page
        var isOnProtectedPage = Page.Url.Contains("/dashboard") || 
                               Page.Url.Contains("/projects") ||
                               Page.Url.Contains(BaseUrl);
        
        Assert.True(isOnProtectedPage, "Should be redirected to a protected page after login");
        _output.WriteLine($"Login successful, redirected to: {Page.Url}");
    }
    
    [Fact]
    public async Task Authentication_InvalidLoginFails_Success()
    {
        // Arrange - Start with clean session
        await Context.ClearCookiesAsync();
        
        // Act - Attempt login with invalid credentials
        await Page.GotoAsync($"{BaseUrl}/Account/Login");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        await Page.FillAsync("input[name='Input.Username']", "invalid@example.com");
        await Page.FillAsync("input[name='Input.Password']", "wrongpassword");
        await Page.ClickAsync("button:has-text('Login')");
        
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000);
        
        // Assert - Should remain on login page or show error
        var isStillOnLogin = Page.Url.Contains("/Account/Login") ||
                            await Page.IsVisibleAsync("input[name='Input.Username']");
        
        Assert.True(isStillOnLogin, "Should remain on login page with invalid credentials");
        _output.WriteLine("Invalid login correctly rejected");
    }
    
    [Fact]
    public async Task Authentication_ProtectedPagesRedirectToLogin_Success()
    {
        // Arrange - Start with clean session
        await Context.ClearCookiesAsync();
        
        var protectedPages = new[]
        {
            "/users",
            "/projects",
            "/dashboard"
        };
        
        // Act & Assert - Try to access protected pages without authentication
        foreach (var page in protectedPages)
        {
            await Page.GotoAsync($"{BaseUrl}{page}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(2000); // Allow for redirects
            
            // Should be redirected to login or OIDC auth flow
            var isRedirectedToAuth = Page.Url.Contains("/Account/Login") ||
                                    Page.Url.Contains("/connect/authorize");
            
            Assert.True(isRedirectedToAuth, $"Protected page {page} should redirect to authentication");
            _output.WriteLine($"Protected page {page} correctly redirects to authentication");
        }
        
        _output.WriteLine("All protected pages correctly require authentication");
    }
    
    [Fact]
    public async Task Authentication_SessionPersistsAfterLogin_Success()
    {
        // Arrange - Start with clean session and login
        await Context.ClearCookiesAsync();
        
        // Login first
        await Page.GotoAsync($"{BaseUrl}/Account/Login");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        await Page.FillAsync("input[name='Input.Username']", "admin@rqmtmgmt.local");
        await Page.FillAsync("input[name='Input.Password']", "Admin123!");
        await Page.ClickAsync("button:has-text('Login')");
        
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(3000);
        
        // Act - Navigate to different protected pages
        var protectedPages = new[] { "/users", "/projects", "/dashboard" };
        
        foreach (var page in protectedPages)
        {
            await Page.GotoAsync($"{BaseUrl}{page}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Assert - Should remain authenticated
            Assert.DoesNotContain("/Account/Login", Page.Url);
            Assert.Contains(page, Page.Url);
            
            _output.WriteLine($"Session persisted for: {page}");
        }
        
        _output.WriteLine("Authentication session persists across page navigation");
    }
}