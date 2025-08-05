using System.Text;
using System.Text.Json;
using RqmtMgmtShared;

namespace frontend.E2ETests.TestData;

/// <summary>
/// Helper class for seeding and cleaning up test data via API
/// </summary>
public class TestDataSeeder
{
    private readonly HttpClient _httpClient;
    private readonly List<string> _createdEntities;
    
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