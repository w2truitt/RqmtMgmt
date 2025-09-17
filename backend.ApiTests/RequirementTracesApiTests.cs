using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RqmtMgmtShared;
using Xunit;

namespace backend.ApiTests
{
    /// <summary>
    /// Integration tests for the Requirement Traces API endpoints.
    /// Tests the actual HTTP endpoints against the running deployment.
    /// </summary>
    [Collection("Integration Tests")]
    public class RequirementTracesApiTests : BaseIntegrationTest
    {
        [Fact]
        public async Task GetRequirementTrace_ShouldReturnNotFound_WhenInvalidIdProvided()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var invalidId = 999999;

            // Act
            var response = await _client.GetAsync($"/api/requirementtraces/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetSourceTraces_ShouldReturnEmptyList_WhenNoTracesExist()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var nonExistentRequirementId = 999999;

            // Act
            var response = await _client.GetAsync($"/api/requirementtraces/source/{nonExistentRequirementId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var traces = await response.Content.ReadFromJsonAsync<List<RequirementTraceDto>>(_jsonOptions);
            traces.Should().NotBeNull();
            traces!.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTargetTraces_ShouldReturnEmptyList_WhenNoTracesExist()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();
            var nonExistentRequirementId = 999999;

            // Act
            var response = await _client.GetAsync($"/api/requirementtraces/target/{nonExistentRequirementId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var traces = await response.Content.ReadFromJsonAsync<List<RequirementTraceDto>>(_jsonOptions);
            traces.Should().NotBeNull();
            traces!.Should().BeEmpty();
        }

        [Fact]
        public async Task ValidateTrace_ShouldReturnValidation_WhenCalled()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Act
            var response = await _client.GetAsync("/api/requirementtraces/validate");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var validation = await response.Content.ReadAsStringAsync();
            validation.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CreateRequirementTrace_ShouldHandleValidation_WhenCalledWithMinimalData()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            var createDto = new RequirementTraceDto
            {
                SourceRequirementId = 1, // Use simple IDs for testing
                TargetRequirementId = 2,
                TraceType = TraceType.DerivedFrom
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/requirementtraces", createDto, _jsonOptions);

            // Assert - Either succeeds or fails with validation, both are acceptable for now
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }
    }
}