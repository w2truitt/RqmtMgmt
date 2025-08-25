using Xunit;

namespace backend.ApiTests
{
    /// <summary>
    /// Collection definition for integration tests to ensure they run sequentially
    /// and don't interfere with each other when using the shared docker-compose instance.
    /// </summary>
    [CollectionDefinition("Integration Tests")]
    public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    /// <summary>
    /// Fixture for integration tests that ensures the docker-compose instance is available.
    /// </summary>
    public class IntegrationTestFixture : IAsyncLifetime
    {
        public async Task InitializeAsync()
        {
            // Verify the docker-compose instance is running before any tests start
            using var client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost");
            
            try
            {
                var healthResponse = await client.GetAsync("/health");
                if (!healthResponse.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException("Docker-compose instance health check failed");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Integration tests require docker-compose.identity.yml to be running. " +
                    "Start it with: cd docker-compose && docker-compose -f docker-compose.identity.yml up -d", ex);
            }
        }

        public async Task DisposeAsync()
        {
            // Cleanup if needed
            await Task.CompletedTask;
        }
    }
}
