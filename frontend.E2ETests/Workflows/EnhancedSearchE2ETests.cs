using frontend.E2ETests.Workflows;
using frontend.E2ETests.Fixtures;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for the Enhanced Section Search functionality
/// Tests the complete user workflow from navigation to search results
/// </summary>
public class EnhancedSearchE2ETests : AuthenticatedE2ETestBase
{
    public EnhancedSearchE2ETests(PlaywrightFixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
    }

    [Fact]
    public async Task EnhancedSearch_NavigateToTestPage_LoadsSuccessfully()
    {
        // Arrange & Act
        await Page.GotoAsync($"{BaseUrl}/test-enhanced-search");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        await Assertions.Expect(Page.Locator("h2")).ToContainTextAsync("Test Enhanced Section Search");
        await Assertions.Expect(Page.Locator(".card-title")).ToContainTextAsync("Select Document");
        await Assertions.Expect(Page.Locator(".card-title")).ToContainTextAsync("Instructions");
    }

    [Fact]
    public async Task EnhancedSearch_SelectDocument_ShowsSearchInterface()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/test-enhanced-search");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for documents to load
        await Page.WaitForTimeoutAsync(2000);

        // Act - Select the first available document
        var documentSelect = Page.Locator("#documentSelect");
        await documentSelect.WaitForAsync();
        
        // Get available options (skip the first "-- Select a document --" option)
        var options = await documentSelect.Locator("option").AllAsync();
        if (options.Count > 1)
        {
            var firstDocumentValue = await options[1].GetAttributeAsync("value");
            if (!string.IsNullOrEmpty(firstDocumentValue) && firstDocumentValue != "0")
            {
                await documentSelect.SelectOptionAsync(firstDocumentValue);
                await Page.WaitForTimeoutAsync(1000);

                // Assert - Enhanced search component should be visible
                await Assertions.Expect(Page.Locator("h5")).ToContainTextAsync("Enhanced Section Search");
                await Assertions.Expect(Page.Locator("#searchTerm")).ToBeVisibleAsync();
                await Assertions.Expect(Page.Locator("#fuzzyMatch")).ToBeVisibleAsync();
                await Assertions.Expect(Page.Locator("#similarityThreshold")).ToBeVisibleAsync();
            }
        }
    }

    [Fact]
    public async Task EnhancedSearch_PerformSearch_ShowsResults()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/test-enhanced-search");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.WaitForTimeoutAsync(2000);

        // Select a document first
        var documentSelect = Page.Locator("#documentSelect");
        var options = await documentSelect.Locator("option").AllAsync();
        
        if (options.Count > 1)
        {
            var firstDocumentValue = await options[1].GetAttributeAsync("value");
            if (!string.IsNullOrEmpty(firstDocumentValue) && firstDocumentValue != "0")
            {
                await documentSelect.SelectOptionAsync(firstDocumentValue);
                await Page.WaitForTimeoutAsync(1000);

                // Act - Perform a search
                var searchInput = Page.Locator("#searchTerm");
                await searchInput.FillAsync("interface"); // Common term likely to have results
                
                var searchButton = Page.Locator("button:has-text('Search')");
                await searchButton.ClickAsync();
                
                // Wait for search results
                await Page.WaitForTimeoutAsync(3000);

                // Assert - Should show search results section
                var resultsSection = Page.Locator(".search-results");
                await Assertions.Expect(resultsSection).ToBeVisibleAsync();
                
                // Should show either results or "no results" message
                var hasResults = await Page.Locator("h6:has-text('Search Results')").IsVisibleAsync();
                var hasNoResults = await Page.Locator("h6:has-text('No sections found')").IsVisibleAsync();
                
                Assert.True(hasResults || hasNoResults, "Should show either search results or no results message");
            }
        }
    }

    [Fact]
    public async Task EnhancedSearch_DuplicateCheck_ShowsResults()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/test-enhanced-search");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.WaitForTimeoutAsync(2000);

        // Select a document first
        var documentSelect = Page.Locator("#documentSelect");
        var options = await documentSelect.Locator("option").AllAsync();
        
        if (options.Count > 1)
        {
            var firstDocumentValue = await options[1].GetAttributeAsync("value");
            if (!string.IsNullOrEmpty(firstDocumentValue) && firstDocumentValue != "0")
            {
                await documentSelect.SelectOptionAsync(firstDocumentValue);
                await Page.WaitForTimeoutAsync(1000);

                // Act - Perform a duplicate check
                var duplicateInput = Page.Locator("#duplicateTitle");
                await duplicateInput.FillAsync("User Authentication"); // Common section name
                
                var checkButton = Page.Locator("button:has-text('Check Duplicates')");
                await checkButton.ClickAsync();
                
                // Wait for duplicate check results
                await Page.WaitForTimeoutAsync(3000);

                // Assert - Should show duplicate results section
                var duplicateSection = Page.Locator("h6:has-text('Potential Duplicates')");
                await Assertions.Expect(duplicateSection).ToBeVisibleAsync();
                
                // Should show either duplicates found or no duplicates message
                var hasDuplicates = await Page.Locator(".alert-warning").IsVisibleAsync();
                var noDuplicates = await Page.Locator(".alert-success").IsVisibleAsync();
                
                Assert.True(hasDuplicates || noDuplicates, "Should show either duplicates found or no duplicates message");
            }
        }
    }

    [Fact]
    public async Task EnhancedSearch_SearchTypeToggle_ChangesSearchBehavior()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/test-enhanced-search");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.WaitForTimeoutAsync(2000);

        // Select a document first
        var documentSelect = Page.Locator("#documentSelect");
        var options = await documentSelect.Locator("option").AllAsync();
        
        if (options.Count > 1)
        {
            var firstDocumentValue = await options[1].GetAttributeAsync("value");
            if (!string.IsNullOrEmpty(firstDocumentValue) && firstDocumentValue != "0")
            {
                await documentSelect.SelectOptionAsync(firstDocumentValue);
                await Page.WaitForTimeoutAsync(1000);

                // Act - Test search type toggle
                var searchTypeSelect = Page.Locator("#fuzzyMatch");
                
                // Verify default is fuzzy match
                var defaultValue = await searchTypeSelect.InputValueAsync();
                Assert.Equal("true", defaultValue);
                
                // Change to exact match
                await searchTypeSelect.SelectOptionAsync("false");
                var newValue = await searchTypeSelect.InputValueAsync();
                Assert.Equal("false", newValue);

                // Change back to fuzzy match
                await searchTypeSelect.SelectOptionAsync("true");
                var finalValue = await searchTypeSelect.InputValueAsync();
                Assert.Equal("true", finalValue);
            }
        }
    }

    [Fact]
    public async Task EnhancedSearch_SimilarityThreshold_AdjustsCorrectly()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/test-enhanced-search");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.WaitForTimeoutAsync(2000);

        // Select a document first
        var documentSelect = Page.Locator("#documentSelect");
        var options = await documentSelect.Locator("option").AllAsync();
        
        if (options.Count > 1)
        {
            var firstDocumentValue = await options[1].GetAttributeAsync("value");
            if (!string.IsNullOrEmpty(firstDocumentValue) && firstDocumentValue != "0")
            {
                await documentSelect.SelectOptionAsync(firstDocumentValue);
                await Page.WaitForTimeoutAsync(1000);

                // Act - Test similarity threshold slider
                var thresholdSlider = Page.Locator("#similarityThreshold");
                
                // Verify default value
                var defaultValue = await thresholdSlider.InputValueAsync();
                Assert.Equal("0.8", defaultValue);
                
                // Change threshold and verify percentage display updates
                await thresholdSlider.FillAsync("0.9");
                await Page.WaitForTimeoutAsync(500);
                
                // Should show 90% somewhere in the UI
                var percentageText = await Page.Locator("small:has-text('90%')").IsVisibleAsync();
                Assert.True(percentageText, "Should display 90% when threshold is set to 0.9");
            }
        }
    }
}
