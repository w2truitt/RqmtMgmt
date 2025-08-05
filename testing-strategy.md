# Testing Strategy for Requirements & Test Management Tool

## 1. Overview

This document outlines the comprehensive testing strategy for the Requirements & Test Management Tool solution. Our testing approach emphasizes quality, reliability, and maintainability through automated testing at multiple levels, with a focus on API integration testing and comprehensive coverage of both happy path and error scenarios.

## 2. Testing Philosophy

- **Quality First**: All features must be thoroughly tested before deployment
- **Automation Over Manual**: Automated tests provide consistent, repeatable validation
- **Real-World Testing**: Integration tests use actual database and HTTP pipeline
- **Type Safety**: Shared DTOs ensure contract consistency across all layers
- **Coverage Goals**: 90-100% for core business logic, 70-90% for auxiliary features

## 3. Testing Architecture Overview

### Technology Stack
- **Test Framework**: xUnit (.NET 9.0)
- **Integration Testing**: `Microsoft.AspNetCore.Mvc.Testing` with `WebApplicationFactory<Program>`
- **Code Coverage**: Coverlet collector for coverage metrics
- **Shared Contracts**: `RqmtMgmtShared` NuGet package for type-safe DTOs
- **HTTP Client Testing**: Built-in ASP.NET Core test client

### Project Structure
```
├── backend.ApiTests/           # API integration tests
│   ├── RequirementApiTests.cs
│   ├── TestCaseApiTests.cs
│   ├── UserApiTests.cs
│   ├── TestSuiteApiTests.cs
│   ├── TestPlanApiTests.cs
│   ├── RoleApiTests.cs
│   └── RequirementTestCaseLinkApiTests.cs
├── backend.Tests/              # Unit tests
└── RqmtMgmtShared/            # Shared DTOs and contracts
```

## 4. API Integration Testing

### Test Class Architecture

Each API controller has a dedicated test class following a consistent pattern:

```csharp
public class XxxApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public XxxApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    // Test methods...
}
```

### In-Memory Test Server Approach

- **Full Integration**: Uses `WebApplicationFactory<Program>` to spin up complete ASP.NET Core test server
- **Real Pipeline**: Tests run against actual API controllers, middleware, and dependency injection
- **No Web Layer Mocking**: True end-to-end HTTP API testing
- **Isolated Clients**: Each test class gets its own HTTP client instance

### Database Testing Strategy

- **Real Database Operations**: Tests use actual Entity Framework context and database
- **Test Data Isolation**: Each test creates and cleans up its own data
- **Realistic Scenarios**: Tests use proper data types and business-realistic scenarios

## 5. Test Coverage Patterns

### CRUD Operations Testing

Every entity follows comprehensive CRUD testing patterns:

#### Happy Path Tests ✅
- Create and retrieve entity
- List all entities with proper serialization
- Update existing entity with validation
- Delete existing entity with confirmation

#### Error Path Tests ✅
- Get non-existent entity returns 404 Not Found
- Update non-existent entity returns 404 Not Found
- Delete non-existent entity returns 404 Not Found
- Invalid input data returns 400 Bad Request

### Advanced Feature Testing

#### Version History & Redline Support
Requirements and Test Cases support full version history:

```csharp
[Fact]
public async Task UpdatingRequirementAddsNewVersion()
{
    // Create initial requirement
    var created = await CreateRequirement();
    
    // Update requirement
    created.Title = "Updated Title";
    await UpdateRequirement(created);
    
    // Verify version history
    var versions = await GetRequirementVersions(created.Id);
    Assert.Equal(2, versions.Count);
    Assert.Equal("Original Title", versions[0].Title);
    Assert.Equal("Updated Title", versions[1].Title);
}
```

#### Complex Relationships
Test Cases with nested Steps:

```csharp
[Fact]
public async Task CanAddAndRemoveTestStep()
{
    var testCase = await CreateTestCase();
    
    // Add step
    var step = await AddTestStep(testCase.Id, stepDto);
    
    // Verify step exists
    var updated = await GetTestCase(testCase.Id);
    Assert.Contains(updated.Steps, s => s.Description == step.Description);
    
    // Remove step
    await RemoveTestStep(testCase.Id, step.Id);
    
    // Verify step removed
    var final = await GetTestCase(testCase.Id);
    Assert.DoesNotContain(final.Steps, s => s.Id == step.Id);
}
```

#### User Role Management
Multi-role assignment and management:

```csharp
[Fact]
public async Task CanAssignGetAndRemoveRoles()
{
    var user = await CreateUser();
    
    // Assign multiple roles
    await AssignRoles(user.Id, ["Admin", "QA"]);
    
    // Verify roles assigned
    var roles = await GetUserRoles(user.Id);
    Assert.Contains("Admin", roles);
    Assert.Contains("QA", roles);
    
    // Remove specific role
    await RemoveRole(user.Id, "Admin");
    
    // Verify role removed
    var updatedRoles = await GetUserRoles(user.Id);
    Assert.DoesNotContain("Admin", updatedRoles);
    Assert.Contains("QA", updatedRoles);
}
```

#### Traceability Links
Requirement-to-Test-Case relationships:

```csharp
[Fact]
public async Task CanCreateAndGetAndDeleteRequirementTestCaseLink()
{
    var (reqId, tcId) = await CreateRequirementAndTestCase();
    
    // Create link
    await CreateLink(reqId, tcId);
    
    // Query by requirement
    var linksByReq = await GetLinksByRequirement(reqId);
    Assert.Contains(linksByReq, l => l.TestCaseId == tcId);
    
    // Query by test case
    var linksByTc = await GetLinksByTestCase(tcId);
    Assert.Contains(linksByTc, l => l.RequirementId == reqId);
    
    // Delete link
    await DeleteLink(reqId, tcId);
    
    // Verify link removed
    var remainingLinks = await GetLinksByRequirement(reqId);
    Assert.DoesNotContain(remainingLinks, l => l.TestCaseId == tcId);
}
```

## 6. Data Management & Test Isolation

### Test Data Creation Strategy

```csharp
// Unique identifiers for test isolation
var uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
var createDto = new RequirementDto
{
    Title = $"Test Requirement {uniqueId}",
    Type = "CRS",
    Status = "Draft",
    Description = "Created by integration test",
    CreatedBy = 1,
    CreatedAt = DateTime.UtcNow
};
```

### Helper Methods for Complex Scenarios

```csharp
private async Task<(int requirementId, int testCaseId)> CreateRequirementAndTestCase()
{
    var req = await CreateRequirement();
    var tc = await CreateTestCase();
    return (req.Id, tc.Id);
}
```

### Cleanup and Verification

```csharp
// Delete entity
var delResp = await _client.DeleteAsync($"/api/requirement/{id}");
delResp.EnsureSuccessStatusCode();

// Verify deletion
var getResp = await _client.GetAsync($"/api/requirement/{id}");
Assert.False(getResp.IsSuccessStatusCode); // Should return 404
```

## 7. HTTP Client Testing Patterns

### Request/Response Handling

```csharp
// POST with JSON body
var response = await _client.PostAsJsonAsync("/api/endpoint", dto);
response.EnsureSuccessStatusCode();
var result = await response.Content.ReadFromJsonAsync<EntityDto>();

// GET with path parameters
var getResp = await _client.GetAsync($"/api/endpoint/{id}");
var entity = await getResp.Content.ReadFromJsonAsync<EntityDto>();

// Error scenario validation
var errorResp = await _client.GetAsync("/api/endpoint/999999");
Assert.False(errorResp.IsSuccessStatusCode);
Assert.Equal(HttpStatusCode.NotFound, errorResp.StatusCode);
```

### Strongly-Typed DTOs

All API communication uses shared DTOs from `RqmtMgmtShared`:
- **Type Safety**: Prevents API contract drift between client and server
- **Intellisense Support**: Full IDE support for API contracts
- **Refactoring Safety**: Changes to DTOs are caught at compile time
- **Documentation**: DTOs serve as living API documentation

## 8. Current Test Coverage Status

### Completed ✅
- **Requirements API**: Full CRUD + versioning + error scenarios
- **Test Cases API**: Full CRUD + steps management + error scenarios  
- **Test Suites API**: Full CRUD + error scenarios
- **Test Plans API**: Full CRUD + error scenarios
- **Users API**: Full CRUD + role management + error scenarios
- **Roles API**: Create, list, delete + error scenarios
- **Traceability Links**: Create, query, delete + error scenarios

### In Progress 🔄
- **Redline API**: Version listing, comparison endpoints
- **Authentication/Authorization**: Role-based access control testing
- **File Upload/Download**: Attachment handling

### Planned 📋
- **Performance Testing**: Load testing for critical endpoints
- **Security Testing**: Input validation, injection prevention
- **Frontend Component Testing**: Blazor component testing with bUnit

## 9. Testing Guidelines for Contributors

### Adding New API Endpoints

When adding a new API endpoint, ensure:

1. **Create corresponding test methods** for both happy path and error scenarios
2. **Follow naming conventions**: `Can[Action][Entity]` for happy path, `[Action]NonExistent[Entity]ReturnsNotFound` for errors
3. **Test data isolation**: Use unique identifiers and clean up test data
4. **Comprehensive coverage**: Test all HTTP methods (GET, POST, PUT, DELETE)
5. **Error scenarios**: Test 404, 400, and other relevant error codes

### Test Method Structure

```csharp
[Fact]
public async Task CanCreateAndGetEntity()
{
    // Arrange
    var createDto = new EntityDto { /* test data */ };
    
    // Act - Create
    var createResp = await _client.PostAsJsonAsync("/api/entity", createDto);
    createResp.EnsureSuccessStatusCode();
    var created = await createResp.Content.ReadFromJsonAsync<EntityDto>();
    
    // Assert - Create
    Assert.NotNull(created);
    Assert.Equal(createDto.Property, created.Property);
    
    // Act - Get
    var getResp = await _client.GetAsync($"/api/entity/{created.Id}");
    getResp.EnsureSuccessStatusCode();
    var fetched = await getResp.Content.ReadFromJsonAsync<EntityDto>();
    
    // Assert - Get
    Assert.NotNull(fetched);
    Assert.Equal(created.Property, fetched.Property);
}
```

### Error Testing Template

```csharp
[Fact]
public async Task GetNonExistentEntityReturnsNotFound()
{
    var resp = await _client.GetAsync("/api/entity/9999999");
    Assert.False(resp.IsSuccessStatusCode);
    Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
}
```

## 10. CI/CD Integration

### Test Execution
- **Automated Runs**: All tests run on every pull request and merge
- **Fast Feedback**: Integration tests complete in under 5 minutes
- **Parallel Execution**: Tests can run in parallel for faster CI/CD

### Coverage Reporting
- **Coverlet Integration**: Automatic code coverage collection
- **Coverage Thresholds**: Build fails if coverage drops below thresholds
- **Coverage Reports**: Generated and published with each build

### Test Data Management
- **Database Seeding**: Minimal seed data for test execution
- **Test Isolation**: Each test run uses isolated test data
- **Cleanup**: Automated cleanup of test artifacts

## 11. Future Testing Enhancements

### Short Term
- **Authentication Testing**: Add OAuth/OIDC integration tests
- **Validation Testing**: Comprehensive input validation scenarios
- **Performance Baselines**: Establish response time baselines

### Medium Term
- **Contract Testing**: API contract validation with tools like Pact
- **Load Testing**: Performance testing under realistic load
- **Security Testing**: Automated security vulnerability scanning

### Long Term
- **End-to-End Testing**: Full user journey automation
- **Chaos Engineering**: Resilience testing under failure conditions
- **A/B Testing**: Framework for feature flag and variant testing

## 12. Troubleshooting Common Issues

### Test Database Issues
- Ensure connection string is configured for test environment
- Verify database migrations are applied before test execution
- Check for database locks or connection pool exhaustion

### Test Data Conflicts
- Use unique identifiers (GUIDs) for test data
- Implement proper test cleanup in finally blocks
- Consider using test-specific database schemas

### Timing Issues
- Use proper async/await patterns throughout
- Avoid Thread.Sleep in favor of proper async waiting
- Consider using test-specific timeouts for slow operations

---

This testing strategy ensures comprehensive coverage, maintainable test code, and reliable validation of the Requirements & Test Management Tool. Regular review and updates of this strategy will keep pace with evolving requirements and best practices. 
## 13. Frontend Testing Strategy

### Overview

The frontend testing strategy employs a three-tier approach to ensure comprehensive coverage of the Blazor WebAssembly application:

1. **Component Testing** - Testing individual Blazor components in isolation using bUnit
2. **Integration Testing** - Testing component interactions with real API services
3. **End-to-End Testing** - Testing complete user workflows across browsers using Playwright

### Frontend Testing Technology Stack

#### Component Testing (bUnit + xUnit)
- **bUnit**: Blazor component testing framework for rendering and interacting with components
- **xUnit**: Test runner and assertion framework
- **Moq**: Mocking framework for dependencies and services
- **Test Utilities**: Custom base classes and helper methods for component testing

#### Integration Testing (bUnit + Real Services)
- **TestHost**: In-memory hosting for integration testing with real services
- **HttpClient Factory**: Mock HTTP client factory for controlled API responses
- **Service Registration**: Real API services with controlled data scenarios

#### End-to-End Testing (Playwright + xUnit)
- **Playwright**: Cross-browser automation for E2E testing
- **WebApplicationFactory**: Full application hosting for E2E scenarios
- **Page Object Model**: Structured approach to page interactions
- **Test Data Management**: Seeding and cleanup utilities for E2E tests

### Frontend Project Structure

```
├── frontend.ComponentTests/    # Blazor component tests
│   ├── Components/
│   │   ├── RequirementsTests/
│   │   │   ├── RequirementsListTests.cs
│   │   │   ├── RequirementFormTests.cs
│   │   │   ├── RequirementDetailsTests.cs
│   │   │   └── RequirementCardTests.cs
│   │   ├── TestManagementTests/
│   │   │   ├── TestSuitesListTests.cs
│   │   │   ├── TestCasesListTests.cs
│   │   │   ├── TestCaseFormTests.cs
│   │   │   ├── TestPlansListTests.cs
│   │   │   └── TestPlanFormTests.cs
│   │   ├── UserManagementTests/
│   │   │   ├── UsersListTests.cs
│   │   │   ├── UserFormTests.cs
│   │   │   └── RoleAssignmentTests.cs
│   │   ├── DashboardTests/
│   │   │   ├── DashboardTests.cs
│   │   │   ├── RequirementsSummaryTests.cs
│   │   │   └── TestExecutionSummaryTests.cs
│   │   └── SharedTests/
│   │       ├── ConfirmDialogTests.cs
│   │       ├── LoadingSpinnerTests.cs
│   │       ├── PaginationTests.cs
│   │       └── DataTableTests.cs
│   ├── TestBase.cs
│   └── TestHelpers/
│       ├── ComponentTestHelpers.cs
│       └── MockServiceHelpers.cs
├── frontend.E2ETests/          # End-to-end tests
│   ├── PageObjects/
│   │   ├── RequirementsPage.cs
│   │   ├── TestCasesPage.cs
│   │   ├── TestPlansPage.cs
│   │   ├── UsersPage.cs
│   │   └── DashboardPage.cs
│   ├── Workflows/
│   │   ├── RequirementsWorkflowTests.cs
│   │   ├── TestManagementWorkflowTests.cs
│   │   ├── UserManagementWorkflowTests.cs
│   │   └── TraceabilityWorkflowTests.cs
│   ├── TestBase.cs
│   └── TestData/
│       ├── TestDataFactory.cs
│       └── TestDataSeeder.cs
```

### Component Testing Strategy

#### Test Base Class Pattern

```csharp
public abstract class ComponentTestBase : TestContext
{
    protected ComponentTestBase()
    {
        // Register common services
        Services.AddSingleton(Mock.Of<IRequirementService>());
        Services.AddSingleton(Mock.Of<ITestCaseService>());
        Services.AddSingleton(Mock.Of<IUserService>());
        
        // Add Blazor testing services
        Services.AddOptions();
        Services.AddLogging();
    }
    
    protected Mock<T> GetMockService<T>() where T : class
    {
        return Mock.Get(Services.GetRequiredService<T>());
    }
}
```

#### Component Test Example

```csharp
public class RequirementFormTests : ComponentTestBase
{
    [Fact]
    public void RequirementForm_RendersCorrectly_WhenCreatingNewRequirement()
    {
        // Arrange
        var component = RenderComponent<RequirementForm>(parameters => parameters
            .Add(p => p.IsNew, true));
        
        // Act & Assert
        Assert.Contains("Create New Requirement", component.Markup);
        Assert.NotNull(component.Find("[data-testid='title-input']"));
        Assert.NotNull(component.Find("[data-testid='description-input']"));
        Assert.NotNull(component.Find("[data-testid='save-button']"));
    }
    
    [Fact]
    public async Task RequirementForm_CallsCreateService_WhenSaveButtonClicked()
    {
        // Arrange
        var mockService = GetMockService<IRequirementService>();
        var component = RenderComponent<RequirementForm>(parameters => parameters
            .Add(p => p.IsNew, true));
        
        // Act
        await component.Find("[data-testid='title-input']").ChangeAsync(new ChangeEventArgs 
        { 
            Value = "Test Requirement" 
        });
        await component.Find("[data-testid='save-button']").ClickAsync();
        
        // Assert
        mockService.Verify(s => s.CreateRequirementAsync(It.IsAny<RequirementDto>()), Times.Once);
    }
}
```

### End-to-End Testing Strategy

#### Page Object Model Pattern

```csharp
public class RequirementsPage
{
    private readonly IPage _page;
    
    public RequirementsPage(IPage page)
    {
        _page = page;
    }
    
    public async Task NavigateToAsync()
    {
        await _page.GotoAsync("/requirements");
    }
    
    public async Task ClickCreateRequirementAsync()
    {
        await _page.ClickAsync("[data-testid='create-requirement-button']");
    }
    
    public async Task FillRequirementFormAsync(string title, string description)
    {
        await _page.FillAsync("[data-testid='title-input']", title);
        await _page.FillAsync("[data-testid='description-input']", description);
    }
    
    public async Task SaveRequirementAsync()
    {
        await _page.ClickAsync("[data-testid='save-button']");
    }
}
```

### Frontend Testing Coverage Goals

#### Component Testing Coverage
- **Target**: 80%+ code coverage for component logic
- **Focus Areas**:
  - Component rendering with various props and states
  - User interaction handling (clicks, form submissions)
  - Conditional rendering based on data and permissions
  - Error state handling and display
  - Loading state management

#### Integration Testing Coverage
- **Target**: 60%+ coverage of API integration scenarios
- **Focus Areas**:
  - API service integration with components
  - Error handling when API calls fail
  - Loading states during API operations
  - Data refresh and state updates
  - Authentication and authorization flows

#### E2E Testing Coverage
- **Target**: 100% coverage of critical user workflows
- **Focus Areas**:
  - Complete CRUD operations for all entities
  - Cross-browser compatibility (Chrome, Firefox, Edge)
  - Responsive design on different viewport sizes
  - Role-based access control workflows
  - Traceability and reporting workflows

### Frontend Testing Best Practices

#### Component Testing Best Practices
1. **Use data-testid attributes** for reliable element selection
2. **Test user interactions** rather than implementation details
3. **Mock external dependencies** to isolate component behavior
4. **Test both happy path and error scenarios**
5. **Use descriptive test names** that explain the scenario

#### End-to-End Testing Best Practices
1. **Use Page Object Model** for maintainable test code
2. **Test complete user workflows** rather than individual actions
3. **Use unique test data** to avoid conflicts between test runs
4. **Wait for elements** rather than using fixed delays
5. **Test cross-browser compatibility** for critical workflows

### CI/CD Integration for Frontend Tests

#### Test Execution Pipeline
```yaml
- name: Run Component Tests
  run: dotnet test frontend.ComponentTests/ --logger trx --collect:"XPlat Code Coverage"
  
- name: Install Playwright
  run: pwsh frontend.E2ETests/bin/Debug/net9.0/playwright.ps1 install
  
- name: Run E2E Tests
  run: dotnet test frontend.E2ETests/ --logger trx
```

#### Quality Gates
- **Component tests** must pass with 80%+ coverage
- **Integration tests** must pass for all API scenarios
- **E2E tests** must pass for critical user workflows
- **Cross-browser E2E tests** must pass on Chrome, Firefox, and Edge

---

This comprehensive frontend testing strategy complements the existing backend testing approach and ensures reliable, maintainable, and thoroughly tested frontend components and workflows. The three-tier approach provides confidence in both individual component behavior and complete user experience across the Requirements & Test Management Tool.