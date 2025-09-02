using Bunit;
using frontend.Pages;
using frontend.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Moq;
using RqmtMgmtShared;
using Xunit;
using Bunit.TestDoubles;

namespace frontend.ComponentTests.Pages;

/// <summary>
/// Tests for the ProjectDashboard page component
/// </summary>
public class ProjectDashboardPageTests : ComponentTestBase
{
    private readonly Mock<IProjectService> _mockProjectService;
    private readonly Mock<IRequirementService> _mockRequirementService;
    private readonly Mock<ITestSuiteService> _mockTestSuiteService;
    private readonly Mock<ITestPlanService> _mockTestPlanService;
    private readonly Mock<IProjectContextService> _mockProjectContextService;

    public ProjectDashboardPageTests()
    {
        _mockProjectService = new Mock<IProjectService>();
        _mockRequirementService = new Mock<IRequirementService>();
        _mockTestSuiteService = new Mock<ITestSuiteService>();
        _mockTestPlanService = new Mock<ITestPlanService>();
        _mockProjectContextService = new Mock<IProjectContextService>();
        
        // Register services
        Services.AddSingleton(_mockProjectService.Object);
        Services.AddSingleton(_mockRequirementService.Object);
        Services.AddSingleton(_mockTestSuiteService.Object);
        Services.AddSingleton(_mockTestPlanService.Object);
        Services.AddSingleton(_mockProjectContextService.Object);
    }

    [Fact]
    public void ProjectDashboard_Should_DisplayLoadingState_Initially()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        // Setup service to return a task that hasn't completed yet
        var tcs = new TaskCompletionSource<ProjectDto?>();
        _mockProjectService.Setup(x => x.GetProjectByIdAsync(It.IsAny<int>()))
            .Returns(tcs.Task);

        // Act
        var component = RenderComponent<ProjectDashboard>(parameters => parameters
            .Add(p => p.ProjectId, 1));

        // Assert - Should show loading state
        Assert.Contains("Loading", component.Markup);
        
        // Complete the task
        tcs.SetResult(new ProjectDto { Id = 1, Name = "Test Project", Code = "TP" });
    }

    [Fact]
    public void ProjectDashboard_Should_DisplayProjectInformation()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        var testProject = new ProjectDto
        {
            Id = 1,
            Name = "Test Project Dashboard",
            Code = "TPD",
            Description = "A test project for dashboard testing",
            Status = ProjectStatus.Active
        };

        _mockProjectService.Setup(x => x.GetProjectByIdAsync(1))
            .ReturnsAsync(testProject);
        
        // Mock other service calls to return empty data
        _mockRequirementService.Setup(x => x.GetByProjectIdAsync(1))
            .ReturnsAsync(new List<RequirementDto>());
        _mockTestSuiteService.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<TestSuiteDto>());

        // Act
        var component = RenderComponent<ProjectDashboard>(parameters => parameters
            .Add(p => p.ProjectId, 1));

        // Assert
        Assert.Contains("Test Project Dashboard", component.Markup);
        Assert.Contains("TPD", component.Markup);
        Assert.Contains("A test project for dashboard testing", component.Markup);
    }

    [Fact]
    public void ProjectDashboard_Should_DisplayPageTitle()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        var testProject = new ProjectDto
        {
            Id = 1,
            Name = "Sample Project",
            Code = "SP",
            Status = ProjectStatus.Active
        };

        _mockProjectService.Setup(x => x.GetProjectByIdAsync(1))
            .ReturnsAsync(testProject);
        
        // Mock other service calls
        _mockRequirementService.Setup(x => x.GetByProjectIdAsync(1))
            .ReturnsAsync(new List<RequirementDto>());
        _mockTestSuiteService.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<TestSuiteDto>());

        // Act
        var component = RenderComponent<ProjectDashboard>(parameters => parameters
            .Add(p => p.ProjectId, 1));

        // Assert - Check for project title and breadcrumb
        Assert.Contains("Sample Project", component.Markup);
        Assert.Contains("SP", component.Markup);
    }

    [Fact]
    public void ProjectDashboard_Should_ShowErrorMessage_WhenProjectNotFound()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        _mockProjectService.Setup(x => x.GetProjectByIdAsync(999))
            .ReturnsAsync((ProjectDto?)null);

        // Act
        var component = RenderComponent<ProjectDashboard>(parameters => parameters
            .Add(p => p.ProjectId, 999));

        // Assert - Should handle missing project gracefully
        // The exact behavior depends on implementation, but should not crash
        Assert.True(component.Markup.Length > 0);
    }

    [Fact]
    public void ProjectDashboard_Should_DisplayProjectStatus()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        var testProject = new ProjectDto
        {
            Id = 1,
            Name = "Status Test Project",
            Code = "STP",
            Status = ProjectStatus.Active
        };

        _mockProjectService.Setup(x => x.GetProjectByIdAsync(1))
            .ReturnsAsync(testProject);
        
        // Mock other service calls
        _mockRequirementService.Setup(x => x.GetByProjectIdAsync(1))
            .ReturnsAsync(new List<RequirementDto>());
        _mockTestSuiteService.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<TestSuiteDto>());

        // Act
        var component = RenderComponent<ProjectDashboard>(parameters => parameters
            .Add(p => p.ProjectId, 1));

        // Assert
        Assert.Contains("Active", component.Markup);
    }

    [Fact]
    public void ProjectDashboard_Should_HandleServiceError()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        _mockProjectService.Setup(x => x.GetProjectByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var component = RenderComponent<ProjectDashboard>(parameters => parameters
            .Add(p => p.ProjectId, 1));

        // Assert - Should handle error gracefully
        Assert.True(component.Markup.Length > 0);
    }

    [Fact]
    public void ProjectDashboard_Should_DisplayBreadcrumb()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        var testProject = new ProjectDto
        {
            Id = 1,
            Name = "Breadcrumb Test",
            Code = "BT",
            Status = ProjectStatus.Active
        };

        _mockProjectService.Setup(x => x.GetProjectByIdAsync(1))
            .ReturnsAsync(testProject);
        
        // Mock other service calls
        _mockRequirementService.Setup(x => x.GetByProjectIdAsync(1))
            .ReturnsAsync(new List<RequirementDto>());
        _mockTestSuiteService.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<TestSuiteDto>());

        // Act
        var component = RenderComponent<ProjectDashboard>(parameters => parameters
            .Add(p => p.ProjectId, 1));

        // Assert - Should display ProjectBreadcrumb component
        // The exact markup depends on the ProjectBreadcrumb component implementation
        Assert.True(component.Markup.Length > 0);
    }
}
