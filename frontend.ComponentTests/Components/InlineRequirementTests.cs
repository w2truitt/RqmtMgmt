using Bunit;
using frontend.Components;
using frontend.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RqmtMgmtShared;
using Xunit;

namespace frontend.ComponentTests.Components;

/// <summary>
/// Tests for the InlineRequirement component
/// </summary>
public class InlineRequirementTests : ComponentTestBase
{
    private Mock<IRequirementService> _mockRequirementService;
    
    public InlineRequirementTests()
    {
        _mockRequirementService = new Mock<IRequirementService>();
    }
    
    [Fact]
    public void InlineRequirement_RendersInViewMode_ByDefault()
    {
        // Arrange
        var requirement = new RequirementDto
        {
            Id = 1,
            Title = "Test Requirement",
            Description = "Test Description",
            Status = RequirementStatus.Draft,
            Type = RequirementType.CRD
        };
        SetupMockRequirementService();
        
        // Act
        var component = RenderComponent<InlineRequirement>(parameters => parameters
            .Add(p => p.Requirement, requirement));
        
        // Assert
        Assert.Contains("Test Requirement", component.Markup);
        Assert.Contains("Test Description", component.Markup);
        Assert.DoesNotContain("form-control", component.Markup); // Not in edit mode
    }
    
    [Fact]
    public void InlineRequirement_ShowsEditMode_WhenClicked()
    {
        // Arrange
        var requirement = new RequirementDto
        {
            Id = 1,
            Title = "Test Requirement",
            Description = "Test Description",
            Status = RequirementStatus.Draft,
            Type = RequirementType.CRD
        };
        SetupMockRequirementService();
        var component = RenderComponent<InlineRequirement>(parameters => parameters
            .Add(p => p.Requirement, requirement));
        
        // Act
        var titleElement = component.Find(".requirement-title");
        titleElement?.Click();
        
        // Assert
        Assert.Contains("form-control", component.Markup); // Now in edit mode
        Assert.Contains("Save", component.Markup);
        Assert.Contains("Cancel", component.Markup);
    }
    
    [Fact]
    public void InlineRequirement_DisplaysCorrectStatus()
    {
        // Arrange
        var requirement = new RequirementDto
        {
            Id = 1,
            Title = "Test Requirement",
            Description = "Test Description",
            Status = RequirementStatus.Approved,
            Type = RequirementType.CRD
        };
        SetupMockRequirementService();
        
        // Act
        var component = RenderComponent<InlineRequirement>(parameters => parameters
            .Add(p => p.Requirement, requirement));
        
        // Assert
        Assert.Contains("Approved", component.Markup);
    }
    
    [Fact]
    public void InlineRequirement_DisplaysCorrectType()
    {
        // Arrange
        var requirement = new RequirementDto
        {
            Id = 1,
            Title = "Test Requirement",
            Description = "Test Description",
            Status = RequirementStatus.Draft,
            Type = RequirementType.PRD
        };
        SetupMockRequirementService();
        
        // Act
        var component = RenderComponent<InlineRequirement>(parameters => parameters
            .Add(p => p.Requirement, requirement));
        
        // Assert
        Assert.Contains("PRD", component.Markup);
    }
    
    [Fact]
    public void InlineRequirement_ShowsDeleteButton_WhenAllowDeleteIsTrue()
    {
        // Arrange
        var requirement = new RequirementDto
        {
            Id = 1,
            Title = "Test Requirement",
            Description = "Test Description",
            Status = RequirementStatus.Draft,
            Type = RequirementType.CRD
        };
        SetupMockRequirementService();
        
        // Act
        var component = RenderComponent<InlineRequirement>(parameters => parameters
            .Add(p => p.Requirement, requirement)
            .Add(p => p.AllowDelete, true));
        
        // Assert
        Assert.Contains("Delete", component.Markup);
    }
    
    [Fact]
    public void InlineRequirement_HidesDeleteButton_WhenAllowDeleteIsFalse()
    {
        // Arrange
        var requirement = new RequirementDto
        {
            Id = 1,
            Title = "Test Requirement",
            Description = "Test Description",
            Status = RequirementStatus.Draft,
            Type = RequirementType.CRD
        };
        SetupMockRequirementService();
        
        // Act
        var component = RenderComponent<InlineRequirement>(parameters => parameters
            .Add(p => p.Requirement, requirement)
            .Add(p => p.AllowDelete, false));
        
        // Assert
        Assert.DoesNotContain("Delete", component.Markup);
    }
    
    [Fact]
    public void InlineRequirement_HandlesNewRequirement_WithoutId()
    {
        // Arrange
        var requirement = new RequirementDto
        {
            Id = 0, // New requirement
            Title = "",
            Description = "",
            Status = RequirementStatus.Draft,
            Type = RequirementType.CRD
        };
        SetupMockRequirementService();
        
        // Act
        var component = RenderComponent<InlineRequirement>(parameters => parameters
            .Add(p => p.Requirement, requirement));
        
        // Assert
        // Should render in edit mode for new requirements
        Assert.Contains("form-control", component.Markup);
        Assert.Contains("Save", component.Markup);
    }
    
    private void SetupMockRequirementService()
    {
        _mockRequirementService
            .Setup(s => s.UpdateAsync(It.IsAny<RequirementDto>()))
            .ReturnsAsync(true);
            
        _mockRequirementService
            .Setup(s => s.CreateAsync(It.IsAny<RequirementDto>()))
            .ReturnsAsync(new RequirementDto { Id = 1, Title = "New Requirement" });
            
        _mockRequirementService
            .Setup(s => s.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(true);
            
        Services.AddSingleton(_mockRequirementService.Object);
    }
}