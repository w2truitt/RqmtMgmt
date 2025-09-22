using Bunit;
using frontend.Components.Traceability;
using frontend.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RqmtMgmtShared;
using Xunit;
using Xunit.Abstractions;

namespace frontend.ComponentTests.Components.Traceability;

/// <summary>
/// Tests for the TraceabilityMatrix component
/// </summary>
public class TraceabilityMatrixTests : ComponentTestBase
{
    private Mock<IDocumentService> _mockDocumentService;
    private readonly ITestOutputHelper _output;
    
    public TraceabilityMatrixTests(ITestOutputHelper output)
    {
        _output = output;
        _mockDocumentService = new Mock<IDocumentService>();
        Services.AddSingleton(_mockDocumentService.Object);
    }
    
    [Fact]
    public void TraceabilityMatrix_RendersCorrectly_WithTraceData()
    {
        // Arrange
        var matrixData = new TraceabilityMatrixDto
        {
            DocumentId = 1,
            DocumentName = "Test Document",
            DocumentType = DocumentType.PRD,
            Direction = "downstream",
            Traceability = new List<RequirementTraceabilityDto>
            {
                new RequirementTraceabilityDto
                {
                    SourceRequirement = new RequirementSummaryDto { Id = 1, Title = "Test Requirement", FullRequirementId = "PRD-001" },
                    Traces = new List<TraceRelationshipDto>
                    {
                        new TraceRelationshipDto 
                        { 
                            TraceId = 1, 
                            TraceType = TraceType.ImplementedBy,
                            TargetRequirement = new RequirementSummaryDto { Id = 2, Title = "Implementation", FullRequirementId = "SRS-001" }
                        }
                    }
                }
            },
            CoverageStats = new CoverageStatsDto
            {
                TotalRequirements = 5,
                CoveredRequirements = 3,
                CoveragePercentage = 60.0M
            }
        };
        
        _mockDocumentService
            .Setup(s => s.GetTraceabilityMatrixAsync(1, "downstream", false))
            .ReturnsAsync(matrixData);
        
        // Act
        var component = RenderComponent<TraceabilityMatrix>(parameters => parameters
            .Add(p => p.DocumentId, 1)
            .Add(p => p.Direction, "downstream"));
        
        // Debug: Print the actual markup
        _output.WriteLine("=== ACTUAL MARKUP ===");
        _output.WriteLine(component.Markup);
        _output.WriteLine("=== END MARKUP ===");
        
        // Assert
        Assert.Contains("Test Requirement", component.Markup);
        Assert.Contains("PRD-001", component.Markup);
        Assert.Contains("60.0%", component.Markup);
    }
    
    [Fact]
    public void TraceabilityMatrix_ShowsLoadingState()
    {
        // Arrange
        _mockDocumentService
            .Setup(s => s.GetTraceabilityMatrixAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(Task.Delay(1000).ContinueWith(_ => (TraceabilityMatrixDto?)new TraceabilityMatrixDto()));
        
        // Act
        var component = RenderComponent<TraceabilityMatrix>(parameters => parameters
            .Add(p => p.DocumentId, 1));
        
        // Assert
        Assert.Contains("Loading traceability", component.Markup);
    }
    
    [Fact]
    public void TraceabilityMatrix_ShowsCoverageStats()
    {
        // Arrange
        var matrixData = new TraceabilityMatrixDto
        {
            DocumentId = 1,
            DocumentName = "Test Document",
            DocumentType = DocumentType.CRD,
            Direction = "downstream",
            Traceability = new List<RequirementTraceabilityDto>(),
            CoverageStats = new CoverageStatsDto
            {
                TotalRequirements = 10,
                CoveredRequirements = 8,
                CoveragePercentage = 80.0M
            }
        };
        
        _mockDocumentService
            .Setup(s => s.GetTraceabilityMatrixAsync(1, "downstream", false))
            .ReturnsAsync(matrixData);
        
        // Act
        var component = RenderComponent<TraceabilityMatrix>(parameters => parameters
            .Add(p => p.DocumentId, 1));
        
        // Debug: Print the actual markup
        _output.WriteLine("=== ACTUAL MARKUP ===");
        _output.WriteLine(component.Markup);
        _output.WriteLine("=== END MARKUP ===");
        
        // Assert
        Assert.Contains("80.0%", component.Markup);
        Assert.Contains("Total Requirements", component.Markup);
    }
    
    [Fact]
    public void TraceabilityMatrix_HandlesEmptyData()
    {
        // Arrange
        var matrixData = new TraceabilityMatrixDto
        {
            DocumentId = 1,
            DocumentName = "Empty Document",
            DocumentType = DocumentType.SRS,
            Direction = "upstream",
            Traceability = new List<RequirementTraceabilityDto>(),
            CoverageStats = new CoverageStatsDto
            {
                TotalRequirements = 0,
                CoveredRequirements = 0,
                CoveragePercentage = 0.0M
            }
        };
        
        _mockDocumentService
            .Setup(s => s.GetTraceabilityMatrixAsync(1, "upstream", false))
            .ReturnsAsync(matrixData);
        
        // Act
        var component = RenderComponent<TraceabilityMatrix>(parameters => parameters
            .Add(p => p.DocumentId, 1)
            .Add(p => p.Direction, "upstream"));
        
        // Assert
        Assert.Contains("no requirements", component.Markup.ToLower());
    }
    
    [Fact]
    public void TraceabilityMatrix_HandlesServiceError()
    {
        // Arrange
        _mockDocumentService
            .Setup(s => s.GetTraceabilityMatrixAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ThrowsAsync(new Exception("Service error"));
        
        // Act
        var component = RenderComponent<TraceabilityMatrix>(parameters => parameters
            .Add(p => p.DocumentId, 1));
        
        // Assert - Should handle error gracefully
        Assert.NotNull(component.Markup);
    }
}