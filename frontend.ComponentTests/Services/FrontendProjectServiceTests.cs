using Moq;
using RqmtMgmtShared;
using frontend.Services;
using frontend.Models;
using Xunit;

namespace frontend.ComponentTests.Services;

/// <summary>
/// Unit tests for FrontendProjectService covering convenience methods and Result pattern implementation
/// </summary>
public class FrontendProjectServiceTests
{
    private readonly Mock<IProjectService> _mockProjectService;
    private readonly FrontendProjectService _service;

    public FrontendProjectServiceTests()
    {
        _mockProjectService = new Mock<IProjectService>();
        _service = new FrontendProjectService(_mockProjectService.Object);
    }

    [Fact]
    public async Task GetProjectAsync_ValidProjectId_ReturnsSuccessResult()
    {
        // Arrange
        var projectId = 1;
        var expectedProject = new ProjectDto
        {
            Id = projectId,
            Name = "Test Project",
            Code = "TEST01",
            Description = "Test Description",
            Status = ProjectStatus.Active
        };

        _mockProjectService
            .Setup(x => x.GetProjectByIdAsync(projectId))
            .ReturnsAsync(expectedProject);

        // Act
        var result = await _service.GetProjectAsync(projectId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedProject.Id, result.Data.Id);
        Assert.Equal(expectedProject.Name, result.Data.Name);
        Assert.Equal(expectedProject.Code, result.Data.Code);
        Assert.Empty(result.Message);
    }

    [Fact]
    public async Task GetProjectAsync_ProjectNotFound_ReturnsFailureResult()
    {
        // Arrange
        var projectId = 999;
        _mockProjectService
            .Setup(x => x.GetProjectByIdAsync(projectId))
            .ReturnsAsync((ProjectDto?)null);

        // Act
        var result = await _service.GetProjectAsync(projectId);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal("Project not found", result.Message);
    }

    [Fact]
    public async Task GetProjectAsync_ServiceThrowsException_ReturnsFailureResult()
    {
        // Arrange
        var projectId = 1;
        var exceptionMessage = "Database connection failed";
        _mockProjectService
            .Setup(x => x.GetProjectByIdAsync(projectId))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _service.GetProjectAsync(projectId);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal($"Error loading project: {exceptionMessage}", result.Message);
    }

    [Fact]
    public async Task GetProjectsAsync_SuccessfulCall_ReturnsSuccessResult()
    {
        // Arrange
        var expectedProjects = new PagedResult<ProjectDto>
        {
            Items = new List<ProjectDto>
            {
                new() { Id = 1, Name = "Project 1", Code = "P001", Status = ProjectStatus.Active },
                new() { Id = 2, Name = "Project 2", Code = "P002", Status = ProjectStatus.Planning }
            },
            TotalItems = 2,
            PageNumber = 1,
            PageSize = 100
        };

        _mockProjectService
            .Setup(x => x.GetProjectsAsync(It.Is<ProjectFilterDto>(f => 
                f.Page == 1 && f.PageSize == 100)))
            .ReturnsAsync(expectedProjects);

        // Act
        var result = await _service.GetProjectsAsync();

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Items.Count);
        Assert.Equal(2, result.Data.TotalItems);
        Assert.Empty(result.Message);
    }

    [Fact]
    public async Task GetProjectsAsync_ServiceThrowsException_ReturnsFailureResult()
    {
        // Arrange
        var exceptionMessage = "Service unavailable";
        _mockProjectService
            .Setup(x => x.GetProjectsAsync(It.IsAny<ProjectFilterDto>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _service.GetProjectsAsync();

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal($"Error loading projects: {exceptionMessage}", result.Message);
    }

    [Fact]
    public async Task GetProjectsAsync_WithFilter_DelegatesCorrectly()
    {
        // Arrange
        var filter = new ProjectFilterDto { Page = 2, PageSize = 50, SearchTerm = "test" };
        var expectedResult = new PagedResult<ProjectDto>
        {
            Items = new List<ProjectDto>(),
            TotalItems = 0,
            PageNumber = 2,
            PageSize = 50
        };

        _mockProjectService
            .Setup(x => x.GetProjectsAsync(filter))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.GetProjectsAsync(filter);

        // Assert
        Assert.Equal(expectedResult, result);
        _mockProjectService.Verify(x => x.GetProjectsAsync(filter), Times.Once);
    }

    [Fact]
    public async Task GetProjectByIdAsync_DelegatesCorrectly()
    {
        // Arrange
        var projectId = 123;
        var expectedProject = new ProjectDto { Id = projectId, Name = "Test" };

        _mockProjectService
            .Setup(x => x.GetProjectByIdAsync(projectId))
            .ReturnsAsync(expectedProject);

        // Act
        var result = await _service.GetProjectByIdAsync(projectId);

        // Assert
        Assert.Equal(expectedProject, result);
        _mockProjectService.Verify(x => x.GetProjectByIdAsync(projectId), Times.Once);
    }

    [Fact]
    public async Task CreateProjectAsync_DelegatesCorrectly()
    {
        // Arrange
        var createDto = new CreateProjectDto
        {
            Name = "New Project",
            Code = "NEW001",
            Status = ProjectStatus.Planning,
            OwnerId = 1
        };
        var expectedProject = new ProjectDto { Id = 1, Name = createDto.Name, Code = createDto.Code };

        _mockProjectService
            .Setup(x => x.CreateProjectAsync(createDto))
            .ReturnsAsync(expectedProject);

        // Act
        var result = await _service.CreateProjectAsync(createDto);

        // Assert
        Assert.Equal(expectedProject, result);
        _mockProjectService.Verify(x => x.CreateProjectAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task UpdateProjectAsync_DelegatesCorrectly()
    {
        // Arrange
        var projectId = 1;
        var updateDto = new UpdateProjectDto
        {
            Name = "Updated Project",
            Status = ProjectStatus.Active
        };
        var expectedProject = new ProjectDto { Id = projectId, Name = updateDto.Name };

        _mockProjectService
            .Setup(x => x.UpdateProjectAsync(projectId, updateDto))
            .ReturnsAsync(expectedProject);

        // Act
        var result = await _service.UpdateProjectAsync(projectId, updateDto);

        // Assert
        Assert.Equal(expectedProject, result);
        _mockProjectService.Verify(x => x.UpdateProjectAsync(projectId, updateDto), Times.Once);
    }

    [Fact]
    public async Task DeleteProjectAsync_DelegatesCorrectly()
    {
        // Arrange
        var projectId = 1;
        _mockProjectService
            .Setup(x => x.DeleteProjectAsync(projectId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteProjectAsync(projectId);

        // Assert
        Assert.True(result);
        _mockProjectService.Verify(x => x.DeleteProjectAsync(projectId), Times.Once);
    }

    [Fact]
    public async Task GetProjectTeamMembersAsync_DelegatesCorrectly()
    {
        // Arrange
        var projectId = 1;
        var expectedMembers = new List<ProjectTeamMemberDto>
        {
            new() { UserId = 1, UserName = "User 1", Role = ProjectRole.Developer },
            new() { UserId = 2, UserName = "User 2", Role = ProjectRole.QAEngineer }
        };

        _mockProjectService
            .Setup(x => x.GetProjectTeamMembersAsync(projectId))
            .ReturnsAsync(expectedMembers);

        // Act
        var result = await _service.GetProjectTeamMembersAsync(projectId);

        // Assert
        Assert.Equal(expectedMembers, result);
        _mockProjectService.Verify(x => x.GetProjectTeamMembersAsync(projectId), Times.Once);
    }

    [Fact]
    public async Task UserHasAccessToProjectAsync_DelegatesCorrectly()
    {
        // Arrange
        var userId = 1;
        var projectId = 1;
        _mockProjectService
            .Setup(x => x.UserHasAccessToProjectAsync(userId, projectId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.UserHasAccessToProjectAsync(userId, projectId);

        // Assert
        Assert.True(result);
        _mockProjectService.Verify(x => x.UserHasAccessToProjectAsync(userId, projectId), Times.Once);
    }

    [Fact]
    public async Task GenerateNextRequirementIdAsync_DelegatesCorrectly()
    {
        // Arrange
        var projectId = 1;
        var expectedId = "REQ-001";
        _mockProjectService
            .Setup(x => x.GenerateNextRequirementIdAsync(projectId))
            .ReturnsAsync(expectedId);

        // Act
        var result = await _service.GenerateNextRequirementIdAsync(projectId);

        // Assert
        Assert.Equal(expectedId, result);
        _mockProjectService.Verify(x => x.GenerateNextRequirementIdAsync(projectId), Times.Once);
    }
}
