using Microsoft.Playwright;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests that verify JWT token workflow and email extraction through the backend API.
/// Tests the complete authentication flow from login to API token validation and user data retrieval.
/// </summary>
public class JwtTokenEmailExtractionTests : AuthenticatedE2ETestBase
{
    public JwtTokenEmailExtractionTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task VerifyJwtTokenContainsEmailClaim()
    {
        _output.WriteLine("=== JWT Token Email Claim Verification Test ===");
        
        // Arrange: Login as admin user
        _output.WriteLine("Step 1: Authenticating as admin user...");
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Failed to authenticate as admin user");
        
        // Act: Extract JWT token from browser storage
        _output.WriteLine("Step 2: Extracting JWT token from browser session storage...");
        var tokenData = await ExtractJwtTokenFromStorageAsync();
        Assert.NotNull(tokenData);
        
        _output.WriteLine($"Token extracted successfully. Token type: {tokenData.TokenType}");
        _output.WriteLine($"Token expires at: {tokenData.ExpiresAt}");
        _output.WriteLine($"Token scopes: {string.Join(", ", tokenData.Scopes)}");
        
        // Verify token contains expected claims
        _output.WriteLine("Step 3: Verifying token contains expected claims...");
        Assert.Contains("admin@rqmtmgmt.local", tokenData.Email ?? string.Empty);
        Assert.Contains("Administrator", tokenData.Role ?? string.Empty);
        Assert.Contains("admin@rqmtmgmt.local", tokenData.Name ?? string.Empty);
        Assert.NotNull(tokenData.SubjectId);
        
        _output.WriteLine($"✅ Token claims verified - Email: {tokenData.Email}, Role: {tokenData.Role}");
    }

    [Fact]
    public async Task VerifyBackendApiExtractsEmailFromJwtToken()
    {
        _output.WriteLine("=== Backend API Email Extraction Verification Test ===");
        
        // Arrange: Login as admin user
        _output.WriteLine("Step 1: Authenticating as admin user...");
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Failed to authenticate as admin user");
        
        // Act: Call the /api/User/me endpoint directly
        _output.WriteLine("Step 2: Making direct API call to /api/User/me endpoint...");
        var apiResponse = await CallUserMeApiEndpointAsync();
        
        // Assert: Verify API response
        Assert.True(apiResponse.IsSuccess, $"API call failed: {apiResponse.ErrorMessage}");
        Assert.NotNull(apiResponse.UserData);
        
        _output.WriteLine($"✅ API call successful - Status: {apiResponse.StatusCode}");
        _output.WriteLine($"✅ User data retrieved - Email: {apiResponse.UserData?.Email}");
        _output.WriteLine($"✅ User roles: {string.Join(", ", apiResponse.UserData?.Roles ?? new List<string>())}");
        
        // Verify the email matches expected admin email
        Assert.Equal("admin@rqmtmgmt.local", apiResponse.UserData?.Email);
        Assert.Contains("Administrator", apiResponse.UserData?.Roles ?? new List<string>());
    }

    [Fact]
    public async Task VerifyEndToEndJwtWorkflowWithMultipleUsers()
    {
        _output.WriteLine("=== End-to-End JWT Workflow Test with Multiple Users ===");
        
        var testUsers = new[]
        {
            new { Email = "admin@rqmtmgmt.local", Password = "Admin123!", ExpectedRole = "Administrator" },
            new { Email = "pm@rqmtmgmt.local", Password = "Pm123!", ExpectedRole = "Product Owner" },
            new { Email = "dev@rqmtmgmt.local", Password = "Dev123!", ExpectedRole = "Engineer" },
            new { Email = "tester@rqmtmgmt.local", Password = "Test123!", ExpectedRole = "Quality Assurance" }
        };

        foreach (var testUser in testUsers)
        {
            _output.WriteLine($"\n--- Testing user: {testUser.Email} ---");
            
            // Step 1: Login as the test user
            _output.WriteLine($"Step 1: Logging in as {testUser.Email}...");
            await LogoutAsync(); // Ensure clean state
            var loginSuccess = await EnsureAuthenticatedAsync(testUser.Email, testUser.Password);
            Assert.True(loginSuccess, $"Failed to authenticate as {testUser.Email}");
            
            // Step 2: Verify JWT token contains correct email
            _output.WriteLine("Step 2: Verifying JWT token claims...");
            var tokenData = await ExtractJwtTokenFromStorageAsync();
            Assert.NotNull(tokenData);
            Assert.Contains(testUser.Email, tokenData.Email ?? string.Empty);
            
            // Step 3: Verify backend API extracts email correctly
            _output.WriteLine("Step 3: Verifying backend API email extraction...");
            var apiResponse = await CallUserMeApiEndpointAsync();
            Assert.True(apiResponse.IsSuccess, $"API call failed for {testUser.Email}: {apiResponse.ErrorMessage}");
            Assert.Equal(testUser.Email, apiResponse.UserData?.Email);
            
            // Step 4: Verify role is correctly extracted
            if (!string.IsNullOrEmpty(testUser.ExpectedRole))
            {
                Assert.Contains(testUser.ExpectedRole, apiResponse.UserData?.Roles ?? new List<string>());
                _output.WriteLine($"✅ Role verification passed: {testUser.ExpectedRole}");
            }
            
            _output.WriteLine($"✅ Complete workflow verified for {testUser.Email}");
        }
    }

    [Fact]
    public async Task VerifyJwtTokenIsUsedInRequirementsPageApiCalls()
    {
        _output.WriteLine("=== JWT Token Usage in Requirements Page API Calls ===");
        
        // Arrange: Login as admin user
        _output.WriteLine("Step 1: Authenticating as admin user...");
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Failed to authenticate as admin user");
        
        // Act: Navigate to Requirements page (which calls /api/User/me internally)
        _output.WriteLine("Step 2: Navigating to Requirements page...");
        var navigationSuccess = await NavigateToProtectedPageAsync("/requirements");
        Assert.True(navigationSuccess, "Failed to navigate to Requirements page");
        
        // Wait for page to fully load and API calls to complete
        await WaitForBlazorAppAsync();
        await Task.Delay(3000); // Give time for API calls
        
        // Verify the page loaded successfully (indicates JWT token worked for API calls)
        _output.WriteLine("Step 3: Verifying Requirements page loaded successfully...");
        var pageTitle = await Page.TitleAsync();
        Assert.Contains("Requirements", pageTitle);
        
        // Verify we can see requirements data (indicates API authentication worked)
        var requirementsTable = await Page.WaitForSelectorAsync("table", new PageWaitForSelectorOptions { Timeout = 10000 });
        Assert.NotNull(requirementsTable);
        
        // Check for specific requirements data
        var hasRequirementsData = await Page.IsVisibleAsync("td");
        Assert.True(hasRequirementsData, "Requirements table should contain data");
        
        _output.WriteLine("✅ Requirements page loaded successfully with data");
        _output.WriteLine("✅ JWT token authentication working for Requirements API calls");
    }

    [Fact]
    public async Task VerifyJwtTokenExpirationHandling()
    {
        _output.WriteLine("=== JWT Token Expiration Handling Test ===");
        
        // Arrange: Login as admin user
        _output.WriteLine("Step 1: Authenticating as admin user...");
        var loginSuccess = await LoginAsAdminAsync();
        Assert.True(loginSuccess, "Failed to authenticate as admin user");
        
        // Extract initial token data
        _output.WriteLine("Step 2: Extracting initial token information...");
        var initialTokenData = await ExtractJwtTokenFromStorageAsync();
        Assert.NotNull(initialTokenData);
        
        var expirationTime = DateTimeOffset.FromUnixTimeSeconds(initialTokenData.ExpiresAt);
        var timeUntilExpiration = expirationTime - DateTimeOffset.UtcNow;
        
        _output.WriteLine($"Token expires at: {expirationTime:yyyy-MM-dd HH:mm:ss} UTC");
        _output.WriteLine($"Time until expiration: {timeUntilExpiration.TotalMinutes:F1} minutes");
        
        // Verify token is currently valid
        _output.WriteLine("Step 3: Verifying token is currently valid...");
        var apiResponse = await CallUserMeApiEndpointAsync();
        Assert.True(apiResponse.IsSuccess, $"Initial API call should succeed with valid token: {apiResponse.ErrorMessage}");
        
        _output.WriteLine("✅ Token validation and expiration information retrieved successfully");
        _output.WriteLine("✅ Current token is valid and working with backend API");
        
        // Note: We don't actually wait for expiration in the test as it would take too long
        // This test verifies we can extract expiration info and the token is currently valid
    }

    /// <summary>
    /// Extracts JWT token data from browser session storage
    /// </summary>
    private async Task<JwtTokenData?> ExtractJwtTokenFromStorageAsync()
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
            
            var tokenData = new JwtTokenData
            {
                AccessToken = oidcData.GetProperty("access_token").GetString() ?? "",
                TokenType = oidcData.GetProperty("token_type").GetString() ?? "",
                ExpiresAt = oidcData.GetProperty("expires_at").GetInt64(),
                Scopes = oidcData.GetProperty("scope").GetString()?.Split(' ') ?? Array.Empty<string>()
            };

            // Extract claims from the profile
            if (oidcData.TryGetProperty("profile", out var profile))
            {
                tokenData.Email = profile.TryGetProperty("email", out var email) ? email.GetString() ?? "" : "";
                tokenData.Name = profile.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "";
                tokenData.Role = profile.TryGetProperty("role", out var role) ? role.GetString() ?? "" : "";
                tokenData.SubjectId = profile.TryGetProperty("sub", out var sub) ? sub.GetString() ?? "" : "";
            }

            return tokenData;
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Failed to extract JWT token: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Makes a direct API call to the /api/User/me endpoint using the browser's current authentication
    /// </summary>
    private async Task<ApiResponse<UserData>> CallUserMeApiEndpointAsync()
    {
        try
        {
            // Extract the JWT token from browser sessionStorage
            var tokenJson = await Page.EvaluateAsync<string>(@"() => {
                const oidcKey = 'oidc.user:https://rqmtmgmt.local:rqmtmgmt-frontend';
                const oidcData = sessionStorage.getItem(oidcKey);
                if (!oidcData) return null;
                const parsed = JSON.parse(oidcData);
                return parsed.access_token;
            }");

            if (string.IsNullOrEmpty(tokenJson))
            {
                return new ApiResponse<UserData>
                {
                    IsSuccess = false,
                    StatusCode = 401,
                    ErrorMessage = "No access token found in browser storage"
                };
            }

            // Make authenticated API request with Authorization header
            var response = await Page.Context.APIRequest.GetAsync($"{BaseUrl}/api/User/me", new()
            {
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {tokenJson}" }
                }
            });
            
            var responseBody = await response.TextAsync();
            var statusCode = response.Status;
            
            _output.WriteLine($"API Response Status: {statusCode}");
            _output.WriteLine($"API Response Body: {responseBody}");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

            if (statusCode == 200)
            {
                    var userData = JsonSerializer.Deserialize<UserData>(responseBody, JsonOptions);

                return new ApiResponse<UserData>
                {
                    IsSuccess = true,
                    StatusCode = statusCode,
                    UserData = userData
                };
            }
            else
            {
                return new ApiResponse<UserData>
                {
                    IsSuccess = false,
                    StatusCode = statusCode,
                    ErrorMessage = $"API returned status {statusCode}: {responseBody}"
                };
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"API call failed with exception: {ex.Message}");
            return new ApiResponse<UserData>
            {
                IsSuccess = false,
                StatusCode = 0,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Data structure for JWT token information
    /// </summary>
    private class JwtTokenData
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
    /// Data structure for API response
    /// </summary>
    private class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; } = "";
        public T? UserData { get; set; }
    }

    /// <summary>
    /// Data structure matching the backend UserDto
    /// </summary>
    private class UserData
    {
        public int Id { get; set; }
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public List<string> Roles { get; set; } = new();
    }
}