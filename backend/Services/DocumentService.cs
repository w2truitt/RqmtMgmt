using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using RqmtMgmtShared;

namespace backend.Services
{
    /// <summary>
    /// Service implementation for managing documents using the database context.
    /// Provides CRUD operations for requirement documents (CRD, PRD, SRS).
    /// </summary>
    public class DocumentService : IDocumentService
    {
        private readonly RqmtMgmtDbContext _context;

        /// <summary>
        /// Initializes a new instance of the DocumentService with the specified database context.
        /// </summary>
        /// <param name="context">The database context for document operations.</param>
        public DocumentService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all documents from the database.
        /// </summary>
        /// <returns>A list of all documents as DTOs.</returns>
        public async Task<List<DocumentDto>> GetAllAsync()
        {
            var entities = await _context.Documents
                .Include(d => d.Creator)
                .Include(d => d.Project)
                .AsNoTracking()
                .ToListAsync();
            
            return entities.Select(EntityToDto).ToList();
        }

        /// <summary>
        /// Retrieves documents with pagination, filtering, and sorting capabilities.
        /// </summary>
        /// <param name="parameters">Pagination parameters including page number, size, search term, and sorting options.</param>
        /// <returns>A paginated result containing documents and pagination metadata.</returns>
        public async Task<PagedResult<DocumentDto>> GetPagedAsync(PaginationParameters parameters)
        {
            var query = _context.Documents
                .Include(d => d.Creator)
                .Include(d => d.Project)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query = query.Where(d => d.Title.Contains(parameters.SearchTerm) ||
                                        (d.DocumentOwner != null && d.DocumentOwner.Contains(parameters.SearchTerm)) ||
                                        (d.Objective != null && d.Objective.Contains(parameters.SearchTerm)));
            }

            // Apply sorting
            query = parameters.SortBy?.ToLower() switch
            {
                "title" => parameters.SortDescending ? query.OrderByDescending(d => d.Title) : query.OrderBy(d => d.Title),
                "type" => parameters.SortDescending ? query.OrderByDescending(d => d.Type) : query.OrderBy(d => d.Type),
                "status" => parameters.SortDescending ? query.OrderByDescending(d => d.Status) : query.OrderBy(d => d.Status),
                "createdat" => parameters.SortDescending ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt),
                "updatedat" => parameters.SortDescending ? query.OrderByDescending(d => d.UpdatedAt) : query.OrderBy(d => d.UpdatedAt),
                _ => query.OrderByDescending(d => d.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<DocumentDto>
            {
                Items = items.Select(EntityToDto).ToList(),
                TotalItems = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        /// <summary>
        /// Retrieves all documents for a specific project.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <returns>A list of documents for the specified project.</returns>
        public async Task<List<DocumentDto>> GetByProjectIdAsync(int projectId)
        {
            var entities = await _context.Documents
                .Include(d => d.Creator)
                .Include(d => d.Project)
                .Where(d => d.ProjectId == projectId)
                .AsNoTracking()
                .ToListAsync();
            
            return entities.Select(EntityToDto).ToList();
        }

        /// <summary>
        /// Retrieves documents for a specific project with pagination.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="parameters">Pagination parameters.</param>
        /// <returns>A paginated result of documents for the specified project.</returns>
        public async Task<PagedResult<DocumentDto>> GetPagedByProjectIdAsync(int projectId, PaginationParameters parameters)
        {
            var query = _context.Documents
                .Include(d => d.Creator)
                .Include(d => d.Project)
                .Where(d => d.ProjectId == projectId)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query = query.Where(d => d.Title.Contains(parameters.SearchTerm) ||
                                        (d.DocumentOwner != null && d.DocumentOwner.Contains(parameters.SearchTerm)) ||
                                        (d.Objective != null && d.Objective.Contains(parameters.SearchTerm)));
            }

            // Apply sorting
            query = parameters.SortBy?.ToLower() switch
            {
                "title" => parameters.SortDescending ? query.OrderByDescending(d => d.Title) : query.OrderBy(d => d.Title),
                "type" => parameters.SortDescending ? query.OrderByDescending(d => d.Type) : query.OrderBy(d => d.Type),
                "status" => parameters.SortDescending ? query.OrderByDescending(d => d.Status) : query.OrderBy(d => d.Status),
                "createdat" => parameters.SortDescending ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt),
                "updatedat" => parameters.SortDescending ? query.OrderByDescending(d => d.UpdatedAt) : query.OrderBy(d => d.UpdatedAt),
                _ => query.OrderByDescending(d => d.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<DocumentDto>
            {
                Items = items.Select(EntityToDto).ToList(),
                TotalItems = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        /// <summary>
        /// Retrieves all documents of a specific type.
        /// </summary>
        /// <param name="type">The document type (CRD, PRD, SRS).</param>
        /// <returns>A list of documents of the specified type.</returns>
        public async Task<List<DocumentDto>> GetByTypeAsync(DocumentType type)
        {
            var entities = await _context.Documents
                .Include(d => d.Creator)
                .Include(d => d.Project)
                .Where(d => d.Type == type)
                .AsNoTracking()
                .ToListAsync();
            
            return entities.Select(EntityToDto).ToList();
        }

        /// <summary>
        /// Retrieves a document by its ID.
        /// </summary>
        /// <param name="id">The ID of the document.</param>
        /// <returns>The document DTO if found, null otherwise.</returns>
        public async Task<DocumentDto?> GetByIdAsync(int id)
        {
            var entity = await _context.Documents
                .Include(d => d.Creator)
                .Include(d => d.Project)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);
            
            return entity == null ? null : EntityToDto(entity);
        }

        /// <summary>
        /// Creates a new document.
        /// </summary>
        /// <param name="document">The document DTO to create.</param>
        /// <returns>The created document DTO with assigned ID, or null if creation failed.</returns>
        public async Task<DocumentDto?> CreateAsync(DocumentDto document)
        {
            try
            {
                var entity = DtoToEntity(document);
                entity.CreatedAt = DateTime.UtcNow;
                
                _context.Documents.Add(entity);
                await _context.SaveChangesAsync();
                
                // Reload with includes
                var createdEntity = await _context.Documents
                    .Include(d => d.Creator)
                    .Include(d => d.Project)
                    .FirstOrDefaultAsync(d => d.Id == entity.Id);
                
                return createdEntity == null ? null : EntityToDto(createdEntity);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Updates an existing document.
        /// </summary>
        /// <param name="document">The document DTO with updated values.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        public async Task<bool> UpdateAsync(DocumentDto document)
        {
            try
            {
                var existingEntity = await _context.Documents.FindAsync(document.Id);
                if (existingEntity == null)
                    return false;

                // Update properties
                existingEntity.Title = document.Title;
                existingEntity.Version = document.Version;
                existingEntity.DocumentOwner = document.DocumentOwner;
                existingEntity.Objective = document.Objective;
                existingEntity.Background = document.Background;
                existingEntity.InScope = document.InScope;
                existingEntity.OutOfScope = document.OutOfScope;
                existingEntity.UserPersonas = document.UserPersonas;
                existingEntity.SuccessCriteria = document.SuccessCriteria;
                existingEntity.Dependencies = document.Dependencies;
                existingEntity.AssumptionsConstraints = document.AssumptionsConstraints;
                existingEntity.Timeline = document.Timeline;
                existingEntity.Appendix = document.Appendix;
                existingEntity.Status = document.Status;
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
        /// Deletes a document by its ID.
        /// </summary>
        /// <param name="id">The ID of the document to delete.</param>
        /// <returns>True if the deletion was successful, false otherwise.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var entity = await _context.Documents.FindAsync(id);
                if (entity == null)
                    return false;

                _context.Documents.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Converts a Document entity to a DocumentDto.
        /// </summary>
        /// <param name="entity">The Document entity to convert.</param>
        /// <returns>The converted DocumentDto.</returns>
        private static DocumentDto EntityToDto(Document entity)
        {
            return new DocumentDto
            {
                Id = entity.Id,
                Type = entity.Type,
                Title = entity.Title,
                Version = entity.Version,
                DocumentOwner = entity.DocumentOwner,
                Objective = entity.Objective,
                Background = entity.Background,
                InScope = entity.InScope,
                OutOfScope = entity.OutOfScope,
                UserPersonas = entity.UserPersonas,
                SuccessCriteria = entity.SuccessCriteria,
                Dependencies = entity.Dependencies,
                AssumptionsConstraints = entity.AssumptionsConstraints,
                Timeline = entity.Timeline,
                Appendix = entity.Appendix,
                Status = entity.Status,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                ProjectId = entity.ProjectId,
                CreatedByUser = entity.Creator == null ? null : new UserDto
                {
                    Id = entity.Creator.Id,
                    UserName = entity.Creator.UserName,
                    Email = entity.Creator.Email
                }
            };
        }

        /// <summary>
        /// Converts a DocumentDto to a Document entity.
        /// </summary>
        /// <param name="dto">The DocumentDto to convert.</param>
        /// <returns>The converted Document entity.</returns>
        private static Document DtoToEntity(DocumentDto dto)
        {
            return new Document
            {
                Id = dto.Id,
                Type = dto.Type,
                Title = dto.Title,
                Version = dto.Version,
                DocumentOwner = dto.DocumentOwner,
                Objective = dto.Objective,
                Background = dto.Background,
                InScope = dto.InScope,
                OutOfScope = dto.OutOfScope,
                UserPersonas = dto.UserPersonas,
                SuccessCriteria = dto.SuccessCriteria,
                Dependencies = dto.Dependencies,
                AssumptionsConstraints = dto.AssumptionsConstraints,
                Timeline = dto.Timeline,
                Appendix = dto.Appendix,
                Status = dto.Status,
                CreatedBy = dto.CreatedBy,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                ProjectId = dto.ProjectId
            };
        }
        /// <summary>
        /// Retrieves traceability matrix data for a document showing requirement trace relationships.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <param name="direction">The trace direction: 'upstream' or 'downstream'.</param>
        /// <param name="uncoveredOnly">If true, returns only requirements without trace links.</param>
        /// <returns>Traceability matrix data for the document.</returns>
        public async Task<TraceabilityMatrixDto?> GetTraceabilityMatrixAsync(int documentId, string direction, bool uncoveredOnly = false)
        {
            // First, get the document to ensure it exists
            var document = await _context.Documents
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
                return null;

            // Get all requirements for this document
            var documentRequirements = await _context.Requirements
                .Where(r => r.DocumentId == documentId)
                .AsNoTracking()
                .ToListAsync();

            if (!documentRequirements.Any())
            {
                return new TraceabilityMatrixDto
                {
                    DocumentId = documentId,
                    DocumentName = document.Title,
                    DocumentType = document.Type,
                    Direction = direction,
                    Traceability = new List<RequirementTraceabilityDto>(),
                    CoverageStats = new CoverageStatsDto
                    {
                        TotalRequirements = 0,
                        CoveredRequirements = 0,
                        CoveragePercentage = 0,
                        UncoveredRequirements = new List<RequirementSummaryDto>()
                    }
                };
            }

            var requirementIds = documentRequirements.Select(r => r.Id).ToList();

            // Get trace relationships based on direction
            List<RequirementTrace> traces;
            if (direction == "downstream")
            {
                // Get traces FROM requirements in this document TO other requirements
                traces = await _context.RequirementTraces
                    .Include(rt => rt.SourceRequirement)
                    .Include(rt => rt.TargetRequirement)
                    .Where(rt => requirementIds.Contains(rt.SourceRequirementId))
                    .AsNoTracking()
                    .ToListAsync();
            }
            else // upstream
            {
                // Get traces FROM other requirements TO requirements in this document
                traces = await _context.RequirementTraces
                    .Include(rt => rt.SourceRequirement)
                    .Include(rt => rt.TargetRequirement)
                    .Where(rt => requirementIds.Contains(rt.TargetRequirementId))
                    .AsNoTracking()
                    .ToListAsync();
            }

            // Build traceability data
            var traceabilityList = new List<RequirementTraceabilityDto>();
            var coveredRequirementIds = new HashSet<int>();

            foreach (var requirement in documentRequirements)
            {
                var relevantTraces = direction == "downstream" 
                    ? traces.Where(t => t.SourceRequirementId == requirement.Id).ToList()
                    : traces.Where(t => t.TargetRequirementId == requirement.Id).ToList();

                if (relevantTraces.Any())
                    coveredRequirementIds.Add(requirement.Id);

                // Skip uncovered requirements if uncoveredOnly is false, or skip covered if uncoveredOnly is true
                if (uncoveredOnly && relevantTraces.Any()) continue;
                if (!uncoveredOnly || !relevantTraces.Any())
                {
                    var traceRelationships = relevantTraces.Select(trace => new TraceRelationshipDto
                    {
                        TraceId = trace.Id,
                        TraceType = trace.TraceType,
                        TargetRequirement = new RequirementSummaryDto
                        {
                            Id = direction == "downstream" ? trace.TargetRequirement!.Id : trace.SourceRequirement!.Id,
                            Title = direction == "downstream" ? trace.TargetRequirement!.Title : trace.SourceRequirement!.Title,
                            FullRequirementId = GenerateFullRequirementId(direction == "downstream" ? trace.TargetRequirement! : trace.SourceRequirement!),
                            Type = direction == "downstream" ? trace.TargetRequirement!.Type : trace.SourceRequirement!.Type,
                            DocumentId = direction == "downstream" ? trace.TargetRequirement!.DocumentId : trace.SourceRequirement!.DocumentId,
                            DocumentName = GetDocumentName(direction == "downstream" ? trace.TargetRequirement!.DocumentId : trace.SourceRequirement!.DocumentId)
                        }
                    }).ToList();

                    traceabilityList.Add(new RequirementTraceabilityDto
                    {
                        SourceRequirement = new RequirementSummaryDto
                        {
                            Id = requirement.Id,
                            Title = requirement.Title,
                            FullRequirementId = GenerateFullRequirementId(requirement),
                            Type = requirement.Type,
                            DocumentId = requirement.DocumentId,
                            DocumentName = document.Title
                        },
                        Traces = traceRelationships
                    });
                }
            }

            // Calculate coverage statistics
            var uncoveredRequirements = documentRequirements
                .Where(r => !coveredRequirementIds.Contains(r.Id))
                .Select(r => new RequirementSummaryDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    FullRequirementId = GenerateFullRequirementId(r),
                    Type = r.Type,
                    DocumentId = r.DocumentId,
                    DocumentName = document.Title
                }).ToList();

            var coverageStats = new CoverageStatsDto
            {
                TotalRequirements = documentRequirements.Count,
                CoveredRequirements = coveredRequirementIds.Count,
                CoveragePercentage = documentRequirements.Count > 0 
                    ? Math.Round((decimal)coveredRequirementIds.Count / documentRequirements.Count * 100, 1) 
                    : 0,
                UncoveredRequirements = uncoveredRequirements
            };

            return new TraceabilityMatrixDto
            {
                DocumentId = documentId,
                DocumentName = document.Title,
                DocumentType = document.Type,
                Direction = direction,
                Traceability = traceabilityList,
                CoverageStats = coverageStats
            };
        }

        /// <summary>
        /// Generates the full requirement ID (e.g., "PRD-001") for a requirement.
        /// </summary>
        private static string GenerateFullRequirementId(Requirement requirement)
        {
            // This is a simplified version - you may want to implement more sophisticated logic
            var prefix = requirement.Type.ToString().ToUpper();
            return $"{prefix}-{requirement.Id:D3}";
        }

        /// <summary>
        /// Gets the document name for a given document ID.
        /// </summary>
        private string GetDocumentName(int? documentId)
        {
            if (!documentId.HasValue) return string.Empty;
            
            var doc = _context.Documents
                .AsNoTracking()
                .FirstOrDefault(d => d.Id == documentId.Value);
            
            return doc?.Title ?? string.Empty;
        }
    }
}
