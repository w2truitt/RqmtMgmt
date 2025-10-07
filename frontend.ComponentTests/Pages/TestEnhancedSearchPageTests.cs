using Bunit;
using frontend.Pages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RqmtMgmtShared;
using Xunit;

namespace frontend.ComponentTests.Pages;

/// <summary>
/// Tests for the TestEnhancedSearch page component
/// </summary>
public class TestEnhancedSearchPageTests : ComponentTestBase
{
    private readonly Mock<IDocumentService> _mockDocumentService;

    public TestEnhancedSearchPageTests()
    {
        _mockDocumentService = GetMockService<IDocumentService>();
    }

    [Fact]
    public void TestEnhancedSearchPage_RendersCorrectly()
    {
        // Arrange
        _mockDocumentService
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<DocumentDto>());

        // Act
        var component = RenderComponent<TestEnhancedSearch>();

        // Assert
        Assert.Contains("Test Enhanced Section Search", component.Markup);
        Assert.Contains("Select Document", component.Markup);
        Assert.Contains("Instructions", component.Markup);
        Assert.Contains("Loading documents...", component.Markup);
    }

    [Fact]
    public async Task TestEnhancedSearchPage_LoadsDocuments_OnInitialization()
    {
        // Arrange
        var documents = new List<DocumentDto>
        {
            new DocumentDto
            {
                Id = 1,
                Title = "Software Requirements Specification",
                Type = DocumentType.SRS,
                Status = DocumentStatus.Published,
                Version = "1.0",
                ProjectId = 1,
                CreatedBy = 1,
                CreatedAt = DateTime.Now
            },
            new DocumentDto
            {
                Id = 2,
                Title = "System Design Document",
                Type = DocumentType.PRD,
                Status = DocumentStatus.Draft,
                Version = "0.1",
                ProjectId = 1,
                CreatedBy = 1,
                CreatedAt = DateTime.Now
            }
        };

        _mockDocumentService
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(documents);

        // Act
        var component = RenderComponent<TestEnhancedSearch>();
        
        // Wait for component to finish loading
        await Task.Delay(100);

        // Assert
        _mockDocumentService.Verify(s => s.GetAllAsync(), Times.Once);
        
        // Should show documents in dropdown
        Assert.Contains("Software Requirements Specification", component.Markup);
        Assert.Contains("System Design Document", component.Markup);
        Assert.DoesNotContain("Loading documents...", component.Markup);
    }

    [Fact]
    public async Task TestEnhancedSearchPage_ShowsEnhancedSearch_WhenDocumentSelected()
    {
        // Arrange
        var documents = new List<DocumentDto>
        {
            new DocumentDto
            {
                Id = 1,
                Title = "Software Requirements Specification",
                Type = DocumentType.SRS,
                Status = DocumentStatus.Published,
                Version = "1.0",
                ProjectId = 1,
                CreatedBy = 1,
                CreatedAt = DateTime.Now
            }
        };

        _mockDocumentService
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(documents);

        var component = RenderComponent<TestEnhancedSearch>();
        await Task.Delay(100); // Wait for documents to load

        // Act - Select a document
        var documentSelect = component.Find("#documentSelect");
        await documentSelect.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs
        {
            Value = "1"
        });

        // Assert
        Assert.Contains("Selected:", component.Markup);
        Assert.Contains("Software Requirements Specification", component.Markup);
        Assert.Contains("Type: SRS | Status: Published | Version: 1.0", component.Markup);
        
        // Enhanced search component should be rendered
        Assert.Contains("Enhanced Section Search", component.Markup);
    }

    [Fact]
    public async Task TestEnhancedSearchPage_ShowsNoDocuments_WhenListIsEmpty()
    {
        // Arrange
        _mockDocumentService
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<DocumentDto>());

        // Act
        var component = RenderComponent<TestEnhancedSearch>();
        await Task.Delay(100); // Wait for loading to complete

        // Assert
        Assert.Contains("No documents found.", component.Markup);
        Assert.DoesNotContain("Loading documents...", component.Markup);
    }

    [Fact]
    public async Task TestEnhancedSearchPage_HandlesDocumentLoadError_Gracefully()
    {
        // Arrange
        _mockDocumentService
            .Setup(s => s.GetAllAsync())
            .ThrowsAsync(new Exception("Service unavailable"));

        // Act
        var component = RenderComponent<TestEnhancedSearch>();
        await Task.Delay(100); // Wait for error handling

        // Assert - Should not crash and should show empty state
        Assert.DoesNotContain("Loading documents...", component.Markup);
        Assert.Contains("No documents found.", component.Markup);
    }

    [Fact]
    public void TestEnhancedSearchPage_ShowsInstructions()
    {
        // Arrange
        _mockDocumentService
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<DocumentDto>());

        // Act
        var component = RenderComponent<TestEnhancedSearch>();

        // Assert - Check that instructions are present
        Assert.Contains("Select a document from the dropdown", component.Markup);
        Assert.Contains("Use the search functionality to find sections", component.Markup);
        Assert.Contains("Test fuzzy matching with different similarity thresholds", component.Markup);
        Assert.Contains("Try the duplicate detection feature", component.Markup);
        Assert.Contains("View section details by clicking the View button", component.Markup);
        
        // Check search tips
        Assert.Contains("Search Tips:", component.Markup);
        Assert.Contains("Exact Match:", component.Markup);
        Assert.Contains("Fuzzy Match:", component.Markup);
        Assert.Contains("Similarity Threshold:", component.Markup);
    }
}
