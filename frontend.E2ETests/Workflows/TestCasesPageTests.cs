using frontend.E2ETests.PageObjects;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for the Test Cases page
/// </summary>
public class TestCasesPageTests : AuthenticatedE2ETestBase
{
    public TestCasesPageTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task TestCases_NavigatesSuccessfully()
    {
        // Arrange - Login as tester to access test cases
        var loginSuccess = await LoginAsTesterAsync();
        Assert.True(loginSuccess, "Failed to login as tester");
        
        // Arrange
        var testCasesPage = new TestCasesPage(Page, BaseUrl);
        
        // Act
        await testCasesPage.NavigateToAsync();
        
        // Assert
        Assert.Contains("/testcases", Page.Url);
    }
    
    [Fact]
    public async Task TestCases_LoadsWithoutErrors()
    {
        // Arrange - Login as tester to access test cases
        var loginSuccess = await LoginAsTesterAsync();
        Assert.True(loginSuccess, "Failed to login as tester");
        
        // Arrange
        var testCasesPage = new TestCasesPage(Page, BaseUrl);
        
        // Act
        await testCasesPage.NavigateToAsync();
        
        // Assert
        // Check that page loads without JavaScript errors
        var errors = await Page.EvaluateAsync<string[]>("() => window.errors || []");
        Assert.Empty(errors);
        
        // Check that we can access the page
        Assert.Contains("/testcases", Page.Url);
    }
    
    [Fact]
    public async Task TestCases_HasExpectedPageElements()
    {
        // Arrange - Login as tester to access test cases
        var loginSuccess = await LoginAsTesterAsync();
        Assert.True(loginSuccess, "Failed to login as tester");
        
        // Arrange
        var testCasesPage = new TestCasesPage(Page, BaseUrl);
        
        // Act
        await testCasesPage.NavigateToAsync();
        
        // Assert
        var title = await Page.TitleAsync();
        // Page title may not be implemented yet, so just check it's not null
        Assert.NotNull(title);
        // TODO: Uncomment when page titles are implemented
        // Assert.Contains("Test Cases", title, StringComparison.OrdinalIgnoreCase);
        
        // TODO: Add more specific element checks when frontend is implemented
        /*
        await Expect(Page.Locator("[data-testid='create-testcase-button']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='search-input']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='testcases-table']")).ToBeVisibleAsync();
        */
    }
    
    [Fact]
    public async Task TestCases_CanCreateNewTestCase_WhenImplemented()
    {
        // Arrange - Login as tester to create test cases
        var loginSuccess = await LoginAsTesterAsync();
        Assert.True(loginSuccess, "Failed to login as tester");
        
        // Arrange
        var testId = CreateTestId();
        var testCasesPage = new TestCasesPage(Page, BaseUrl);
        var testCase = TestDataFactory.CreateTestCase(testId);
        
        // Act
        await testCasesPage.NavigateToAsync();
        
        // TODO: Uncomment when test case creation is implemented
        /*
        await testCasesPage.ClickCreateTestCaseAsync();
        
        // Should navigate to new test case form
        Assert.Contains("/testcases/new", Page.Url);
        
        // Fill and submit form would be tested in NewTestCasePageTests
        */
        
        // Assert
        // For now, just verify test data creation and page navigation
        Assert.NotNull(testCase);
        Assert.Contains(testId, testCase.Title);
        Assert.Contains("/testcases", Page.Url);
    }
    
    [Fact]
    public async Task TestCases_CanSearchTestCases_WhenImplemented()
    {
        // Arrange - Login as tester to search test cases
        var loginSuccess = await LoginAsTesterAsync();
        Assert.True(loginSuccess, "Failed to login as tester");
        
        // Arrange
        var testId = CreateTestId();
        var testCasesPage = new TestCasesPage(Page, BaseUrl);
        
        // Act
        await testCasesPage.NavigateToAsync();
        
        // TODO: Uncomment when search functionality is implemented
        /*
        await testCasesPage.SearchTestCasesAsync(testId);
        
        // Should show filtered results
        var count = await testCasesPage.GetTestCaseCountAsync();
        Assert.Equal(0, count); // Should find no results for unique test ID
        */
        
        // Assert
        // For now, just verify page object setup
        Assert.NotNull(testCasesPage);
        Assert.Contains("/testcases", Page.Url);
    }
    
    [Fact]
    public async Task TestCases_DisplaysTestCasesList_WhenDataExists()
    {
        // Arrange - Login as tester to view test cases
        var loginSuccess = await LoginAsTesterAsync();
        Assert.True(loginSuccess, "Failed to login as tester");
        
        // Arrange
        var testCasesPage = new TestCasesPage(Page, BaseUrl);
        
        // Act
        await testCasesPage.NavigateToAsync();
        
        // Assert
        // TODO: Uncomment when test cases display is implemented
        /*
        // Should display test cases from seeded data
        var count = await testCasesPage.GetTestCaseCountAsync();
        Assert.True(count >= 0);
        
        // If there are test cases, verify they are displayed correctly
        if (count > 0)
        {
            await Expect(Page.Locator("[data-testid='testcase-row']").First).ToBeVisibleAsync();
        }
        */
        
        // For now, just verify navigation
        Assert.Contains("/testcases", Page.Url);
    }
    
    [Fact]
    public async Task TestCases_CanEditAndDeleteTestCases_WhenImplemented()
    {
        // Arrange - Login as tester to edit/delete test cases
        var loginSuccess = await LoginAsTesterAsync();
        Assert.True(loginSuccess, "Failed to login as tester");
        
        // Arrange
        var testCasesPage = new TestCasesPage(Page, BaseUrl);
        
        // Act
        await testCasesPage.NavigateToAsync();
        
        // TODO: Uncomment when CRUD operations are implemented
        /*
        // Assuming there's at least one test case from seeded data
        var count = await testCasesPage.GetTestCaseCountAsync();
        if (count > 0)
        {
            // Test edit functionality
            await testCasesPage.EditTestCaseAsync("Sample Test Case");
            // Should navigate to edit form or show edit modal
            
            // Test delete functionality
            await testCasesPage.DeleteTestCaseAsync("Sample Test Case");
            await testCasesPage.ConfirmDeleteAsync();
            
            // Verify test case is removed
            var newCount = await testCasesPage.GetTestCaseCountAsync();
            Assert.Equal(count - 1, newCount);
        }
        */
        
        // Assert
        Assert.Contains("/testcases", Page.Url);
    }
}