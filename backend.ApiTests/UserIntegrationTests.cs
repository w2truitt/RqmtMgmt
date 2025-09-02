using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using RqmtMgmtShared;
using System;
using System.Collections.Generic;
using FluentAssertions;

namespace backend.ApiTests
{
    /// <summary>
    /// Integration tests for the Users controller endpoints.
    /// These tests run against the actual docker-compose.identity.yml instance with JWT authentication.
    /// </summary>
    [Collection("Integration Tests")]
    public class UserIntegrationTests : BaseIntegrationTest
    {
        [Fact]
        public async Task GetUsers_ShouldReturnUsersList_WhenAuthenticated()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Act
            var response = await _client.GetAsync("/api/User");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var users = await response.Content.ReadFromJsonAsync<List<UserDto>>(_jsonOptions);
            users.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateUser_ShouldCreateUser_WhenValidDataProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var createDto = new UserDto
            {
                UserName = $"testuser{Guid.NewGuid():N}",
                Email = $"test{Guid.NewGuid():N}@example.com"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/User", createDto, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await response.Content.ReadFromJsonAsync<UserDto>(_jsonOptions);
            created.Should().NotBeNull();
            created!.UserName.Should().Be(createDto.UserName);
            created.Email.Should().Be(createDto.Email);
        }

        [Fact]
        public async Task GetUser_ShouldReturnUser_WhenValidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First create a user
            var createDto = new UserDto
            {
                UserName = $"getuser{Guid.NewGuid():N}",
                Email = $"get{Guid.NewGuid():N}@example.com"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/User", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<UserDto>(_jsonOptions);

            // Act
            var response = await _client.GetAsync($"/api/User/{created!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var user = await response.Content.ReadFromJsonAsync<UserDto>(_jsonOptions);
            user.Should().NotBeNull();
            user!.Id.Should().Be(created.Id);
            user.UserName.Should().Be(created.UserName);
        }

        [Fact]
        public async Task UpdateUser_ShouldUpdateUser_WhenValidDataProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First create a user
            var createDto = new UserDto
            {
                UserName = $"updateuser{Guid.NewGuid():N}",
                Email = $"update{Guid.NewGuid():N}@example.com"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/User", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<UserDto>(_jsonOptions);

            // Update the user
            created!.UserName = created.UserName + "Updated";
            created.Email = $"updated{Guid.NewGuid():N}@example.com";

            // Act
            var response = await _client.PutAsJsonAsync($"/api/User/{created.Id}", created, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await response.Content.ReadFromJsonAsync<UserDto>(_jsonOptions);
            updated.Should().NotBeNull();
            updated!.UserName.Should().Be(created.UserName);
            updated.Email.Should().Be(created.Email);
        }

        [Fact]
        public async Task DeleteUser_ShouldDeleteUser_WhenValidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // First create a user
            var createDto = new UserDto
            {
                UserName = $"deleteuser{Guid.NewGuid():N}",
                Email = $"delete{Guid.NewGuid():N}@example.com"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/User", createDto, _jsonOptions);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<UserDto>(_jsonOptions);

            // Act
            var response = await _client.DeleteAsync($"/api/User/{created!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify deletion
            var getResponse = await _client.GetAsync($"/api/User/{created.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetUser_ShouldReturnNotFound_WhenInvalidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var invalidId = 999999;

            // Act
            var response = await _client.GetAsync($"/api/User/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
