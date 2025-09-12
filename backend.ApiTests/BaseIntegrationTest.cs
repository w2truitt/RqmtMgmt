using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using IdentityModel.Client;
using Xunit;

namespace backend.ApiTests
{
    /// <summary>
    /// Base class for integration tests that run against the Kubernetes deployment.
    /// These tests authenticate with the Identity Server running in Kubernetes and test against the real database.
    /// </summary>
    public abstract class BaseIntegrationTest : IAsyncLifetime
    {
        protected readonly HttpClient _client;
        protected readonly JsonSerializerOptions _jsonOptions;
        protected string? _accessToken;

        private const string BaseUrl = "https://rqmtmgmt.local";
        private const string ClientId = "rqmtmgmt-backend";
        private const string ClientSecret = "backend-secret";
        private const string Scope = "rqmtmgmt.api";

        protected BaseIntegrationTest()
        {
            // Create HttpClientHandler that bypasses SSL certificate validation for local development
            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            
            _client = new HttpClient(handler)
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
                throw new InvalidOperationException($"Failed to authenticate with Identity Server. Ensure Kubernetes deployment is running and accessible at {BaseUrl}. Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifies that the Kubernetes deployment is running and accessible.
        /// </summary>
        protected async Task<bool> IsSystemAvailableAsync()
        {
            try
            {
                    var healthResponse = await _client.GetAsync("/health/");
                return healthResponse.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Skips the test if the Kubernetes system is not available.
        /// </summary>
        protected async Task SkipIfSystemNotAvailableAsync()
        {
            if (!await IsSystemAvailableAsync())
            {
                Skip.If(true, "Integration test skipped: Kubernetes deployment is not running or not accessible at rqmtmgmt.local. Ensure the deployment is running with: ./scripts/deploy-local-k8s.sh");
            }
        }
    }
}