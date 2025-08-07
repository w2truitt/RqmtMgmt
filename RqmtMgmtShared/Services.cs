namespace RqmtMgmtShared;

/// <summary>
/// Service interface for requirement operations
/// </summary>
public interface IRequirementService
{
    Task<List<RequirementDto>> GetAllAsync();
    Task<PagedResult<RequirementDto>> GetPagedAsync(PaginationParameters parameters);
    Task<RequirementDto?> GetByIdAsync(int id);
    Task<RequirementDto?> CreateAsync(RequirementDto requirement);
    Task<bool> UpdateAsync(RequirementDto requirement);
    Task<bool> DeleteAsync(int id);
    Task<List<RequirementVersionDto>> GetVersionsAsync(int requirementId);
}

/// <summary>
/// Service interface for test case operations
/// </summary>
public interface ITestCaseService
{
    Task<List<TestCaseDto>> GetAllAsync();
    Task<TestCaseDto?> GetByIdAsync(int id);
    Task<TestCaseDto?> CreateAsync(TestCaseDto testCase);
    Task<bool> UpdateAsync(TestCaseDto testCase);
    Task<bool> DeleteAsync(int id);
}

/// <summary>
/// Service interface for test suite operations
/// </summary>
public interface ITestSuiteService
{
    Task<List<TestSuiteDto>> GetAllAsync();
    Task<TestSuiteDto?> GetByIdAsync(int id);
    Task<TestSuiteDto?> CreateAsync(TestSuiteDto testSuite);
    Task<bool> UpdateAsync(TestSuiteDto testSuite);
    Task<bool> DeleteAsync(int id);
}

/// <summary>
/// Service interface for test plan operations
/// </summary>
public interface ITestPlanService
{
    Task<List<TestPlanDto>> GetAllAsync();
    Task<TestPlanDto?> GetByIdAsync(int id);
    Task<TestPlanDto?> CreateAsync(TestPlanDto testPlan);
    Task<bool> UpdateAsync(TestPlanDto testPlan);
    Task<bool> DeleteAsync(int id);
}

/// <summary>
/// Service interface for user operations
/// </summary>
public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto?> CreateAsync(UserDto user);
    Task<bool> UpdateAsync(UserDto user);
    Task<bool> DeleteAsync(int id);
    Task<List<string>> GetUserRolesAsync(int userId);
    Task AssignRoleAsync(int userId, string role);
    Task RemoveRoleAsync(int userId, string role);
}

/// <summary>
/// Service interface for role operations
/// </summary>
public interface IRoleService
{
    Task<List<RoleDto>> GetAllRolesAsync();
    Task<RoleDto?> CreateRoleAsync(string roleName);
    Task<bool> DeleteRoleAsync(int roleId);
}

/// <summary>
/// Service interface for requirement-test case link operations
/// </summary>
public interface IRequirementTestCaseLinkService
{
    Task<List<RequirementTestCaseLinkDto>> GetLinksForRequirement(int requirementId);
    Task<List<RequirementTestCaseLinkDto>> GetLinksForTestCase(int testCaseId);
    Task AddLink(int requirementId, int testCaseId);
    Task RemoveLink(int requirementId, int testCaseId);
}

/// <summary>
/// Service interface for dashboard operations
/// </summary>
public interface IDashboardService
{
    Task<DashboardStatisticsDto> GetStatisticsAsync();
    Task<List<RecentActivityDto>> GetRecentActivityAsync(int count = 5);
}

/// <summary>
/// Enhanced service interface for dashboard operations with new statistics
/// </summary>
public interface IEnhancedDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<RequirementStatsDto> GetRequirementStatsAsync();
    Task<TestManagementStatsDto> GetTestManagementStatsAsync();
    Task<TestExecutionStatsDto> GetTestExecutionStatsAsync();
    Task<List<RecentActivityDto>> GetRecentActivityAsync(int count = 5);
}

/// <summary>
/// Service interface for test run session management operations
/// </summary>
public interface ITestRunSessionService
{
    Task<List<TestRunSessionDto>> GetAllAsync();
    Task<TestRunSessionDto?> GetByIdAsync(int id);
    Task<TestRunSessionDto?> CreateAsync(TestRunSessionDto testRunSession);
    Task<bool> UpdateAsync(TestRunSessionDto testRunSession);
    Task<bool> DeleteAsync(int id);
    Task<TestRunSessionDto?> StartTestRunSessionAsync(TestRunSessionDto testRunSession);
    Task<bool> CompleteTestRunSessionAsync(int id);
    Task<bool> AbortTestRunSessionAsync(int id);
    Task<List<TestRunSessionDto>> GetActiveSessionsAsync();
}

/// <summary>
/// Service interface for test execution and results tracking
/// </summary>
public interface ITestExecutionService
{
    Task<TestCaseExecutionDto?> ExecuteTestCaseAsync(TestCaseExecutionDto execution);
    Task<bool> UpdateTestCaseExecutionAsync(TestCaseExecutionDto execution);
    Task<TestStepExecutionDto?> UpdateStepResultAsync(TestStepExecutionDto stepExecution);
    Task<List<TestCaseExecutionDto>> GetExecutionsForSessionAsync(int testRunSessionId);
    Task<List<TestStepExecutionDto>> GetStepExecutionsForCaseAsync(int testCaseExecutionId);
    Task<TestExecutionStatsDto> GetExecutionStatsAsync();
    Task<TestExecutionStatsDto> GetExecutionStatsForSessionAsync(int testRunSessionId);
}