using Bunit;
using frontend.Pages;
using frontend.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RqmtMgmtShared;
using Xunit;

namespace frontend.ComponentTests.Pages;

/// <summary>
/// Tests for the DocumentDetails page component
/// </summary>
public class DocumentDetailsTests : ComponentTestBase
{
    private Mock<IDocumentService> _mockDocumentService;
    private Mock<IDocumentSectionService> _mockSectionService;
    private Mock<IRequirementService> _mockRequirementService;
    private Mock<IProjectContextService> _mockProjectContextService;
    
    public DocumentDetailsTests()
    {
        _mockDocumentService = new Mock<IDocumentService>();
        _mockSectionService = new Mock<IDocumentSectionService>();
        _mockRequirementService = new Mock<IRequirementService>();
        _mockProjectContextService = new Mock<IProjectContextService>();
    }
    
    [Fact]
    public void DocumentDetails_RendersCorrectly_WithDocument()
    {
        // Arrange
        var document = new DocumentDto
        {
            Id = 1,
            Title = "Test Document",
            Type = DocumentType.PRD,
            Status = DocumentStatus.Draft,
            Version = "1.0"
        };
        SetupMockServices(document);
        
        // Act
        var component = RenderComponent<DocumentDetails>(parameters => parameters
            .Add(p => p.DocumentId, 1));
        
        // Assert
        Assert.Contains("Test Document", component.Markup);
        Assert.Contains("Product Requirement Document", component.Markup);
        Assert.Contains("Draft", component.Markup);
    }
    
    [Fact]
    public void DocumentDetails_ShowsLoadingState_Initially()
    {
        // Arrange
        SetupMockServicesWithDelay();
        
        // Act
        var component = RenderComponent<DocumentDetails>(parameters => parameters
            .Add(p => p.DocumentId, 1));
        
        // Assert
        Assert.Contains("Loading document", component.Markup);
    }
    
    [Fact]
    public void DocumentDetails_ShowsDocumentNotFound_WhenDocumentMissing()
    {
        // Arrange
        SetupMockServices(null);
        
        // Act
        var component = RenderComponent<DocumentDetails>(parameters => parameters
            .Add(p => p.DocumentId, 999));
        
        // Assert
        Assert.Contains("Document Not Found", component.Markup);
    }
    
    [Fact]
    public void DocumentDetails_DisplaysSections_WhenAvailable()
    {
        // Arrange
        var document = new DocumentDto { Id = 1, Title = "Test Document" };
        var sections = new List<DocumentSectionDto>
        {
            new DocumentSectionDto { Id = 1, Title = "Overview", SectionOrder = 1 },
            new DocumentSectionDto { Id = 2, Title = "Requirements", SectionOrder = 2 }
        };
        SetupMockServices(document, sections);
        
        // Act
        var component = RenderComponent<DocumentDetails>(parameters => parameters
            .Add(p => p.DocumentId, 1));
        
        // Assert
        Assert.Contains("Overview", component.Markup);
        Assert.Contains("Requirements", component.Markup);
    }
    
    [Fact]
    public void DocumentDetails_DisplaysRequirements_WhenAvailable()
    {
        // Arrange
        var document = new DocumentDto { Id = 1, Title = "Test Document" };
        var requirements = new List<RequirementDto>
        {
            new RequirementDto { Id = 1, Title = "Req 1", Type = RequirementType.CRD },
            new RequirementDto { Id = 2, Title = "Req 2", Type = RequirementType.PRD }
        };
        SetupMockServices(document, null, requirements);
        
        // Act
        var component = RenderComponent<DocumentDetails>(parameters => parameters
            .Add(p => p.DocumentId, 1));
        
        // Assert
        Assert.Contains("Req 1", component.Markup);
        Assert.Contains("Req 2", component.Markup);
    }
    
    [Fact]
    public void DocumentDetails_ShowsEditButton()
    {
        // Arrange
        var document = new DocumentDto { Id = 1, Title = "Test Document" };
        SetupMockServices(document);
        
        // Act
        var component = RenderComponent<DocumentDetails>(parameters => parameters
            .Add(p => p.DocumentId, 1));
        
        // Assert
        var editButton = component.Find("button:contains('Edit')");
        Assert.NotNull(editButton);
    }
    
    [Fact]
    public void DocumentDetails_ShowsBackButton()
    {
        // Arrange
        var document = new DocumentDto { Id = 1, Title = "Test Document" };
        SetupMockServices(document);
        
        // Act
        var component = RenderComponent<DocumentDetails>(parameters => parameters
            .Add(p => p.DocumentId, 1));
        
        // Assert
        var backButton = component.Find("button:contains('Back to Documents')");
        Assert.NotNull(backButton);
    }
    
    [Fact]
    public void DocumentDetails_ShowsRequirementAndSectionCounts()
    {
        // Arrange
        var document = new DocumentDto { Id = 1, Title = "Test Document" };
        var sections = new List<DocumentSectionDto>
        {
            new DocumentSectionDto { Id = 1, Title = "Section 1" }
        };
        var requirements = new List<RequirementDto>
        {
            new RequirementDto { Id = 1, Title = "Req 1" },
            new RequirementDto { Id = 2, Title = "Req 2" }
        };
        SetupMockServices(document, sections, requirements);
        
        // Act
        var component = RenderComponent<DocumentDetails>(parameters => parameters
            .Add(p => p.DocumentId, 1));
        
        // Assert
        Assert.Contains("Requirements:", component.Markup);
        Assert.Contains("Sections:", component.Markup);
        Assert.Contains("1", component.Markup); // Section count
        Assert.Contains("2", component.Markup); // Requirements count
    }
    
    private void SetupMockServices(DocumentDto? document = null, List<DocumentSectionDto>? sections = null, List<RequirementDto>? requirements = null)
    {
        sections ??= new List<DocumentSectionDto>();
        requirements ??= new List<RequirementDto>();
        
        _mockDocumentService
            .Setup(s => s.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(document);
            
        _mockSectionService
            .Setup(s => s.GetByDocumentIdAsync(It.IsAny<int>()))
            .ReturnsAsync(sections);
            
        _mockRequirementService
            .Setup(s => s.GetByDocumentIdAsync(It.IsAny<int>()))
            .ReturnsAsync(requirements);
            
        _mockProjectContextService
            .Setup(s => s.IsInProjectContext)
            .Returns(false);
            
        Services.AddSingleton(_mockDocumentService.Object);
        Services.AddSingleton(_mockSectionService.Object);
        Services.AddSingleton(_mockRequirementService.Object);
        Services.AddSingleton(_mockProjectContextService.Object);
    }
    
    private void SetupMockServicesWithDelay()
    {
        _mockDocumentService
            .Setup(s => s.GetByIdAsync(It.IsAny<int>()))
            .Returns(async () =>
            {
                await Task.Delay(1000);
                return new DocumentDto { Id = 1, Title = "Test Document" };
            });
            
        Services.AddSingleton(_mockDocumentService.Object);
        Services.AddSingleton(_mockSectionService.Object);
        Services.AddSingleton(_mockRequirementService.Object);
        Services.AddSingleton(_mockProjectContextService.Object);
    }
}