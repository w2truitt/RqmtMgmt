using Microsoft.JSInterop;
using Moq;
using RqmtMgmtShared;
using frontend.Services;
using Xunit;

namespace frontend.ComponentTests.Services;

/// <summary>
/// Unit tests for ProjectContextService covering project context management and persistence
/// </summary>
public class ProjectContextServiceTests
{
    private readonly Mock<IProjectService> _mockProjectService;
    private readonly Mock<IJSRuntime> _mockJSRuntime;
    private readonly ProjectContextService _service;

    public ProjectContextServiceTests()
    {
        _mockProjectService = new Mock<IProjectService>();
        _mockJSRuntime = new Mock<IJSRuntime>();
        _service = new ProjectContextService(_mockProjectService.Object, _mockJSRuntime.Object);
    }

    [Fact]
    public void CurrentProject_InitiallyNull()
    {
        // Act & Assert
        Assert.Null(_service.CurrentProject);
        Assert.False(_service.IsInProjectContext);
    }

    [Fact]
    public async Task SetCurrentProjectAsync_WithProject_SetsProjectAndRaisesEvent()
    {
        // Arrange
        var project = new ProjectDto
        {
            Id = 1,
            Name = "Test Project",
            Code = "TEST01",
            Status = ProjectStatus.Active
        };

        var eventRaised = false;
        ProjectDto? eventProject = null;
        _service.ProjectChanged += (p) => { eventRaised = true; eventProject = p; };

        // Act
        await _service.SetCurrentProjectAsync(project);

        // Assert
        Assert.Equal(project, _service.CurrentProject);
        Assert.True(_service.IsInProjectContext);
        Assert.True(eventRaised);
        Assert.Equal(project, eventProject);
    }

    [Fact]
    public async Task SetCurrentProjectAsync_WithNull_ClearsProjectAndRaisesEvent()
    {
        // Arrange - First set a project
        var project = new ProjectDto
        {
            Id = 1,
            Name = "Test Project",
            Code = "TEST01",
            Status = ProjectStatus.Active
        };

        await _service.SetCurrentProjectAsync(project);

        var eventRaised = false;
        ProjectDto? eventProject = null;
        _service.ProjectChanged += (p) => { eventRaised = true; eventProject = p; };

        // Act - Clear the project
        await _service.SetCurrentProjectAsync(null);

        // Assert
        Assert.Null(_service.CurrentProject);
        Assert.False(_service.IsInProjectContext);
        Assert.True(eventRaised);
        Assert.Null(eventProject);
    }

    [Fact]
    public async Task LoadPersistedProjectContextAsync_WithValidId_LoadsProject()
    {
        // Arrange
        var project = new ProjectDto
        {
            Id = 123,
            Name = "Stored Project",
            Code = "STORED01",
            Status = ProjectStatus.Active
        };

        _mockJSRuntime
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync("123");

        _mockProjectService
            .Setup(x => x.GetProjectByIdAsync(123))
            .ReturnsAsync(project);

        // Act
        var result = await _service.LoadPersistedProjectContextAsync();

        // Assert
        Assert.True(result);
        Assert.Equal(project, _service.CurrentProject);
        Assert.True(_service.IsInProjectContext);

        _mockProjectService.Verify(x => x.GetProjectByIdAsync(123), Times.Once);
    }

    [Fact]
    public async Task LoadPersistedProjectContextAsync_WithInvalidId_DoesNotLoadProject()
    {
        // Arrange
        _mockJSRuntime
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync("invalid");

        // Act
        var result = await _service.LoadPersistedProjectContextAsync();

        // Assert
        Assert.False(result);
        Assert.Null(_service.CurrentProject);
        Assert.False(_service.IsInProjectContext);

        _mockProjectService.Verify(x => x.GetProjectByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task LoadPersistedProjectContextAsync_WithNullStorage_DoesNotLoadProject()
    {
        // Arrange
        _mockJSRuntime
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _service.LoadPersistedProjectContextAsync();

        // Assert
        Assert.False(result);
        Assert.Null(_service.CurrentProject);
        Assert.False(_service.IsInProjectContext);

        _mockProjectService.Verify(x => x.GetProjectByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task LoadProjectContextFromRouteAsync_WithValidRoute_ReturnsTrue()
    {
        // Arrange
        var project = new ProjectDto
        {
            Id = 123,
            Name = "Route Project",
            Code = "ROUTE01",
            Status = ProjectStatus.Active
        };

        _mockProjectService
            .Setup(x => x.GetProjectByIdAsync(123))
            .ReturnsAsync(project);

        // Act
        var result = await _service.LoadProjectContextFromRouteAsync("/projects/123/requirements");

        // Assert
        Assert.True(result);
        Assert.Equal(project, _service.CurrentProject);
        Assert.True(_service.IsInProjectContext);

        _mockProjectService.Verify(x => x.GetProjectByIdAsync(123), Times.Once);
    }

    [Fact]
    public async Task LoadProjectContextFromRouteAsync_WithInvalidRoute_ReturnsFalse()
    {
        // Act
        var result = await _service.LoadProjectContextFromRouteAsync("/dashboard");

        // Assert
        Assert.False(result);
        Assert.Null(_service.CurrentProject);
        Assert.False(_service.IsInProjectContext);

        _mockProjectService.Verify(x => x.GetProjectByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task LoadProjectContextFromRouteAsync_WithInvalidProjectId_ReturnsFalse()
    {
        // Act
        var result = await _service.LoadProjectContextFromRouteAsync("/projects/invalid/requirements");

        // Assert
        Assert.False(result);
        Assert.Null(_service.CurrentProject);
        Assert.False(_service.IsInProjectContext);

        _mockProjectService.Verify(x => x.GetProjectByIdAsync(It.IsAny<int>()), Times.Never);
    }
}