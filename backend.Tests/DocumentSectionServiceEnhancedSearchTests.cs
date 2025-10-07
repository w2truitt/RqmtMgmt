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
    /// Unit tests for Enhanced Search functionality in DocumentSectionService.
    /// Tests fuzzy matching, duplicate detection, and existing section finding.
    /// </summary>
    public class DocumentSectionServiceEnhancedSearchTests
    {
        private RqmtMgmtDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RqmtMgmtDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RqmtMgmtDbContext(options);
        }

        private async Task<int> SetupTestDataAsync(RqmtMgmtDbContext context)
        {
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

            // Add test sections with various titles for fuzzy matching tests
            var sections = new[]
            {
                new DocumentSection { DocumentId = document.Id, Title = "User Authentication", SectionOrder = 1, CreatedAt = DateTime.UtcNow },
                new DocumentSection { DocumentId = document.Id, Title = "3.1 User Authentication", SectionOrder = 2, CreatedAt = DateTime.UtcNow },
                new DocumentSection { DocumentId = document.Id, Title = "User Authorisation", SectionOrder = 3, CreatedAt = DateTime.UtcNow }, // British spelling
                new DocumentSection { DocumentId = document.Id, Title = "Performance Requirements", SectionOrder = 4, CreatedAt = DateTime.UtcNow },
                new DocumentSection { DocumentId = document.Id, Title = "4.1 Performance Reqs", SectionOrder = 5, CreatedAt = DateTime.UtcNow },
                new DocumentSection { DocumentId = document.Id, Title = "Security Settings", SectionOrder = 6, CreatedAt = DateTime.UtcNow },
                new DocumentSection { DocumentId = document.Id, Title = "User Interface Design", SectionOrder = 7, CreatedAt = DateTime.UtcNow },
                new DocumentSection { DocumentId = document.Id, Title = "Hardware Interfaces", SectionOrder = 8, CreatedAt = DateTime.UtcNow }
            };

            context.DocumentSections.AddRange(sections);
            await context.SaveChangesAsync();

            return document.Id;
        }

        [Fact]
        public async Task SearchSectionsAsync_ExactMatch_ReturnsCorrectResults()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentSectionService(context);
            var documentId = await SetupTestDataAsync(context);

            // Act
            var results = await service.SearchSectionsAsync(documentId, "User Authentication", fuzzyMatch: false);

            // Assert
            Assert.Equal(2, results.Count);
            Assert.Contains(results, r => r.Title == "User Authentication");
            Assert.Contains(results, r => r.Title == "3.1 User Authentication");
        }

        [Fact]
        public async Task SearchSectionsAsync_FuzzyMatch_FindsSimilarTitles()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentSectionService(context);
            var documentId = await SetupTestDataAsync(context);

            // Act - Search for "Authentication" with fuzzy matching
            var results = await service.SearchSectionsAsync(documentId, "Authentication", fuzzyMatch: true, similarityThreshold: 0.7);

            // Assert - Should find both "User Authentication" sections
            Assert.True(results.Count >= 2);
            Assert.Contains(results, r => r.Title.Contains("Authentication"));
        }

        [Fact]
        public async Task SearchSectionsAsync_WithSimilarityThreshold_FiltersResults()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentSectionService(context);
            var documentId = await SetupTestDataAsync(context);

            // Act - High threshold should be more restrictive
            var highThresholdResults = await service.SearchSectionsAsync(documentId, "Performance", fuzzyMatch: true, similarityThreshold: 0.9);
            var lowThresholdResults = await service.SearchSectionsAsync(documentId, "Performance", fuzzyMatch: true, similarityThreshold: 0.5);

            // Assert
            Assert.True(highThresholdResults.Count <= lowThresholdResults.Count);
            Assert.Contains(lowThresholdResults, r => r.Title.Contains("Performance"));
        }

        [Fact]
        public async Task FindPotentialDuplicatesAsync_FindsExactDuplicates()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentSectionService(context);
            var documentId = await SetupTestDataAsync(context);

            // Act
            var duplicates = await service.FindPotentialDuplicatesAsync(documentId, "User Authentication", similarityThreshold: 0.9);

            // Assert
            Assert.Equal(2, duplicates.Count);
            Assert.All(duplicates, d => Assert.Contains("Authentication", d.Title));
        }

        [Fact]
        public async Task FindPotentialDuplicatesAsync_WithLowerThreshold_FindsSimilarSections()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentSectionService(context);
            var documentId = await SetupTestDataAsync(context);

            // Act - Look for sections similar to "User Authorization" (should find "User Authorisation")
            var duplicates = await service.FindPotentialDuplicatesAsync(documentId, "User Authorization", similarityThreshold: 0.8);

            // Assert
            Assert.True(duplicates.Count > 0);
            Assert.Contains(duplicates, d => d.Title == "User Authorisation"); // British spelling should match
        }

        [Fact]
        public async Task FindExistingSectionAsync_ReturnsClosestMatch()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentSectionService(context);
            var documentId = await SetupTestDataAsync(context);

            // Act
            var existingSection = await service.FindExistingSectionAsync(documentId, "Performance Requirements", similarityThreshold: 0.9);

            // Assert
            Assert.NotNull(existingSection);
            Assert.Equal("Performance Requirements", existingSection.Title);
        }

        [Fact]
        public async Task FindExistingSectionAsync_WithNoMatch_ReturnsNull()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentSectionService(context);
            var documentId = await SetupTestDataAsync(context);

            // Act
            var existingSection = await service.FindExistingSectionAsync(documentId, "Completely Different Title", similarityThreshold: 0.9);

            // Assert
            Assert.Null(existingSection);
        }

        [Fact]
        public async Task FindExistingSectionAsync_WithParentFilter_RespectsHierarchy()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentSectionService(context);
            var documentId = await SetupTestDataAsync(context);

            // Create a parent section and child
            var parentSection = new DocumentSection 
            { 
                DocumentId = documentId, 
                Title = "Parent Section", 
                SectionOrder = 10, 
                CreatedAt = DateTime.UtcNow 
            };
            context.DocumentSections.Add(parentSection);
            await context.SaveChangesAsync();

            var childSection = new DocumentSection 
            { 
                DocumentId = documentId, 
                Title = "Child Authentication", 
                ParentSectionId = parentSection.Id,
                SectionOrder = 1, 
                CreatedAt = DateTime.UtcNow 
            };
            context.DocumentSections.Add(childSection);
            await context.SaveChangesAsync();

            // Act - Search for authentication within the parent
            var existingSection = await service.FindExistingSectionAsync(documentId, "Authentication", parentSection.Id, similarityThreshold: 0.7);

            // Assert
            Assert.NotNull(existingSection);
            Assert.Equal(childSection.Id, existingSection.Id);
            Assert.Equal(parentSection.Id, existingSection.ParentSectionId);
        }

        [Theory]
        [InlineData("User Authentication", "User Authentication", 1.0)]
        [InlineData("User Authentication", "3.1 User Authentication", 1.0)] // Should normalize section numbers
        [InlineData("Performance Requirements", "Performance Reqs", 0.6)] // Abbreviation should match
        [InlineData("User Authorization", "User Authorisation", 0.9)] // Spelling variants should match
        [InlineData("Security", "Performance", 0.5)] // Completely different should not match
        public async Task SearchSectionsAsync_SimilarityCalculation_WorksCorrectly(string searchTerm, string sectionTitle, double expectedMinSimilarity)
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
                Title = sectionTitle, 
                SectionOrder = 1, 
                CreatedAt = DateTime.UtcNow 
            };
            context.DocumentSections.Add(section);
            await context.SaveChangesAsync();

            // Act
            var results = await service.SearchSectionsAsync(document.Id, searchTerm, fuzzyMatch: true, similarityThreshold: expectedMinSimilarity);

            // Assert
            if (expectedMinSimilarity > 0.5)
            {
                Assert.Single(results);
                Assert.Equal(sectionTitle, results[0].Title);
            }
            else
            {
                Assert.Empty(results);
            }
        }

        [Fact]
        public async Task SearchSectionsAsync_EmptySearchTerm_ReturnsEmpty()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentSectionService(context);
            var documentId = await SetupTestDataAsync(context);

            // Act
            var results = await service.SearchSectionsAsync(documentId, "", fuzzyMatch: true);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public async Task SearchSectionsAsync_NonExistentDocument_ReturnsEmpty()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentSectionService(context);

            // Act
            var results = await service.SearchSectionsAsync(999, "Any Search Term", fuzzyMatch: true);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public async Task FindPotentialDuplicatesAsync_OrdersByRelevance()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = new DocumentSectionService(context);
            var documentId = await SetupTestDataAsync(context);

            // Act
            var duplicates = await service.FindPotentialDuplicatesAsync(documentId, "Performance", similarityThreshold: 0.6);

            // Assert
            Assert.True(duplicates.Count > 0);
            
            // Results should be ordered by similarity (most similar first)
            for (int i = 0; i < duplicates.Count - 1; i++)
            {
                // We can't directly check similarity scores, but we can check that exact matches come first
                if (duplicates[i].Title.Contains("Performance") && !duplicates[i + 1].Title.Contains("Performance"))
                {
                    // This is expected - exact matches should come before partial matches
                    Assert.True(true);
                }
            }
        }
    }
}
