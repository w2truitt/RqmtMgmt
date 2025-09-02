using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Net;

namespace backend.ApiTests
{
    /// <summary>
    /// Debug test to verify authentication is working properly
    /// </summary>
    [Collection("Integration Tests")]
    public class AuthenticationDebugTest : BaseIntegrationTest
    {
        [Fact]
        public async Task Authentication_ShouldWork_WhenCallingHealthEndpoint()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Act - Call a simple endpoint to verify auth is working
            var response = await _client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Authentication_ShouldHaveValidToken()
        {
            // Arrange
            await SkipIfSystemNotAvailableAsync();

            // Assert
            _accessToken.Should().NotBeNullOrEmpty();
            
            // Verify the Authorization header is set
            _client.DefaultRequestHeaders.Authorization.Should().NotBeNull();
            _client.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
            _client.DefaultRequestHeaders.Authorization.Parameter.Should().NotBeNullOrEmpty();
        }
    }
}