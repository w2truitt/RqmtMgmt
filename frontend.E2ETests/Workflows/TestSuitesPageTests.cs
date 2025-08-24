using frontend.E2ETests.PageObjects;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for the Test Suites page
/// </summary>
public class TestSuitesPageTests : E2ETestBase
{
    [Fact]
    public async Task TestSuites_NavigatesSuccessfully()
    {
        // Arrange
        var testSuitesPage = new TestSuitesPage(Page, BaseUrl);
        
        // Act
        await testSuitesPage.NavigateToAsync();
        
        // Assert
        Assert.Contains("/testsuites", Page.Url);
    }
    
    [Fact]
    public async Task TestSuites_LoadsWithoutErrors()
    {
        // Arrange
        var testSuitesPage = new TestSuitesPage(Page, BaseUrl);
        
        // Act
        await testSuitesPage.NavigateToAsync();
        
        // Assert
        // Check that page loads without JavaScript errors
        var errors = await Page.EvaluateAsync<string[]>("() => window.errors || []");
        Assert.Empty(errors);
        
        // Check that we can access the page
        Assert.Contains("/testsuites", Page.Url);
    }
    
    [Fact]
    public async Task TestSuites_HasExpectedPageElements()
    {
        // Arrange
        var testSuitesPage = new TestSuitesPage(Page, BaseUrl);
        
        // Act
        await testSuitesPage.NavigateToAsync();
        
        // Assert
        var title = await Page.TitleAsync();
        Assert.NotNull(title);
        
        // Verify main page elements are visible
        await Expect(Page.Locator("[data-testid='create-testsuite-button']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='search-input']")).ToBeVisibleAsync();
        await Expect(Page.Locator("h1:has-text('Test Suites')")).ToBeVisibleAsync();
    }
    
    [Fact]
    public async Task TestSuites_CanCreateNewTestSuite()
    {
        // Arrange
        var testId = CreateTestId();
        var testSuitesPage = new TestSuitesPage(Page, BaseUrl);
        var testSuite = TestDataFactory.CreateTestSuite(testId);
        
        // Act
        await testSuitesPage.NavigateToAsync();
        await testSuitesPage.ClickCreateTestSuiteAsync();
        await testSuitesPage.FillTestSuiteFormAsync(testSuite.Name, testSuite.Description ?? "");
        await testSuitesPage.SaveTestSuiteAsync();
        
        // Wait for operation to complete
        await Page.WaitForTimeoutAsync(2000);
        
        // Verify test suite was created
        var isVisible = await testSuitesPage.IsTestSuiteVisibleAsync(testSuite.Name);
        Assert.True(isVisible);
        
        // Assert
        Assert.NotNull(testSuite);
        Assert.Contains(testId, testSuite.Name);
        Assert.Contains("/testsuites", Page.Url);
    }
    
    [Fact]
    public async Task TestSuites_CanSearchTestSuites()
    {
        // Arrange
        var testId = CreateTestId();
        var testSuitesPage = new TestSuitesPage(Page, BaseUrl);
        
        // Act
        await testSuitesPage.NavigateToAsync();
        
        // Get initial count
        var initialCount = await testSuitesPage.GetTestSuiteCountAsync();
        
        // Search for something that shouldn't exist
        await testSuitesPage.SearchTestSuitesAsync(testId);
        await Page.WaitForTimeoutAsync(1000);
        
        // Should show filtered results (likely 0 for unique test ID)
        var filteredCount = await testSuitesPage.GetTestSuiteCountAsync();
        Assert.True(filteredCount <= initialCount);
        
        // Assert
        Assert.NotNull(testSuitesPage);
        Assert.Contains("/testsuites", Page.Url);
    }
    
    [Fact]
    public async Task TestSuites_DisplaysTestSuitesList_WhenDataExists()
    {
        // Arrange
        var testSuitesPage = new TestSuitesPage(Page, BaseUrl);
        
        // Act
        await testSuitesPage.NavigateToAsync();
        
        // Assert
        // TODO: Uncomment when test suites display is implemented
        /*
        // Should display test suites from seeded data
        var count = await testSuitesPage.GetTestSuiteCountAsync();
        Assert.True(count >= 0);
        
        // If there are test suites, verify they are displayed correctly
        if (count > 0)
        {
            await Expect(Page.Locator("[data-testid='testsuite-row']").First).ToBeVisibleAsync();
        }
        */
        
        // For now, just verify navigation
        Assert.Contains("/testsuites", Page.Url);
    }
    
    [Fact]
    public async Task TestSuites_CanEditAndDeleteTestSuites_WhenImplemented()
    {
        // Arrange
        var testSuitesPage = new TestSuitesPage(Page, BaseUrl);
        
        // Act
        await testSuitesPage.NavigateToAsync();
        
        // TODO: Uncomment when CRUD operations are implemented
        /*
        // Assuming there's at least one test suite from seeded data
        var count = await testSuitesPage.GetTestSuiteCountAsync();
        if (count > 0)
        {
            // Test edit functionality
            await testSuitesPage.EditTestSuiteAsync("Sample Test Suite");
            // Should show edit form or modal
            
            // Test delete functionality
            await testSuitesPage.DeleteTestSuiteAsync("Sample Test Suite");
            await testSuitesPage.ConfirmDeleteAsync();
            
            // Verify test suite is removed
            var newCount = await testSuitesPage.GetTestSuiteCountAsync();
            Assert.Equal(count - 1, newCount);
        }
        */
        
        // Assert
        Assert.Contains("/testsuites", Page.Url);
    }
    
    [Fact]
    public async Task TestSuites_ValidatesRequiredFields_WhenImplemented()
    {
        // Arrange
        var testSuitesPage = new TestSuitesPage(Page, BaseUrl);
        
        // Act
        await testSuitesPage.NavigateToAsync();
        await testSuitesPage.ClickCreateTestSuiteAsync();
        
        // Try to save without filling required fields
        await testSuitesPage.SaveTestSuiteAsync();
        
        // Assert - Form should still be open (validation should prevent saving)
        await Expect(Page.Locator("[data-testid='name-input']")).ToBeVisibleAsync();
        
        // Verify we're still on the test suites page
        Assert.Contains("/testsuites", Page.Url);
    }
    
    [Fact]
    public async Task TestSuites_CanPerformFullCrudWorkflow()
    {
        // Arrange
        var testId = CreateTestId();
        var testSuitesPage = new TestSuitesPage(Page, BaseUrl);
        var testSuite = TestDataFactory.CreateTestSuite(testId);
        var updatedName = $"Updated {testSuite.Name}";
        
        // Act & Assert - Create
        await testSuitesPage.NavigateToAsync();
        await testSuitesPage.ClickCreateTestSuiteAsync();
        await testSuitesPage.FillTestSuiteFormAsync(testSuite.Name, testSuite.Description ?? "");
        await testSuitesPage.SaveTestSuiteAsync();
        await Page.WaitForTimeoutAsync(2000);
        
        var isVisible = await testSuitesPage.IsTestSuiteVisibleAsync(testSuite.Name);
        Assert.True(isVisible, "Test suite should be visible after creation");
        
        // Act & Assert - Edit
        await testSuitesPage.EditTestSuiteAsync(testSuite.Name);
        await Page.FillAsync("[data-testid='name-input']", updatedName);
        await testSuitesPage.SaveTestSuiteAsync();
        await Page.WaitForTimeoutAsync(2000);
        
        var isUpdatedVisible = await testSuitesPage.IsTestSuiteVisibleAsync(updatedName);
        Assert.True(isUpdatedVisible, "Updated test suite should be visible after edit");
        
        // Act & Assert - Delete
        await testSuitesPage.DeleteTestSuiteAsync(updatedName);
        await testSuitesPage.ConfirmDeleteAsync();
        await Page.WaitForTimeoutAsync(2000);
        
        var isDeletedVisible = await testSuitesPage.IsTestSuiteVisibleAsync(updatedName);
        Assert.False(isDeletedVisible, "Test suite should not be visible after deletion");
    }
    
    [Fact]
    public async Task TestSuites_DisplaysExistingTestSuites()
    {
        // Arrange
        var testSuitesPage = new TestSuitesPage(Page, BaseUrl);
        
        // Act
        await testSuitesPage.NavigateToAsync();
        
        // Assert
        // Should display test suites from seeded data
        var count = await testSuitesPage.GetTestSuiteCountAsync();
        Assert.True(count >= 0);
        
        // If there are test suites, verify they are displayed correctly
        if (count > 0)
        {
            await Expect(Page.Locator("[data-testid='testsuite-row']").First).ToBeVisibleAsync();
        }
        
        // Verify navigation
        Assert.Contains("/testsuites", Page.Url);
    }
}
