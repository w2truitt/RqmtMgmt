using System.Text;
using System.Text.Json;
using RqmtMgmtShared;

namespace frontend.E2ETests.TestData;

/// <summary>
/// Helper class for seeding and cleaning up test data via API
/// Enhanced to automatically verify and seed missing test users
/// </summary>
public class TestDataSeeder
{
    private readonly HttpClient _httpClient;
    private readonly List<string> _createdEntities;
    
    /// <summary>
    /// Test user accounts that should exist for E2E testing
    /// NOTE: These must match exactly what Identity Server creates in identityserver/Program.cs
    /// </summary>
    public static readonly Dictionary<string, (string email, string password, string role, string name)> RequiredTestUsers = 
        new Dictionary<string, (string, string, string, string)>
        {
            { "admin", ("admin@rqmtmgmt.local", "Admin123!", "Administrator", "System Administrator") },
            { "pm", ("pm@rqmtmgmt.local", "Pm123!", "ProjectManager", "Project Manager") },
            { "tester", ("tester@rqmtmgmt.local", "Test123!", "Tester", "Quality Tester") },
            { "viewer", ("viewer@rqmtmgmt.local", "View123!", "Viewer", "Requirements Viewer") },
            { "developer", ("dev@rqmtmgmt.local", "Dev123!", "Developer", "Developer") }
        };
    
    public TestDataSeeder(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _createdEntities = new List<string>();
    }
    
    /// <summary>
    /// Seeds a requirement via API
    /// </summary>
    /// <param name="requirement">Requirement to create</param>
    /// <returns>Created requirement with ID</returns>
    public async Task<RequirementDto> SeedRequirementAsync(RequirementDto requirement)
    {
        var json = JsonSerializer.Serialize(requirement);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("/api/requirement", content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdRequirement = JsonSerializer.Deserialize<RequirementDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        if (createdRequirement != null)
        {
            _createdEntities.Add($"requirement:{createdRequirement.Id}");
        }
        
        return createdRequirement!;
    }
    
    /// <summary>
    /// Seeds a test case via API
    /// </summary>
    /// <param name="testCase">Test case to create</param>
    /// <returns>Created test case with ID</returns>
    public async Task<TestCaseDto> SeedTestCaseAsync(TestCaseDto testCase)
    {
        var json = JsonSerializer.Serialize(testCase);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("/api/testcase", content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdTestCase = JsonSerializer.Deserialize<TestCaseDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        if (createdTestCase != null)
        {
            _createdEntities.Add($"testcase:{createdTestCase.Id}");
        }
        
        return createdTestCase!;
    }
    
    /// <summary>
    /// Seeds a user via API
    /// </summary>
    /// <param name="user">User to create</param>
    /// <returns>Created user with ID</returns>
    public async Task<UserDto> SeedUserAsync(UserDto user)
    {
        var json = JsonSerializer.Serialize(user);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("/api/user", content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdUser = JsonSerializer.Deserialize<UserDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        if (createdUser != null)
        {
            _createdEntities.Add($"user:{createdUser.Id}");
        }
        
        return createdUser!;
    }
    
    /// <summary>
    /// Ensures all required test users exist in the system
    /// This method checks for missing users and reports what would need to be created
    /// Note: This can only verify users in the application database, not the Identity Server database
    /// </summary>
    /// <returns>List of missing users that need to be seeded</returns>
    public async Task<List<string>> VerifyRequiredTestUsersAsync()
    {
        var missingUsers = new List<string>();
        
        foreach (var (key, (email, password, role, name)) in RequiredTestUsers)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/User/by-email?email={Uri.EscapeDataString(email)}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    missingUsers.Add($"{key} ({email})");
                }
                else if (!response.IsSuccessStatusCode)
                {
                    // If we can't check the user, assume they might be missing
                    missingUsers.Add($"{key} ({email}) - Unable to verify (HTTP {response.StatusCode})");
                }
            }
            catch (Exception ex)
            {
                missingUsers.Add($"{key} ({email}) - Error checking: {ex.Message}");
            }
        }
        
        return missingUsers;
    }
    
    /// <summary>
    /// Attempts to seed missing test users via API
    /// Note: This only creates users in the application database, not in Identity Server
    /// For full authentication, users must also exist in Identity Server with passwords
    /// </summary>
    /// <returns>Results of seeding operations</returns>
    public async Task<Dictionary<string, string>> SeedMissingTestUsersAsync()
    {
        var results = new Dictionary<string, string>();
        
        foreach (var (key, (email, password, role, name)) in RequiredTestUsers)
        {
            try
            {
                // Check if user already exists
                var checkResponse = await _httpClient.GetAsync($"/api/User/by-email?email={Uri.EscapeDataString(email)}");
                if (checkResponse.IsSuccessStatusCode)
                {
                    results[key] = "Already exists";
                    continue;
                }
                
                // User doesn't exist, try to create
                var userDto = new UserDto
                {
                    UserName = email.Split('@')[0], // Use part before @ as username
                    Email = email,
                    Roles = new List<string> { role }
                };
                
                var createdUser = await SeedUserAsync(userDto);
                if (createdUser != null)
                {
                    results[key] = $"Created successfully (ID: {createdUser.Id})";
                }
                else
                {
                    results[key] = "Failed to create - unknown error";
                }
            }
            catch (Exception ex)
            {
                results[key] = $"Error: {ex.Message}";
            }
        }
        
        return results;
    }
    
    /// <summary>
    /// Gets the status of all required test users
    /// </summary>
    /// <returns>Dictionary with user status information</returns>
    public async Task<Dictionary<string, string>> GetTestUserStatusAsync()
    {
        var status = new Dictionary<string, string>();
        
        foreach (var (key, (email, password, role, name)) in RequiredTestUsers)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/User/by-email?email={Uri.EscapeDataString(email)}");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<UserDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    
                    var rolesStr = user?.Roles != null && user.Roles.Any() 
                        ? string.Join(", ", user.Roles) 
                        : "No roles";
                    status[key] = $"✅ Exists (ID: {user?.Id}, Roles: {rolesStr})";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    status[key] = "❌ Missing";
                }
                else
                {
                    status[key] = $"❓ Unknown (HTTP {response.StatusCode})";
                }
            }
            catch (Exception ex)
            {
                status[key] = $"❌ Error: {ex.Message}";
            }
        }
        
        return status;
    }
    
    /// <summary>
    /// Cleans up all seeded test data
    /// </summary>
    public async Task CleanupAsync()
    {
        foreach (var entity in _createdEntities.AsEnumerable().Reverse())
        {
            var parts = entity.Split(':');
            var entityType = parts[0];
            var entityId = parts[1];
            
            try
            {
                await _httpClient.DeleteAsync($"/api/{entityType}/{entityId}");
            }
            catch
            {
                // Ignore cleanup errors - entity might already be deleted
            }
        }
        
        _createdEntities.Clear();
    }
}