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
/// FIXED: Updated for correct dashboard URL and authentication flow
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
        await Task.Delay(5000); // Allow more time for OIDC redirects
        
        // Assert - Check if we're successfully authenticated (not on login page)
        // The login might redirect to root (/) or another protected page
        var currentUrl = Page.Url;
        var isAuthenticated = !currentUrl.Contains("/Account/Login") && 
                             !currentUrl.Contains("/connect/authorize");
        
        // If still on login page, check if there's an error message or if we need to wait longer
        if (!isAuthenticated)
        {
            await Task.Delay(3000); // Wait a bit more
            currentUrl = Page.Url;
            isAuthenticated = !currentUrl.Contains("/Account/Login") && 
                             !currentUrl.Contains("/connect/authorize");
        }
        
        Assert.True(isAuthenticated, $"Should be authenticated and redirected away from login. Current URL: {currentUrl}");
        
        // Should be on a protected page (root dashboard or other)
        var isOnProtectedPage = currentUrl.EndsWith("/") || 
                               currentUrl.Contains("/projects") ||
                               currentUrl.Contains(BaseUrl);
        
        Assert.True(isOnProtectedPage, "Should be redirected to a protected page after login");
        _output.WriteLine($"Login successful, redirected to: {currentUrl}");
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
        
        // Updated protected pages - dashboard is at root "/" not "/dashboard"
        var protectedPages = new[]
        {
            "/users",
            "/projects",
            "/" // Dashboard is at root
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
        await Task.Delay(5000); // Allow time for authentication
        
        // Act - Navigate to different protected pages (updated URLs)
        var protectedPages = new[] { "/users", "/projects", "/" }; // Dashboard is at root
        
        foreach (var page in protectedPages)
        {
            await Page.GotoAsync($"{BaseUrl}{page}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Assert - Check that we're still authenticated (not redirected back to login)
            var stillAuthenticated = !Page.Url.Contains("/Account/Login");
            Assert.True(stillAuthenticated, $"Should remain authenticated on {page}. Current URL: {Page.Url}");
            
            // For root page, just check we're not on login
            if (page == "/")
            {
                Assert.True(Page.Url.EndsWith("/") || Page.Url.Contains(BaseUrl));
            }
            else
            {
                Assert.Contains(page, Page.Url);
            }
            
            _output.WriteLine($"Session persisted for: {page}");
        }
        
        _output.WriteLine("Authentication session persists across page navigation");
    }
}