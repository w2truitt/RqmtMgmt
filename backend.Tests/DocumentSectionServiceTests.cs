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
    /// Unit tests for DocumentSectionService to verify section management operations.
    /// </summary>
    public class DocumentSectionServiceTests
    {
        private RqmtMgmtDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        [Fact]
        public async Task GetByDocumentIdAsync_ReturnsSectionsOrderedBySectionOrder()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentSectionService(context);
            
            var user = new User { UserName = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow };
            var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            var document = new Document
            {
                Type = DocumentType.CRD,
                Title = "Test Document",
                CreatedBy = 1,
                ProjectId = 1,
                CreatedAt = DateTime.UtcNow
            };
            
            context.Users.Add(user);
            context.Projects.Add(project);
            context.Documents.Add(document);
            await context.SaveChangesAsync();

            var section1 = new DocumentSection
            {
                DocumentId = document.Id,
                Title = "Section B",
                SectionOrder = 2,
                CreatedAt = DateTime.UtcNow
            };
            var section2 = new DocumentSection
            {
                DocumentId = document.Id,
                Title = "Section A",
                SectionOrder = 1,
                CreatedAt = DateTime.UtcNow
            };
            
            context.DocumentSections.AddRange(section1, section2);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetByDocumentIdAsync(document.Id);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Section A", result.First().Title);
            Assert.Equal("Section B", result.Last().Title);
        }

        [Fact]
        public async Task CreateAsync_AutoAssignsSectionOrder()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentSectionService(context);
            
            var user = new User { UserName = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow };
            var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            var document = new Document
            {
                Type = DocumentType.CRD,
                Title = "Test Document",
                CreatedBy = 1,
                ProjectId = 1,
                CreatedAt = DateTime.UtcNow
            };
            
            context.Users.Add(user);
            context.Projects.Add(project);
            context.Documents.Add(document);
            await context.SaveChangesAsync();

            // Create first section with explicit order
            var existingSection = new DocumentSection
            {
                DocumentId = document.Id,
                Title = "Existing Section",
                SectionOrder = 1,
                CreatedAt = DateTime.UtcNow
            };
            context.DocumentSections.Add(existingSection);
            await context.SaveChangesAsync();

            var newSectionDto = new DocumentSectionDto
            {
                DocumentId = document.Id,
                Title = "New Section",
                Description = "Test Description",
                SectionOrder = 0 // Should auto-assign to 2
            };

            // Act
            var result = await service.CreateAsync(newSectionDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Section", result.Title);
            Assert.Equal(2, result.SectionOrder);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesSection()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentSectionService(context);
            
            var user = new User { UserName = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow };
            var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            var document = new Document
            {
                Type = DocumentType.CRD,
                Title = "Test Document",
                CreatedBy = 1,
                ProjectId = 1,
                CreatedAt = DateTime.UtcNow
            };
            
            context.Users.Add(user);
            context.Projects.Add(project);
            context.Documents.Add(document);
            await context.SaveChangesAsync();

            var section = new DocumentSection
            {
                DocumentId = document.Id,
                Title = "Original Title",
                Description = "Original Description",
                SectionOrder = 1,
                IsNotApplicable = false,
                CreatedAt = DateTime.UtcNow
            };
            context.DocumentSections.Add(section);
            await context.SaveChangesAsync();

            var updateDto = new DocumentSectionDto
            {
                Id = section.Id,
                DocumentId = document.Id,
                Title = "Updated Title",
                Description = "Updated Description",
                SectionOrder = 2,
                IsNotApplicable = true
            };

            // Act
            var result = await service.UpdateAsync(updateDto);

            // Assert
            Assert.True(result);
            var updated = await context.DocumentSections.FindAsync(section.Id);
            Assert.Equal("Updated Title", updated.Title);
            Assert.Equal("Updated Description", updated.Description);
            Assert.Equal(2, updated.SectionOrder);
            Assert.True(updated.IsNotApplicable);
            Assert.NotNull(updated.UpdatedAt);
        }

        [Fact]
        public async Task ReorderSectionsAsync_UpdatesSectionOrders()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentSectionService(context);
            
            var user = new User { UserName = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow };
            var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            var document = new Document
            {
                Type = DocumentType.CRD,
                Title = "Test Document",
                CreatedBy = 1,
                ProjectId = 1,
                CreatedAt = DateTime.UtcNow
            };
            
            context.Users.Add(user);
            context.Projects.Add(project);
            context.Documents.Add(document);
            await context.SaveChangesAsync();

            var section1 = new DocumentSection
            {
                DocumentId = document.Id,
                Title = "Section 1",
                SectionOrder = 1,
                CreatedAt = DateTime.UtcNow
            };
            var section2 = new DocumentSection
            {
                DocumentId = document.Id,
                Title = "Section 2",
                SectionOrder = 2,
                CreatedAt = DateTime.UtcNow
            };
            var section3 = new DocumentSection
            {
                DocumentId = document.Id,
                Title = "Section 3",
                SectionOrder = 3,
                CreatedAt = DateTime.UtcNow
            };
            
            context.DocumentSections.AddRange(section1, section2, section3);
            await context.SaveChangesAsync();

            // Reorder: section3, section1, section2
            var newOrder = new List<int> { section3.Id, section1.Id, section2.Id };

            // Act
            var result = await service.ReorderSectionsAsync(document.Id, newOrder);

            // Assert
            Assert.True(result);
            
            var reorderedSections = await context.DocumentSections
                .Where(s => s.DocumentId == document.Id)
                .OrderBy(s => s.SectionOrder)
                .ToListAsync();

            Assert.Equal(section3.Id, reorderedSections[0].Id);
            Assert.Equal(1, reorderedSections[0].SectionOrder);
            Assert.Equal(section1.Id, reorderedSections[1].Id);
            Assert.Equal(2, reorderedSections[1].SectionOrder);
            Assert.Equal(section2.Id, reorderedSections[2].Id);
            Assert.Equal(3, reorderedSections[2].SectionOrder);
        }

        [Fact]
        public async Task DeleteAsync_DeletesSection()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentSectionService(context);
            
            var user = new User { UserName = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow };
            var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            var document = new Document
            {
                Type = DocumentType.CRD,
                Title = "Test Document",
                CreatedBy = 1,
                ProjectId = 1,
                CreatedAt = DateTime.UtcNow
            };
            
            context.Users.Add(user);
            context.Projects.Add(project);
            context.Documents.Add(document);
            await context.SaveChangesAsync();

            var section = new DocumentSection
            {
                DocumentId = document.Id,
                Title = "Test Section",
                SectionOrder = 1,
                CreatedAt = DateTime.UtcNow
            };
            context.DocumentSections.Add(section);
            await context.SaveChangesAsync();

            // Act
            var result = await service.DeleteAsync(section.Id);
            var notFound = await service.DeleteAsync(999);

            // Assert
            Assert.True(result);
            Assert.False(notFound);
            Assert.Null(await context.DocumentSections.FindAsync(section.Id));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectSectionOrNull()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentSectionService(context);
            
            var user = new User { UserName = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow };
            var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            var document = new Document
            {
                Type = DocumentType.CRD,
                Title = "Test Document",
                CreatedBy = 1,
                ProjectId = 1,
                CreatedAt = DateTime.UtcNow
            };
            
            context.Users.Add(user);
            context.Projects.Add(project);
            context.Documents.Add(document);
            await context.SaveChangesAsync();

            var section = new DocumentSection
            {
                DocumentId = document.Id,
                Title = "Test Section",
                Description = "Test Description",
                SectionOrder = 1,
                IsNotApplicable = false,
                CreatedAt = DateTime.UtcNow
            };
            context.DocumentSections.Add(section);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetByIdAsync(section.Id);
            var notFound = await service.GetByIdAsync(999);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Section", result.Title);
            Assert.Equal("Test Description", result.Description);
            Assert.False(result.IsNotApplicable);
            Assert.Null(notFound);
        }

        [Fact]
        public async Task CreateAsync_WithSpecificOrder_UsesProvidedOrder()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentSectionService(context);
            
            var user = new User { UserName = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow };
            var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            var document = new Document
            {
                Type = DocumentType.CRD,
                Title = "Test Document",
                CreatedBy = 1,
                ProjectId = 1,
                CreatedAt = DateTime.UtcNow
            };
            
            context.Users.Add(user);
            context.Projects.Add(project);
            context.Documents.Add(document);
            await context.SaveChangesAsync();

            var sectionDto = new DocumentSectionDto
            {
                DocumentId = document.Id,
                Title = "Specific Order Section",
                SectionOrder = 5 // Specific order
            };

            // Act
            var result = await service.CreateAsync(sectionDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.SectionOrder);
        }
    }
}