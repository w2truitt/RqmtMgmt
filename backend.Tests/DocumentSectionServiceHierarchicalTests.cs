using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Comprehensive unit tests for hierarchical section operations in DocumentSectionService.
    /// Tests cover multi-level hierarchy, circular reference prevention, section numbering,
    /// and recursive operations.
    /// </summary>
    public class DocumentSectionServiceHierarchicalTests
    {
        private RqmtMgmtDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        private async Task<(RqmtMgmtDbContext context, int documentId)> SetupTestDocumentAsync()
        {
            var context = GetInMemoryContext();
            
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

            return (context, document.Id);
        }

        #region Create Hierarchical Sections Tests

        [Fact]
        public async Task CreateAsync_CreatesRootSection_WithLevel1()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var sectionDto = new DocumentSectionDto
            {
                DocumentId = documentId,
                ParentSectionId = null,
                Title = "Root Section",
                SectionOrder = 1
            };

            // Act
            var result = await service.CreateAsync(sectionDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Level);
            Assert.Equal(documentId, result.DocumentId);
            Assert.Null(result.ParentSectionId);
            Assert.NotNull(result.SectionNumber);
        }

        [Fact]
        public async Task CreateAsync_CreatesSubsection_WithLevel2()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            // Create root section
            var rootSection = new DocumentSection
            {
                DocumentId = documentId,
                Title = "Root Section",
                Level = 1,
                SectionOrder = 1,
                CreatedAt = DateTime.UtcNow
            };
            context.DocumentSections.Add(rootSection);
            await context.SaveChangesAsync();

            var subsectionDto = new DocumentSectionDto
            {
                DocumentId = null,
                ParentSectionId = rootSection.Id,
                Title = "Subsection",
                SectionOrder = 1
            };

            // Act
            var result = await service.CreateAsync(subsectionDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Level);
            Assert.Null(result.DocumentId);
            Assert.Equal(rootSection.Id, result.ParentSectionId);
        }

        [Fact]
        public async Task CreateAsync_CreatesThreeLevelHierarchy()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            // Level 1
            var level1Dto = new DocumentSectionDto
            {
                DocumentId = documentId,
                Title = "1. Main Section",
                SectionOrder = 1
            };
            var level1 = await service.CreateAsync(level1Dto);

            // Level 2
            var level2Dto = new DocumentSectionDto
            {
                ParentSectionId = level1!.Id,
                Title = "1.1 Subsection",
                SectionOrder = 1
            };
            var level2 = await service.CreateAsync(level2Dto);

            // Level 3
            var level3Dto = new DocumentSectionDto
            {
                ParentSectionId = level2!.Id,
                Title = "1.1.1 Sub-subsection",
                SectionOrder = 1
            };

            // Act
            var level3 = await service.CreateAsync(level3Dto);

            // Assert
            Assert.NotNull(level3);
            Assert.Equal(3, level3.Level);
            Assert.Equal(level2.Id, level3.ParentSectionId);
        }

        [Fact]
        public async Task CreateAsync_AutoAssignsOrderForSubsection()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var rootSection = new DocumentSection
            {
                DocumentId = documentId,
                Title = "Root",
                Level = 1,
                SectionOrder = 1,
                CreatedAt = DateTime.UtcNow
            };
            context.DocumentSections.Add(rootSection);
            await context.SaveChangesAsync();

            // Create first subsection
            var subsection1 = new DocumentSection
            {
                ParentSectionId = rootSection.Id,
                Title = "Sub 1",
                Level = 2,
                SectionOrder = 1,
                CreatedAt = DateTime.UtcNow
            };
            context.DocumentSections.Add(subsection1);
            await context.SaveChangesAsync();

            var newSubsectionDto = new DocumentSectionDto
            {
                ParentSectionId = rootSection.Id,
                Title = "Sub 2",
                SectionOrder = 0 // Auto-assign
            };

            // Act
            var result = await service.CreateAsync(newSubsectionDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.SectionOrder);
        }

        #endregion

        #region Hierarchical Query Tests

        [Fact]
        public async Task GetSectionHierarchyAsync_ReturnsTreeStructure()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            // Create hierarchy: Root -> Sub1 -> SubSub1, Sub2
            var root = new DocumentSection { DocumentId = documentId, Title = "Root", Level = 1, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(root);
            await context.SaveChangesAsync();

            var sub1 = new DocumentSection { ParentSectionId = root.Id, Title = "Sub1", Level = 2, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            var sub2 = new DocumentSection { ParentSectionId = root.Id, Title = "Sub2", Level = 2, SectionOrder = 2, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.AddRange(sub1, sub2);
            await context.SaveChangesAsync();

            var subSub1 = new DocumentSection { ParentSectionId = sub1.Id, Title = "SubSub1", Level = 3, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(subSub1);
            await context.SaveChangesAsync();

            // Act
            var hierarchy = await service.GetSectionHierarchyAsync(documentId);

            // Assert
            Assert.Single(hierarchy); // One root
            Assert.Equal("Root", hierarchy[0].Title);
            Assert.Equal(2, hierarchy[0].ChildSections!.Count); // Two children
            Assert.Equal("Sub1", hierarchy[0].ChildSections[0].Title);
            Assert.Single(hierarchy[0].ChildSections[0].ChildSections!); // Sub1 has one child
            Assert.Equal("SubSub1", hierarchy[0].ChildSections[0].ChildSections[0].Title);
        }

        [Fact]
        public async Task GetSectionWithChildrenAsync_ReturnsWithChildrenAtDepth1()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var root = new DocumentSection { DocumentId = documentId, Title = "Root", Level = 1, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(root);
            await context.SaveChangesAsync();

            var sub1 = new DocumentSection { ParentSectionId = root.Id, Title = "Sub1", Level = 2, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            var sub2 = new DocumentSection { ParentSectionId = root.Id, Title = "Sub2", Level = 2, SectionOrder = 2, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.AddRange(sub1, sub2);
            await context.SaveChangesAsync();

            var subSub1 = new DocumentSection { ParentSectionId = sub1.Id, Title = "SubSub1", Level = 3, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(subSub1);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetSectionWithChildrenAsync(root.Id, depth: 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.ChildSections!.Count);
            Assert.Empty(result.ChildSections[0].ChildSections!); // Depth 1, no grandchildren
        }

        [Fact]
        public async Task GetSectionWithChildrenAsync_ReturnsWithUnlimitedDepth()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var root = new DocumentSection { DocumentId = documentId, Title = "Root", Level = 1, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(root);
            await context.SaveChangesAsync();

            var sub1 = new DocumentSection { ParentSectionId = root.Id, Title = "Sub1", Level = 2, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(sub1);
            await context.SaveChangesAsync();

            var subSub1 = new DocumentSection { ParentSectionId = sub1.Id, Title = "SubSub1", Level = 3, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(subSub1);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetSectionWithChildrenAsync(root.Id, depth: -1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.ChildSections!);
            Assert.Single(result.ChildSections[0].ChildSections!); // Has grandchildren
            Assert.Equal("SubSub1", result.ChildSections[0].ChildSections[0].Title);
        }

        [Fact]
        public async Task GetChildSectionsAsync_ReturnsDirectChildren()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var root = new DocumentSection { DocumentId = documentId, Title = "Root", Level = 1, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(root);
            await context.SaveChangesAsync();

            var sub1 = new DocumentSection { ParentSectionId = root.Id, Title = "Sub1", Level = 2, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            var sub2 = new DocumentSection { ParentSectionId = root.Id, Title = "Sub2", Level = 2, SectionOrder = 2, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.AddRange(sub1, sub2);
            await context.SaveChangesAsync();

            // Act
            var children = await service.GetChildSectionsAsync(root.Id);

            // Assert
            Assert.Equal(2, children.Count);
            Assert.Equal("Sub1", children[0].Title);
            Assert.Equal("Sub2", children[1].Title);
        }

        [Fact]
        public async Task GetRootSectionsAsync_ReturnsOnlyRootSections()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var root1 = new DocumentSection { DocumentId = documentId, Title = "Root1", Level = 1, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            var root2 = new DocumentSection { DocumentId = documentId, Title = "Root2", Level = 1, SectionOrder = 2, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.AddRange(root1, root2);
            await context.SaveChangesAsync();

            var sub1 = new DocumentSection { ParentSectionId = root1.Id, Title = "Sub1", Level = 2, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(sub1);
            await context.SaveChangesAsync();

            // Act
            var roots = await service.GetRootSectionsAsync(documentId);

            // Assert
            Assert.Equal(2, roots.Count);
            Assert.All(roots, s => Assert.Null(s.ParentSectionId));
        }

        #endregion

        #region Move Section Tests

        [Fact]
        public async Task MoveSectionAsync_MovesToNewParent()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var root1 = new DocumentSection { DocumentId = documentId, Title = "Root1", Level = 1, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            var root2 = new DocumentSection { DocumentId = documentId, Title = "Root2", Level = 1, SectionOrder = 2, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.AddRange(root1, root2);
            await context.SaveChangesAsync();

            var sub1 = new DocumentSection { ParentSectionId = root1.Id, Title = "Sub1", Level = 2, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(sub1);
            await context.SaveChangesAsync();

            // Act
            var result = await service.MoveSectionAsync(sub1.Id, root2.Id);

            // Assert
            Assert.True(result);
            var moved = await context.DocumentSections.FindAsync(sub1.Id);
            Assert.Equal(root2.Id, moved!.ParentSectionId);
            Assert.Equal(2, moved.Level); // Still level 2
        }

        [Fact]
        public async Task MoveSectionAsync_MovesToDocumentRoot()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var root = new DocumentSection { DocumentId = documentId, Title = "Root", Level = 1, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(root);
            await context.SaveChangesAsync();

            var sub1 = new DocumentSection { ParentSectionId = root.Id, Title = "Sub1", Level = 2, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(sub1);
            await context.SaveChangesAsync();

            // Act
            var result = await service.MoveSectionAsync(sub1.Id, null);

            // Assert
            Assert.True(result);
            var moved = await context.DocumentSections.FindAsync(sub1.Id);
            Assert.Null(moved!.ParentSectionId);
            Assert.Equal(1, moved.Level); // Now level 1
        }

        [Fact]
        public async Task MoveSectionAsync_PreventsCircularReference()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var root = new DocumentSection { DocumentId = documentId, Title = "Root", Level = 1, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(root);
            await context.SaveChangesAsync();

            var sub1 = new DocumentSection { ParentSectionId = root.Id, Title = "Sub1", Level = 2, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(sub1);
            await context.SaveChangesAsync();

            var subSub1 = new DocumentSection { ParentSectionId = sub1.Id, Title = "SubSub1", Level = 3, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(subSub1);
            await context.SaveChangesAsync();

            // Act - Try to move root under its own descendant
            var result = await service.MoveSectionAsync(root.Id, subSub1.Id);

            // Assert
            Assert.False(result); // Should fail
        }

        #endregion

        #region Circular Reference Validation Tests

        [Fact]
        public async Task ValidateParentReferenceAsync_AcceptsValidParent()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var root1 = new DocumentSection { DocumentId = documentId, Title = "Root1", Level = 1, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            var root2 = new DocumentSection { DocumentId = documentId, Title = "Root2", Level = 1, SectionOrder = 2, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.AddRange(root1, root2);
            await context.SaveChangesAsync();

            // Act
            var result = await service.ValidateParentReferenceAsync(root2.Id, root1.Id);

            // Assert
            Assert.True(result); // Valid parent
        }

        [Fact]
        public async Task ValidateParentReferenceAsync_RejectsCircularReference()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var root = new DocumentSection { DocumentId = documentId, Title = "Root", Level = 1, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(root);
            await context.SaveChangesAsync();

            var sub1 = new DocumentSection { ParentSectionId = root.Id, Title = "Sub1", Level = 2, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(sub1);
            await context.SaveChangesAsync();

            // Act
            var result = await service.ValidateParentReferenceAsync(root.Id, sub1.Id);

            // Assert
            Assert.False(result); // Circular reference
        }

        [Fact]
        public async Task ValidateParentReferenceAsync_RejectsSelfReference()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var root = new DocumentSection { DocumentId = documentId, Title = "Root", Level = 1, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(root);
            await context.SaveChangesAsync();

            // Act
            var result = await service.ValidateParentReferenceAsync(root.Id, root.Id);

            // Assert
            Assert.False(result); // Self-reference
        }

        [Fact]
        public async Task ValidateParentReferenceAsync_AcceptsNullParent()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var root = new DocumentSection { DocumentId = documentId, Title = "Root", Level = 1, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(root);
            await context.SaveChangesAsync();

            // Act
            var result = await service.ValidateParentReferenceAsync(root.Id, null);

            // Assert
            Assert.True(result); // Null parent is valid
        }

        #endregion

        #region Section Number Generation Tests

        [Fact]
        public async Task GenerateSectionNumberAsync_GeneratesCorrectNumberForRootSection()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var root = new DocumentSection { DocumentId = documentId, Title = "Root", Level = 1, SectionOrder = 3, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(root);
            await context.SaveChangesAsync();

            // Act
            var number = await service.GenerateSectionNumberAsync(root.Id);

            // Assert
            Assert.Equal("3", number);
        }

        [Fact]
        public async Task GenerateSectionNumberAsync_GeneratesCorrectNumberForSubsection()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var root = new DocumentSection { DocumentId = documentId, Title = "Root", Level = 1, SectionOrder = 2, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(root);
            await context.SaveChangesAsync();

            var sub1 = new DocumentSection { ParentSectionId = root.Id, Title = "Sub1", Level = 2, SectionOrder = 4, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(sub1);
            await context.SaveChangesAsync();

            // Act
            var number = await service.GenerateSectionNumberAsync(sub1.Id);

            // Assert
            Assert.Equal("2.4", number);
        }

        [Fact]
        public async Task GenerateSectionNumberAsync_GeneratesCorrectNumberForThreeLevels()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var root = new DocumentSection { DocumentId = documentId, Title = "Root", Level = 1, SectionOrder = 3, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(root);
            await context.SaveChangesAsync();

            var sub1 = new DocumentSection { ParentSectionId = root.Id, Title = "Sub1", Level = 2, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(sub1);
            await context.SaveChangesAsync();

            var subSub1 = new DocumentSection { ParentSectionId = sub1.Id, Title = "SubSub1", Level = 3, SectionOrder = 2, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(subSub1);
            await context.SaveChangesAsync();

            // Act
            var number = await service.GenerateSectionNumberAsync(subSub1.Id);

            // Assert
            Assert.Equal("3.1.2", number);
        }

        #endregion

        #region Reorder Tests

        [Fact]
        public async Task ReorderSectionsAsync_ReordersSubsectionsWithinParent()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var root = new DocumentSection { DocumentId = documentId, Title = "Root", Level = 1, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(root);
            await context.SaveChangesAsync();

            var sub1 = new DocumentSection { ParentSectionId = root.Id, Title = "Sub1", Level = 2, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            var sub2 = new DocumentSection { ParentSectionId = root.Id, Title = "Sub2", Level = 2, SectionOrder = 2, CreatedAt = DateTime.UtcNow };
            var sub3 = new DocumentSection { ParentSectionId = root.Id, Title = "Sub3", Level = 2, SectionOrder = 3, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.AddRange(sub1, sub2, sub3);
            await context.SaveChangesAsync();

            // Reorder: sub3, sub1, sub2
            var newOrder = new List<int> { sub3.Id, sub1.Id, sub2.Id };

            // Act
            var result = await service.ReorderSectionsAsync(root.Id, newOrder);

            // Assert
            Assert.True(result);
            var reordered = await context.DocumentSections
                .Where(s => s.ParentSectionId == root.Id)
                .OrderBy(s => s.SectionOrder)
                .ToListAsync();

            Assert.Equal(sub3.Id, reordered[0].Id);
            Assert.Equal(1, reordered[0].SectionOrder);
            Assert.Equal(sub1.Id, reordered[1].Id);
            Assert.Equal(2, reordered[1].SectionOrder);
            Assert.Equal(sub2.Id, reordered[2].Id);
            Assert.Equal(3, reordered[2].SectionOrder);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task DeleteAsync_PreventsDeleteWhenSectionHasChildren()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var root = new DocumentSection { DocumentId = documentId, Title = "Root", Level = 1, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(root);
            await context.SaveChangesAsync();

            var sub1 = new DocumentSection { ParentSectionId = root.Id, Title = "Sub1", Level = 2, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(sub1);
            await context.SaveChangesAsync();

            // Act
            var result = await service.DeleteAsync(root.Id);

            // Assert
            Assert.False(result); // Should fail - has children
            Assert.NotNull(await context.DocumentSections.FindAsync(root.Id)); // Still exists
        }

        [Fact]
        public async Task DeleteAsync_AllowsDeleteOfLeafSection()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var root = new DocumentSection { DocumentId = documentId, Title = "Root", Level = 1, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(root);
            await context.SaveChangesAsync();

            var sub1 = new DocumentSection { ParentSectionId = root.Id, Title = "Sub1", Level = 2, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(sub1);
            await context.SaveChangesAsync();

            // Act
            var result = await service.DeleteAsync(sub1.Id);

            // Assert
            Assert.True(result); // Should succeed - leaf section
            Assert.Null(await context.DocumentSections.FindAsync(sub1.Id)); // Deleted
        }

        #endregion

        #region Requirement Count Tests

        [Fact]
        public async Task GetRequirementCountAsync_CountsDirectRequirements()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var section = new DocumentSection { DocumentId = documentId, Title = "Section", Level = 1, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(section);
            await context.SaveChangesAsync();

            // Add requirements
            for (int i = 0; i < 3; i++)
            {
                context.Requirements.Add(new Requirement
                {
                    Title = $"Req {i}",
                    SectionId = section.Id,
                    ProjectId = 1,
                    DocumentId = documentId,
                    Type = RequirementType.CRD,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync();

            // Act
            var count = await service.GetRequirementCountAsync(section.Id, recursive: false);

            // Assert
            Assert.Equal(3, count);
        }

        [Fact]
        public async Task GetRequirementCountAsync_CountsRecursively()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var root = new DocumentSection { DocumentId = documentId, Title = "Root", Level = 1, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(root);
            await context.SaveChangesAsync();

            var sub1 = new DocumentSection { ParentSectionId = root.Id, Title = "Sub1", Level = 2, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(sub1);
            await context.SaveChangesAsync();

            // Add 2 requirements to root
            for (int i = 0; i < 2; i++)
            {
                context.Requirements.Add(new Requirement
                {
                    Title = $"Root Req {i}",
                    SectionId = root.Id,
                    ProjectId = 1,
                    DocumentId = documentId,
                    Type = RequirementType.CRD,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Add 3 requirements to sub1
            for (int i = 0; i < 3; i++)
            {
                context.Requirements.Add(new Requirement
                {
                    Title = $"Sub Req {i}",
                    SectionId = sub1.Id,
                    ProjectId = 1,
                    DocumentId = documentId,
                    Type = RequirementType.CRD,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync();

            // Act
            var count = await service.GetRequirementCountAsync(root.Id, recursive: true);

            // Assert
            Assert.Equal(5, count); // 2 + 3
        }

        #endregion

        #region GetByIdAsync Extended Tests

        [Fact]
        public async Task GetByIdAsync_ReturnsRequirementCounts()
        {
            // Arrange
            var (context, documentId) = await SetupTestDocumentAsync();
            var service = new DocumentSectionService(context);

            var root = new DocumentSection { DocumentId = documentId, Title = "Root", Level = 1, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(root);
            await context.SaveChangesAsync();

            var sub1 = new DocumentSection { ParentSectionId = root.Id, Title = "Sub1", Level = 2, SectionOrder = 1, CreatedAt = DateTime.UtcNow };
            context.DocumentSections.Add(sub1);
            await context.SaveChangesAsync();

            // Add 1 requirement to root
            context.Requirements.Add(new Requirement
            {
                Title = "Root Req",
                SectionId = root.Id,
                ProjectId = 1,
                DocumentId = documentId,
                Type = RequirementType.CRD,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            });

            // Add 2 requirements to sub1
            for (int i = 0; i < 2; i++)
            {
                context.Requirements.Add(new Requirement
                {
                    Title = $"Sub Req {i}",
                    SectionId = sub1.Id,
                    ProjectId = 1,
                    DocumentId = documentId,
                    Type = RequirementType.CRD,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetByIdAsync(root.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.RequirementCount); // Direct only
            Assert.Equal(3, result.TotalRequirementCount); // Recursive
        }

        #endregion
    }
}
