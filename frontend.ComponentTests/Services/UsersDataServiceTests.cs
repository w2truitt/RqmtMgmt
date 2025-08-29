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
/// Unit tests for UsersDataService covering HTTP communication, user management, and role operations
/// </summary>
public class UsersDataServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly UsersDataService _service;
    private readonly JsonSerializerOptions _jsonOptions;

    public UsersDataServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://localhost:5001/")
        };
        _service = new UsersDataService(_httpClient);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    [Fact]
    public async Task GetAllAsync_SuccessfulResponse_ReturnsUserList()
    {
        // Arrange
        var expectedUsers = new List<UserDto>
        {
            new() { Id = 1, UserName = "user1", Email = "user1@test.com" },
            new() { Id = 2, UserName = "user2", Email = "user2@test.com" }
        };

        var jsonResponse = JsonSerializer.Serialize(expectedUsers, _jsonOptions);
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
                    req.RequestUri!.ToString().Contains("/api/User")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("user1", result[0].UserName);
        Assert.Equal("user2@test.com", result[1].Email);
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
    public async Task GetByIdAsync_SuccessfulResponse_ReturnsUser()
    {
        // Arrange
        var userId = 1;
        var expectedUser = new UserDto { Id = userId, UserName = "testuser", Email = "test@example.com" };

        var jsonResponse = JsonSerializer.Serialize(expectedUser, _jsonOptions);
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
                    req.RequestUri!.ToString().Contains($"/api/User/{userId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUser.Id, result.Id);
        Assert.Equal(expectedUser.UserName, result.UserName);
        Assert.Equal(expectedUser.Email, result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_UserNotFound_ReturnsNull()
    {
        // Arrange
        var userId = 999;
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_SuccessfulResponse_ReturnsCreatedUser()
    {
        // Arrange
        var newUser = new UserDto { UserName = "newuser", Email = "new@example.com" };
        var createdUser = new UserDto { Id = 1, UserName = "newuser", Email = "new@example.com" };

        var jsonResponse = JsonSerializer.Serialize(createdUser, _jsonOptions);
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
                    req.RequestUri!.ToString().Contains("/api/User")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.CreateAsync(newUser);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(newUser.UserName, result.UserName);
        Assert.Equal(newUser.Email, result.Email);
    }

    [Fact]
    public async Task CreateAsync_HttpError_ReturnsNull()
    {
        // Arrange
        var newUser = new UserDto { UserName = "newuser", Email = "new@example.com" };
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.CreateAsync(newUser);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_SuccessfulResponse_ReturnsTrue()
    {
        // Arrange
        var user = new UserDto { Id = 1, UserName = "updateduser", Email = "updated@example.com" };
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Put && 
                    req.RequestUri!.ToString().Contains($"/api/User/{user.Id}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.UpdateAsync(user);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateAsync_HttpError_ReturnsFalse()
    {
        // Arrange
        var user = new UserDto { Id = 1, UserName = "updateduser", Email = "updated@example.com" };
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.UpdateAsync(user);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_SuccessfulResponse_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Delete && 
                    req.RequestUri!.ToString().Contains($"/api/User/{userId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.DeleteAsync(userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_HttpError_ReturnsFalse()
    {
        // Arrange
        var userId = 1;
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.DeleteAsync(userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetUserRolesAsync_SuccessfulResponse_ReturnsRoles()
    {
        // Arrange
        var userId = 1;
        var expectedRoles = new List<string> { "Admin", "User" };

        var jsonResponse = JsonSerializer.Serialize(expectedRoles, _jsonOptions);
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
                    req.RequestUri!.ToString().Contains($"/api/User/{userId}/roles")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetUserRolesAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains("Admin", result);
        Assert.Contains("User", result);
    }

    [Fact]
    public async Task GetUserRolesAsync_HttpError_ReturnsEmptyList()
    {
        // Arrange
        var userId = 1;
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetUserRolesAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByEmailAsync_ValidEmail_ReturnsUser()
    {
        // Arrange
        var targetEmail = "user1@test.com";
        var users = new List<UserDto>
        {
            new() { Id = 1, UserName = "user1", Email = "user1@test.com" },
            new() { Id = 2, UserName = "user2", Email = "user2@test.com" }
        };

        var jsonResponse = JsonSerializer.Serialize(users, _jsonOptions);
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetByEmailAsync(targetEmail);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(targetEmail, result.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_EmailNotFound_ReturnsNull()
    {
        // Arrange
        var targetEmail = "notfound@test.com";
        var users = new List<UserDto>
        {
            new() { Id = 1, UserName = "user1", Email = "user1@test.com" }
        };

        var jsonResponse = JsonSerializer.Serialize(users, _jsonOptions);
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetByEmailAsync(targetEmail);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetByEmailAsync_InvalidEmail_ReturnsNull(string email)
    {
        // Act
        var result = await _service.GetByEmailAsync(email);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_NullEmail_ReturnsNull()
    {
        // Act
        var result = await _service.GetByEmailAsync(null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentUserAsync_SuccessfulResponse_ReturnsUser()
    {
        // Arrange
        var expectedUser = new UserDto { Id = 1, UserName = "currentuser", Email = "current@example.com" };

        var jsonResponse = JsonSerializer.Serialize(expectedUser, _jsonOptions);
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
                    req.RequestUri!.ToString().Contains("/api/User/me")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetCurrentUserAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUser.Id, result.Id);
        Assert.Equal(expectedUser.UserName, result.UserName);
    }

    [Fact]
    public async Task GetCurrentUserAsync_HttpRequestException_ReturnsNull()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Unauthorized"));

        // Act
        var result = await _service.GetCurrentUserAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AssignRoleAsync_MakesCorrectRequest()
    {
        // Arrange
        var userId = 1;
        var role = "Admin";
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri!.ToString().Contains($"/api/User/{userId}/roles")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _service.AssignRoleAsync(userId, role);

        // Assert
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Method == HttpMethod.Post && 
                req.RequestUri!.ToString().Contains($"/api/User/{userId}/roles")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task RemoveRoleAsync_MakesCorrectRequest()
    {
        // Arrange
        var userId = 1;
        var role = "Admin";
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Delete && 
                    req.RequestUri!.ToString().Contains($"/api/User/{userId}/roles/{role}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _service.RemoveRoleAsync(userId, role);

        // Assert
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Method == HttpMethod.Delete && 
                req.RequestUri!.ToString().Contains($"/api/User/{userId}/roles/{role}")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetPagedAsync_WithParameters_ReturnsPagedResult()
    {
        // Arrange
        var parameters = new PaginationParameters
        {
            PageNumber = 2,
            PageSize = 10,
            SearchTerm = "test user",
            SortBy = "name",
            SortDescending = true
        };

        var expectedResult = new PagedResult<UserDto>
        {
            Items = new List<UserDto>
            {
                new() { Id = 1, UserName = "testuser1", Email = "test1@example.com" }
            },
            TotalItems = 1,
            PageNumber = 2,
            PageSize = 10
        };

        var jsonResponse = JsonSerializer.Serialize(expectedResult, _jsonOptions);
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
                    req.RequestUri!.ToString().Contains("/api/User")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetPagedAsync(parameters);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetPagedAsync_HttpError_ReturnsEmptyPagedResult()
    {
        // Arrange
        var parameters = new PaginationParameters { PageNumber = 1, PageSize = 10 };
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _service.GetPagedAsync(parameters);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
    }
}
