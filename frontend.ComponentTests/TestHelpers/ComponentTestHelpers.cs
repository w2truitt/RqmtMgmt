using Bunit;
using Microsoft.AspNetCore.Components;
using RqmtMgmtShared;

namespace frontend.ComponentTests.TestHelpers;

/// <summary>
/// Helper methods for component testing
/// </summary>
public static class ComponentTestHelpers
{
    /// <summary>
    /// Creates a test requirement DTO
    /// </summary>
    /// <param name="testId">Unique test identifier</param>
    /// <returns>Test requirement</returns>
    public static RequirementDto CreateTestRequirement(string testId)
    {
        return new RequirementDto
        {
            Id = 1,
            Title = $"Test Requirement {testId}",
            Description = $"Test description for {testId}",
            Type = "CRS",
            Status = "Draft",
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a test test case DTO
    /// </summary>
    /// <param name="testId">Unique test identifier</param>
    /// <returns>Test test case</returns>
    public static TestCaseDto CreateTestTestCase(string testId)
    {
        return new TestCaseDto
        {
            Id = 1,
            Title = $"Test Case {testId}",
            Description = $"Test case description for {testId}",
            SuiteId = 1,
            Steps = new List<TestStepDto>
            {
                new TestStepDto
                {
                    Id = 1,
                    Description = "Test step description",
                    ExpectedResult = "Expected result"
                }
            },
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a test user DTO
    /// </summary>
    /// <param name="testId">Unique test identifier</param>
    /// <returns>Test user</returns>
    public static UserDto CreateTestUser(string testId)
    {
        return new UserDto
        {
            Id = 1,
            UserName = $"testuser{testId}",
            Email = $"test{testId}@example.com",
            Roles = new List<string> { "QA" }
        };
    }
    
    /// <summary>
    /// Finds an element by data-testid attribute
    /// </summary>
    /// <param name="component">Component to search in</param>
    /// <param name="testId">Test ID to find</param>
    /// <returns>Element or null if not found</returns>
    public static AngleSharp.Dom.IElement? FindByTestId(this IRenderedComponent<ComponentBase> component, string testId)
    {
        return component.Find($"[data-testid='{testId}']");
    }
    
    /// <summary>
    /// Finds all elements by data-testid attribute
    /// </summary>
    /// <param name="component">Component to search in</param>
    /// <param name="testId">Test ID to find</param>
    /// <returns>Collection of elements</returns>
    public static IRefreshableElementCollection<AngleSharp.Dom.IElement> FindAllByTestId(this IRenderedComponent<ComponentBase> component, string testId)
    {
        return component.FindAll($"[data-testid='{testId}']");
    }
}