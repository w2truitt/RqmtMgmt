using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using IdentityModel.Client;
using Xunit;

namespace backend.ApiTests
{
    /// <summary>
    /// Base class for integration tests that run against the actual docker-compose.identity.yml instance.
    /// These tests authenticate with the real Identity Server and test against the real database.
    /// </summary>
    public abstract class BaseIntegrationTest : IAsyncLifetime
    {
        protected readonly HttpClient _client;
        protected readonly JsonSerializerOptions _jsonOptions;
        protected string? _accessToken;

        private const string BaseUrl = "https://localhost";
        private const string ClientId = "rqmtmgmt-backend";
        private const string ClientSecret = "backend-secret";
        private const string Scope = "rqmtmgmt.api";

        protected BaseIntegrationTest()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        /// <summary>
        /// Initialize the test by obtaining an access token from the Identity Server.
        /// </summary>
        public async Task InitializeAsync()
        {
            await ObtainAccessTokenAsync();
        }

        /// <summary>
        /// Cleanup resources after the test.
        /// </summary>
        public async Task DisposeAsync()
        {
            _client?.Dispose();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Obtains an access token from the Identity Server using client credentials flow.
        /// </summary>
        private async Task ObtainAccessTokenAsync()
        {
            try
            {
                var tokenResponse = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                {
                    Address = $"{BaseUrl}/connect/token",
                    ClientId = ClientId,
                    ClientSecret = ClientSecret,
                    Scope = Scope
                });

                if (tokenResponse.IsError)
                {
                    throw new InvalidOperationException($"Failed to obtain access token: {tokenResponse.Error} - {tokenResponse.ErrorDescription}");
                }

                _accessToken = tokenResponse.AccessToken;
                if (!string.IsNullOrEmpty(_accessToken))
                {
                    _client.SetBearerToken(_accessToken);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to authenticate with Identity Server. Ensure docker-compose.identity.yml is running on {BaseUrl}. Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifies that the docker-compose.identity.yml instance is running and accessible.
        /// </summary>
        protected async Task<bool> IsSystemAvailableAsync()
        {
            try
            {
                var healthResponse = await _client.GetAsync("/health");
                return healthResponse.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Skips the test if the docker-compose system is not available.
        /// </summary>
        protected async Task SkipIfSystemNotAvailableAsync()
        {
            if (!await IsSystemAvailableAsync())
            {
                Skip.If(true, "Integration test skipped: docker-compose.identity.yml instance is not running on localhost:443. Start it with: cd docker-compose && docker-compose -f docker-compose.identity.yml up -d");
            }
        }
    }
}
