# Frontend E2E Tests

This project contains end-to-end tests using Playwright for browser automation.

## Structure

- `PageObjects/` - Page object model classes for UI interactions
- `Workflows/` - End-to-end test workflows
- `TestData/` - Test data factories and seeding utilities
- `E2ETestBase.cs` - Base class for all E2E tests

## Dependencies

- **Playwright**: Cross-browser automation
- **Microsoft.AspNetCore.Mvc.Testing**: Web application testing
- **xUnit**: Test runner and assertions

## Setup

1. Install Playwright browsers:
```bash
pwsh bin/Debug/net9.0/playwright.ps1 install
```

## Running Tests

```bash
# Run all E2E tests
dotnet test

# Run with specific browser
dotnet test -- Playwright.BrowserName=firefox

# Run in headed mode (visible browser)
dotnet test -- Playwright.LaunchOptions.Headless=false
```

## Writing Tests

1. Inherit from `E2ETestBase`
2. Use page objects for UI interactions
3. Use `TestDataFactory` for creating test data
4. Use `TestDataSeeder` for API-based data setup
5. Use `CreateTestId()` for unique test identifiers

## Example Test

```csharp
[Fact]
public async Task CanCreateEditAndDeleteRequirement()
{
    // Arrange
    var requirementsPage = new RequirementsPage(Page);
    var uniqueTitle = $"Test Requirement {CreateTestId()}";
    
    // Act - Create requirement
    await requirementsPage.NavigateToAsync();
    await requirementsPage.ClickCreateRequirementAsync();
    await requirementsPage.FillRequirementFormAsync(uniqueTitle, "Test description");
    await requirementsPage.SaveRequirementAsync();
    
    // Assert - Requirement created
    await Expect(Page.Locator($"text={uniqueTitle}")).ToBeVisibleAsync();
}
```

## Page Object Pattern

Page objects encapsulate UI interactions:

```csharp
public class RequirementsPage
{
    private readonly IPage _page;
    
    public RequirementsPage(IPage page) => _page = page;
    
    public async Task NavigateToAsync() => 
        await _page.GotoAsync("/requirements");
    
    public async Task ClickCreateRequirementAsync() => 
        await _page.ClickAsync("[data-testid='create-requirement-button']");
}
```

## Best Practices

- Use Page Object Model for maintainable test code
- Test complete user workflows rather than individual actions
- Use unique test data to avoid conflicts between test runs
- Wait for elements rather than using fixed delays
- Test cross-browser compatibility for critical workflows
- Clean up test data after each test