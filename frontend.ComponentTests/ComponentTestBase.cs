using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RqmtMgmtShared;
using frontend.Services;

namespace frontend.ComponentTests;

/// <summary>
/// Base class for component tests providing common setup and utilities
/// </summary>
public abstract class ComponentTestBase : TestContext
{
    protected ComponentTestBase()
    {
        // Register interface-based mock services only
        Services.AddSingleton(Mock.Of<IRequirementService>());
        Services.AddSingleton(Mock.Of<ITestCaseService>());
        Services.AddSingleton(Mock.Of<ITestSuiteService>());
        Services.AddSingleton(Mock.Of<ITestPlanService>());
        Services.AddSingleton(Mock.Of<IUserService>());
        Services.AddSingleton(Mock.Of<IRoleService>());
        Services.AddSingleton(Mock.Of<IRequirementTestCaseLinkService>());
        Services.AddSingleton(Mock.Of<IDashboardService>());
        Services.AddSingleton(Mock.Of<IEnhancedDashboardService>());

        // Add Blazor testing services
        Services.AddOptions();
        Services.AddLogging();

        // Add authorization services for testing
        Services.AddAuthorizationCore();
    }
    
    /// <summary>
    /// Gets a mock service for testing
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <returns>Mock instance</returns>
    protected Mock<T> GetMockService<T>() where T : class
    {
        return Mock.Get(Services.GetRequiredService<T>());
    }
    
    /// <summary>
    /// Creates a unique test identifier for test isolation
    /// </summary>
    /// <returns>Unique test ID</returns>
    protected string CreateTestId()
    {
        return Guid.NewGuid().ToString()[..8];
    }
}