using Xunit;

namespace backend.ApiTests
{
    /// <summary>
    /// Collection definition for integration tests to ensure they run sequentially
    /// and don't interfere with each other when using the shared deployment instance.
    /// </summary>
    [CollectionDefinition("Integration Tests")]
    public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    /// <summary>
    /// Fixture for integration tests that ensures the deployment instance is available.
    /// </summary>
    public class IntegrationTestFixture : IAsyncLifetime
    {
        public async Task InitializeAsync()
        {
            // Verify the deployment instance is running before any tests start
            // Create HttpClientHandler that bypasses SSL certificate validation for local development
            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            
            using var client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://rqmtmgmt.local");
            
            try
            {
                var healthResponse = await client.GetAsync("/health");
                if (!healthResponse.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException("Deployment instance health check failed");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Integration tests require deployment to be running and accessible at https://rqmtmgmt.local. " +
                    "For Docker Compose: 'cd docker-compose && docker-compose up -d'. " +
                    "For Kubernetes: './scripts/deploy-local-k8s.sh'", ex);
            }
        }

        public async Task DisposeAsync()
        {
            // Cleanup if needed
            await Task.CompletedTask;
        }
    }
}