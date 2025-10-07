using Bunit;
using frontend.Components;
using frontend.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RqmtMgmtShared;
using Xunit;

namespace frontend.ComponentTests.Components;

/// <summary>
/// Tests for the RequirementDocumentContext component
/// </summary>
public class RequirementDocumentContextTests : ComponentTestBase
{
    [Fact]
    public void RequirementDocumentContext_RendersCorrectly_WithDocumentInfo()
    {
        // Arrange
        var document = new DocumentDto
        {
            Id = 1,
            Title = "Test Document",
            Type = DocumentType.PRD,
            Status = DocumentStatus.Draft
        };
        
        var section = new DocumentSectionDto
        {
            Id = 1,
            ProjectId = 1,
            Title = "Test Section",
            SectionOrder = 1
        };
        
        var requirement = new RequirementDto
        {
            Id = 1,
            Title = "Test Requirement",
            DocumentId = 1,
            SectionId = 1
        };
        
        // Act
        var component = RenderComponent<RequirementDocumentContext>(parameters => parameters
            .Add(p => p.Requirement, requirement)
            .Add(p => p.Documents, new List<DocumentDto> { document })
            .Add(p => p.Sections, new List<DocumentSectionDto> { section }));
        
        // Assert
        Assert.Contains("Test Document", component.Markup);
        Assert.Contains("Test Section", component.Markup);
        Assert.Contains("PRD", component.Markup);
    }
    
    [Fact]
    public void RequirementDocumentContext_ShowsDocumentType()
    {
        // Arrange
        var document = new DocumentDto
        {
            Id = 1,
            Title = "Customer Requirements",
            Type = DocumentType.CRD,
            Status = DocumentStatus.Draft
        };
        
        var requirement = new RequirementDto
        {
            Id = 1,
            Title = "Test Requirement",
            DocumentId = 1,
            SectionId = 1 // Required field, use 1 as default section
        };
        
        // Act
        var component = RenderComponent<RequirementDocumentContext>(parameters => parameters
            .Add(p => p.Requirement, requirement)
            .Add(p => p.Documents, new List<DocumentDto> { document }));
        
        // Assert
        Assert.Contains("CRD", component.Markup);
        Assert.Contains("Customer Requirements", component.Markup);
    }
    
    [Fact]
    public void RequirementDocumentContext_ShowsSectionOrder()
    {
        // Arrange
        var document = new DocumentDto
        {
            Id = 1,
            Title = "Test Document",
            Type = DocumentType.PRD,
            Status = DocumentStatus.Draft
        };
        
        var section = new DocumentSectionDto
        {
            Id = 1,
            ProjectId = 1,
            Title = "Requirements Section",
            SectionOrder = 3
        };
        
        var requirement = new RequirementDto
        {
            Id = 1,
            Title = "Test Requirement",
            DocumentId = 1,
            SectionId = 1
        };
        
        // Act
        var component = RenderComponent<RequirementDocumentContext>(parameters => parameters
            .Add(p => p.Requirement, requirement)
            .Add(p => p.Documents, new List<DocumentDto> { document })
            .Add(p => p.Sections, new List<DocumentSectionDto> { section }));
        
        // Assert
        Assert.Contains("3.", component.Markup);
        Assert.Contains("Requirements Section", component.Markup);
    }
    
    [Fact]
    public void RequirementDocumentContext_HandlesStandaloneRequirement()
    {
        // Arrange
        var requirement = new RequirementDto
        {
            Id = 1,
            Title = "Standalone Requirement",
            DocumentId = null,
            SectionId = 0 // Use 0 to indicate no section (since it's now required)
        };
        
        // Act
        var component = RenderComponent<RequirementDocumentContext>(parameters => parameters
            .Add(p => p.Requirement, requirement));
        
        // Assert
        Assert.Contains("Standalone", component.Markup);
    }
    
    [Fact]
    public void RequirementDocumentContext_HandlesRequirementWithoutSection()
    {
        // Arrange
        var document = new DocumentDto
        {
            Id = 1,
            Title = "Test Document",
            Type = DocumentType.SRS,
            Status = DocumentStatus.Approved
        };
        
        var requirement = new RequirementDto
        {
            Id = 1,
            Title = "Test Requirement",
            DocumentId = 1,
            SectionId = 0 // Use 0 to indicate no section (since it's now required)
        };
        
        // Act
        var component = RenderComponent<RequirementDocumentContext>(parameters => parameters
            .Add(p => p.Requirement, requirement)
            .Add(p => p.Documents, new List<DocumentDto> { document }));
        
        // Assert
        Assert.Contains("SRS", component.Markup);
        Assert.Contains("Test Document", component.Markup);
        Assert.DoesNotContain("list-ol", component.Markup); // No section icon
    }
}