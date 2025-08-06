using RqmtMgmtShared;

namespace frontend.E2ETests.TestData;

/// <summary>
/// Factory for creating test data objects
/// </summary>
public static class TestDataFactory
{
    /// <summary>
    /// Creates a test requirement with unique data
    /// </summary>
    /// <param name="testId">Unique test identifier</param>
    /// <returns>Test requirement</returns>
    public static RequirementDto CreateRequirement(string testId)
    {
        return new RequirementDto
        {
            Title = $"E2E Test Requirement {testId}",
            Description = $"This is a test requirement created for E2E testing with ID {testId}",
            Type = RequirementType.CRS,
            Status = RequirementStatus.Draft,
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a test test case with unique data
    /// </summary>
    /// <param name="testId">Unique test identifier</param>
    /// <param name="suiteId">Test suite ID</param>
    /// <returns>Test test case</returns>
    public static TestCaseDto CreateTestCase(string testId, int? suiteId = 1)
    {
        return new TestCaseDto
        {
            Title = $"E2E Test Case {testId}",
            Description = $"This is a test case created for E2E testing with ID {testId}",
            SuiteId = suiteId,
            Steps = new List<TestStepDto>
            {
                new TestStepDto
                {
                    Description = $"First test step for {testId}",
                    ExpectedResult = $"Expected result for step 1 of {testId}"
                },
                new TestStepDto
                {
                    Description = $"Second test step for {testId}",
                    ExpectedResult = $"Expected result for step 2 of {testId}"
                }
            },
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a test user with unique data
    /// </summary>
    /// <param name="testId">Unique test identifier</param>
    /// <returns>Test user</returns>
    public static UserDto CreateUser(string testId)
    {
        return new UserDto
        {
            UserName = $"e2euser{testId}",
            Email = $"e2etest{testId}@example.com",
            Roles = new List<string> { "QA" }
        };
    }
    
    /// <summary>
    /// Creates a test suite with unique data
    /// </summary>
    /// <param name="testId">Unique test identifier</param>
    /// <returns>Test suite</returns>
    public static TestSuiteDto CreateTestSuite(string testId)
    {
        return new TestSuiteDto
        {
            Name = $"E2E Test Suite {testId}",
            Description = $"This is a test suite created for E2E testing with ID {testId}",
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a test plan with unique data
    /// </summary>
    /// <param name="testId">Unique test identifier</param>
    /// <returns>Test plan</returns>
    public static TestPlanDto CreateTestPlan(string testId)
    {
        return new TestPlanDto
        {
            Name = $"E2E Test Plan {testId}",
            Type = "Functional",
            Description = $"This is a test plan created for E2E testing with ID {testId}",
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow
        };
    }
}