using System.Net;
using System.Text;
using System.Text.Json;
using Moq;
using Moq.Protected;
using RqmtMgmtShared;
using frontend.Services;
using Xunit;

namespace frontend.ComponentTests.Services;

/// <summary>
/// Unit tests for ProjectsDataService covering all project management operations
/// </summary>
public class ProjectsDataServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly ProjectsDataService _service;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProjectsDataServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://localhost:5001/")
        };
        _service = new ProjectsDataService(_httpClient);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    [Fact]
    public async Task GetProjectsAsync_WithFilter_ReturnsPagedProjects()
    {
        // Arrange
        var filter = new ProjectFilterDto
        {
            Page = 1,
            PageSize = 10,
            SearchTerm = "Test",
            Status = ProjectStatus.Active,
            OwnerId = 1,
            UserIsMember = true
        };

        var expectedProjects = new PagedResult<ProjectDto>
        {
            Items = new List<ProjectDto>
            {
                new ProjectDto { Id = 1, Name = "Test Project 1", Code = "TEST1", Status = ProjectStatus.Active },
                new ProjectDto { Id = 2, Name = "Test Project 2", Code = "TEST2", Status = ProjectStatus.Active }
            },
            TotalItems = 2,
            PageNumber = 1,
            PageSize = 10
        };

        var jsonResponse = JsonSerializer.Serialize(expectedProjects, _jsonOptions);
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString().Contains("/api/Projects") &&
                    req.RequestUri!.ToString().Contains("SearchTerm=Test") &&
                    req.RequestUri!.ToString().Contains("Status=Active")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetProjectsAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal("Test Project 1", result.Items[0].Name);
        Assert.Equal("TEST1", result.Items[0].Code);
    }

    [Fact]
    public async Task GetProjectByIdAsync_ValidId_ReturnsProject()
    {
        // Arrange
        var projectId = 1;
        var expectedProject = new ProjectDto 
        { 
            Id = projectId, 
            Name = "Test Project", 
            Code = "TEST", 
            Status = ProjectStatus.Active 
        };

        var jsonResponse = JsonSerializer.Serialize(expectedProject, _jsonOptions);
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString().Contains($"/api/Projects/{projectId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetProjectByIdAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedProject.Id, result.Id);
        Assert.Equal(expectedProject.Name, result.Name);
        Assert.Equal(expectedProject.Code, result.Code);
    }

    [Fact]
    public async Task GetProjectByCodeAsync_ValidCode_ReturnsProject()
    {
        // Arrange
        var projectCode = "TEST";
        var expectedProject = new ProjectDto 
        { 
            Id = 1, 
            Name = "Test Project", 
            Code = projectCode, 
            Status = ProjectStatus.Active 
        };

        var jsonResponse = JsonSerializer.Serialize(expectedProject, _jsonOptions);
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString().Contains($"/api/Projects/by-code/{projectCode}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetProjectByCodeAsync(projectCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedProject.Code, result.Code);
        Assert.Equal(expectedProject.Name, result.Name);
    }

    [Fact]
    public async Task CreateProjectAsync_ValidProject_ReturnsCreatedProject()
    {
        // Arrange
        var createDto = new CreateProjectDto
        {
            Name = "New Project",
            Code = "NEW",
            Description = "A new test project"
        };

        var createdProject = new ProjectDto
        {
            Id = 1,
            Name = createDto.Name,
            Code = createDto.Code,
            Description = createDto.Description,
            Status = ProjectStatus.Active
        };

        var jsonResponse = JsonSerializer.Serialize(createdProject, _jsonOptions);
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri!.ToString().Contains("/api/Projects")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.CreateProjectAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdProject.Name, result.Name);
        Assert.Equal(createdProject.Code, result.Code);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task UpdateProjectAsync_ValidUpdate_ReturnsUpdatedProject()
    {
        // Arrange
        var projectId = 1;
        var updateDto = new UpdateProjectDto
        {
            Name = "Updated Project",
            Description = "Updated description"
        };

        var updatedProject = new ProjectDto
        {
            Id = projectId,
            Name = updateDto.Name,
            Description = updateDto.Description,
            Status = ProjectStatus.Active
        };

        var jsonResponse = JsonSerializer.Serialize(updatedProject, _jsonOptions);
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Put && 
                    req.RequestUri!.ToString().Contains($"/api/Projects/{projectId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.UpdateProjectAsync(projectId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updatedProject.Name, result.Name);
        Assert.Equal(updatedProject.Description, result.Description);
    }

    [Fact]
    public async Task DeleteProjectAsync_ValidId_ReturnsTrue()
    {
        // Arrange
        var projectId = 1;
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NoContent);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Delete && 
                    req.RequestUri!.ToString().Contains($"/api/Projects/{projectId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.DeleteProjectAsync(projectId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetProjectTeamMembersAsync_ValidProjectId_ReturnsTeamMembers()
    {
        // Arrange
        var projectId = 1;
        var expectedMembers = new List<ProjectTeamMemberDto>
        {
            new ProjectTeamMemberDto { UserId = 1, UserName = "User 1", Role = ProjectRole.ProjectOwner },
            new ProjectTeamMemberDto { UserId = 2, UserName = "User 2", Role = ProjectRole.Developer }
        };

        var jsonResponse = JsonSerializer.Serialize(expectedMembers, _jsonOptions);
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString().Contains($"/api/Projects/{projectId}/team")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetProjectTeamMembersAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("User 1", result[0].UserName);
        Assert.Equal(ProjectRole.ProjectOwner, result[0].Role);
    }

    [Fact]
    public async Task AddTeamMemberAsync_ValidMember_ReturnsAddedMember()
    {
        // Arrange
        var projectId = 1;
        var addDto = new AddProjectTeamMemberDto
        {
            UserId = 3,
            Role = ProjectRole.QAEngineer
        };

        var addedMember = new ProjectTeamMemberDto
        {
            UserId = addDto.UserId,
            UserName = "New User",
            Role = addDto.Role
        };

        var jsonResponse = JsonSerializer.Serialize(addedMember, _jsonOptions);
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri!.ToString().Contains($"/api/Projects/{projectId}/team")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.AddTeamMemberAsync(projectId, addDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(addedMember.UserId, result.UserId);
        Assert.Equal(addedMember.Role, result.Role);
    }

    [Fact]
    public async Task RemoveTeamMemberAsync_ValidIds_ReturnsTrue()
    {
        // Arrange
        var projectId = 1;
        var userId = 2;
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NoContent);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Delete && 
                    req.RequestUri!.ToString().Contains($"/api/Projects/{projectId}/team/{userId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.RemoveTeamMemberAsync(projectId, userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UserHasAccessToProjectAsync_ValidAccess_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var projectId = 1;
        var jsonResponse = JsonSerializer.Serialize(true, _jsonOptions);
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString().Contains($"/api/Projects/{projectId}/access/{userId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.UserHasAccessToProjectAsync(userId, projectId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GenerateNextRequirementIdAsync_ValidProject_ReturnsRequirementId()
    {
        // Arrange
        var projectId = 1;
        var expectedId = "TEST-REQ-001";
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedId, Encoding.UTF8, "text/plain")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString().Contains($"/api/Projects/{projectId}/next-requirement-id")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GenerateNextRequirementIdAsync(projectId);

        // Assert
        Assert.Equal(expectedId, result);
    }

    private void Dispose()
    {
        _httpClient?.Dispose();
    }
}
