using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using RqmtMgmtShared;

namespace backend.Services
{
    /// <summary>
    /// Service implementation for managing document sections using the database context.
    /// Provides CRUD operations for sections within requirement documents.
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

        /// <summary>
        /// Retrieves all sections for a specific document.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <returns>A list of sections for the specified document, ordered by SectionOrder.</returns>
        public async Task<List<DocumentSectionDto>> GetByDocumentIdAsync(int documentId)
        {
            var entities = await _context.DocumentSections
                .Where(ds => ds.DocumentId == documentId)
                .OrderBy(ds => ds.SectionOrder)
                .AsNoTracking()
                .ToListAsync();
            
            return entities.Select(EntityToDto).ToList();
        }

        /// <summary>
        /// Retrieves a document section by its ID.
        /// </summary>
        /// <param name="id">The ID of the document section.</param>
        /// <returns>The document section DTO if found, null otherwise.</returns>
        public async Task<DocumentSectionDto?> GetByIdAsync(int id)
        {
            var entity = await _context.DocumentSections
                .AsNoTracking()
                .FirstOrDefaultAsync(ds => ds.Id == id);
            
            return entity == null ? null : EntityToDto(entity);
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
                var entity = DtoToEntity(section);
                entity.CreatedAt = DateTime.UtcNow;
                
                // If no section order specified, set it to the next available order
                if (entity.SectionOrder == 0)
                {
                    var maxOrder = await _context.DocumentSections
                        .Where(ds => ds.DocumentId == entity.DocumentId)
                        .MaxAsync(ds => (int?)ds.SectionOrder) ?? 0;
                    entity.SectionOrder = maxOrder + 1;
                }
                
                _context.DocumentSections.Add(entity);
                await _context.SaveChangesAsync();
                
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

                // Update properties
                existingEntity.Title = section.Title;
                existingEntity.Description = section.Description;
                existingEntity.SectionOrder = section.SectionOrder;
                existingEntity.IsNotApplicable = section.IsNotApplicable;
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
        /// </summary>
        /// <param name="id">The ID of the document section to delete.</param>
        /// <returns>True if the deletion was successful, false otherwise.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var entity = await _context.DocumentSections.FindAsync(id);
                if (entity == null)
                    return false;

                _context.DocumentSections.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reorders sections within a document based on the provided section IDs.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <param name="sectionIds">The list of section IDs in the desired order.</param>
        /// <returns>True if the reordering was successful, false otherwise.</returns>
        public async Task<bool> ReorderSectionsAsync(int documentId, List<int> sectionIds)
        {
            try
            {
                var sections = await _context.DocumentSections
                    .Where(ds => ds.DocumentId == documentId && sectionIds.Contains(ds.Id))
                    .ToListAsync();

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
        /// Converts a DocumentSection entity to a DocumentSectionDto.
        /// </summary>
        /// <param name="entity">The DocumentSection entity to convert.</param>
        /// <returns>The converted DocumentSectionDto.</returns>
        private static DocumentSectionDto EntityToDto(DocumentSection entity)
        {
            return new DocumentSectionDto
            {
                Id = entity.Id,
                DocumentId = entity.DocumentId,
                Title = entity.Title,
                Description = entity.Description,
                SectionOrder = entity.SectionOrder,
                IsNotApplicable = entity.IsNotApplicable,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        /// <summary>
        /// Converts a DocumentSectionDto to a DocumentSection entity.
        /// </summary>
        /// <param name="dto">The DocumentSectionDto to convert.</param>
        /// <returns>The converted DocumentSection entity.</returns>
        private static DocumentSection DtoToEntity(DocumentSectionDto dto)
        {
            return new DocumentSection
            {
                Id = dto.Id,
                DocumentId = dto.DocumentId,
                Title = dto.Title,
                Description = dto.Description,
                SectionOrder = dto.SectionOrder,
                IsNotApplicable = dto.IsNotApplicable,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }
    }
}