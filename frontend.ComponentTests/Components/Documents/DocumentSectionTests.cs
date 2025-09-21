using Bunit;
using frontend.Components.Documents;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RqmtMgmtShared;
using Xunit;

namespace frontend.ComponentTests.Components.Documents;

/// <summary>
/// Tests for the DocumentSection component
/// </summary>
public class DocumentSectionTests : ComponentTestBase
{
    [Fact]
    public void DocumentSection_RendersCorrectly_WithBasicSection()
    {
        // Arrange
        var section = new DocumentSectionDto
        {
            Id = 1,
            Title = "Test Section",
            Description = "Test Description",
            SectionOrder = 1,
            IsNotApplicable = false
        };
        
        // Act
        var component = RenderComponent<DocumentSection>(parameters => parameters
            .Add(p => p.Section, section));
        
        // Assert
        Assert.Contains("Test Section", component.Markup);
        Assert.Contains("Test Description", component.Markup);
        Assert.DoesNotContain("Not Applicable", component.Markup);
    }
    
    [Fact]
    public void DocumentSection_ShowsNotApplicable_WhenMarkedNA()
    {
        // Arrange
        var section = new DocumentSectionDto
        {
            Id = 1,
            Title = "Test Section",
            Description = "Test Description",
            SectionOrder = 1,
            IsNotApplicable = true
        };
        
        // Act
        var component = RenderComponent<DocumentSection>(parameters => parameters
            .Add(p => p.Section, section));
        
        // Assert
        Assert.Contains("Test Section", component.Markup);
        Assert.Contains("Not Applicable", component.Markup);
    }
    
    [Fact]
    public void DocumentSection_DisplaysCorrectOrder()
    {
        // Arrange
        var section = new DocumentSectionDto
        {
            Id = 1,
            Title = "Test Section",
            Description = "Test Description",
            SectionOrder = 5,
            IsNotApplicable = false
        };
        
        // Act
        var component = RenderComponent<DocumentSection>(parameters => parameters
            .Add(p => p.Section, section));
        
        // Assert
        Assert.Contains("5.", component.Markup);
    }
    
    [Fact]
    public void DocumentSection_HandlesEmptyDescription()
    {
        // Arrange
        var section = new DocumentSectionDto
        {
            Id = 1,
            Title = "Test Section",
            Description = null,
            SectionOrder = 1,
            IsNotApplicable = false
        };
        
        // Act
        var component = RenderComponent<DocumentSection>(parameters => parameters
            .Add(p => p.Section, section));
        
        // Assert
        Assert.Contains("Test Section", component.Markup);
        // Should not crash with null description
        Assert.NotNull(component.Markup);
    }
    
    [Fact]
    public void DocumentSection_RendersWithRequirements_WhenProvided()
    {
        // Arrange
        var section = new DocumentSectionDto
        {
            Id = 1,
            Title = "Test Section",
            Description = "Test Description",
            SectionOrder = 1,
            IsNotApplicable = false
        };
        
        var requirements = new List<RequirementDto>
        {
            new RequirementDto { Id = 1, Title = "Req 1", SectionId = 1 },
            new RequirementDto { Id = 2, Title = "Req 2", SectionId = 1 }
        };
        
        // Act
        var component = RenderComponent<DocumentSection>(parameters => parameters
            .Add(p => p.Section, section)
            .Add(p => p.SectionRequirements, requirements));
        
        // Assert
        Assert.Contains("Req 1", component.Markup);
        Assert.Contains("Req 2", component.Markup);
    }
}