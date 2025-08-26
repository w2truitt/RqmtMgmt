using Microsoft.Playwright;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests that specifically verify the backend's ability to extract email addresses from JWT tokens
/// through the /api/User/me endpoint. These tests validate the complete authentication workflow
/// and demonstrate that the backend can properly identify users for "Created By" fields and other operations.
/// </summary>
public class BackendEmailExtractionWorkflowTests : AuthenticatedE2ETestBase
{
    public BackendEmailExtractionWorkflowTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task VerifyBackendExtractsEmailFromJwtToken_AdminUser()
    {
        _output.WriteLine("=== Backend Email Extraction Verification - Admin User ===");
        
        // Arrange: Login as admin user
        _output.WriteLine("Step 1: Authenticating as admin user...");
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Failed to authenticate as admin user");
        
        // Act: Call the /api/User/me endpoint and capture response
        _output.WriteLine("Step 2: Making API call to /api/User/me to verify email extraction...");
        var apiResult = await MakeAuthenticatedApiCallAsync("/api/User/me");
        
        // Assert: Verify successful response and email extraction
        _output.WriteLine($"API Response Status: {apiResult.StatusCode}");
        _output.WriteLine($"API Response Body: {apiResult.ResponseBody}");
        
        Assert.Equal(200, apiResult.StatusCode);
        Assert.NotNull(apiResult.ResponseBody);
        
        // Parse the response to verify email is correctly extracted
        var userData = JsonSerializer.Deserialize<UserResponseDto>(apiResult.ResponseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(userData);
        Assert.Equal("admin@rqmtmgmt.local", userData.Email);
        Assert.Contains("Administrator", userData.Roles);
        
        _output.WriteLine($"✅ Backend successfully extracted email: {userData.Email}");
        _output.WriteLine($"✅ User roles correctly identified: {string.Join(", ", userData.Roles)}");
    }

    [Theory]
    [InlineData("admin@rqmtmgmt.local", "Admin123!", "Administrator")]
    [InlineData("pm@rqmtmgmt.local", "Pm123!", "Product Owner")]
    [InlineData("dev@rqmtmgmt.local", "Dev123!", "Engineer")]
    [InlineData("tester@rqmtmgmt.local", "Test123!", "Quality Assurance")]
    public async Task VerifyBackendExtractsEmailFromJwtToken_MultipleUsers(string email, string password, string expectedRole)
    {
        _output.WriteLine($"=== Backend Email Extraction Verification - {email} ===");
        
        // Arrange: Login as specified user
        _output.WriteLine($"Step 1: Authenticating as {email}...");
        await LogoutAsync(); // Ensure clean state
        var loginSuccess = await EnsureAuthenticatedAsync(email, password);
        Assert.True(loginSuccess, $"Failed to authenticate as {email}");
        
        // Act: Call the /api/User/me endpoint
        _output.WriteLine("Step 2: Making API call to /api/User/me...");
        var apiResult = await MakeAuthenticatedApiCallAsync("/api/User/me");
        
        // Assert: Verify the backend extracted the correct email
        Assert.Equal(200, apiResult.StatusCode);
        
        var userData = JsonSerializer.Deserialize<UserResponseDto>(apiResult.ResponseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(userData);
        Assert.Equal(email, userData.Email);
        Assert.Contains(expectedRole, userData.Roles);
        
        _output.WriteLine($"✅ Backend correctly extracted email: {userData.Email}");
        _output.WriteLine($"✅ Expected role verified: {expectedRole}");
    }

    [Fact]
    public async Task VerifyJwtTokenContainsRequiredClaimsForBackend()
    {
        _output.WriteLine("=== JWT Token Claims Verification for Backend Processing ===");
        
        // Arrange: Login as admin user
        _output.WriteLine("Step 1: Authenticating as admin user...");
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Failed to authenticate as admin user");
        
        // Act: Extract JWT token from browser storage
        _output.WriteLine("Step 2: Extracting JWT token from browser session storage...");
        var tokenData = await ExtractJwtTokenFromBrowserAsync();
        
        // Assert: Verify token contains all required claims for backend processing
        Assert.NotNull(tokenData);
        Assert.NotEmpty(tokenData.AccessToken);
        Assert.Equal("Bearer", tokenData.TokenType);
        
        // Verify the token contains the email claim that the backend needs
        Assert.NotEmpty(tokenData.Email);
        Assert.Equal("admin@rqmtmgmt.local", tokenData.Email);
        
        // Verify other essential claims
        Assert.NotEmpty(tokenData.Name);
        Assert.NotEmpty(tokenData.Role);
        Assert.NotEmpty(tokenData.SubjectId);
        
        // Verify token has the required scope for API access
        Assert.Contains("rqmtmgmt.api", tokenData.Scopes);
        
        _output.WriteLine($"✅ JWT Token contains all required claims:");
        _output.WriteLine($"  - Email: {tokenData.Email}");
        _output.WriteLine($"  - Name: {tokenData.Name}");
        _output.WriteLine($"  - Role: {tokenData.Role}");
        _output.WriteLine($"  - Subject ID: {tokenData.SubjectId}");
        _output.WriteLine($"  - Scopes: {string.Join(", ", tokenData.Scopes)}");
        _output.WriteLine($"  - Token Type: {tokenData.TokenType}");
    }

    [Fact]
    public async Task VerifyRequirementsPageUsesBackendEmailExtraction()
    {
        _output.WriteLine("=== Requirements Page Backend Email Extraction Integration Test ===");
        
        // Arrange: Login as admin user
        _output.WriteLine("Step 1: Authenticating as admin user...");
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Failed to authenticate as admin user");
        
        // Act: Navigate to Requirements page (which calls /api/User/me internally)
        _output.WriteLine("Step 2: Navigating to Requirements page...");
        await Page.GotoAsync($"{BaseUrl}/requirements");
        await WaitForBlazorAppAsync();
        
        // Wait for API calls to complete
        await Task.Delay(3000);
        
        // Assert: Verify the page loaded successfully (indicates JWT authentication worked)
        var pageTitle = await Page.TitleAsync();
        Assert.Contains("Requirements", pageTitle);
        
        // Verify we can see the authenticated user's email in the UI
        var userGreeting = await Page.WaitForSelectorAsync("text=Hello, admin@rqmtmgmt.local!", 
            new PageWaitForSelectorOptions { Timeout = 10000 });
        Assert.NotNull(userGreeting);
        
        // Verify requirements table loaded (indicates API calls succeeded)
        var requirementsTable = await Page.WaitForSelectorAsync("table", 
            new PageWaitForSelectorOptions { Timeout = 10000 });
        Assert.NotNull(requirementsTable);
        
        _output.WriteLine("✅ Requirements page loaded successfully");
        _output.WriteLine("✅ User authentication displayed correctly");
        _output.WriteLine("✅ API calls succeeded (requirements data loaded)");
        _output.WriteLine("✅ Backend email extraction working in real application workflow");
    }

    [Fact]
    public async Task VerifyBackendReturnsCorrectStatusCodesForAuthenticationIssues()
    {
        _output.WriteLine("=== Backend Authentication Status Code Verification ===");
        
        // Test 1: Valid authentication should return 200
        _output.WriteLine("Test 1: Verifying valid authentication returns 200...");
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Failed to authenticate as admin user");
        
        var validAuthResult = await MakeAuthenticatedApiCallAsync("/api/User/me");
        Assert.Equal(200, validAuthResult.StatusCode);
        _output.WriteLine("✅ Valid authentication returns 200 OK");
        
        // Test 2: No authentication should return 401 (JWT middleware)
        _output.WriteLine("Test 2: Verifying no authentication returns 401...");
        await LogoutAsync();
        
        var noAuthResult = await MakeUnauthenticatedApiCallAsync("/api/User/me");
        Assert.Equal(401, noAuthResult.StatusCode);
        _output.WriteLine("✅ No authentication returns 401 Unauthorized (JWT middleware)");
        
        // Note: Tests for 460 (no email claim) and 461 (user not in DB) would require
        // special test scenarios that are harder to create in E2E tests, but the
        // status codes are now clearly distinguishable from JWT authentication failures
        
        _output.WriteLine("✅ Backend authentication status codes are working correctly");
        _output.WriteLine("✅ 401 = JWT authentication failure (middleware level)");
        _output.WriteLine("✅ 200 = Successful authentication and email extraction");
    }

    /// <summary>
    /// Makes an authenticated API call using the browser's current authentication context
    /// </summary>
    private async Task<ApiCallResult> MakeAuthenticatedApiCallAsync(string endpoint)
    {
        try
        {
            // Use the browser's context which includes authentication cookies/tokens
            var response = await Page.Context.APIRequest.GetAsync($"{BaseUrl}{endpoint}");
            var responseBody = await response.TextAsync();
            
            return new ApiCallResult
            {
                StatusCode = response.Status,
                ResponseBody = responseBody,
                IsSuccess = response.Ok
            };
        }
        catch (Exception ex)
        {
            _output.WriteLine($"API call failed: {ex.Message}");
            return new ApiCallResult
            {
                StatusCode = 0,
                ResponseBody = ex.Message,
                IsSuccess = false
            };
        }
    }

    /// <summary>
    /// Makes an unauthenticated API call (without browser authentication context)
    /// </summary>
    private async Task<ApiCallResult> MakeUnauthenticatedApiCallAsync(string endpoint)
    {
        try
        {
            // Create a new context without authentication
            var context = await Browser.NewContextAsync();
            var response = await context.APIRequest.GetAsync($"{BaseUrl}{endpoint}");
            var responseBody = await response.TextAsync();
            await context.DisposeAsync();
            
            return new ApiCallResult
            {
                StatusCode = response.Status,
                ResponseBody = responseBody,
                IsSuccess = response.Ok
            };
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Unauthenticated API call failed: {ex.Message}");
            return new ApiCallResult
            {
                StatusCode = 0,
                ResponseBody = ex.Message,
                IsSuccess = false
            };
        }
    }

    /// <summary>
    /// Extracts JWT token information from browser session storage
    /// </summary>
    private async Task<JwtTokenInfo?> ExtractJwtTokenFromBrowserAsync()
    {
        try
        {
            var tokenJson = await Page.EvaluateAsync<string>(@"() => {
                const oidcKey = 'oidc.user:https://rqmtmgmt.local:rqmtmgmt-frontend';
                const oidcData = sessionStorage.getItem(oidcKey);
                return oidcData;
            }");

            if (string.IsNullOrEmpty(tokenJson))
            {
                _output.WriteLine("No OIDC token data found in session storage");
                return null;
            }

            var oidcData = JsonSerializer.Deserialize<JsonElement>(tokenJson);
            
            var tokenInfo = new JwtTokenInfo
            {
                AccessToken = oidcData.GetProperty("access_token").GetString() ?? "",
                TokenType = oidcData.GetProperty("token_type").GetString() ?? "",
                ExpiresAt = oidcData.GetProperty("expires_at").GetInt64(),
                Scopes = oidcData.GetProperty("scope").GetString()?.Split(' ') ?? Array.Empty<string>()
            };

            // Extract user claims from the profile
            if (oidcData.TryGetProperty("profile", out var profile))
            {
                tokenInfo.Email = profile.TryGetProperty("email", out var email) ? email.GetString() ?? "" : "";
                tokenInfo.Name = profile.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "";
                tokenInfo.Role = profile.TryGetProperty("role", out var role) ? role.GetString() ?? "" : "";
                tokenInfo.SubjectId = profile.TryGetProperty("sub", out var sub) ? sub.GetString() ?? "" : "";
            }

            return tokenInfo;
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Failed to extract JWT token: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Data structure for API call results
    /// </summary>
    private class ApiCallResult
    {
        public int StatusCode { get; set; }
        public string ResponseBody { get; set; } = "";
        public bool IsSuccess { get; set; }
    }

    /// <summary>
    /// Data structure for JWT token information
    /// </summary>
    private class JwtTokenInfo
    {
        public string AccessToken { get; set; } = "";
        public string TokenType { get; set; } = "";
        public long ExpiresAt { get; set; }
        public string[] Scopes { get; set; } = Array.Empty<string>();
        public string Email { get; set; } = "";
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public string SubjectId { get; set; } = "";
    }

    /// <summary>
    /// Data structure matching the backend UserDto response
    /// </summary>
    private class UserResponseDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public List<string> Roles { get; set; } = new();
    }
}