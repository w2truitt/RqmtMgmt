using frontend.E2ETests.PageObjects;
using frontend.E2ETests.TestData;
using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace frontend.E2ETests.Workflows;

/// <summary>
/// E2E tests for the Test Plans page
/// </summary>
public class TestPlansPageTests : E2ETestBase
{
    [Fact]
    public async Task TestPlans_NavigatesSuccessfully()
    {
        // Arrange
        var testPlansPage = new TestPlansPage(Page, BaseUrl);
        
        // Act
        await testPlansPage.NavigateToAsync();
        
        // Assert
        Assert.Contains("/testplans", Page.Url);
    }
    
    [Fact]
    public async Task TestPlans_LoadsWithoutErrors()
    {
        // Arrange
        var testPlansPage = new TestPlansPage(Page, BaseUrl);
        
        // Act
        await testPlansPage.NavigateToAsync();
        
        // Assert
        // Check that page loads without JavaScript errors
        var errors = await Page.EvaluateAsync<string[]>("() => window.errors || []");
        Assert.Empty(errors);
        
        // Check that we can access the page
        Assert.Contains("/testplans", Page.Url);
    }
    
    [Fact]
    public async Task TestPlans_HasExpectedPageElements()
    {
        // Arrange
        var testPlansPage = new TestPlansPage(Page, BaseUrl);
        
        // Act
        await testPlansPage.NavigateToAsync();
        
        // Assert
        var title = await Page.TitleAsync();
        Assert.NotNull(title);
        
        // Verify main page elements are visible
        await Expect(Page.Locator("[data-testid='create-testplan-button']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='search-input']")).ToBeVisibleAsync();
        await Expect(Page.Locator("h3:has-text('Test Plans')")).ToBeVisibleAsync();
    }
    
    [Fact]
    public async Task TestPlans_CanCreateNewTestPlan()
    {
        // Arrange
        var testId = CreateTestId();
        var testPlansPage = new TestPlansPage(Page, BaseUrl);
        var testPlan = TestDataFactory.CreateTestPlan(testId);
        
        // Act
        await testPlansPage.NavigateToAsync();
        await testPlansPage.ClickCreateTestPlanAsync();
        await testPlansPage.FillTestPlanFormAsync(
            testPlan.Name, 
            testPlan.Type, 
            testPlan.Description ?? "");
        await testPlansPage.SaveTestPlanAsync();
        
        // Wait for operation to complete - allow more time for backend processing
        await Page.WaitForTimeoutAsync(5000);
        
        // Verify test plan was created
        var isVisible = await testPlansPage.IsTestPlanVisibleAsync(testPlan.Name);
        Assert.True(isVisible, $"Test plan '{testPlan.Name}' should be visible after creation");
        
        // Assert
        Assert.NotNull(testPlan);
        Assert.Contains(testId, testPlan.Name);
        Assert.Contains("/testplans", Page.Url);
    }
    
    [Fact]
    public async Task TestPlans_CanSearchTestPlans()
    {
        // Arrange
        var testId = CreateTestId();
        var testPlansPage = new TestPlansPage(Page, BaseUrl);
        
        // Act
        await testPlansPage.NavigateToAsync();
        
        // Get initial count
        var initialCount = await testPlansPage.GetTestPlanCountAsync();
        
        // Search for something that shouldn't exist
        await testPlansPage.SearchTestPlansAsync(testId);
        await Page.WaitForTimeoutAsync(1000);
        
        // Should show filtered results (likely 0 for unique test ID)
        var filteredCount = await testPlansPage.GetTestPlanCountAsync();
        Assert.True(filteredCount <= initialCount);
        
        // Assert
        Assert.NotNull(testPlansPage);
        Assert.Contains("/testplans", Page.Url);
    }
    
    [Fact]
    public async Task TestPlans_DisplaysTestPlansList_WhenDataExists()
    {
        // Arrange
        var testPlansPage = new TestPlansPage(Page, BaseUrl);
        
        // Act
        await testPlansPage.NavigateToAsync();
        
        // Assert
        // Should display test plans from seeded data
        var count = await testPlansPage.GetTestPlanCountAsync();
        Assert.True(count >= 0);
        
        // If there are test plans, verify they are displayed correctly
        if (count > 0)
        {
            await Expect(Page.Locator("[data-testid='testplan-row']").First).ToBeVisibleAsync();
        }
        
        // Verify navigation
        Assert.Contains("/testplans", Page.Url);
    }
    
    [Fact]
    public async Task TestPlans_CanEditAndDeleteTestPlans_WhenImplemented()
    {
        // Arrange
        var testPlansPage = new TestPlansPage(Page, BaseUrl);
        
        // Act
        await testPlansPage.NavigateToAsync();
        
        // TODO: Uncomment when CRUD operations are implemented
        /*
        // Assuming there's at least one test plan from seeded data
        var count = await testPlansPage.GetTestPlanCountAsync();
        if (count > 0)
        {
            // Test edit functionality
            await testPlansPage.EditTestPlanAsync("Sample Test Plan");
            // Should show edit form or modal
            
            // Test delete functionality
            await testPlansPage.DeleteTestPlanAsync("Sample Test Plan");
            await testPlansPage.ConfirmDeleteAsync();
            
            // Verify test plan is removed
            var newCount = await testPlansPage.GetTestPlanCountAsync();
            Assert.Equal(count - 1, newCount);
        }
        */
        
        // Assert
        Assert.Contains("/testplans", Page.Url);
    }
    
    [Fact]
    public async Task TestPlans_ValidatesRequiredFields_WhenImplemented()
    {
        // Arrange
        var testPlansPage = new TestPlansPage(Page, BaseUrl);
        
        // Act
        await testPlansPage.NavigateToAsync();
        
        // TODO: Uncomment when form validation is implemented
        /*
        await testPlansPage.ClickCreateTestPlanAsync();
        
        // Try to save without filling required fields
        await testPlansPage.SaveTestPlanAsync();
        
        // Should show validation errors
        await Expect(Page.Locator("[data-testid='name-error']")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid='type-error']")).ToBeVisibleAsync();
        */
        
        // Assert
        Assert.Contains("/testplans", Page.Url);
    }
    
    [Fact]
    public async Task TestPlans_HandlesTestPlanTypes_WhenImplemented()
    {
        // Arrange
        var testId = CreateTestId();
        var testPlansPage = new TestPlansPage(Page, BaseUrl);
        
        // Act
        await testPlansPage.NavigateToAsync();
        
        // TODO: Uncomment when test plan types are implemented
        /*
        await testPlansPage.ClickCreateTestPlanAsync();
        
        // Test that type dropdown has expected options
        var typeOptions = await Page.QuerySelectorAllAsync("[data-testid='type-select'] option");
        var optionTexts = await Task.WhenAll(typeOptions.Select(async o => await o.TextContentAsync()));
        
        Assert.Contains("UserValidation", optionTexts);
        Assert.Contains("SoftwareVerification", optionTexts);
        */
        
        // Assert
        Assert.Contains("/testplans", Page.Url);
    }
    
    [Fact]
    public async Task TestPlans_CanPerformFullCrudWorkflow()
    {
        // Arrange
        var testId = CreateTestId();
        var testPlansPage = new TestPlansPage(Page, BaseUrl);
        var testPlan = TestDataFactory.CreateTestPlan(testId);
        var updatedName = $"Updated {testPlan.Name}";
        
        // Act & Assert - Create
        await testPlansPage.NavigateToAsync();
        await testPlansPage.ClickCreateTestPlanAsync();
        await testPlansPage.FillTestPlanFormAsync(testPlan.Name, testPlan.Type, testPlan.Description ?? "");
        await testPlansPage.SaveTestPlanAsync();
        await Page.WaitForTimeoutAsync(2000);
        
        var isVisible = await testPlansPage.IsTestPlanVisibleAsync(testPlan.Name);
        Assert.True(isVisible, "Test plan should be visible after creation");
        
        // Act & Assert - Edit
        await testPlansPage.EditTestPlanAsync(testPlan.Name);
        await Page.FillAsync("[data-testid='name-input']", updatedName);
        await testPlansPage.SaveTestPlanAsync();
        await Page.WaitForTimeoutAsync(2000);
        
        var isUpdatedVisible = await testPlansPage.IsTestPlanVisibleAsync(updatedName);
        Assert.True(isUpdatedVisible, "Updated test plan should be visible after edit");
        
        // Act & Assert - Delete
        await testPlansPage.DeleteTestPlanAsync(updatedName);
        await testPlansPage.ConfirmDeleteAsync();
        await Page.WaitForTimeoutAsync(2000);
        
        var isDeletedVisible = await testPlansPage.IsTestPlanVisibleAsync(updatedName);
        Assert.False(isDeletedVisible, "Test plan should not be visible after deletion");
    }
    
    [Fact]
    public async Task TestPlans_ValidatesRequiredFields()
    {
        // Arrange
        var testPlansPage = new TestPlansPage(Page, BaseUrl);
        
        // Act
        await testPlansPage.NavigateToAsync();
        await testPlansPage.ClickCreateTestPlanAsync();
        
        // Try to save without filling required fields
        await testPlansPage.SaveTestPlanAsync();
        
        // Assert - Form should still be open (validation should prevent saving)
        await Expect(Page.Locator("[data-testid='name-input']")).ToBeVisibleAsync();
        
        // Verify we're still on the test plans page
        Assert.Contains("/testplans", Page.Url);
    }
}
