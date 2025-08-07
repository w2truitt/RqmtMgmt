using Bunit;
using frontend.Pages;
using frontend.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RqmtMgmtShared;
using Xunit;

namespace frontend.ComponentTests.Components.RequirementsTests;

/// <summary>
/// Tests for the Requirements component
/// </summary>
public class RequirementsTests : ComponentTestBase
{
    [Fact]
    public void Requirements_RendersCorrectly_WithTitle()
    {
        // Arrange
        SetupMockServices();
        
        // Act
        var component = RenderComponent<Requirements>();
        
        // Assert
        Assert.Contains("Requirements", component.Markup);
        Assert.Contains("<h1>Requirements</h1>", component.Markup);
    }
    
    [Fact]
    public void Requirements_RendersTable_WithCorrectHeaders()
    {
        // Arrange
        var mockRequirements = new List<RequirementDto>
        {
            new RequirementDto { Id = 1, Title = "Test Requirement", Status = RequirementStatus.Draft, Type = RequirementType.CRS }
        };
        SetupMockServices(mockRequirements);
        
        // Act
        var component = RenderComponent<Requirements>();
        
        // Assert
        var table = component.Find("table.table");
        Assert.NotNull(table);
        
        var headers = table.QuerySelectorAll("th");
        Assert.Equal(5, headers.Length);
        Assert.Equal("ID", headers[0].TextContent);
        Assert.Equal("Title", headers[1].TextContent);
        Assert.Equal("Status", headers[2].TextContent);
        Assert.Equal("Type", headers[3].TextContent);
        Assert.Equal("Actions", headers[4].TextContent);
    }
    
    [Fact]
    public void Requirements_DisplaysRequirements_WhenDataLoaded()
    {
        // Arrange
        var mockRequirements = new List<RequirementDto>
        {
            new RequirementDto { Id = 1, Title = "Test Requirement 1", Status = RequirementStatus.Draft, Type = RequirementType.CRS },
            new RequirementDto { Id = 2, Title = "Test Requirement 2", Status = RequirementStatus.Approved, Type = RequirementType.CRS }
        };
        
        SetupMockServices(mockRequirements);
        
        // Act
        var component = RenderComponent<Requirements>();
        
        // Assert
        var rows = component.FindAll("tbody tr");
        Assert.Equal(2, rows.Count);
        
        // Check first row
        var firstRowCells = rows[0].QuerySelectorAll("td");
        Assert.Equal("1", firstRowCells[0].TextContent);
        Assert.Contains("Test Requirement 1", firstRowCells[1].TextContent);
        Assert.Contains("Draft", firstRowCells[2].TextContent);
        
        // Check second row
        var secondRowCells = rows[1].QuerySelectorAll("td");
        Assert.Equal("2", secondRowCells[0].TextContent);
        Assert.Contains("Test Requirement 2", secondRowCells[1].TextContent);
        Assert.Contains("Approved", secondRowCells[2].TextContent);
    }
    
    [Fact]
    public void Requirements_DisplaysActionButtons_ForEachRequirement()
    {
        // Arrange
        var mockRequirements = new List<RequirementDto>
        {
            new RequirementDto { Id = 1, Title = "Test Requirement", Status = RequirementStatus.Draft, Type = RequirementType.CRS }
        };
        
        SetupMockServices(mockRequirements);
        
        // Act
        var component = RenderComponent<Requirements>();
        
        // Assert
        var actionButtons = component.FindAll("tbody tr td button");
        Assert.Equal(4, actionButtons.Count);
        
        Assert.Contains("Link Test Cases", actionButtons[0].TextContent);
        Assert.Contains("Details", actionButtons[1].TextContent);
        Assert.Contains("Edit", actionButtons[2].TextContent);
        Assert.Contains("Delete", actionButtons[3].TextContent);
    }
    
    [Fact]
    public void Requirements_ShowsAddButton()
    {
        // Arrange
        SetupMockServices();
        
        // Act
        var component = RenderComponent<Requirements>();
        
        // Assert
        var addButton = component.Find("button.btn-success");
        Assert.NotNull(addButton);
        Assert.Contains("Add Requirement", addButton.TextContent);
    }
    
    [Fact]
    public void Requirements_ShowsAddForm_WhenAddButtonClicked()
    {
        // Arrange
        SetupMockServices();
        var component = RenderComponent<Requirements>();
        
        // Act
        var addButton = component.Find("button.btn-success");
        addButton.Click();
        
        // Assert
        var modal = component.Find(".modal.show");
        Assert.NotNull(modal);
        Assert.Contains("Add Requirement", modal.QuerySelector(".modal-title")?.TextContent);
        
        // Verify form fields
        var titleInput = modal.QuerySelector("input.form-control");
        var statusSelect = modal.QuerySelector("select.form-control");
        Assert.NotNull(titleInput);
        Assert.NotNull(statusSelect);
    }
    
    [Fact]
    public void Requirements_ShowsEditForm_WhenEditButtonClicked()
    {
        // Arrange
        var mockRequirements = new List<RequirementDto>
        {
            new RequirementDto { Id = 1, Title = "Test Requirement", Status = RequirementStatus.Draft, Type = RequirementType.CRS }
        };
        
        SetupMockServices(mockRequirements);
        var component = RenderComponent<Requirements>();
        
        // Act
        var editButton = component.FindAll("button").First(b => b.TextContent.Contains("Edit"));
        editButton.Click();
        
        // Assert
        var modal = component.Find(".modal.show");
        Assert.NotNull(modal);
        Assert.Contains("Edit Requirement", modal.QuerySelector(".modal-title")?.TextContent);
        
        // Verify form is pre-populated
        var titleInput = modal.QuerySelector("input.form-control") as AngleSharp.Html.Dom.IHtmlInputElement;
        Assert.NotNull(titleInput);
        Assert.Equal("Test Requirement", titleInput.Value);
    }
    
    [Fact]
    public void Requirements_ShowsDetailsModal_WhenDetailsButtonClicked()
    {
        // Arrange
        var mockRequirements = new List<RequirementDto>
        {
            new RequirementDto { Id = 1, Title = "Test Requirement", Status = RequirementStatus.Draft, Type = RequirementType.CRS, Description = "Test Description" }
        };
        
        var mockReqService = GetMockService<IRequirementService>();
        var pagedResult = new PagedResult<RequirementDto>
        {
            Items = mockRequirements,
            PageNumber = 1,
            PageSize = 20,
            TotalItems = mockRequirements.Count
        };
        mockReqService.Setup(s => s.GetPagedAsync(It.IsAny<PaginationParameters>())).ReturnsAsync(pagedResult);
        mockReqService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(mockRequirements[0]);
        mockReqService.Setup(s => s.GetVersionsAsync(1)).ReturnsAsync(new List<RequirementVersionDto>());
        
        SetupOtherMockServices();
        
        var component = RenderComponent<Requirements>();
        
        // Act
        var detailsButton = component.FindAll("button").First(b => b.TextContent.Contains("Details"));
        detailsButton.Click();
        
        // Assert
        var modal = component.Find(".modal.show");
        Assert.NotNull(modal);
        Assert.Contains("Requirement Details", modal.QuerySelector(".modal-title")?.TextContent);
        Assert.Contains("Test Requirement", modal.TextContent);
        Assert.Contains("Test Description", modal.TextContent);
        Assert.Contains("Version History", modal.TextContent);
    }
    
    [Fact]
    public void Requirements_ShowsLinkModal_WhenLinkButtonClicked()
    {
        // Arrange
        var mockRequirements = new List<RequirementDto>
        {
            new RequirementDto { Id = 1, Title = "Test Requirement", Status = RequirementStatus.Draft, Type = RequirementType.CRS }
        };
        
        var mockTestCases = new List<TestCaseDto>
        {
            new TestCaseDto { Id = 1, Title = "Test Case 1" },
            new TestCaseDto { Id = 2, Title = "Test Case 2" }
        };
        
        var mockReqService = GetMockService<IRequirementService>();
        var pagedResult = new PagedResult<RequirementDto>
        {
            Items = mockRequirements,
            PageNumber = 1,
            PageSize = 20,
            TotalItems = mockRequirements.Count
        };
        mockReqService.Setup(s => s.GetPagedAsync(It.IsAny<PaginationParameters>())).ReturnsAsync(pagedResult);

        var mockTestCaseService = GetMockService<ITestCaseService>();
        mockTestCaseService.Setup(s => s.GetAllAsync()).ReturnsAsync(mockTestCases);

        var mockLinkService = GetMockService<IRequirementTestCaseLinkService>();
        mockLinkService.Setup(s => s.GetLinksForRequirement(1))
            .ReturnsAsync(new List<RequirementTestCaseLinkDto>());
        
        var component = RenderComponent<Requirements>();
        
        // Act
        var linkButton = component.FindAll("button").First(b => b.TextContent.Contains("Link Test Cases"));
        linkButton.Click();
        
        // Assert
        var modal = component.Find(".modal.show");
        Assert.NotNull(modal);
        Assert.Contains("Link Test Cases to Requirement", modal.QuerySelector(".modal-title")?.TextContent);
        
        var checkboxes = modal.QuerySelectorAll("input[type='checkbox']");
        Assert.Equal(2, checkboxes.Length); // One for each test case
    }
    
    [Fact]
    public void Requirements_HidesForm_WhenCancelButtonClicked()
    {
        // Arrange
        SetupMockServices();
        var component = RenderComponent<Requirements>();
        
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
    public async Task Requirements_CallsDeleteService_WhenDeleteButtonClicked()
    {
        // Arrange
        var mockRequirements = new List<RequirementDto>
        {
            new RequirementDto { Id = 1, Title = "Test Requirement", Status = RequirementStatus.Draft, Type = RequirementType.CRS }
        };
        
        var mockReqService = GetMockService<IRequirementService>();
        var pagedResult = new PagedResult<RequirementDto>
        {
            Items = mockRequirements,
            PageNumber = 1,
            PageSize = 20,
            TotalItems = mockRequirements.Count
        };
        mockReqService.Setup(s => s.GetPagedAsync(It.IsAny<PaginationParameters>())).ReturnsAsync(pagedResult);
        mockReqService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        SetupOtherMockServices();
        
        var component = RenderComponent<Requirements>();
        
        // Act
        var deleteButton = component.FindAll("button").First(b => b.TextContent.Contains("Delete"));
        deleteButton.Click();
        
        // Wait for async operation
        await Task.Delay(100);
        
        // Assert
        mockReqService.Verify(s => s.DeleteAsync(1), Times.Once);
        mockReqService.Verify(s => s.GetPagedAsync(It.IsAny<PaginationParameters>()), Times.AtLeast(2)); // Initial load + after delete
    }
    
    private void SetupMockServices(List<RequirementDto>? requirements = null)
    {
        requirements ??= new List<RequirementDto>();
        
        var mockReqService = GetMockService<IRequirementService>();
        var pagedResult = new PagedResult<RequirementDto>
        {
            Items = requirements,
            PageNumber = 1,
            PageSize = 20,
            TotalItems = requirements.Count
        };
        mockReqService.Setup(s => s.GetPagedAsync(It.IsAny<PaginationParameters>())).ReturnsAsync(pagedResult);
        
        SetupOtherMockServices();
    }
    
    private void SetupOtherMockServices()
    {
        var mockTestCaseService = GetMockService<ITestCaseService>();
        mockTestCaseService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<TestCaseDto>());

        var mockLinkService = GetMockService<IRequirementTestCaseLinkService>();
        mockLinkService.Setup(s => s.GetLinksForRequirement(It.IsAny<int>()))
            .ReturnsAsync(new List<RequirementTestCaseLinkDto>());
    }
}