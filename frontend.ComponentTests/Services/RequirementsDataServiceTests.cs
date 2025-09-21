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
/// Unit tests for RequirementsDataService covering all requirement management operations
/// </summary>
public class RequirementsDataServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly RequirementsDataService _service;
    private readonly JsonSerializerOptions _jsonOptions;

    public RequirementsDataServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://localhost:5001/")
        };
        _service = new RequirementsDataService(_httpClient);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    [Fact]
    public async Task GetAllAsync_SuccessfulResponse_ReturnsRequirements()
    {
        // Arrange
        var expectedRequirements = new List<RequirementDto>
        {
            new RequirementDto 
            { 
                Id = 1, 
                Title = "Test Requirement 1", 
                Type = RequirementType.CRD, 
                Status = RequirementStatus.Approved,
                ProjectId = 1
            },
            new RequirementDto 
            { 
                Id = 2, 
                Title = "Test Requirement 2", 
                Type = RequirementType.PRD, 
                Status = RequirementStatus.Draft,
                ProjectId = 1
            }
        };

        var jsonResponse = JsonSerializer.Serialize(expectedRequirements, _jsonOptions);
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
                    req.RequestUri!.ToString().Contains("/api/Requirement")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Test Requirement 1", result[0].Title);
        Assert.Equal(RequirementType.CRD, result[0].Type);
        Assert.Equal(RequirementStatus.Approved, result[0].Status);
    }

    [Fact]
    public async Task GetByProjectIdAsync_ValidProjectId_ReturnsProjectRequirements()
    {
        // Arrange
        var projectId = 1;
        var expectedRequirements = new List<RequirementDto>
        {
            new RequirementDto 
            { 
                Id = 1, 
                Title = "Project Requirement 1", 
                ProjectId = projectId,
                Type = RequirementType.CRD, 
                Status = RequirementStatus.Approved
            }
        };

        var jsonResponse = JsonSerializer.Serialize(expectedRequirements, _jsonOptions);
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
                    req.RequestUri!.ToString().Contains($"/api/Requirement/project/{projectId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetByProjectIdAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(projectId, result[0].ProjectId);
        Assert.Equal("Project Requirement 1", result[0].Title);
    }

    [Fact]
    public async Task GetPagedByProjectIdAsync_WithPagination_ReturnsPagedRequirements()
    {
        // Arrange
        var projectId = 1;
        var parameters = new PaginationParameters
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = "test",
            SortBy = "title"
        };

        var expectedPagedResult = new PagedResult<RequirementDto>
        {
            Items = new List<RequirementDto>
            {
                new RequirementDto { Id = 1, Title = "Test Requirement", ProjectId = projectId }
            },
            TotalItems = 1,
            PageNumber = 1,
            PageSize = 10
        };

        var jsonResponse = JsonSerializer.Serialize(expectedPagedResult, _jsonOptions);
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
                    req.RequestUri!.ToString().Contains($"/api/Requirement/project/{projectId}/paged") &&
                    req.RequestUri!.ToString().Contains("PageNumber=1") &&
                    req.RequestUri!.ToString().Contains("PageSize=10") &&
                    req.RequestUri!.ToString().Contains("SearchTerm=test")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetPagedByProjectIdAsync(projectId, parameters);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(1, result.PageNumber);
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_ReturnsRequirement()
    {
        // Arrange
        var requirementId = 1;
        var expectedRequirement = new RequirementDto
        {
            Id = requirementId,
            Title = "Test Requirement",
            Type = RequirementType.CRD,
            Status = RequirementStatus.Approved,
            ProjectId = 1
        };

        var jsonResponse = JsonSerializer.Serialize(expectedRequirement, _jsonOptions);
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
                    req.RequestUri!.ToString().Contains($"/api/Requirement/{requirementId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetByIdAsync(requirementId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(requirementId, result.Id);
        Assert.Equal("Test Requirement", result.Title);
        Assert.Equal(RequirementType.CRD, result.Type);
    }

    [Fact]
    public async Task CreateAsync_ValidRequirement_ReturnsCreatedRequirement()
    {
        // Arrange
        var newRequirement = new RequirementDto
        {
            Title = "New Requirement",
            Description = "Test description",
            Type = RequirementType.CRD,
            Status = RequirementStatus.Draft,
            ProjectId = 1
        };

        var createdRequirement = new RequirementDto
        {
            Id = 1,
            Title = newRequirement.Title,
            Description = newRequirement.Description,
            Type = newRequirement.Type,
            Status = newRequirement.Status,
            ProjectId = newRequirement.ProjectId
        };

        var jsonResponse = JsonSerializer.Serialize(createdRequirement, _jsonOptions);
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
                    req.RequestUri!.ToString().Contains("/api/Requirement")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.CreateAsync(newRequirement);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("New Requirement", result.Title);
        Assert.Equal(RequirementType.CRD, result.Type);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequirement_ReturnsTrue()
    {
        // Arrange
        var requirement = new RequirementDto
        {
            Id = 1,
            Title = "Updated Requirement",
            Type = RequirementType.CRD,
            Status = RequirementStatus.Approved,
            ProjectId = 1
        };

        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NoContent);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Put && 
                    req.RequestUri!.ToString().Contains($"/api/Requirement/{requirement.Id}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.UpdateAsync(requirement);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ValidId_ReturnsTrue()
    {
        // Arrange
        var requirementId = 1;
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NoContent);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Delete && 
                    req.RequestUri!.ToString().Contains($"/api/Requirement/{requirementId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.DeleteAsync(requirementId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetAllAsync_HttpError_ReturnsEmptyList()
    {
        // Arrange
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        // Arrange
        var requirementId = 999;
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetByIdAsync(requirementId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_HttpError_ReturnsFalse()
    {
        // Arrange
        var requirement = new RequirementDto { Id = 1, Title = "Test" };
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.UpdateAsync(requirement);

        // Assert
        Assert.False(result);
    }

    private void Dispose()
    {
        _httpClient?.Dispose();
    }
}