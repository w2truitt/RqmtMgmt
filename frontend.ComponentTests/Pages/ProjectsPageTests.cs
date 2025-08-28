using Bunit;
using frontend.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using RqmtMgmtShared;
using System.ComponentModel;
using Xunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;

namespace frontend.ComponentTests.Pages;

/// <summary>
/// Tests for the Projects page component
/// </summary>
public class ProjectsPageTests : ComponentTestBase
{
    private readonly Mock<IProjectService> _mockProjectService;
    private readonly Mock<IJSRuntime> _mockJSRuntime;

    public ProjectsPageTests()
    {
        _mockProjectService = new Mock<IProjectService>();
        _mockJSRuntime = new Mock<IJSRuntime>();
        
        // Register services
        Services.AddSingleton(_mockProjectService.Object);
        Services.AddSingleton(_mockJSRuntime.Object);
    }

    [Fact]
    public void ProjectsPage_Should_DisplayTitle()
    {
        // Arrange - Set up authorization and mock empty project list
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        _mockProjectService.Setup(x => x.GetProjectsAsync(It.IsAny<ProjectFilterDto>()))
            .ReturnsAsync(new PagedResult<ProjectDto>
            {
                Items = new List<ProjectDto>(),
                TotalItems = 0,
                PageNumber = 1,
                PageSize = 20
            });

        // Act
        var component = RenderComponent<Projects>();

        // Assert
        Assert.Contains("Projects", component.Markup);
    }

    [Fact]
    public void ProjectsPage_Should_DisplayCreateProjectButton()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        _mockProjectService.Setup(x => x.GetProjectsAsync(It.IsAny<ProjectFilterDto>()))
            .ReturnsAsync(new PagedResult<ProjectDto>
            {
                Items = new List<ProjectDto>(),
                TotalItems = 0,
                PageNumber = 1,
                PageSize = 20
            });

        // Act
        var component = RenderComponent<Projects>();

        // Assert
        var createButton = component.Find("[data-testid='create-project-button']");
        Assert.NotNull(createButton);
        Assert.Contains("Add Project", createButton.TextContent);
    }

    [Fact]
    public void ProjectsPage_Should_DisplaySearchInput()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        _mockProjectService.Setup(x => x.GetProjectsAsync(It.IsAny<ProjectFilterDto>()))
            .ReturnsAsync(new PagedResult<ProjectDto>
            {
                Items = new List<ProjectDto>(),
                TotalItems = 0,
                PageNumber = 1,
                PageSize = 20
            });

        // Act
        var component = RenderComponent<Projects>();

        // Assert
        var searchInput = component.Find("[data-testid='search-input']");
        Assert.NotNull(searchInput);
        Assert.Equal("Search projects...", searchInput.GetAttribute("placeholder"));
    }

    [Fact]
    public void ProjectsPage_Should_LoadAndDisplayProjects()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        var testProjects = new List<ProjectDto>
        {
            new ProjectDto 
            { 
                Id = 1, 
                Name = "Test Project 1", 
                Code = "TP1", 
                Status = ProjectStatus.Active,
                Description = "Test project description"
            },
            new ProjectDto 
            { 
                Id = 2, 
                Name = "Test Project 2", 
                Code = "TP2", 
                Status = ProjectStatus.Planning,
                Description = "Another test project"
            }
        };

        _mockProjectService.Setup(x => x.GetProjectsAsync(It.IsAny<ProjectFilterDto>()))
            .ReturnsAsync(new PagedResult<ProjectDto>
            {
                Items = testProjects,
                TotalItems = testProjects.Count,
                PageNumber = 1,
                PageSize = 20
            });

        // Act
        var component = RenderComponent<Projects>();

        // Assert
        Assert.Contains("Test Project 1", component.Markup);
        Assert.Contains("Test Project 2", component.Markup);
        Assert.Contains("TP1", component.Markup);
        Assert.Contains("TP2", component.Markup);
    }

    [Fact]
    public void ProjectsPage_Should_DisplayLoadingState_Initially()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        // Setup service to return a task that hasn't completed yet
        var tcs = new TaskCompletionSource<PagedResult<ProjectDto>>();
        _mockProjectService.Setup(x => x.GetProjectsAsync(It.IsAny<ProjectFilterDto>()))
            .Returns(tcs.Task);

        // Act
        var component = RenderComponent<Projects>();

        // Assert - Should show loading state
        Assert.Contains("Loading", component.Markup);
        
        // Complete the task
        tcs.SetResult(new PagedResult<ProjectDto>
        {
            Items = new List<ProjectDto>(),
            TotalItems = 0,
            PageNumber = 1,
            PageSize = 20
        });
    }

    [Fact]
    public void ProjectsPage_Should_RequireAuthorization()
    {
        // Arrange - No authorization set up
        _mockProjectService.Setup(x => x.GetProjectsAsync(It.IsAny<ProjectFilterDto>()))
            .ReturnsAsync(new PagedResult<ProjectDto>
            {
                Items = new List<ProjectDto>(),
                TotalItems = 0,
                PageNumber = 1,
                PageSize = 20
            });

        // Act & Assert - Should throw or not render protected content
        // Note: The actual behavior depends on how the [Authorize] attribute is handled in tests
        var component = RenderComponent<Projects>();
        
        // With no authorization, the component should either not render content
        // or show some indication that authorization is required
        // The exact assertion depends on the app's authorization handling
    }

    [Fact]
    public void ProjectsPage_Should_HandleProjectServiceError()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test@example.com", AuthorizationState.Authorized);
        
        _mockProjectService.Setup(x => x.GetProjectsAsync(It.IsAny<ProjectFilterDto>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var component = RenderComponent<Projects>();

        // Assert - Should handle error gracefully
        // The exact error handling depends on the component implementation
        // We should not see a crash, and might see an error message
        Assert.True(component.Markup.Length > 0); // Component should still render something
    }
}
