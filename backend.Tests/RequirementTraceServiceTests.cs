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
    /// Unit tests for RequirementTraceService to verify traceability operations and validation.
    /// </summary>
    public class RequirementTraceServiceTests
    {
        private RqmtMgmtDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        private async Task<(User user, Project project, Requirement req1, Requirement req2)> SetupTestDataAsync(RqmtMgmtDbContext context)
        {
            var user = new User { UserName = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow };
            var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            context.Users.Add(user);
            context.Projects.Add(project);
            await context.SaveChangesAsync();

            var req1 = new Requirement
            {
                Type = RequirementType.CRD,
                Title = "Customer Requirement",
                CreatedBy = user.Id,
                ProjectId = project.Id,
                CreatedAt = DateTime.UtcNow,
                Status = RequirementStatus.Approved,
                Version = 1
            };
            var req2 = new Requirement
            {
                Type = RequirementType.PRD,
                Title = "Product Requirement",
                CreatedBy = user.Id,
                ProjectId = project.Id,
                CreatedAt = DateTime.UtcNow,
                Status = RequirementStatus.Draft,
                Version = 1
            };
            context.Requirements.AddRange(req1, req2);
            await context.SaveChangesAsync();

            return (user, project, req1, req2);
        }

        [Fact]
        public async Task CreateAsync_ValidTrace_CreatesTrace()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new RequirementTraceService(context);
            var (user, project, req1, req2) = await SetupTestDataAsync(context);

            var traceDto = new RequirementTraceDto
            {
                SourceRequirementId = req1.Id,
                TargetRequirementId = req2.Id,
                TraceType = TraceType.DerivedFrom,
                CreatedBy = user.Id
            };

            // Act
            var result = await service.CreateAsync(traceDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(req1.Id, result.SourceRequirementId);
            Assert.Equal(req2.Id, result.TargetRequirementId);
            Assert.Equal(TraceType.DerivedFrom, result.TraceType);
            Assert.True(result.Id > 0);
        }

        [Fact]
        public async Task CreateAsync_SelfReference_ReturnsNull()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new RequirementTraceService(context);
            var (user, project, req1, req2) = await SetupTestDataAsync(context);

            var traceDto = new RequirementTraceDto
            {
                SourceRequirementId = req1.Id,
                TargetRequirementId = req1.Id, // Self-reference
                TraceType = TraceType.DerivedFrom,
                CreatedBy = user.Id
            };

            // Act
            var result = await service.CreateAsync(traceDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_DuplicateTrace_ReturnsNull()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new RequirementTraceService(context);
            var (user, project, req1, req2) = await SetupTestDataAsync(context);

            // Create first trace
            var existingTrace = new RequirementTrace
            {
                SourceRequirementId = req1.Id,
                TargetRequirementId = req2.Id,
                TraceType = TraceType.DerivedFrom,
                CreatedBy = user.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.RequirementTraces.Add(existingTrace);
            await context.SaveChangesAsync();

            var duplicateTraceDto = new RequirementTraceDto
            {
                SourceRequirementId = req1.Id,
                TargetRequirementId = req2.Id,
                TraceType = TraceType.DerivedFrom, // Same trace type
                CreatedBy = user.Id
            };

            // Act
            var result = await service.CreateAsync(duplicateTraceDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetBySourceRequirementIdAsync_ReturnsOutgoingTraces()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new RequirementTraceService(context);
            var (user, project, req1, req2) = await SetupTestDataAsync(context);

            var trace = new RequirementTrace
            {
                SourceRequirementId = req1.Id,
                TargetRequirementId = req2.Id,
                TraceType = TraceType.ImplementedBy,
                CreatedBy = user.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.RequirementTraces.Add(trace);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetBySourceRequirementIdAsync(req1.Id);

            // Assert
            Assert.Single(result);
            Assert.Equal(req2.Id, result.First().TargetRequirementId);
            Assert.Equal(TraceType.ImplementedBy, result.First().TraceType);
        }

        [Fact]
        public async Task GetByTargetRequirementIdAsync_ReturnsIncomingTraces()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new RequirementTraceService(context);
            var (user, project, req1, req2) = await SetupTestDataAsync(context);

            var trace = new RequirementTrace
            {
                SourceRequirementId = req1.Id,
                TargetRequirementId = req2.Id,
                TraceType = TraceType.ValidatedBy,
                CreatedBy = user.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.RequirementTraces.Add(trace);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetByTargetRequirementIdAsync(req2.Id);

            // Assert
            Assert.Single(result);
            Assert.Equal(req1.Id, result.First().SourceRequirementId);
            Assert.Equal(TraceType.ValidatedBy, result.First().TraceType);
        }

        [Fact]
        public async Task GetTraceChainAsync_ReturnsAllRelatedTraces()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new RequirementTraceService(context);
            var (user, project, req1, req2) = await SetupTestDataAsync(context);

            // Create third requirement
            var req3 = new Requirement
            {
                Type = RequirementType.SRS,
                Title = "Software Requirement",
                CreatedBy = user.Id,
                ProjectId = project.Id,
                CreatedAt = DateTime.UtcNow,
                Status = RequirementStatus.Draft,
                Version = 1
            };
            context.Requirements.Add(req3);
            await context.SaveChangesAsync();

            // Create traces: req1 -> req2 -> req3
            var trace1 = new RequirementTrace
            {
                SourceRequirementId = req1.Id,
                TargetRequirementId = req2.Id,
                TraceType = TraceType.DerivedFrom,
                CreatedBy = user.Id,
                CreatedAt = DateTime.UtcNow
            };
            var trace2 = new RequirementTrace
            {
                SourceRequirementId = req2.Id,
                TargetRequirementId = req3.Id,
                TraceType = TraceType.ImplementedBy,
                CreatedBy = user.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.RequirementTraces.AddRange(trace1, trace2);
            await context.SaveChangesAsync();

            // Act - Get chain for middle requirement
            var result = await service.GetTraceChainAsync(req2.Id);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.SourceRequirementId == req1.Id && t.TargetRequirementId == req2.Id);
            Assert.Contains(result, t => t.SourceRequirementId == req2.Id && t.TargetRequirementId == req3.Id);
        }

        [Fact]
        public async Task ValidateTraceAsync_ValidTrace_ReturnsTrue()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new RequirementTraceService(context);
            var (user, project, req1, req2) = await SetupTestDataAsync(context);

            // Act
            var result = await service.ValidateTraceAsync(req1.Id, req2.Id, TraceType.RelatedTo);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateTraceAsync_NonExistentRequirement_ReturnsFalse()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new RequirementTraceService(context);
            var (user, project, req1, req2) = await SetupTestDataAsync(context);

            // Act
            var result = await service.ValidateTraceAsync(req1.Id, 999, TraceType.RelatedTo);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateTraceAsync_CircularReference_ReturnsFalse()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new RequirementTraceService(context);
            var (user, project, req1, req2) = await SetupTestDataAsync(context);

            // Create existing trace: req1 -> req2
            var existingTrace = new RequirementTrace
            {
                SourceRequirementId = req1.Id,
                TargetRequirementId = req2.Id,
                TraceType = TraceType.DerivedFrom,
                CreatedBy = user.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.RequirementTraces.Add(existingTrace);
            await context.SaveChangesAsync();

            // Act - Try to create reverse trace: req2 -> req1
            var result = await service.ValidateTraceAsync(req2.Id, req1.Id, TraceType.ImplementedBy);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ExistingTrace_DeletesAndReturnsTrue()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new RequirementTraceService(context);
            var (user, project, req1, req2) = await SetupTestDataAsync(context);

            var trace = new RequirementTrace
            {
                SourceRequirementId = req1.Id,
                TargetRequirementId = req2.Id,
                TraceType = TraceType.DerivedFrom,
                CreatedBy = user.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.RequirementTraces.Add(trace);
            await context.SaveChangesAsync();

            // Act
            var result = await service.DeleteAsync(trace.Id);

            // Assert
            Assert.True(result);
            Assert.Null(await context.RequirementTraces.FindAsync(trace.Id));
        }

        [Fact]
        public async Task DeleteAsync_NonExistentTrace_ReturnsFalse()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new RequirementTraceService(context);

            // Act
            var result = await service.DeleteAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingTrace_ReturnsTraceWithNavigationProperties()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new RequirementTraceService(context);
            var (user, project, req1, req2) = await SetupTestDataAsync(context);

            var trace = new RequirementTrace
            {
                SourceRequirementId = req1.Id,
                TargetRequirementId = req2.Id,
                TraceType = TraceType.DerivedFrom,
                CreatedBy = user.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.RequirementTraces.Add(trace);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetByIdAsync(trace.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(req1.Id, result.SourceRequirementId);
            Assert.Equal(req2.Id, result.TargetRequirementId);
            Assert.Equal(TraceType.DerivedFrom, result.TraceType);
            Assert.NotNull(result.SourceRequirement);
            Assert.NotNull(result.TargetRequirement);
            Assert.Equal("Customer Requirement", result.SourceRequirement.Title);
            Assert.Equal("Product Requirement", result.TargetRequirement.Title);
        }
    }
}