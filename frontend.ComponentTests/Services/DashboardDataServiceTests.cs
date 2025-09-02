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
/// Unit tests for DashboardDataService covering HTTP communication, error handling, and data serialization
/// </summary>
public class DashboardDataServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly DashboardDataService _service;
    private readonly JsonSerializerOptions _jsonOptions;

    public DashboardDataServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://localhost:5001/")
        };
        _service = new DashboardDataService(_httpClient);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    [Fact]
    public async Task GetStatisticsAsync_SuccessfulResponse_ReturnsStatistics()
    {
        // Arrange
        var expectedStats = new DashboardStatisticsDto
        {
            Requirements = new RequirementStatisticsDto { Total = 10, Approved = 5, Draft = 5 },
            TestCases = new TestCaseStatisticsDto { Total = 20, Passed = 15, Failed = 5 },
            TestSuites = new TestSuiteStatisticsDto { Total = 5, Active = 3, Completed = 2 },
            TestPlans = new TestPlanStatisticsDto { Total = 3, ExecutionProgress = 75, CoveragePercentage = 85 }
        };

        var jsonResponse = JsonSerializer.Serialize(expectedStats, _jsonOptions);
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
                    req.RequestUri!.ToString().Contains("api/dashboard/statistics")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedStats.Requirements.Total, result.Requirements.Total);
        Assert.Equal(expectedStats.TestCases.Total, result.TestCases.Total);
        Assert.Equal(expectedStats.TestSuites.Total, result.TestSuites.Total);
        Assert.Equal(expectedStats.TestPlans.Total, result.TestPlans.Total);
        Assert.Equal(expectedStats.Requirements.Approved, result.Requirements.Approved);
        Assert.Equal(expectedStats.TestCases.Passed, result.TestCases.Passed);
    }

    [Fact]
    public async Task GetStatisticsAsync_HttpError_ReturnsEmptyStatistics()
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
        var result = await _service.GetStatisticsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Requirements.Total);
        Assert.Equal(0, result.TestCases.Total);
        Assert.Equal(0, result.TestSuites.Total);
        Assert.Equal(0, result.TestPlans.Total);
    }

    [Fact]
    public async Task GetStatisticsAsync_NetworkException_ReturnsEmptyStatistics()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Requirements.Total);
        Assert.Equal(0, result.TestCases.Total);
        Assert.Equal(0, result.TestSuites.Total);
        Assert.Equal(0, result.TestPlans.Total);
    }

    [Fact]
    public async Task GetStatisticsAsync_NullResponse_ReturnsEmptyStatistics()
    {
        // Arrange
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Requirements.Total);
    }

    [Fact]
    public async Task GetRecentActivityAsync_SuccessfulResponse_ReturnsActivities()
    {
        // Arrange
        var expectedActivities = new List<RecentActivityDto>
        {
            new RecentActivityDto 
            { 
                Id = 1, 
                Description = "Test Requirement was created",
                EntityType = "Requirement", 
                EntityId = 1,
                Action = "Created",
                CreatedAt = DateTime.UtcNow,
                UserId = 1,
                UserName = "Test User"
            },
            new RecentActivityDto 
            { 
                Id = 2, 
                Description = "Test Case was updated",
                EntityType = "TestCase", 
                EntityId = 2,
                Action = "Updated",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                UserId = 2,
                UserName = "Another User"
            }
        };

        var jsonResponse = JsonSerializer.Serialize(expectedActivities, _jsonOptions);
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
                    req.RequestUri!.ToString().Contains("api/dashboard/recent-activity")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetRecentActivityAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(expectedActivities[0].Description, result[0].Description);
        Assert.Equal(expectedActivities[0].Action, result[0].Action);
        Assert.Equal(expectedActivities[1].EntityType, result[1].EntityType);
        Assert.Equal(expectedActivities[1].UserName, result[1].UserName);
    }

    [Fact]
    public async Task GetRecentActivityAsync_WithCustomCount_UsesCorrectQueryParameter()
    {
        // Arrange
        var activities = new List<RecentActivityDto>();
        var jsonResponse = JsonSerializer.Serialize(activities, _jsonOptions);
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
                    req.RequestUri!.ToString().Contains("api/dashboard/recent-activity?count=10")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetRecentActivityAsync(10);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecentActivityAsync_HttpError_ReturnsEmptyList()
    {
        // Arrange
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetRecentActivityAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecentActivityAsync_NetworkException_ReturnsEmptyList()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var result = await _service.GetRecentActivityAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecentActivityAsync_NullResponse_ReturnsEmptyList()
    {
        // Arrange
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetRecentActivityAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    private void Dispose()
    {
        _httpClient?.Dispose();
    }
}
