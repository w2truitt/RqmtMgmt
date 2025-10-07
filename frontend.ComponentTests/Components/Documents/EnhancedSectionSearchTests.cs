using Bunit;
using frontend.Components.Documents;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RqmtMgmtShared;
using Xunit;

namespace frontend.ComponentTests.Components.Documents;

/// <summary>
/// Tests for the EnhancedSectionSearch component
/// </summary>
public class EnhancedSectionSearchTests : ComponentTestBase
{
    private readonly Mock<IDocumentSectionService> _mockDocumentSectionService;

    public EnhancedSectionSearchTests()
    {
        _mockDocumentSectionService = GetMockService<IDocumentSectionService>();
    }

    [Fact]
    public void EnhancedSectionSearch_RendersCorrectly_WithValidDocumentId()
    {
        // Arrange
        const int documentId = 1;
        
        // Act
        var component = RenderComponent<EnhancedSectionSearch>(parameters => parameters
            .Add(p => p.DocumentId, documentId));
        
        // Assert
        Assert.Contains("Enhanced Section Search", component.Markup);
        Assert.Contains("Search Term", component.Markup);
        Assert.Contains("Search Type", component.Markup);
        Assert.Contains("Similarity Threshold", component.Markup);
        Assert.Contains("Check for Duplicates", component.Markup);
    }

    [Fact]
    public void EnhancedSectionSearch_HasCorrectDefaultValues()
    {
        // Arrange
        const int documentId = 1;
        
        // Act
        var component = RenderComponent<EnhancedSectionSearch>(parameters => parameters
            .Add(p => p.DocumentId, documentId));
        
        // Assert - Check that fuzzy match option exists and is the default behavior
        var fuzzyMatchSelect = component.Find("#fuzzyMatch");
        var trueOption = fuzzyMatchSelect.QuerySelector("option[value='true']");
        Assert.NotNull(trueOption);
        Assert.Contains("Fuzzy Match", trueOption.TextContent);
        
        // Check that similarity threshold input exists with correct attributes
        var thresholdInput = component.Find("#similarityThreshold");
        Assert.Equal("0.5", thresholdInput.GetAttribute("min"));
        Assert.Equal("1.0", thresholdInput.GetAttribute("max"));
        Assert.Equal("0.1", thresholdInput.GetAttribute("step"));
    }

    [Fact]
    public void EnhancedSectionSearch_SearchButton_IsEnabledByDefault()
    {
        // Arrange
        const int documentId = 1;
        
        // Act
        var component = RenderComponent<EnhancedSectionSearch>(parameters => parameters
            .Add(p => p.DocumentId, documentId));
        
        // Assert
        var searchButton = component.Find("button:contains('Search')");
        Assert.False(searchButton.HasAttribute("disabled"));
    }

    [Fact]
    public async Task EnhancedSectionSearch_PerformsSearch_WhenSearchButtonClicked()
    {
        // Arrange
        const int documentId = 1;
        const string searchTerm = "authentication";
        
        var expectedResults = new List<DocumentSectionDto>
        {
            new DocumentSectionDto
            {
                Id = 1,
                Title = "User Authentication",
                Description = "Authentication requirements",
                SectionNumber = "3.1.1",
                ParentSectionId = null,
                SectionOrder = 1,
                IsNotApplicable = false,
                DocumentId = documentId,
                CreatedAt = DateTime.Now
            }
        };
        
        _mockDocumentSectionService
            .Setup(s => s.SearchSectionsAsync(documentId, searchTerm, true, 0.8))
            .ReturnsAsync(expectedResults);
        
        var component = RenderComponent<EnhancedSectionSearch>(parameters => parameters
            .Add(p => p.DocumentId, documentId));
        
        // Act
        var searchInput = component.Find("#searchTerm");
        await searchInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs
        {
            Value = searchTerm
        });
        
        var searchButton = component.Find("button:contains('Search')");
        await searchButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
        
        // Assert
        _mockDocumentSectionService.Verify(
            s => s.SearchSectionsAsync(documentId, searchTerm, true, 0.8), 
            Times.Once);
        
        // Check that results are displayed
        Assert.Contains("Search Results (1 found)", component.Markup);
        Assert.Contains("User Authentication", component.Markup);
        Assert.Contains("3.1.1", component.Markup);
    }

    [Fact]
    public async Task EnhancedSectionSearch_ShowsNoResults_WhenSearchReturnsEmpty()
    {
        // Arrange
        const int documentId = 1;
        const string searchTerm = "nonexistent";
        
        _mockDocumentSectionService
            .Setup(s => s.SearchSectionsAsync(documentId, searchTerm, true, 0.8))
            .ReturnsAsync(new List<DocumentSectionDto>());
        
        var component = RenderComponent<EnhancedSectionSearch>(parameters => parameters
            .Add(p => p.DocumentId, documentId));
        
        // Act
        var searchInput = component.Find("#searchTerm");
        await searchInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs
        {
            Value = searchTerm
        });
        
        var searchButton = component.Find("button:contains('Search')");
        await searchButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
        
        // Assert
        Assert.Contains("Search Results (0 found)", component.Markup);
        Assert.Contains("No sections found", component.Markup);
        Assert.Contains("Try adjusting your search term", component.Markup);
    }

    [Fact]
    public async Task EnhancedSectionSearch_PerformsDuplicateCheck_WhenCheckDuplicatesClicked()
    {
        // Arrange
        const int documentId = 1;
        const string duplicateTitle = "User Authentication";
        
        var duplicateResults = new List<DocumentSectionDto>
        {
            new DocumentSectionDto
            {
                Id = 2,
                Title = "User Authentication",
                Description = "Duplicate authentication section",
                SectionNumber = "4.1.1",
                ParentSectionId = null,
                SectionOrder = 2,
                IsNotApplicable = false,
                DocumentId = documentId,
                CreatedAt = DateTime.Now
            }
        };
        
        _mockDocumentSectionService
            .Setup(s => s.FindPotentialDuplicatesAsync(documentId, duplicateTitle, 0.8))
            .ReturnsAsync(duplicateResults);
        
        var component = RenderComponent<EnhancedSectionSearch>(parameters => parameters
            .Add(p => p.DocumentId, documentId));
        
        // Act
        var duplicateInput = component.Find("#duplicateTitle");
        await duplicateInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs
        {
            Value = duplicateTitle
        });
        
        var checkButton = component.Find("button:contains('Check Duplicates')");
        await checkButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
        
        // Assert
        _mockDocumentSectionService.Verify(
            s => s.FindPotentialDuplicatesAsync(documentId, duplicateTitle, 0.8), 
            Times.Once);
        
        // Check that duplicate results are displayed
        Assert.Contains("Potential Duplicates (1 found)", component.Markup);
        Assert.Contains("Found 1 potential duplicate", component.Markup);
        Assert.Contains("User Authentication", component.Markup);
    }

    [Fact]
    public async Task EnhancedSectionSearch_ShowsNoDuplicates_WhenNoneFound()
    {
        // Arrange
        const int documentId = 1;
        const string duplicateTitle = "Unique Section";
        
        _mockDocumentSectionService
            .Setup(s => s.FindPotentialDuplicatesAsync(documentId, duplicateTitle, 0.8))
            .ReturnsAsync(new List<DocumentSectionDto>());
        
        var component = RenderComponent<EnhancedSectionSearch>(parameters => parameters
            .Add(p => p.DocumentId, documentId));
        
        // Act
        var duplicateInput = component.Find("#duplicateTitle");
        await duplicateInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs
        {
            Value = duplicateTitle
        });
        
        var checkButton = component.Find("button:contains('Check Duplicates')");
        await checkButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
        
        // Assert
        Assert.Contains("Potential Duplicates (0 found)", component.Markup);
        Assert.Contains("No duplicates found! This title appears to be unique.", component.Markup);
    }

    [Fact]
    public void EnhancedSectionSearch_DisplaysCorrectSimilarityPercentage()
    {
        // Arrange
        const int documentId = 1;
        
        // Act
        var component = RenderComponent<EnhancedSectionSearch>(parameters => parameters
            .Add(p => p.DocumentId, documentId));
        
        // Assert - Default threshold should show as 80% in the component
        // The percentage is displayed next to the similarity threshold slider
        Assert.Contains("80%", component.Markup);
    }

    [Fact]
    public async Task EnhancedSectionSearch_HandlesSearchError_Gracefully()
    {
        // Arrange
        const int documentId = 1;
        const string searchTerm = "authentication";
        
        _mockDocumentSectionService
            .Setup(s => s.SearchSectionsAsync(documentId, searchTerm, true, 0.8))
            .ThrowsAsync(new Exception("Search service error"));
        
        // Mock JSRuntime to capture alert calls - use SetupVoid for void methods
        JSInterop.SetupVoid("alert", _ => true);
        
        var component = RenderComponent<EnhancedSectionSearch>(parameters => parameters
            .Add(p => p.DocumentId, documentId));
        
        // Act
        var searchInput = component.Find("#searchTerm");
        await searchInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs
        {
            Value = searchTerm
        });
        
        var searchButton = component.Find("button:contains('Search')");
        await searchButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
        
        // Assert - Should not crash and should show alert
        JSInterop.VerifyInvoke("alert");
    }
}