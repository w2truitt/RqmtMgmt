using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using RqmtMgmtShared;

namespace backend.Services
{
    /// <summary>
    /// Service implementation for managing document sections using the database context.
    /// Provides CRUD operations and hierarchical management for sections within requirement documents.
    /// </summary>
    public class DocumentSectionService : IDocumentSectionService
    {
        private readonly RqmtMgmtDbContext _context;

        /// <summary>
        /// Initializes a new instance of the DocumentSectionService with the specified database context.
        /// </summary>
        /// <param name="context">The database context for document section operations.</param>
        public DocumentSectionService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        #region Basic CRUD Operations

        /// <summary>
        /// Retrieves all sections for a specific document (flat list, no hierarchy).
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <returns>A flat list of all sections for the specified document, ordered by SectionOrder.</returns>
        public async Task<List<DocumentSectionDto>> GetByDocumentIdAsync(int documentId)
        {
            // Get all sections that belong to this document (directly or through parent hierarchy)
            var allSections = await GetAllSectionsForDocumentAsync(documentId);
            return allSections.Select(EntityToDto).ToList();
        }

        /// <summary>
        /// Retrieves a document section by its ID.
        /// </summary>
        /// <param name="id">The ID of the document section.</param>
        /// <returns>The document section DTO if found, null otherwise.</returns>
        public async Task<DocumentSectionDto?> GetByIdAsync(int id)
        {
            var entity = await _context.DocumentSections
                .Include(ds => ds.ChildSections)
                .AsNoTracking()
                .FirstOrDefaultAsync(ds => ds.Id == id);
            
            if (entity == null) return null;

            var dto = EntityToDto(entity);
            
            // Calculate requirement counts
            dto.RequirementCount = await _context.Requirements
                .CountAsync(r => r.SectionId == id);
            dto.TotalRequirementCount = await GetRequirementCountAsync(id, recursive: true);
            
            return dto;
        }

        /// <summary>
        /// Creates a new document section.
        /// </summary>
        /// <param name="section">The document section DTO to create.</param>
        /// <returns>The created document section DTO with assigned ID, or null if creation failed.</returns>
        public async Task<DocumentSectionDto?> CreateAsync(DocumentSectionDto section)
        {
            try
            {
                // Validate parent reference
                if (section.ParentSectionId.HasValue)
                {
                    if (!await ValidateParentReferenceAsync(0, section.ParentSectionId.Value))
                        return null;
                }

                var entity = DtoToEntity(section);
                entity.CreatedAt = DateTime.UtcNow;
                
                // Determine level based on parent
                if (entity.ParentSectionId.HasValue)
                {
                    var parent = await _context.DocumentSections.FindAsync(entity.ParentSectionId.Value);
                    entity.Level = parent != null ? parent.Level + 1 : 1;
                }
                else
                {
                    entity.Level = 1;
                }
                
                // If no section order specified, set it to the next available order
                if (entity.SectionOrder == 0)
                {
                    int maxOrder;
                    if (entity.ParentSectionId.HasValue)
                    {
                        maxOrder = await _context.DocumentSections
                            .Where(ds => ds.ParentSectionId == entity.ParentSectionId)
                            .MaxAsync(ds => (int?)ds.SectionOrder) ?? 0;
                    }
                    else
                    {
                        maxOrder = await _context.DocumentSections
                            .Where(ds => ds.DocumentId == entity.DocumentId)
                            .MaxAsync(ds => (int?)ds.SectionOrder) ?? 0;
                    }
                    entity.SectionOrder = maxOrder + 1;
                }
                
                _context.DocumentSections.Add(entity);
                await _context.SaveChangesAsync();
                
                // Generate section number if not provided
                if (string.IsNullOrEmpty(entity.SectionNumber))
                {
                    entity.SectionNumber = await GenerateSectionNumberAsync(entity.Id);
                    await _context.SaveChangesAsync();
                }
                
                return EntityToDto(entity);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Updates an existing document section.
        /// </summary>
        /// <param name="section">The document section DTO with updated values.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        public async Task<bool> UpdateAsync(DocumentSectionDto section)
        {
            try
            {
                var existingEntity = await _context.DocumentSections.FindAsync(section.Id);
                if (existingEntity == null)
                    return false;

                // Validate parent reference if changed
                if (section.ParentSectionId != existingEntity.ParentSectionId)
                {
                    if (section.ParentSectionId.HasValue)
                    {
                        if (!await ValidateParentReferenceAsync(section.Id, section.ParentSectionId.Value))
                            return false;
                    }
                }

                // Update properties
                existingEntity.Title = section.Title;
                existingEntity.Description = section.Description;
                existingEntity.SectionOrder = section.SectionOrder;
                existingEntity.IsNotApplicable = section.IsNotApplicable;
                existingEntity.ParentSectionId = section.ParentSectionId;
                existingEntity.Level = section.Level;
                existingEntity.SectionNumber = section.SectionNumber;
                existingEntity.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes a document section by its ID.
        /// Will fail if section has child sections.
        /// </summary>
        /// <param name="id">The ID of the document section to delete.</param>
        /// <returns>True if the deletion was successful, false otherwise.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var entity = await _context.DocumentSections
                    .Include(ds => ds.ChildSections)
                    .FirstOrDefaultAsync(ds => ds.Id == id);
                    
                if (entity == null)
                    return false;

                // Check if section has children
                if (entity.ChildSections.Any())
                    return false; // Cannot delete section with children

                _context.DocumentSections.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Hierarchical Query Operations

        /// <summary>
        /// Retrieves the full section hierarchy for a document as a tree structure.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <returns>A hierarchical list of sections with children populated.</returns>
        public async Task<List<DocumentSectionDto>> GetSectionHierarchyAsync(int documentId)
        {
            var rootSections = await _context.DocumentSections
                .Where(ds => ds.DocumentId == documentId && ds.ParentSectionId == null)
                .OrderBy(ds => ds.SectionOrder)
                .AsNoTracking()
                .ToListAsync();

            var result = new List<DocumentSectionDto>();
            foreach (var root in rootSections)
            {
                var dto = await GetSectionWithChildrenAsync(root.Id, depth: -1);
                if (dto != null)
                    result.Add(dto);
            }

            return result;
        }

        /// <summary>
        /// Retrieves a section with its child sections to a specified depth.
        /// </summary>
        /// <param name="sectionId">The ID of the section.</param>
        /// <param name="depth">The depth to retrieve children (-1 for unlimited, 0 for none, 1+ for levels).</param>
        /// <returns>The section DTO with children populated.</returns>
        public async Task<DocumentSectionDto?> GetSectionWithChildrenAsync(int sectionId, int depth = -1)
        {
            var entity = await _context.DocumentSections
                .AsNoTracking()
                .FirstOrDefaultAsync(ds => ds.Id == sectionId);
                
            if (entity == null) return null;

            var dto = EntityToDto(entity);
            
            // Calculate requirement counts
            dto.RequirementCount = await _context.Requirements
                .CountAsync(r => r.SectionId == sectionId);
            dto.TotalRequirementCount = await GetRequirementCountAsync(sectionId, recursive: true);

            // Always initialize ChildSections
            dto.ChildSections = new List<DocumentSectionDto>();

            // Load children if depth allows
            if (depth != 0)
            {
                var children = await _context.DocumentSections
                    .Where(ds => ds.ParentSectionId == sectionId)
                    .OrderBy(ds => ds.SectionOrder)
                    .AsNoTracking()
                    .ToListAsync();

                foreach (var child in children)
                {
                    var childDto = await GetSectionWithChildrenAsync(child.Id, depth == -1 ? -1 : depth - 1);
                    if (childDto != null)
                        dto.ChildSections.Add(childDto);
                }
            }

            return dto;
        }

        /// <summary>
        /// Retrieves direct child sections of a parent section.
        /// </summary>
        /// <param name="parentSectionId">The ID of the parent section.</param>
        /// <returns>A list of direct child sections.</returns>
        public async Task<List<DocumentSectionDto>> GetChildSectionsAsync(int parentSectionId)
        {
            var entities = await _context.DocumentSections
                .Where(ds => ds.ParentSectionId == parentSectionId)
                .OrderBy(ds => ds.SectionOrder)
                .AsNoTracking()
                .ToListAsync();
            
            var result = new List<DocumentSectionDto>();
            foreach (var entity in entities)
            {
                var dto = EntityToDto(entity);
                dto.RequirementCount = await _context.Requirements
                    .CountAsync(r => r.SectionId == entity.Id);
                dto.TotalRequirementCount = await GetRequirementCountAsync(entity.Id, recursive: true);
                result.Add(dto);
            }

            return result;
        }

        /// <summary>
        /// Retrieves root-level sections for a document.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <returns>A list of root sections.</returns>
        public async Task<List<DocumentSectionDto>> GetRootSectionsAsync(int documentId)
        {
            var entities = await _context.DocumentSections
                .Where(ds => ds.DocumentId == documentId && ds.ParentSectionId == null)
                .OrderBy(ds => ds.SectionOrder)
                .AsNoTracking()
                .ToListAsync();
            
            return entities.Select(EntityToDto).ToList();
        }

        #endregion

        #region Hierarchy Management Operations

        /// <summary>
        /// Moves a section to a new parent (or document root) and optionally reorders it.
        /// </summary>
        /// <param name="sectionId">The ID of the section to move.</param>
        /// <param name="newParentId">The new parent section ID (null for document root).</param>
        /// <param name="newOrder">The new order position (null to append).</param>
        /// <returns>True if the move was successful, false otherwise.</returns>
        public async Task<bool> MoveSectionAsync(int sectionId, int? newParentId, int? newOrder = null)
        {
            try
            {
                var section = await _context.DocumentSections.FindAsync(sectionId);
                if (section == null)
                    return false;

                // Validate parent reference
                if (newParentId.HasValue)
                {
                    if (!await ValidateParentReferenceAsync(sectionId, newParentId.Value))
                        return false;
                }

                // Update parent and level
                section.ParentSectionId = newParentId;
                
                if (newParentId.HasValue)
                {
                    var parent = await _context.DocumentSections.FindAsync(newParentId.Value);
                    section.Level = parent != null ? parent.Level + 1 : 1;
                    section.DocumentId = null; // Subsections don't have direct document reference
                }
                else
                {
                    section.Level = 1;
                    // Keep existing DocumentId for root sections
                }

                // Update order
                if (newOrder.HasValue)
                {
                    section.SectionOrder = newOrder.Value;
                }
                else
                {
                    // Append to end
                    int maxOrder;
                    if (newParentId.HasValue)
                    {
                        maxOrder = await _context.DocumentSections
                            .Where(ds => ds.ParentSectionId == newParentId)
                            .MaxAsync(ds => (int?)ds.SectionOrder) ?? 0;
                    }
                    else
                    {
                        maxOrder = await _context.DocumentSections
                            .Where(ds => ds.DocumentId == section.DocumentId)
                            .MaxAsync(ds => (int?)ds.SectionOrder) ?? 0;
                    }
                    section.SectionOrder = maxOrder + 1;
                }

                section.UpdatedAt = DateTime.UtcNow;
                
                // Regenerate section number
                section.SectionNumber = await GenerateSectionNumberAsync(sectionId);

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reorders sections within a parent (or document root).
        /// </summary>
        /// <param name="parentId">The parent section ID (null for document root).</param>
        /// <param name="sectionIds">The list of section IDs in the desired order.</param>
        /// <returns>True if the reordering was successful, false otherwise.</returns>
        public async Task<bool> ReorderSectionsAsync(int? parentId, List<int> sectionIds)
        {
            try
            {
                List<DocumentSection> sections;
                
                if (parentId.HasValue)
                {
                    sections = await _context.DocumentSections
                        .Where(ds => ds.ParentSectionId == parentId && sectionIds.Contains(ds.Id))
                        .ToListAsync();
                }
                else
                {
                    // For root sections, need to know document ID - get from first section
                    var firstSection = await _context.DocumentSections
                        .FirstOrDefaultAsync(ds => sectionIds.Contains(ds.Id));
                    if (firstSection == null) return false;
                    
                    sections = await _context.DocumentSections
                        .Where(ds => ds.DocumentId == firstSection.DocumentId && 
                                    ds.ParentSectionId == null && 
                                    sectionIds.Contains(ds.Id))
                        .ToListAsync();
                }

                if (sections.Count != sectionIds.Count)
                    return false; // Some sections not found

                for (int i = 0; i < sectionIds.Count; i++)
                {
                    var section = sections.FirstOrDefault(s => s.Id == sectionIds[i]);
                    if (section != null)
                    {
                        section.SectionOrder = i + 1;
                        section.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates that a parent reference is valid (no circular references, parent exists).
        /// </summary>
        /// <param name="sectionId">The ID of the section being validated.</param>
        /// <param name="parentId">The proposed parent section ID.</param>
        /// <returns>True if the parent reference is valid, false otherwise.</returns>
        public async Task<bool> ValidateParentReferenceAsync(int sectionId, int? parentId)
        {
            if (!parentId.HasValue)
                return true; // Null parent is valid (root section)

            // Check for self-reference
            if (sectionId != 0 && parentId.Value == sectionId)
                return false;

            // Check if parent exists
            var parent = await _context.DocumentSections.FindAsync(parentId.Value);
            if (parent == null)
                return false;

            // Check for circular reference (parent cannot be descendant)
            if (sectionId != 0 && await IsDescendantAsync(parentId.Value, sectionId))
                return false;

            return true;
        }

        #endregion

        #region Utility Operations

        /// <summary>
        /// Generates a section number based on the section's position in the hierarchy.
        /// </summary>
        /// <param name="sectionId">The ID of the section.</param>
        /// <returns>The generated section number (e.g., "3.1.2").</returns>
        public async Task<string?> GenerateSectionNumberAsync(int sectionId)
        {
            var section = await _context.DocumentSections
                .Include(ds => ds.ParentSection)
                .FirstOrDefaultAsync(ds => ds.Id == sectionId);
                
            if (section == null)
                return null;

            var numbers = new List<int>();
            var current = section;

            while (current != null)
            {
                numbers.Insert(0, current.SectionOrder);
                
                if (current.ParentSectionId.HasValue)
                {
                    current = await _context.DocumentSections
                        .FirstOrDefaultAsync(ds => ds.Id == current.ParentSectionId.Value);
                }
                else
                {
                    current = null;
                }
            }

            return string.Join(".", numbers);
        }

        /// <summary>
        /// Gets the count of requirements in a section, optionally including child sections.
        /// </summary>
        /// <param name="sectionId">The ID of the section.</param>
        /// <param name="recursive">Whether to include requirements from child sections.</param>
        /// <returns>The count of requirements.</returns>
        public async Task<int> GetRequirementCountAsync(int sectionId, bool recursive = false)
        {
            if (!recursive)
            {
                return await _context.Requirements
                    .CountAsync(r => r.SectionId == sectionId);
            }

            // Recursive count - get all descendant sections
            var descendantIds = await GetDescendantSectionIdsAsync(sectionId);
            descendantIds.Add(sectionId);

            return await _context.Requirements
                .CountAsync(r => descendantIds.Contains(r.SectionId));
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Gets all sections belonging to a document, including subsections.
        /// </summary>
        private async Task<List<DocumentSection>> GetAllSectionsForDocumentAsync(int documentId)
        {
            // Get root sections
            var rootSections = await _context.DocumentSections
                .Where(ds => ds.DocumentId == documentId)
                .ToListAsync();

            var allSections = new List<DocumentSection>(rootSections);

            // Get all descendants
            foreach (var root in rootSections)
            {
                var descendants = await GetDescendantSectionsAsync(root.Id);
                allSections.AddRange(descendants);
            }

            return allSections.OrderBy(s => s.Level).ThenBy(s => s.SectionOrder).ToList();
        }

        /// <summary>
        /// Gets all descendant sections of a parent section recursively.
        /// </summary>
        private async Task<List<DocumentSection>> GetDescendantSectionsAsync(int parentId)
        {
            var children = await _context.DocumentSections
                .Where(ds => ds.ParentSectionId == parentId)
                .ToListAsync();

            var descendants = new List<DocumentSection>(children);

            foreach (var child in children)
            {
                var childDescendants = await GetDescendantSectionsAsync(child.Id);
                descendants.AddRange(childDescendants);
            }

            return descendants;
        }

        /// <summary>
        /// Gets all descendant section IDs of a parent section.
        /// </summary>
        private async Task<List<int>> GetDescendantSectionIdsAsync(int parentId)
        {
            var descendants = await GetDescendantSectionsAsync(parentId);
            return descendants.Select(d => d.Id).ToList();
        }

        /// <summary>
        /// Checks if a section is a descendant of another section.
        /// </summary>
        private async Task<bool> IsDescendantAsync(int potentialDescendantId, int ancestorId)
        {
            var section = await _context.DocumentSections.FindAsync(potentialDescendantId);
            
            while (section != null && section.ParentSectionId.HasValue)
            {
                if (section.ParentSectionId.Value == ancestorId)
                    return true;
                    
                section = await _context.DocumentSections.FindAsync(section.ParentSectionId.Value);
            }

            return false;
        }

        /// <summary>
        /// Converts a DocumentSection entity to a DocumentSectionDto.
        /// </summary>
        private static DocumentSectionDto EntityToDto(DocumentSection entity)
        {
            return new DocumentSectionDto
            {
                Id = entity.Id,
                DocumentId = entity.DocumentId,
                ParentSectionId = entity.ParentSectionId,
                Title = entity.Title,
                Description = entity.Description,
                SectionOrder = entity.SectionOrder,
                Level = entity.Level,
                SectionNumber = entity.SectionNumber,
                IsNotApplicable = entity.IsNotApplicable,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        /// <summary>
        /// Converts a DocumentSectionDto to a DocumentSection entity.
        /// </summary>
        private static DocumentSection DtoToEntity(DocumentSectionDto dto)
        {
            return new DocumentSection
            {
                Id = dto.Id,
                DocumentId = dto.DocumentId,
                ParentSectionId = dto.ParentSectionId,
                Title = dto.Title,
                Description = dto.Description,
                SectionOrder = dto.SectionOrder,
                Level = dto.Level,
                SectionNumber = dto.SectionNumber,
                IsNotApplicable = dto.IsNotApplicable,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }

        #endregion

        #region Search and Duplicate Detection Operations

        /// <summary>
        /// Searches for sections within a document using fuzzy matching.
        /// </summary>
        /// <param name="documentId">The ID of the document to search within.</param>
        /// <param name="searchTerm">The search term to match against section titles.</param>
        /// <param name="fuzzyMatch">Whether to use fuzzy matching or exact matching.</param>
        /// <param name="similarityThreshold">The minimum similarity threshold for fuzzy matching (0.0 to 1.0).</param>
        /// <returns>A list of matching sections ordered by relevance.</returns>
        public async Task<List<DocumentSectionDto>> SearchSectionsAsync(int documentId, string searchTerm, bool fuzzyMatch = true, double similarityThreshold = 0.8)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<DocumentSectionDto>();

            var allSections = await GetAllSectionsForDocumentAsync(documentId);
            var results = new List<(DocumentSection section, double score)>();

            foreach (var section in allSections)
            {
                if (string.IsNullOrWhiteSpace(section.Title))
                    continue;

                double score;
                if (fuzzyMatch)
                {
                    var normalizedSearchTerm = NormalizeTitle(searchTerm);
                    var normalizedSectionTitle = NormalizeTitle(section.Title);
                    score = CalculateStringSimilarity(normalizedSearchTerm, normalizedSectionTitle);
                    if (score >= similarityThreshold)
                    {
                        results.Add((section, score));
                    }
                }
                else
                {
                    if (section.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        score = section.Title.Equals(searchTerm, StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.8;
                        results.Add((section, score));
                    }
                }
            }

            return results
                .OrderByDescending(r => r.score)
                .ThenBy(r => r.section.Title)
                .Select(r => EntityToDto(r.section))
                .ToList();
        }

        /// <summary>
        /// Finds potential duplicate sections based on title similarity.
        /// </summary>
        /// <param name="documentId">The ID of the document to search within.</param>
        /// <param name="title">The title to find duplicates for.</param>
        /// <param name="similarityThreshold">The minimum similarity threshold (0.0 to 1.0).</param>
        /// <returns>A list of potential duplicate sections ordered by similarity.</returns>
        public async Task<List<DocumentSectionDto>> FindPotentialDuplicatesAsync(int documentId, string title, double similarityThreshold = 0.8)
        {
            if (string.IsNullOrWhiteSpace(title))
                return new List<DocumentSectionDto>();

            var allSections = await GetAllSectionsForDocumentAsync(documentId);
            var results = new List<(DocumentSection section, double score)>();

            var normalizedTitle = NormalizeTitle(title);

            foreach (var section in allSections)
            {
                if (string.IsNullOrWhiteSpace(section.Title))
                    continue;

                var normalizedSectionTitle = NormalizeTitle(section.Title);
                var similarity = CalculateStringSimilarity(normalizedTitle, normalizedSectionTitle);

                if (similarity >= similarityThreshold)
                {
                    results.Add((section, similarity));
                }
            }

            return results
                .OrderByDescending(r => r.score)
                .Select(r => EntityToDto(r.section))
                .ToList();
        }

        /// <summary>
        /// Finds an existing section that closely matches the given criteria.
        /// </summary>
        /// <param name="documentId">The ID of the document to search within.</param>
        /// <param name="title">The title to match.</param>
        /// <param name="parentId">The parent section ID to match (optional).</param>
        /// <param name="similarityThreshold">The minimum similarity threshold (0.0 to 1.0).</param>
        /// <returns>The best matching existing section, or null if none found.</returns>
        public async Task<DocumentSectionDto?> FindExistingSectionAsync(int documentId, string title, int? parentId = null, double similarityThreshold = 0.9)
        {
            if (string.IsNullOrWhiteSpace(title))
                return null;

            var allSections = await GetAllSectionsForDocumentAsync(documentId);
            var normalizedTitle = NormalizeTitle(title);
            
            DocumentSection? bestMatch = null;
            double bestScore = 0.0;

            foreach (var section in allSections)
            {
                // If parent ID is specified, only consider sections with matching parent
                if (parentId.HasValue && section.ParentSectionId != parentId)
                    continue;

                if (string.IsNullOrWhiteSpace(section.Title))
                    continue;

                var normalizedSectionTitle = NormalizeTitle(section.Title);
                var similarity = CalculateStringSimilarity(normalizedTitle, normalizedSectionTitle);

                if (similarity >= similarityThreshold && similarity > bestScore)
                {
                    bestMatch = section;
                    bestScore = similarity;
                }
            }

            return bestMatch != null ? EntityToDto(bestMatch) : null;
        }

        /// <summary>
        /// Calculates string similarity using Levenshtein distance algorithm.
        /// </summary>
        /// <param name="s1">First string to compare.</param>
        /// <param name="s2">Second string to compare.</param>
        /// <returns>Similarity score between 0.0 and 1.0.</returns>
        private static double CalculateStringSimilarity(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2))
                return 1.0;
            
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return 0.0;

            var maxLength = Math.Max(s1.Length, s2.Length);
            var distance = CalculateLevenshteinDistance(s1, s2);
            
            return 1.0 - (double)distance / maxLength;
        }

        /// <summary>
        /// Calculates the Levenshtein distance between two strings.
        /// </summary>
        /// <param name="s1">First string.</param>
        /// <param name="s2">Second string.</param>
        /// <returns>The Levenshtein distance.</returns>
        private static int CalculateLevenshteinDistance(string s1, string s2)
        {
            var matrix = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                matrix[i, 0] = i;
            
            for (int j = 0; j <= s2.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[s1.Length, s2.Length];
        }

        /// <summary>
        /// Normalizes a title for comparison by removing common prefixes, numbers, and extra whitespace.
        /// </summary>
        /// <param name="title">The title to normalize.</param>
        /// <returns>The normalized title.</returns>
        private static string NormalizeTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return string.Empty;

            // Convert to lowercase and trim
            var normalized = title.ToLowerInvariant().Trim();

            // Remove common section number patterns (e.g., "3.1", "3.1.1", etc.)
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"^\d+(\.\d+)*\s*", "");

            // Remove extra whitespace
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\s+", " ").Trim();

            return normalized;
        }

        #endregion
    }
}
