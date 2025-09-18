using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Services;
using backend.Models;
using RqmtMgmtShared;
using Xunit;

namespace backend.Tests
{
    /// <summary>
    /// Unit tests for DocumentService traceability functionality.
    /// </summary>
    public class DocumentServiceTraceabilityTests
    {
        private RqmtMgmtDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        [Fact]
        public async Task GetTraceabilityMatrixAsync_DocumentNotFound_ReturnsNull()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentService(context);

            // Act
            var result = await service.GetTraceabilityMatrixAsync(999, "downstream");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTraceabilityMatrixAsync_NoRequirements_ReturnsEmptyMatrix()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentService(context);

	    var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            var document = new Document
		{
		    Type = DocumentType.PRD,
		    Title = "Test PRD",
		    ProjectId = 1,
		    CreatedAt = DateTime.UtcNow
		};

            context.Projects.Add(project);
            context.Documents.Add(document);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetTraceabilityMatrixAsync(document.Id, "downstream");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(document.Id, result.DocumentId);
            Assert.Equal("Test PRD", result.DocumentName);
            Assert.Equal(DocumentType.PRD, result.DocumentType);
            Assert.Equal("downstream", result.Direction);
            Assert.Empty(result.Traceability);
            Assert.Equal(0, result.CoverageStats.TotalRequirements);
            Assert.Equal(0, result.CoverageStats.CoveredRequirements);
            Assert.Equal(0, result.CoverageStats.CoveragePercentage);
        }

        [Fact]
        public async Task GetTraceabilityMatrixAsync_DownstreamDirection_ReturnsCorrectTraces()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentService(context);

            // Setup test data
	    var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            var prdDocument = new Document
		{
		    Type = DocumentType.PRD,
		    Title = "Test PRD",
		    ProjectId = 1,
		    CreatedAt = DateTime.UtcNow
		};
            var srsDocument = new Document
		{
		    Type = DocumentType.SRS,
		    Title = "Test SRS",
		    ProjectId = 1,
		    CreatedAt = DateTime.UtcNow
		};

            context.Projects.Add(project);
            context.Documents.AddRange(prdDocument, srsDocument);
            await context.SaveChangesAsync();

            var prdRequirement = new Requirement
		{
		    Title = "PRD Requirement",
		    Type = RequirementType.PRD,
		    Status = RequirementStatus.Draft,
		    DocumentId = prdDocument.Id,
		    ProjectId = project.Id,
		    CreatedAt = DateTime.UtcNow
		};

            var srsRequirement = new Requirement
		{
		    Title = "SRS Requirement",
		    Type = RequirementType.SRS,
		    Status = RequirementStatus.Draft,
		    DocumentId = srsDocument.Id,
		    ProjectId = project.Id,
		    CreatedAt = DateTime.UtcNow
		};

            context.Requirements.AddRange(prdRequirement, srsRequirement);
            await context.SaveChangesAsync();

            var trace = new RequirementTrace
		{
		    SourceRequirementId = prdRequirement.Id,
		    TargetRequirementId = srsRequirement.Id,
		    TraceType = TraceType.ImplementedBy,
		    CreatedAt = DateTime.UtcNow,
		    CreatedBy = 1
		};

            context.RequirementTraces.Add(trace);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetTraceabilityMatrixAsync(prdDocument.Id, "downstream");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(prdDocument.Id, result.DocumentId);
            Assert.Equal("downstream", result.Direction);
            Assert.Single(result.Traceability);

            var traceItem = result.Traceability.First();
            Assert.Equal(prdRequirement.Id, traceItem.SourceRequirement.Id);
            Assert.Single(traceItem.Traces);
            Assert.Equal(srsRequirement.Id, traceItem.Traces.First().TargetRequirement.Id);
            Assert.Equal(TraceType.ImplementedBy, traceItem.Traces.First().TraceType);

            // Coverage stats
            Assert.Equal(1, result.CoverageStats.TotalRequirements);
            Assert.Equal(1, result.CoverageStats.CoveredRequirements);
            Assert.Equal(100.0m, result.CoverageStats.CoveragePercentage);
            Assert.Empty(result.CoverageStats.UncoveredRequirements);
        }

        [Fact]
        public async Task GetTraceabilityMatrixAsync_UpstreamDirection_ReturnsCorrectTraces()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentService(context);

            // Setup test data (similar to downstream but reversed perspective)
	    var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            var crdDocument = new Document
		{
		    Type = DocumentType.CRD,
		    Title = "Test CRD",
		    ProjectId = 1,
		    CreatedAt = DateTime.UtcNow
		};
            var prdDocument = new Document
		{
		    Type = DocumentType.PRD,
		    Title = "Test PRD",
		    ProjectId = 1,
		    CreatedAt = DateTime.UtcNow
		};

            context.Projects.Add(project);
            context.Documents.AddRange(crdDocument, prdDocument);
            await context.SaveChangesAsync();

            var crdRequirement = new Requirement
		{
		    Title = "CRD Requirement",
		    Type = RequirementType.CRD,
		    Status = RequirementStatus.Draft,
		    DocumentId = crdDocument.Id,
		    ProjectId = project.Id,
		    CreatedAt = DateTime.UtcNow
		};

            var prdRequirement = new Requirement
		{
		    Title = "PRD Requirement",
		    Type = RequirementType.PRD,
		    Status = RequirementStatus.Draft,
		    DocumentId = prdDocument.Id,
		    ProjectId = project.Id,
		    CreatedAt = DateTime.UtcNow
		};

            context.Requirements.AddRange(crdRequirement, prdRequirement);
            await context.SaveChangesAsync();

            var trace = new RequirementTrace
		{
		    SourceRequirementId = crdRequirement.Id,
		    TargetRequirementId = prdRequirement.Id,
		    TraceType = TraceType.DerivedFrom,
		    CreatedAt = DateTime.UtcNow,
		    CreatedBy = 1
		};

            context.RequirementTraces.Add(trace);
            await context.SaveChangesAsync();

            // Act - Get upstream traces for PRD document (should show CRD requirements that trace TO it)
            var result = await service.GetTraceabilityMatrixAsync(prdDocument.Id, "upstream");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(prdDocument.Id, result.DocumentId);
            Assert.Equal("upstream", result.Direction);
            Assert.Single(result.Traceability);

            var traceItem = result.Traceability.First();
            Assert.Equal(prdRequirement.Id, traceItem.SourceRequirement.Id);
            Assert.Single(traceItem.Traces);
            Assert.Equal(crdRequirement.Id, traceItem.Traces.First().TargetRequirement.Id);
        }

        [Fact]
        public async Task GetTraceabilityMatrixAsync_UncoveredOnly_ReturnsOnlyUncoveredRequirements()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentService(context);

	    var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            var document = new Document
		{
		    Type = DocumentType.PRD,
		    Title = "Test PRD",
		    ProjectId = 1,
		    CreatedAt = DateTime.UtcNow
		};

            context.Projects.Add(project);
            context.Documents.Add(document);
            await context.SaveChangesAsync();

            // Add two requirements - one with trace, one without
            var coveredRequirement = new Requirement
		{
		    Title = "Covered Requirement",
		    Type = RequirementType.PRD,
		    Status = RequirementStatus.Draft,
		    DocumentId = document.Id,
		    ProjectId = project.Id,
		    CreatedAt = DateTime.UtcNow
		};

            var uncoveredRequirement = new Requirement
		{
		    Title = "Uncovered Requirement",
		    Type = RequirementType.PRD,
		    Status = RequirementStatus.Draft,
		    DocumentId = document.Id,
		    ProjectId = project.Id,
		    CreatedAt = DateTime.UtcNow
		};

            var targetRequirement = new Requirement
		{
		    Title = "Target Requirement",
		    Type = RequirementType.SRS,
		    Status = RequirementStatus.Draft,
		    ProjectId = project.Id,
		    CreatedAt = DateTime.UtcNow
		};

            context.Requirements.AddRange(coveredRequirement, uncoveredRequirement, targetRequirement);
            await context.SaveChangesAsync();

            // Add trace only for the covered requirement
            var trace = new RequirementTrace
		{
		    SourceRequirementId = coveredRequirement.Id,
		    TargetRequirementId = targetRequirement.Id,
		    TraceType = TraceType.ImplementedBy,
		    CreatedAt = DateTime.UtcNow,
		    CreatedBy = 1
		};

            context.RequirementTraces.Add(trace);
            await context.SaveChangesAsync();

            // Act
	    var result = await service.GetTraceabilityMatrixAsync(document.Id, "downstream", uncoveredOnly: true);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Traceability); // Only uncovered requirement should be returned
            Assert.Equal(uncoveredRequirement.Id, result.Traceability.First().SourceRequirement.Id);
            Assert.Empty(result.Traceability.First().Traces); // No traces for this requirement

            // Coverage stats should still show all requirements
            Assert.Equal(2, result.CoverageStats.TotalRequirements);
            Assert.Equal(1, result.CoverageStats.CoveredRequirements);
            Assert.Equal(50.0m, result.CoverageStats.CoveragePercentage);
            Assert.Single(result.CoverageStats.UncoveredRequirements);
        }
    }
}
