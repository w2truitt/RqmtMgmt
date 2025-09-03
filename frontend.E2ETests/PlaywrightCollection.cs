using frontend.E2ETests.Fixtures;
using Xunit;

namespace frontend.E2ETests;

/// <summary>
/// xUnit collection definition that enables sharing PlaywrightFixture across test classes.
/// All test classes decorated with [Collection("Playwright")] will share the same browser instance.
/// This dramatically improves performance by eliminating per-test browser startup overhead.
/// </summary>
[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}