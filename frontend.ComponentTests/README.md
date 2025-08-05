# Frontend Component Tests

This project contains unit tests for Blazor components using bUnit, xUnit, and Moq.

## Structure

- `Components/` - Test files organized by feature area
  - `RequirementsTests/` - Tests for requirements management components
  - `TestManagementTests/` - Tests for test management components
  - `UserManagementTests/` - Tests for user management components
  - `DashboardTests/` - Tests for dashboard components
  - `SharedTests/` - Tests for shared/common components
- `TestHelpers/` - Helper classes and utilities for testing
- `ComponentTestBase.cs` - Base class for all component tests

## Dependencies

- **bUnit**: Blazor component testing framework
- **xUnit**: Test runner and assertions
- **Moq**: Mocking framework for services
- **Coverlet**: Code coverage collection

## Running Tests

```bash
# Run all component tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "RequirementsListTests"
```

## Writing Tests

1. Inherit from `ComponentTestBase`
2. Use `GetMockService<T>()` to get mock services
3. Use `RenderComponent<T>()` to render components
4. Use `FindByTestId()` helper for element selection
5. Use `CreateTestId()` for unique test data

## Example Test

```csharp
[Fact]
public void RequirementForm_RendersCorrectly_WhenCreatingNewRequirement()
{
    // Arrange
    var component = RenderComponent<RequirementForm>(parameters => parameters
        .Add(p => p.IsNew, true));
    
    // Act & Assert
    Assert.Contains("Create New Requirement", component.Markup);
    Assert.NotNull(component.FindByTestId("title-input"));
    Assert.NotNull(component.FindByTestId("save-button"));
}
```

## Best Practices

- Use `data-testid` attributes for reliable element selection
- Mock external dependencies to isolate component behavior
- Test both happy path and error scenarios
- Use descriptive test names that explain the scenario
- Keep tests focused and independent