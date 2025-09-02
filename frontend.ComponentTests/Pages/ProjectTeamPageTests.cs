using Bunit;
using frontend.Pages;
using frontend.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Moq;
using RqmtMgmtShared;
using Xunit;
using Bunit.TestDoubles;

namespace frontend.ComponentTests.Pages;

/// <summary>
/// Tests for the ProjectTeam page component
/// </summary>
public class ProjectTeamPageTests : ComponentTestBase
{
    private readonly Mock<IProjectService> _mockProjectService;
    private readonly Mock<IProjectContextService> _mockProjectContextService;
    private readonly Mock<IUserService> _mockUserService;

    public ProjectTeamPageTests()
    {
        _mockProjectService = new Mock<IProjectService>();
        _mockProjectContextService = new Mock<IProjectContextService>();
        _mockUserService = new Mock<IUserService>();
        
        // Register services
        Services.AddSingleton(_mockProjectService.Object);
        Services.AddSingleton(_mockProjectContextService.Object);
        Services.AddSingleton(_mockUserService.Object);
    }

    [Fact]
    public void ProjectTeam_Should_DisplayTitle()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        // Mock project data
        var testProject = new ProjectDto
        {
            Id = 1,
            Name = "Test Project",
            Code = "TEST",
            Status = ProjectStatus.Active
        };
        
        _mockProjectService.Setup(x => x.GetProjectByIdAsync(1))
            .ReturnsAsync(testProject);
        
        // Mock team members list
        _mockProjectService.Setup(x => x.GetProjectTeamMembersAsync(1))
            .ReturnsAsync(new List<ProjectTeamMemberDto>());
            
        // Mock users list
        _mockUserService.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<UserDto>());

        // Act
        var component = RenderComponent<ProjectTeam>(parameters => parameters
            .Add(p => p.ProjectId, 1));

        // Assert - Check for title with project name
        Assert.Contains("Team Management - Test Project", component.Markup);
    }

    [Fact]
    public void ProjectTeam_Should_DisplayAddTeamMemberButton()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        _mockProjectService.Setup(x => x.GetProjectTeamMembersAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<ProjectTeamMemberDto>());

        // Act
        var component = RenderComponent<ProjectTeam>(parameters => parameters
            .Add(p => p.ProjectId, 1));

        // Assert
        var addButton = component.Find("[data-testid='add-team-member-button']");
        Assert.NotNull(addButton);
        Assert.Contains("Add Team Member", addButton.TextContent);
    }

    [Fact]
    public void ProjectTeam_Should_DisplayLoadingState_Initially()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        // Mock project data
        var testProject = new ProjectDto
        {
            Id = 1,
            Name = "Test Project",
            Code = "TEST",
            Status = ProjectStatus.Active
        };
        
        _mockProjectService.Setup(x => x.GetProjectByIdAsync(1))
            .ReturnsAsync(testProject);
            
        // Mock users list
        _mockUserService.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<UserDto>());
        
        // Setup service to return a task that hasn't completed yet
        var tcs = new TaskCompletionSource<List<ProjectTeamMemberDto>>();
        _mockProjectService.Setup(x => x.GetProjectTeamMembersAsync(It.IsAny<int>()))
            .Returns(tcs.Task);

        // Act
        var component = RenderComponent<ProjectTeam>(parameters => parameters
            .Add(p => p.ProjectId, 1));

        // Assert - Should show loading state
        Assert.Contains("Loading team members...", component.Markup);
        
        // Complete the task
        tcs.SetResult(new List<ProjectTeamMemberDto>());
    }

    [Fact]
    public void ProjectTeam_Should_DisplayTeamMembers()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        // Mock project data
        var testProject = new ProjectDto
        {
            Id = 1,
            Name = "Test Project",
            Code = "TEST",
            Status = ProjectStatus.Active
        };
        
        _mockProjectService.Setup(x => x.GetProjectByIdAsync(1))
            .ReturnsAsync(testProject);
            
        // Mock users list
        _mockUserService.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<UserDto>());
        
        var testTeamMembers = new List<ProjectTeamMemberDto>
        {
            new ProjectTeamMemberDto
            {
                ProjectId = 1,
                UserId = 1,
                UserName = "John Doe",
                UserEmail = "john.doe@example.com",
                Role = ProjectRole.Developer,
                JoinedAt = DateTime.Now.AddDays(-30),
                IsActive = true
            },
            new ProjectTeamMemberDto
            {
                ProjectId = 1,
                UserId = 2,
                UserName = "Jane Smith",
                UserEmail = "jane.smith@example.com",
                Role = ProjectRole.ProjectOwner,
                JoinedAt = DateTime.Now.AddDays(-60),
                IsActive = true
            }
        };

        _mockProjectService.Setup(x => x.GetProjectTeamMembersAsync(1))
            .ReturnsAsync(testTeamMembers);

        // Act
        var component = RenderComponent<ProjectTeam>(parameters => parameters
            .Add(p => p.ProjectId, 1));

        // Assert
        Assert.Contains("John Doe", component.Markup);
        Assert.Contains("Jane Smith", component.Markup);
        Assert.Contains("john.doe@example.com", component.Markup);
        Assert.Contains("jane.smith@example.com", component.Markup);
        Assert.Contains("Developer", component.Markup);
        Assert.Contains("Project Owner", component.Markup);
    }

    [Fact]
    public void ProjectTeam_Should_DisplayBreadcrumb()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        _mockProjectService.Setup(x => x.GetProjectTeamMembersAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<ProjectTeamMemberDto>());

        // Act
        var component = RenderComponent<ProjectTeam>(parameters => parameters
            .Add(p => p.ProjectId, 1));

        // Assert - Should display breadcrumb navigation
        Assert.Contains("breadcrumb", component.Markup);
        Assert.Contains("Projects", component.Markup);
        Assert.Contains("Team", component.Markup);
    }

    [Fact]
    public void ProjectTeam_Should_DisplayTableHeaders()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        // Mock project data
        var testProject = new ProjectDto
        {
            Id = 1,
            Name = "Test Project",
            Code = "TEST",
            Status = ProjectStatus.Active
        };
        
        _mockProjectService.Setup(x => x.GetProjectByIdAsync(1))
            .ReturnsAsync(testProject);
            
        // Mock users list
        _mockUserService.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<UserDto>());
        
        var testTeamMembers = new List<ProjectTeamMemberDto>
        {
            new ProjectTeamMemberDto { ProjectId = 1, UserId = 1, UserName = "Test User", UserEmail = "test@example.com", Role = ProjectRole.Developer, JoinedAt = DateTime.UtcNow, IsActive = true }
        };

        _mockProjectService.Setup(x => x.GetProjectTeamMembersAsync(1))
            .ReturnsAsync(testTeamMembers);

        // Act
        var component = RenderComponent<ProjectTeam>(parameters => parameters
            .Add(p => p.ProjectId, 1));

        // Assert - Should display table headers
        Assert.Contains("Name", component.Markup);
        Assert.Contains("Email", component.Markup);
        Assert.Contains("Role", component.Markup);
        Assert.Contains("Joined", component.Markup);
        Assert.Contains("Status", component.Markup);
        Assert.Contains("Actions", component.Markup);
    }

    [Fact]
    public void ProjectTeam_Should_HandleEmptyTeamList()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        _mockProjectService.Setup(x => x.GetProjectTeamMembersAsync(1))
            .ReturnsAsync(new List<ProjectTeamMemberDto>());

        // Act
        var component = RenderComponent<ProjectTeam>(parameters => parameters
            .Add(p => p.ProjectId, 1));

        // Assert - Should handle empty list gracefully
        Assert.True(component.Markup.Length > 0);
        // Might show a "no team members" message or empty table
    }

    [Fact]
    public void ProjectTeam_Should_HandleServiceError()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        _mockProjectService.Setup(x => x.GetProjectTeamMembersAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var component = RenderComponent<ProjectTeam>(parameters => parameters
            .Add(p => p.ProjectId, 1));

        // Assert - Should handle error gracefully
        Assert.True(component.Markup.Length > 0);
    }
}
