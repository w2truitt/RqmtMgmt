using frontend.E2ETests.Fixtures;
using Xunit;

namespace frontend.E2ETests;

/// <summary>
/// xUnit collection definition for tests that explicitly need shared browser resources.
/// Most tests now run in parallel without collection constraints via E2ETestBase.
/// Only use [Collection("Playwright")] for tests that specifically require collection-level sharing.
/// This dramatically improves performance by eliminating per-test browser startup overhead.
/// </summary>
[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}