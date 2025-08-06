using Bunit;
using frontend.ComponentTests.TestHelpers;
using frontend.Pages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RqmtMgmtShared;
using Xunit;

namespace frontend.ComponentTests.Components.TestManagementTests;

/// <summary>
/// Tests for the TestCaseForm component
/// </summary>
public class TestCaseFormTests : ComponentTestBase
{
    [Fact]
    public void TestCaseForm_RendersCorrectly_WhenNew()
    {
        // Arrange
        SetupMockServices();
        
        // Act
        var component = RenderComponent<TestCaseForm>();
        
        // Assert
        var titleInput = component.Find("input[placeholder='Title']") ?? component.Find("input.form-control");
        var descriptionInput = component.Find("input[placeholder='Description']") ?? component.FindAll("input.form-control").Skip(1).FirstOrDefault();
        var suiteSelect = component.Find("select.form-control");
        var saveButton = component.Find("button[type='submit']");
        var cancelButton = component.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Cancel"));
        
        Assert.NotNull(titleInput);
        Assert.NotNull(suiteSelect);
        Assert.NotNull(saveButton);
        Assert.NotNull(cancelButton);
        Assert.Contains("Save", saveButton.TextContent);
    }
    
    [Fact]
    public void TestCaseForm_LoadsTestSuites_OnInitialization()
    {
        // Arrange
        var mockSuites = new List<TestSuiteDto>
        {
            new TestSuiteDto { Id = 1, Name = "Suite 1" },
            new TestSuiteDto { Id = 2, Name = "Suite 2" }
        };
        
        var mockTestSuiteService = GetMockService<ITestSuiteService>();
        mockTestSuiteService.Setup(s => s.GetAllAsync()).ReturnsAsync(mockSuites);
        
        var mockTestCaseService = GetMockService<ITestCaseService>();
        mockTestCaseService.Setup(s => s.CreateAsync(It.IsAny<TestCaseDto>())).ReturnsAsync(new TestCaseDto { Title = "Test" });
        
        // Act
        var component = RenderComponent<TestCaseForm>();
        
        // Assert
        var suiteSelect = component.Find("select.form-control");
        var options = suiteSelect.QuerySelectorAll("option");
        
        Assert.True(options.Length >= 3); // Default option + 2 suites
        Assert.Contains("Suite 1", options.Select(o => o.TextContent));
        Assert.Contains("Suite 2", options.Select(o => o.TextContent));
    }
    
    [Fact]
    public void TestCaseForm_ShowsAddStepButton()
    {
        // Arrange
        SetupMockServices();
        
        // Act
        var component = RenderComponent<TestCaseForm>();
        
        // Assert
        var addStepButton = component.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Add Step"));
        Assert.NotNull(addStepButton);
    }
    
    [Fact]
    public void TestCaseForm_AddsStep_WhenAddStepClicked()
    {
        // Arrange
        SetupMockServices();
        var component = RenderComponent<TestCaseForm>();
        
        // Act
        var addStepButton = component.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Add Step"));
        addStepButton?.Click();
        
        // Assert
        var stepInputs = component.FindAll("input[placeholder*='Step']");
        Assert.True(stepInputs.Count >= 2); // Should have at least description and expected result inputs
    }
    
    [Fact]
    public void TestCaseForm_RemovesStep_WhenRemoveClicked()
    {
        // Arrange
        SetupMockServices();
        var component = RenderComponent<TestCaseForm>();
        
        // Add a step first
        var addStepButton = component.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Add Step"));
        addStepButton?.Click();
        
        // Act
        var removeButton = component.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Remove"));
        var initialStepCount = component.FindAll("input[placeholder*='Step']").Count;
        removeButton?.Click();
        
        // Assert
        var finalStepCount = component.FindAll("input[placeholder*='Step']").Count;
        Assert.True(finalStepCount < initialStepCount);
    }
    
    [Fact]
    public void TestCaseForm_PrePopulatesFields_WhenEditModel()
    {
        // Arrange
        var testCase = new TestCaseDto
        {
            Id = 1,
            Title = "Test Case Title",
            Description = "Test Description",
            SuiteId = 1,
            Steps = new List<TestStepDto>
            {
                new TestStepDto { Description = "Step 1", ExpectedResult = "Result 1" }
            }
        };
        
        SetupMockServices();
        
        // Act
        var component = RenderComponent<TestCaseForm>(parameters => parameters
            .Add(p => p.EditModel, testCase));
        
        // Assert
        var titleInput = component.Find("input.form-control") as AngleSharp.Html.Dom.IHtmlInputElement;
        Assert.NotNull(titleInput);
        Assert.Equal("Test Case Title", titleInput.Value);
    }
    
    [Fact]
    public async Task TestCaseForm_CallsCreateService_WhenSavingNewTestCase()
    {
        // Arrange
        var mockTestCaseService = GetMockService<ITestCaseService>();
        mockTestCaseService.Setup(s => s.CreateAsync(It.IsAny<TestCaseDto>())).ReturnsAsync(new TestCaseDto { Id = 999, Title = "Test" });
        
        var mockTestSuiteService = GetMockService<ITestSuiteService>();
        mockTestSuiteService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<TestSuiteDto>());
        
        var component = RenderComponent<TestCaseForm>();
        
        // Fill in required fields
        var titleInput = component.Find("input.form-control") as AngleSharp.Html.Dom.IHtmlInputElement;
        titleInput!.Value = "New Test Case";
        await titleInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = "New Test Case" });
        
        // Act
        var saveButton = component.Find("button[type='submit']");
        saveButton.Click();
        
        // Wait for async operation
        await Task.Delay(100);
        
        // Assert
        mockTestCaseService.Verify(s => s.CreateAsync(It.IsAny<TestCaseDto>()), Times.Once);
    }
    
    [Fact]
    public async Task TestCaseForm_CallsUpdateService_WhenSavingExistingTestCase()
    {
        // Arrange
        var testCase = new TestCaseDto
        {
            Id = 1,
            Title = "Existing Test Case",
            Description = "Test Description"
        };
        
        var mockTestCaseService = GetMockService<ITestCaseService>();
        mockTestCaseService.Setup(s => s.UpdateAsync(It.IsAny<TestCaseDto>())).ReturnsAsync(true);
        
        var mockTestSuiteService = GetMockService<ITestSuiteService>();
        mockTestSuiteService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<TestSuiteDto>());
        
        var component = RenderComponent<TestCaseForm>(parameters => parameters
            .Add(p => p.EditModel, testCase));
        
        // Act
        var saveButton = component.Find("button[type='submit']");
        saveButton.Click();
        
        // Wait for async operation
        await Task.Delay(100);
        
        // Assert
        mockTestCaseService.Verify(s => s.UpdateAsync(It.IsAny<TestCaseDto>()), Times.Once);
    }
    
    private void SetupMockServices()
    {
        var mockTestCaseService = GetMockService<ITestCaseService>();
        mockTestCaseService.Setup(s => s.CreateAsync(It.IsAny<TestCaseDto>())).ReturnsAsync(new TestCaseDto { Title = "Test" });
        mockTestCaseService.Setup(s => s.UpdateAsync(It.IsAny<TestCaseDto>())).ReturnsAsync(true);
        
        var mockTestSuiteService = GetMockService<ITestSuiteService>();
        mockTestSuiteService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<TestSuiteDto>
        {
            new TestSuiteDto { Id = 1, Name = "Default Suite" }
        });
    }
}