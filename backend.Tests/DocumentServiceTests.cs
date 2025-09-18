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
    /// Unit tests for DocumentService to verify CRUD operations and business logic.
    /// </summary>
    public class DocumentServiceTests
    {
        private RqmtMgmtDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllDocuments()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentService(context);
            
            var user = new User { UserName = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow };
            var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            context.Users.Add(user);
            context.Projects.Add(project);
            await context.SaveChangesAsync();

            var document = new Document
            {
                Type = DocumentType.CRD,
                Title = "Test Document",
                CreatedBy = user.Id,
                ProjectId = project.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Documents.Add(document);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Test Document", result.First().Title);
            Assert.Equal(DocumentType.CRD, result.First().Type);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectDocumentOrNull()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentService(context);
            
            var user = new User { UserName = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow };
            var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            context.Users.Add(user);
            context.Projects.Add(project);
            await context.SaveChangesAsync();

            var document = new Document
            {
                Type = DocumentType.PRD,
                Title = "Product Requirements",
                CreatedBy = user.Id,
                ProjectId = project.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Documents.Add(document);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetByIdAsync(document.Id);
            var notFound = await service.GetByIdAsync(999);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Product Requirements", result.Title);
            Assert.Equal(DocumentType.PRD, result.Type);
            Assert.Null(notFound);
        }

        [Fact]
        public async Task CreateAsync_AddsDocument()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentService(context);
            
            var user = new User { UserName = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow };
            var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            context.Users.Add(user);
            context.Projects.Add(project);
            await context.SaveChangesAsync();

            var documentDto = new DocumentDto
            {
                Type = DocumentType.SRS,
                Title = "Software Requirements",
                Version = "1.0",
                DocumentOwner = "Test Owner",
                Objective = "Test Objective",
                CreatedBy = user.Id,
                ProjectId = project.Id
            };

            // Act
            var result = await service.CreateAsync(documentDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Software Requirements", result.Title);
            Assert.Equal(DocumentType.SRS, result.Type);
            Assert.Equal("Test Owner", result.DocumentOwner);
            Assert.Equal("Test Objective", result.Objective);
            Assert.True(result.Id > 0);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesDocument()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentService(context);
            
            var user = new User { UserName = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow };
            var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            context.Users.Add(user);
            context.Projects.Add(project);
            await context.SaveChangesAsync();

            var document = new Document
            {
                Type = DocumentType.CRD,
                Title = "Original Title",
                Version = "1.0",
                CreatedBy = user.Id,
                ProjectId = project.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Documents.Add(document);
            await context.SaveChangesAsync();

            var updateDto = new DocumentDto
            {
                Id = document.Id,
                Type = DocumentType.CRD,
                Title = "Updated Title",
                Version = "1.1",
                DocumentOwner = "New Owner",
                Objective = "Updated Objective",
                CreatedBy = user.Id,
                ProjectId = project.Id
            };

            // Act
            var result = await service.UpdateAsync(updateDto);

            // Assert
            Assert.True(result);
            var updated = await context.Documents.FindAsync(document.Id);
            Assert.Equal("Updated Title", updated!.Title);
            Assert.Equal("1.1", updated.Version);
            Assert.Equal("New Owner", updated.DocumentOwner);
            Assert.Equal("Updated Objective", updated.Objective);
            Assert.NotNull(updated.UpdatedAt);
        }

        [Fact]
        public async Task DeleteAsync_DeletesWhenExists_ReturnsTrueElseFalse()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentService(context);
            
            var user = new User { UserName = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow };
            var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            context.Users.Add(user);
            context.Projects.Add(project);
            await context.SaveChangesAsync();

            var document = new Document
            {
                Type = DocumentType.CRD,
                Title = "Test Document",
                CreatedBy = user.Id,
                ProjectId = project.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Documents.Add(document);
            await context.SaveChangesAsync();

            // Act
            var deleteResult = await service.DeleteAsync(document.Id);
            var deleteNonExistent = await service.DeleteAsync(999);

            // Assert
            Assert.True(deleteResult);
            Assert.False(deleteNonExistent);
            Assert.Null(await context.Documents.FindAsync(document.Id));
        }

        [Fact]
        public async Task GetByProjectIdAsync_ReturnsProjectDocuments()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentService(context);
            
            var user = new User { UserName = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow };
            var project1 = new Project { Name = "Project 1", Code = "P1", CreatedAt = DateTime.UtcNow };
            var project2 = new Project { Name = "Project 2", Code = "P2", CreatedAt = DateTime.UtcNow };
            context.Users.Add(user);
            context.Projects.AddRange(project1, project2);
            await context.SaveChangesAsync();

            var doc1 = new Document
            {
                Type = DocumentType.CRD,
                Title = "Doc 1",
                CreatedBy = user.Id,
                ProjectId = project1.Id,
                CreatedAt = DateTime.UtcNow
            };
            var doc2 = new Document
            {
                Type = DocumentType.PRD,
                Title = "Doc 2",
                CreatedBy = user.Id,
                ProjectId = project2.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Documents.AddRange(doc1, doc2);
            await context.SaveChangesAsync();

            // Act
            var project1Docs = await service.GetByProjectIdAsync(project1.Id);

            // Assert
            Assert.Single(project1Docs);
            Assert.Equal("Doc 1", project1Docs.First().Title);
        }

        [Fact]
        public async Task GetByTypeAsync_ReturnsDocumentsOfSpecificType()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentService(context);
            
            var user = new User { UserName = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow };
            var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            context.Users.Add(user);
            context.Projects.Add(project);
            await context.SaveChangesAsync();

            var crdDoc = new Document
            {
                Type = DocumentType.CRD,
                Title = "CRD Document",
                CreatedBy = user.Id,
                ProjectId = project.Id,
                CreatedAt = DateTime.UtcNow
            };
            var prdDoc = new Document
            {
                Type = DocumentType.PRD,
                Title = "PRD Document",
                CreatedBy = user.Id,
                ProjectId = project.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Documents.AddRange(crdDoc, prdDoc);
            await context.SaveChangesAsync();

            // Act
            var crdDocs = await service.GetByTypeAsync(DocumentType.CRD);

            // Assert
            Assert.Single(crdDocs);
            Assert.Equal("CRD Document", crdDocs.First().Title);
            Assert.Equal(DocumentType.CRD, crdDocs.First().Type);
        }

        [Fact]
        public async Task GetPagedAsync_ReturnsPaginatedResults()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentService(context);
            
            var user = new User { UserName = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow };
            var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            context.Users.Add(user);
            context.Projects.Add(project);
            await context.SaveChangesAsync();

            // Create multiple documents
            for (int i = 1; i <= 5; i++)
            {
                var doc = new Document
                {
                    Type = DocumentType.CRD,
                    Title = $"Document {i}",
                    CreatedBy = user.Id,
                    ProjectId = project.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-i)
                };
                context.Documents.Add(doc);
            }
            await context.SaveChangesAsync();

            var parameters = new PaginationParameters
            {
                PageNumber = 1,
                PageSize = 3,
                SortBy = "title",
                SortDescending = false
            };

            // Act
            var result = await service.GetPagedAsync(parameters);

            // Assert
            Assert.Equal(3, result.Items.Count);
            Assert.Equal(5, result.TotalItems);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(3, result.PageSize);
        }

        [Fact]
        public async Task GetPagedAsync_WithSearchTerm_FiltersResults()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentService(context);
            
            var user = new User { UserName = "testuser", Email = "test@example.com", CreatedAt = DateTime.UtcNow };
            var project = new Project { Name = "Test Project", Code = "TEST", CreatedAt = DateTime.UtcNow };
            context.Users.Add(user);
            context.Projects.Add(project);
            await context.SaveChangesAsync();

            var doc1 = new Document
            {
                Type = DocumentType.CRD,
                Title = "Customer Requirements",
                CreatedBy = user.Id,
                ProjectId = project.Id,
                CreatedAt = DateTime.UtcNow
            };
            var doc2 = new Document
            {
                Type = DocumentType.PRD,
                Title = "Product Specification",
                CreatedBy = user.Id,
                ProjectId = project.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Documents.AddRange(doc1, doc2);
            await context.SaveChangesAsync();

            var parameters = new PaginationParameters
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = "Customer"
            };

            // Act
            var result = await service.GetPagedAsync(parameters);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("Customer Requirements", result.Items.First().Title);
        }
    }
}