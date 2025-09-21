using Bunit;
using frontend.Pages;
using frontend.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RqmtMgmtShared;
using Xunit;

namespace frontend.ComponentTests.Pages;

/// <summary>
/// Tests for the Documents page component
/// </summary>
public class DocumentsTests : ComponentTestBase
{
    private Mock<IDocumentService> _mockDocumentService;
    private Mock<IProjectContextService> _mockProjectContextService;
    
    public DocumentsTests()
    {
        _mockDocumentService = new Mock<IDocumentService>();
        _mockProjectContextService = new Mock<IProjectContextService>();
    }
    
    [Fact]
    public void Documents_RendersCorrectly_WithTitle()
    {
        // Arrange
        SetupMockServices();
        
        // Act
        var component = RenderComponent<Documents>();
        
        // Assert
        Assert.Contains("Documents", component.Markup);
        Assert.Contains("New Document", component.Markup);
    }
    
    [Fact]
    public void Documents_DisplaysDocumentCards_WhenDataLoaded()
    {
        // Arrange
        var documents = new List<DocumentDto>
        {
            new DocumentDto 
            { 
                Id = 1, 
                Title = "Customer Requirements", 
                Type = DocumentType.CRD, 
                Status = DocumentStatus.Draft 
            },
            new DocumentDto 
            { 
                Id = 2, 
                Title = "Product Requirements", 
                Type = DocumentType.PRD, 
                Status = DocumentStatus.InReview 
            }
        };
        SetupMockServices(documents);
        
        // Act
        var component = RenderComponent<Documents>();
        
        // Assert
        Assert.Contains("Customer Requirements", component.Markup);
        Assert.Contains("Product Requirements", component.Markup);
        Assert.Contains("CRD", component.Markup);
        Assert.Contains("PRD", component.Markup);
    }
    
    [Fact]
    public void Documents_ShowsFilterOptions()
    {
        // Arrange
        SetupMockServices();
        
        // Act
        var component = RenderComponent<Documents>();
        
        // Assert
        Assert.Contains("All Document Types", component.Markup);
        Assert.Contains("CRD", component.Markup);
        Assert.Contains("PRD", component.Markup);
        Assert.Contains("SRS", component.Markup);
    }
    
    [Fact]
    public void Documents_ShowsSearchBox()
    {
        // Arrange
        SetupMockServices();
        
        // Act
        var component = RenderComponent<Documents>();
        
        // Assert
        var searchInput = component.Find("input[placeholder*='Search']");
        Assert.NotNull(searchInput);
    }
    
    [Fact]
    public void Documents_ShowsCreateButton()
    {
        // Arrange
        SetupMockServices();
        
        // Act
        var component = RenderComponent<Documents>();
        
        // Assert
        var createButton = component.Find("button:contains('New Document')");
        Assert.NotNull(createButton);
    }
    
    [Fact]
    public void Documents_DisplaysEmptyState_WhenNoDocuments()
    {
        // Arrange
        SetupMockServices(new List<DocumentDto>());
        
        // Act
        var component = RenderComponent<Documents>();
        
        // Assert
        Assert.Contains("No Documents Found", component.Markup);
    }
    
    [Fact]
    public void Documents_ShowsLoadingState()
    {
        // Arrange
        SetupMockServicesWithDelay();
        
        // Act
        var component = RenderComponent<Documents>();
        
        // Assert
        Assert.Contains("Loading", component.Markup);
    }
    
    private void SetupMockServices(List<DocumentDto>? documents = null)
    {
        documents ??= new List<DocumentDto>();
        
        _mockDocumentService
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(documents);
            
        _mockDocumentService
            .Setup(s => s.GetByProjectIdAsync(It.IsAny<int>()))
            .ReturnsAsync(documents);
            
        _mockProjectContextService
            .Setup(s => s.IsInProjectContext)
            .Returns(false);
            
        Services.AddSingleton(_mockDocumentService.Object);
        Services.AddSingleton(_mockProjectContextService.Object);
    }
    
    private void SetupMockServicesWithDelay()
    {
        _mockDocumentService
            .Setup(s => s.GetAllAsync())
            .Returns(async () =>
            {
                await Task.Delay(1000);
                return new List<DocumentDto>();
            });
            
        _mockProjectContextService
            .Setup(s => s.IsInProjectContext)
            .Returns(false);
            
        Services.AddSingleton(_mockDocumentService.Object);
        Services.AddSingleton(_mockProjectContextService.Object);
    }
}