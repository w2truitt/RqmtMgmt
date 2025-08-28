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
        
        // Wait for async initialization to complete
        component.WaitForState(() => component.Markup.Contains("Test Plans"));
        
        // Assert
        Assert.Contains("Test Plans", component.Markup);
        // The actual markup uses h1 with h3 class, not h3 element
        Assert.Contains("Test Plans", component.Find("h1").TextContent);
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
        // Arrange - need some test data for table to render
        var testPlans = new List<TestPlanDto>
        {
            new TestPlanDto { Id = 1, Name = "Test Plan", Type = "UserValidation", CreatedAt = DateTime.Now }
        };
        SetupMockServices(testPlans);
        
        // Act
        var component = RenderComponent<TestPlans>();
        
        // Wait for async initialization and table rendering
        component.WaitForState(() => component.FindAll("table.table").Count > 0, TimeSpan.FromSeconds(5));
        
        // Assert
        var table = component.Find("table.table");
        Assert.NotNull(table);
        
        var headers = table.QuerySelectorAll("th");
        Assert.Equal(6, headers.Length); // Updated to match actual table structure
        Assert.Equal("ID", headers[0].TextContent);
        Assert.Equal("Name", headers[1].TextContent);
        Assert.Equal("Description", headers[2].TextContent);
        Assert.Equal("Type", headers[3].TextContent);
        Assert.Equal("Created", headers[4].TextContent);
        Assert.Equal("Actions", headers[5].TextContent);
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
        
        // Wait for async data loading and rendering
        component.WaitForState(() => component.FindAll("tbody tr").Count == 2, TimeSpan.FromSeconds(5));
        
        // Assert
        var rows = component.FindAll("tbody tr");
        Assert.Equal(2, rows.Count);
        
        // Check first row
        var firstRowCells = rows[0].QuerySelectorAll("td");
        Assert.Contains("1", firstRowCells[0].TextContent); // ID column
        Assert.Contains("Test Plan 1", firstRowCells[1].TextContent); // Name column  
        Assert.Contains("No description", firstRowCells[2].TextContent); // Description column (empty)
        Assert.Contains("User Validation", firstRowCells[3].TextContent); // Type column (display name)
        
        // Check second row
        var secondRowCells = rows[1].QuerySelectorAll("td");
        Assert.Contains("2", secondRowCells[0].TextContent); // ID column
        Assert.Contains("Test Plan 2", secondRowCells[1].TextContent); // Name column
        Assert.Contains("No description", secondRowCells[2].TextContent); // Description column (empty)
        Assert.Contains("Software Verification", secondRowCells[3].TextContent); // Type column (display name)
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
        
        // Wait for async data loading to complete
        component.WaitForState(() => component.FindAll("tbody tr").Count > 0, TimeSpan.FromSeconds(5));
        
        // Assert
        var actionButtons = component.FindAll("tbody tr td button");
        Assert.Equal(3, actionButtons.Count); // View, Edit, Delete buttons
        
        // Check for the icons since buttons only contain icons
        Assert.True(actionButtons.Any(b => b.QuerySelector("i.bi-eye") != null), "Should have View button with eye icon");
        Assert.True(actionButtons.Any(b => b.QuerySelector("i.bi-pencil") != null), "Should have Edit button with pencil icon");
        Assert.True(actionButtons.Any(b => b.QuerySelector("i.bi-trash") != null), "Should have Delete button with trash icon");
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
        
        // Wait for component to load
        component.WaitForState(() => component.FindAll("button.btn-success").Count > 0, TimeSpan.FromSeconds(5));
        
        // Act
        var addButton = component.Find("button.btn-success");
        addButton.Click();
        
        // Wait for modal to appear
        component.WaitForState(() => component.FindAll(".modal.show").Count > 0, TimeSpan.FromSeconds(5));
        
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
        
        // Wait for data to load
        component.WaitForState(() => component.FindAll("tbody tr").Count > 0, TimeSpan.FromSeconds(5));
        
        // Act
        var editButton = component.FindAll("button").First(b => b.QuerySelector("i.bi-pencil") != null);
        editButton.Click();
        
        // Wait for modal to appear
        component.WaitForState(() => component.FindAll(".modal.show").Count > 0, TimeSpan.FromSeconds(5));
        
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
    public async Task TestPlans_CallsDeleteService_WhenDeleteButtonClicked()
    {
        // Arrange
        var mockTestPlans = new List<TestPlanDto>
        {
            new TestPlanDto { Id = 1, Name = "Test Plan", Type = "UserValidation" }
        };
        
        var mockTestPlanService = GetMockService<ITestPlanService>();
        mockTestPlanService.Setup(s => s.GetPagedAsync(It.IsAny<PaginationParameters>())).ReturnsAsync(new PagedResult<TestPlanDto>
        {
            Items = mockTestPlans,
            PageNumber = 1,
            PageSize = 10,
            TotalItems = mockTestPlans.Count
        });
        mockTestPlanService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);
        
        var component = RenderComponent<TestPlans>();
        
        // Wait for data to load
        component.WaitForState(() => component.FindAll("tbody tr").Count > 0, TimeSpan.FromSeconds(5));
        
        // Act
        // Step 1: Click the initial Delete button to show the confirmation modal
        var deleteButton = component.Find("button[data-testid='delete-testplan']");
        deleteButton.Click();
        
        // Wait for delete modal to appear
        component.WaitForState(() => component.FindAll(".modal.show").Count > 0, TimeSpan.FromSeconds(5));
        
        // Step 2: Click the confirm delete button in the modal
        var confirmDeleteButton = component.FindAll("button").First(b => b.TextContent.Contains("Delete Test Plan"));
        confirmDeleteButton.Click();
        
        // Wait for async operation
        await Task.Delay(100);
        
        // Assert
        mockTestPlanService.Verify(s => s.DeleteAsync(1), Times.Once);
        mockTestPlanService.Verify(s => s.GetPagedAsync(It.IsAny<PaginationParameters>()), Times.AtLeast(2)); // Initial load + after delete
    }
    
    [Fact]
    public void TestPlans_HidesForm_WhenCancelButtonClicked()
    {
        // Arrange
        SetupMockServices();
        var component = RenderComponent<TestPlans>();
        
        // Wait for component to load
        component.WaitForState(() => component.FindAll("button.btn-success").Count > 0, TimeSpan.FromSeconds(5));
        
        // Show form first
        var addButton = component.Find("button.btn-success");
        addButton.Click();
        
        // Wait for modal to appear
        component.WaitForState(() => component.FindAll(".modal.show").Count > 0, TimeSpan.FromSeconds(5));
        Assert.NotNull(component.Find(".modal.show"));
        
        // Act
        var cancelButton = component.FindAll("button").First(b => b.TextContent.Contains("Cancel"));
        cancelButton.Click();
        
        // Wait for modal to disappear
        component.WaitForState(() => component.FindAll(".modal.show").Count == 0, TimeSpan.FromSeconds(5));
        
        // Assert
        var modals = component.FindAll(".modal.show");
        Assert.Empty(modals);
    }
    
    [Fact]
    public async Task TestPlans_CallsCreateService_WhenSavingNewTestPlan()
    {
        // Arrange
        var mockTestPlanService = GetMockService<ITestPlanService>();
        mockTestPlanService.Setup(s => s.GetPagedAsync(It.IsAny<PaginationParameters>())).ReturnsAsync(new PagedResult<TestPlanDto>
        {
            Items = new List<TestPlanDto>(),
            PageNumber = 1,
            PageSize = 10,
            TotalItems = 0
        });
        mockTestPlanService.Setup(s => s.CreateAsync(It.IsAny<TestPlanDto>())).ReturnsAsync(new TestPlanDto { Id = 999, Name = "Test", Type = "Test" });
        
        var component = RenderComponent<TestPlans>();
        
        // Wait for component to load
        component.WaitForState(() => component.FindAll("button.btn-success").Count > 0, TimeSpan.FromSeconds(5));
        
        // Show add form
        var addButton = component.Find("button.btn-success");
        addButton.Click();
        
        // Wait for modal to appear
        component.WaitForState(() => component.FindAll(".modal.show").Count > 0, TimeSpan.FromSeconds(5));
        
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
        mockTestPlanService.Verify(s => s.GetPagedAsync(It.IsAny<PaginationParameters>()), Times.AtLeast(2)); // Initial load + after save
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
        mockTestPlanService.Setup(s => s.GetPagedAsync(It.IsAny<PaginationParameters>())).ReturnsAsync(new PagedResult<TestPlanDto>
        {
            Items = mockTestPlans,
            PageNumber = 1,
            PageSize = 10,
            TotalItems = mockTestPlans.Count
        });
        mockTestPlanService.Setup(s => s.UpdateAsync(It.IsAny<TestPlanDto>())).ReturnsAsync(true);
        
        var component = RenderComponent<TestPlans>();
        
        // Wait for data to load
        component.WaitForState(() => component.FindAll("tbody tr").Count > 0, TimeSpan.FromSeconds(5));
        
        // Show edit form
        var editButton = component.Find("button[data-testid='edit-testplan']");
        editButton.Click();
        
        // Wait for modal to appear
        component.WaitForState(() => component.FindAll(".modal.show").Count > 0, TimeSpan.FromSeconds(5));
        
        // Act
        var saveButton = component.FindAll("button").First(b => b.TextContent.Contains("Save"));
        saveButton.Click();
        
        // Wait for async operation
        await Task.Delay(100);
        
        // Assert
        mockTestPlanService.Verify(s => s.UpdateAsync(It.IsAny<TestPlanDto>()), Times.Once);
        mockTestPlanService.Verify(s => s.GetPagedAsync(It.IsAny<PaginationParameters>()), Times.AtLeast(2)); // Initial load + after save
    }
    
    private void SetupMockServices(List<TestPlanDto>? testPlans = null)
    {
        testPlans ??= new List<TestPlanDto>();
        
        // Create a proper PagedResult for the component
        var pagedResult = new PagedResult<TestPlanDto>
        {
            Items = testPlans,
            PageNumber = 1,
            PageSize = 10,
            TotalItems = testPlans.Count
        };
        
        var mockTestPlanService = GetMockService<ITestPlanService>();
        mockTestPlanService.Setup(s => s.GetPagedAsync(It.IsAny<PaginationParameters>())).ReturnsAsync(pagedResult);
        mockTestPlanService.Setup(s => s.GetPagedByProjectIdAsync(It.IsAny<int>(), It.IsAny<PaginationParameters>())).ReturnsAsync(pagedResult);
        mockTestPlanService.Setup(s => s.GetAllAsync()).ReturnsAsync(testPlans);
        mockTestPlanService.Setup(s => s.CreateAsync(It.IsAny<TestPlanDto>())).ReturnsAsync(new TestPlanDto { Name = "Test", Type = "Test" });
        mockTestPlanService.Setup(s => s.UpdateAsync(It.IsAny<TestPlanDto>())).ReturnsAsync(true);
        mockTestPlanService.Setup(s => s.DeleteAsync(It.IsAny<int>())).ReturnsAsync(true);
    }
}