# Example: Updating BackendEmailExtraction_VerifyTokenPersistence

This shows how to update an existing test to use the new logging system.

## Before (Original Code)

```csharp
[Fact]
public async Task BackendEmailExtraction_VerifyTokenPersistence()
{
    // Arrange - Admin user already authenticated
    
    Output.WriteLine("Testing authentication token persistence");
    
    // Act - Navigate between multiple pages to test token persistence
    var pages = new[] { "/dashboard", "/projects", "/users", "/requirements" };
    
    foreach (var page in pages)
    {
        await Page.GotoAsync($"{BaseUrl}{page}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - Should remain authenticated
        Assert.DoesNotContain("/Account/Login", Page.Url);
        Assert.Contains(page, Page.Url);
        
        Output.WriteLine($"Token persisted for page: {page}");
    }
    
    Output.WriteLine("Token persistence verification completed");
}
```

## After (Using TestLogger)

```csharp
// Add this using statement at the top of the file
using frontend.E2ETests.Infrastructure;

[Fact]
public async Task BackendEmailExtraction_VerifyTokenPersistence()
{
    // Arrange - Admin user already authenticated
    
    TestLogger.LogTestStep("Testing authentication token persistence", Output);
    
    // Act - Navigate between multiple pages to test token persistence
    var pages = new[] { "/dashboard", "/projects", "/users", "/requirements" };
    
    foreach (var page in pages)
    {
        await Page.GotoAsync($"{BaseUrl}{page}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - Should remain authenticated
        Assert.DoesNotContain("/Account/Login", Page.Url);
        Assert.Contains(page, Page.Url);
        
        TestLogger.LogTestStep($"Token persisted for page: {page}", Output);
    }
    
    TestLogger.LogTestStep("Token persistence verification completed", Output);
}
```

## Output Comparison

### Normal Level (Default)
```
[AUTH] Using cached authentication file for: admin@rqmtmgmt.local
[TEST] Creating authenticated context for: admin@rqmtmgmt.local
[TEST] Authenticated context ready for: admin@rqmtmgmt.local
[TEST] Testing authentication token persistence
[TEST] Token persisted for page: /dashboard
[TEST] Token persisted for page: /projects
[TEST] Token persisted for page: /users
[TEST] Token persisted for page: /requirements
[TEST] Token persistence verification completed
  Passed frontend.E2ETests.Workflows.BackendEmailExtractionWorkflowTests.BackendEmailExtraction_VerifyTokenPersistence [4 s]
```

### Minimal Level (Reduced Output)
```
[TEST] Testing authentication token persistence
[TEST] Token persisted for page: /dashboard
[TEST] Token persisted for page: /projects
[TEST] Token persisted for page: /users
[TEST] Token persisted for page: /requirements
[TEST] Token persistence verification completed
  Passed frontend.E2ETests.Workflows.BackendEmailExtractionWorkflowTests.BackendEmailExtraction_VerifyTokenPersistence [4 s]
```

### Silent Level (Minimal Output)
```
  Passed frontend.E2ETests.Workflows.BackendEmailExtractionWorkflowTests.BackendEmailExtraction_VerifyTokenPersistence [4 s]
```

## Benefits

1. **Categorized Output**: `[TEST]` and `[AUTH]` prefixes make it clear what type of operation is happening
2. **Configurable Verbosity**: Authentication messages can be suppressed while keeping test step information
3. **Consistent Format**: All logging follows the same pattern across the test suite
4. **Better CI/CD**: Use Silent or Minimal levels in automated environments for cleaner output