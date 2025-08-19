using Bunit;
using frontend.ComponentTests.TestHelpers;
using frontend.Pages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RqmtMgmtShared;
using Xunit;

namespace frontend.ComponentTests.Components.TestManagementTests;

/// <summary>
/// Tests for the TestPlans page component
/// </summary>
public class TestPlansTests : ComponentTestBase
{
    [Fact]
    public void TestPlans_RendersCorrectly_WithTitle()
    {
        // Arrange
        SetupMockServices();
        
        // Act
        var component = RenderComponent<TestPlans>();
        
        // Assert
        Assert.Contains("Test Plans", component.Markup);
        Assert.Contains("<h3>Test Plans</h3>", component.Markup);
    }
    
    [Fact]
    public void TestPlans_ShowsAddButton()
    {
        // Arrange
        SetupMockServices();
        
        // Act
        var component = RenderComponent<TestPlans>();
        
        // Assert
        var addButton = component.Find("button.btn-success");
        Assert.NotNull(addButton);
        Assert.Contains("Add Test Plan", addButton.TextContent);
    }
    
    [Fact]
    public void TestPlans_RendersTable_WithCorrectHeaders()
    {
        // Arrange
        SetupMockServices();
        
        // Act
        var component = RenderComponent<TestPlans>();
        
        // Assert
        var table = component.Find("table.table");
        Assert.NotNull(table);
        
        var headers = table.QuerySelectorAll("th");
        Assert.Equal(4, headers.Length);
        Assert.Equal("ID", headers[0].TextContent);
        Assert.Equal("Name", headers[1].TextContent);
        Assert.Equal("Type", headers[2].TextContent);
        Assert.Equal("Actions", headers[3].TextContent);
    }
    
    [Fact]
    public void TestPlans_DisplaysTestPlans_WhenDataLoaded()
    {
        // Arrange
        var mockTestPlans = new List<TestPlanDto>
        {
            new TestPlanDto { Id = 1, Name = "Test Plan 1", Type = "UserValidation" },
            new TestPlanDto { Id = 2, Name = "Test Plan 2", Type = "SoftwareVerification" }
        };
        
        SetupMockServices(mockTestPlans);
        
        // Act
        var component = RenderComponent<TestPlans>();
        
        // Assert
        var rows = component.FindAll("tbody tr");
        Assert.Equal(2, rows.Count);
        
        // Check first row
        var firstRowCells = rows[0].QuerySelectorAll("td");
        Assert.Equal("1", firstRowCells[0].TextContent);
        Assert.Contains("Test Plan 1", firstRowCells[1].TextContent);
        Assert.Contains("UserValidation", firstRowCells[2].TextContent);
        
        // Check second row
        var secondRowCells = rows[1].QuerySelectorAll("td");
        Assert.Equal("2", secondRowCells[0].TextContent);
        Assert.Contains("Test Plan 2", secondRowCells[1].TextContent);
        Assert.Contains("SoftwareVerification", secondRowCells[2].TextContent);
    }
    
    [Fact]
    public void TestPlans_DisplaysActionButtons_ForEachTestPlan()
    {
        // Arrange
        var mockTestPlans = new List<TestPlanDto>
        {
            new TestPlanDto { Id = 1, Name = "Test Plan", Type = "UserValidation" }
        };
        
        SetupMockServices(mockTestPlans);
        
        // Act
        var component = RenderComponent<TestPlans>();
        
        // Assert
        var actionButtons = component.FindAll("tbody tr td button");
        Assert.Equal(2, actionButtons.Count);
        
        Assert.Contains("Edit", actionButtons[0].TextContent);
        Assert.Contains("Delete", actionButtons[1].TextContent);
    }
    
    [Fact]
    public void TestPlans_ShowsLoadingMessage_WhenDataIsNull()
    {
        // Arrange
        var mockTestPlanService = GetMockService<ITestPlanService>();
        mockTestPlanService.Setup(s => s.GetAllAsync()).ReturnsAsync((List<TestPlanDto>)null!);
        
        // Act
        var component = RenderComponent<TestPlans>();
        
        // Assert
        Assert.Contains("Loading...", component.Markup);
    }
    
    [Fact]
    public void TestPlans_ShowsAddForm_WhenAddButtonClicked()
    {
        // Arrange
        SetupMockServices();
        var component = RenderComponent<TestPlans>();
        
        // Act
        var addButton = component.Find("button.btn-success");
        addButton.Click();
        
        // Assert
        var modal = component.Find(".modal.show");
        Assert.NotNull(modal);
        Assert.Contains("Add Test Plan", modal.QuerySelector(".modal-title")?.TextContent);
        
        // Verify form fields
        var nameInput = modal.QuerySelector("input.form-control");
        var saveButton = modal.QuerySelector("button.btn-primary");
        var cancelButton = modal.QuerySelector("button.btn-secondary");
        
        Assert.NotNull(nameInput);
        Assert.NotNull(saveButton);
        Assert.NotNull(cancelButton);
        Assert.Contains("Save", saveButton.TextContent);
        Assert.Contains("Cancel", cancelButton.TextContent);
    }
    
    [Fact]
    public void TestPlans_ShowsEditForm_WhenEditButtonClicked()
    {
        // Arrange
        var mockTestPlans = new List<TestPlanDto>
        {
            new TestPlanDto { Id = 1, Name = "Test Plan", Type = "UserValidation", Description = "Description" }
        };
        
        SetupMockServices(mockTestPlans);
        var component = RenderComponent<TestPlans>();
        
        // Act
        var editButton = component.FindAll("button").First(b => b.TextContent.Contains("Edit"));
        editButton.Click();
        
        // Assert
        var modal = component.Find(".modal.show");
        Assert.NotNull(modal);
        Assert.Contains("Edit Test Plan", modal.QuerySelector(".modal-title")?.TextContent);
        
        // Verify form is pre-populated
        var nameInput = modal.QuerySelector("input.form-control") as AngleSharp.Html.Dom.IHtmlInputElement;
        Assert.NotNull(nameInput);
        Assert.Equal("Test Plan", nameInput.Value);
    }
    
    [Fact]
    public void TestPlans_HidesForm_WhenCancelButtonClicked()
    {
        // Arrange
        SetupMockServices();
        var component = RenderComponent<TestPlans>();
        
        // Show form first
        var addButton = component.Find("button.btn-success");
        addButton.Click();
        Assert.NotNull(component.Find(".modal.show"));
        
        // Act
        var cancelButton = component.FindAll("button").First(b => b.TextContent.Contains("Cancel"));
        cancelButton.Click();
        
        // Assert
        var modals = component.FindAll(".modal.show");
        Assert.Empty(modals);
    }
    
    [Fact]
    public async Task TestPlans_CallsDeleteService_WhenDeleteButtonClicked()
    {
        // Arrange
        var mockTestPlans = new List<TestPlanDto>
        {
            new TestPlanDto { Id = 1, Name = "Test Plan", Type = "UserValidation" }
        };
        
        var mockTestPlanService = GetMockService<ITestPlanService>();
        mockTestPlanService.Setup(s => s.GetAllAsync()).ReturnsAsync(mockTestPlans);
        mockTestPlanService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);
        
        var component = RenderComponent<TestPlans>();
        
        // Act
        // Step 1: Click the initial Delete button to show the confirmation modal
        var deleteButton = component.FindAll("button").First(b => b.TextContent.Contains("Delete") && !b.TextContent.Contains("Test Plan"));
        deleteButton.Click();
        
        // Step 2: Click the confirmation button in the modal to actually delete
        var confirmDeleteButton = component.Find("button[data-testid='confirm-delete']");
        confirmDeleteButton.Click();
        
        // Wait for async operation
        await Task.Delay(100);
        
        // Assert
        mockTestPlanService.Verify(s => s.DeleteAsync(1), Times.Once);
        mockTestPlanService.Verify(s => s.GetAllAsync(), Times.AtLeast(2)); // Initial load + after delete
    }
    
    [Fact]
    public async Task TestPlans_CallsCreateService_WhenSavingNewTestPlan()
    {
        // Arrange
        var mockTestPlanService = GetMockService<ITestPlanService>();
        mockTestPlanService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<TestPlanDto>());
        mockTestPlanService.Setup(s => s.CreateAsync(It.IsAny<TestPlanDto>())).ReturnsAsync(new TestPlanDto { Id = 999, Name = "Test", Type = "Test" });
        
        var component = RenderComponent<TestPlans>();
        
        // Show add form
        var addButton = component.Find("button.btn-success");
        addButton.Click();
        
        // Fill in required fields
        var nameInput = component.Find(".modal input.form-control") as AngleSharp.Html.Dom.IHtmlInputElement;
        nameInput!.Value = "New Test Plan";
        await nameInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = "New Test Plan" });
        
        // Act
        var saveButton = component.FindAll("button").First(b => b.TextContent.Contains("Save"));
        saveButton.Click();
        
        // Wait for async operation
        await Task.Delay(100);
        
        // Assert
        mockTestPlanService.Verify(s => s.CreateAsync(It.IsAny<TestPlanDto>()), Times.Once);
        mockTestPlanService.Verify(s => s.GetAllAsync(), Times.AtLeast(2)); // Initial load + after save
    }
    
    [Fact]
    public async Task TestPlans_CallsUpdateService_WhenSavingExistingTestPlan()
    {
        // Arrange
        var mockTestPlans = new List<TestPlanDto>
        {
            new TestPlanDto { Id = 1, Name = "Test Plan", Type = "UserValidation" }
        };
        
        var mockTestPlanService = GetMockService<ITestPlanService>();
        mockTestPlanService.Setup(s => s.GetAllAsync()).ReturnsAsync(mockTestPlans);
        mockTestPlanService.Setup(s => s.UpdateAsync(It.IsAny<TestPlanDto>())).ReturnsAsync(true);
        
        var component = RenderComponent<TestPlans>();
        
        // Show edit form
        var editButton = component.FindAll("button").First(b => b.TextContent.Contains("Edit"));
        editButton.Click();
        
        // Act
        var saveButton = component.FindAll("button").First(b => b.TextContent.Contains("Save"));
        saveButton.Click();
        
        // Wait for async operation
        await Task.Delay(100);
        
        // Assert
        mockTestPlanService.Verify(s => s.UpdateAsync(It.IsAny<TestPlanDto>()), Times.Once);
        mockTestPlanService.Verify(s => s.GetAllAsync(), Times.AtLeast(2)); // Initial load + after save
    }
    
    private void SetupMockServices(List<TestPlanDto>? testPlans = null)
    {
        testPlans ??= new List<TestPlanDto>();
        
        var mockTestPlanService = GetMockService<ITestPlanService>();
        mockTestPlanService.Setup(s => s.GetAllAsync()).ReturnsAsync(testPlans);
        mockTestPlanService.Setup(s => s.CreateAsync(It.IsAny<TestPlanDto>())).ReturnsAsync(new TestPlanDto { Name = "Test", Type = "Test" });
        mockTestPlanService.Setup(s => s.UpdateAsync(It.IsAny<TestPlanDto>())).ReturnsAsync(true);
        mockTestPlanService.Setup(s => s.DeleteAsync(It.IsAny<int>())).ReturnsAsync(true);
    }
}