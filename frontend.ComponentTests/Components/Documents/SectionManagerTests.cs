using Bunit;
using frontend.Components.Documents;
using frontend.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RqmtMgmtShared;
using Xunit;

namespace frontend.ComponentTests.Components.Documents;

/// <summary>
/// Tests for the SectionManager component
/// </summary>
public class SectionManagerTests : ComponentTestBase
{
    private Mock<IDocumentSectionService> _mockSectionService;
    
    public SectionManagerTests()
    {
        _mockSectionService = new Mock<IDocumentSectionService>();
    }
    
    [Fact]
    public void SectionManager_RendersCorrectly_WithDocumentId()
    {
        // Arrange
        SetupMockSectionService();
        
        // Act
        var component = RenderComponent<SectionManager>(parameters => parameters
            .Add(p => p.DocumentId, 1));
        
        // Assert
        Assert.Contains("Document Sections", component.Markup);
        Assert.Contains("Add Section", component.Markup);
    }
    
    [Fact]
    public void SectionManager_DisplaysSections_WhenLoaded()
    {
        // Arrange
        var sections = new List<DocumentSectionDto>
        {
            new DocumentSectionDto { Id = 1, Title = "Overview", SectionOrder = 1 },
            new DocumentSectionDto { Id = 2, Title = "Requirements", SectionOrder = 2 }
        };
        SetupMockSectionService(sections);
        
        // Act
        var component = RenderComponent<SectionManager>(parameters => parameters
            .Add(p => p.DocumentId, 1));
        
        // Assert
        Assert.Contains("Overview", component.Markup);
        Assert.Contains("Requirements", component.Markup);
    }
    
    [Fact]
    public void SectionManager_ShowsAddForm_WhenAddButtonClicked()
    {
        // Arrange
        SetupMockSectionService();
        
        // Act
        var component = RenderComponent<SectionManager>(parameters => parameters
            .Add(p => p.DocumentId, 1));
        
        var addButton = component.Find("button:contains('Add Section')");
        addButton.Click();
        
        // Assert - Check for the actual form elements in the modal
        Assert.Contains("Title", component.Markup); // The label text is "Title" not "Section Title"
        Assert.Contains("Add Section", component.Markup); // The button text for new sections
        Assert.Contains("form-control", component.Markup); // Input field classes
    }
    
    [Fact]
    public void SectionManager_ShowsEditButtons_ForEachSection()
    {
        // Arrange
        var sections = new List<DocumentSectionDto>
        {
            new DocumentSectionDto { Id = 1, Title = "Overview", SectionOrder = 1 },
            new DocumentSectionDto { Id = 2, Title = "Requirements", SectionOrder = 2 }
        };
        SetupMockSectionService(sections);
        
        // Act
        var component = RenderComponent<SectionManager>(parameters => parameters
            .Add(p => p.DocumentId, 1));
        
        // Assert - Look for edit buttons with pencil icons (the actual implementation)
        var editButtons = component.FindAll("button i.bi-pencil");
        Assert.Equal(2, editButtons.Count);
    }
    
    [Fact]
    public void SectionManager_ShowsReorderButtons_ForMultipleSections()
    {
        // Arrange
        var sections = new List<DocumentSectionDto>
        {
            new DocumentSectionDto { Id = 1, Title = "Overview", SectionOrder = 1 },
            new DocumentSectionDto { Id = 2, Title = "Requirements", SectionOrder = 2 },
            new DocumentSectionDto { Id = 3, Title = "Conclusion", SectionOrder = 3 }
        };
        SetupMockSectionService(sections);
        
        // Act
        var component = RenderComponent<SectionManager>(parameters => parameters
            .Add(p => p.DocumentId, 1));
        
        // Assert - Look for up/down arrow icons (the actual implementation)
        var upButtons = component.FindAll("button i.bi-arrow-up");
        var downButtons = component.FindAll("button i.bi-arrow-down");
        
        // Should have up/down buttons for reordering
        Assert.True(upButtons.Count > 0 && downButtons.Count > 0);
    }
    
    [Fact]
    public void SectionManager_ShowsNotApplicableOption()
    {
        // Arrange
        var sections = new List<DocumentSectionDto>
        {
            new DocumentSectionDto { Id = 1, Title = "Overview", SectionOrder = 1, IsNotApplicable = true }
        };
        SetupMockSectionService(sections);
        
        // Act
        var component = RenderComponent<SectionManager>(parameters => parameters
            .Add(p => p.DocumentId, 1));
        
        // Assert - Check for N/A badge (the actual implementation shows "N/A" not "Not Applicable")
        Assert.Contains("N/A", component.Markup);
    }
    
    private void SetupMockSectionService(List<DocumentSectionDto>? sections = null)
    {
        sections ??= new List<DocumentSectionDto>();
        
        _mockSectionService
            .Setup(s => s.GetByDocumentIdAsync(It.IsAny<int>()))
            .ReturnsAsync(sections);
            
        _mockSectionService
            .Setup(s => s.CreateAsync(It.IsAny<DocumentSectionDto>()))
            .ReturnsAsync((DocumentSectionDto section) => section);
            
        _mockSectionService
            .Setup(s => s.UpdateAsync(It.IsAny<DocumentSectionDto>()))
            .ReturnsAsync(true);
            
        _mockSectionService
            .Setup(s => s.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(true);
            
        Services.AddSingleton(_mockSectionService.Object);
    }
}