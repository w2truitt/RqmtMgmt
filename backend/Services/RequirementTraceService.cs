using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using RqmtMgmtShared;

namespace backend.Services
{
    /// <summary>
    /// Service implementation for managing requirement traces using the database context.
    /// Provides operations for creating and querying traceability links between requirements.
    /// </summary>
    public class RequirementTraceService : IRequirementTraceService
    {
        private readonly RqmtMgmtDbContext _context;

        /// <summary>
        /// Initializes a new instance of the RequirementTraceService with the specified database context.
        /// </summary>
        /// <param name="context">The database context for requirement trace operations.</param>
        public RequirementTraceService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all trace links where the specified requirement is the source.
        /// </summary>
        /// <param name="sourceRequirementId">The ID of the source requirement.</param>
        /// <returns>A list of trace links originating from the specified requirement.</returns>
        public async Task<List<RequirementTraceDto>> GetBySourceRequirementIdAsync(int sourceRequirementId)
        {
            var entities = await _context.RequirementTraces
                .Include(rt => rt.SourceRequirement)
                .Include(rt => rt.TargetRequirement)
                .Include(rt => rt.Creator)
                .Where(rt => rt.SourceRequirementId == sourceRequirementId)
                .AsNoTracking()
                .ToListAsync();
            
            return entities.Select(EntityToDto).ToList();
        }

        /// <summary>
        /// Retrieves all trace links where the specified requirement is the target.
        /// </summary>
        /// <param name="targetRequirementId">The ID of the target requirement.</param>
        /// <returns>A list of trace links pointing to the specified requirement.</returns>
        public async Task<List<RequirementTraceDto>> GetByTargetRequirementIdAsync(int targetRequirementId)
        {
            var entities = await _context.RequirementTraces
                .Include(rt => rt.SourceRequirement)
                .Include(rt => rt.TargetRequirement)
                .Include(rt => rt.Creator)
                .Where(rt => rt.TargetRequirementId == targetRequirementId)
                .AsNoTracking()
                .ToListAsync();
            
            return entities.Select(EntityToDto).ToList();
        }

        /// <summary>
        /// Retrieves the complete trace chain for a requirement (both incoming and outgoing traces).
        /// </summary>
        /// <param name="requirementId">The ID of the requirement.</param>
        /// <returns>A list of all trace links related to the specified requirement.</returns>
        public async Task<List<RequirementTraceDto>> GetTraceChainAsync(int requirementId)
        {
            var entities = await _context.RequirementTraces
                .Include(rt => rt.SourceRequirement)
                .Include(rt => rt.TargetRequirement)
                .Include(rt => rt.Creator)
                .Where(rt => rt.SourceRequirementId == requirementId || rt.TargetRequirementId == requirementId)
                .AsNoTracking()
                .ToListAsync();
            
            return entities.Select(EntityToDto).ToList();
        }

        /// <summary>
        /// Retrieves a requirement trace by its ID.
        /// </summary>
        /// <param name="id">The ID of the requirement trace.</param>
        /// <returns>The requirement trace DTO if found, null otherwise.</returns>
        public async Task<RequirementTraceDto?> GetByIdAsync(int id)
        {
            var entity = await _context.RequirementTraces
                .Include(rt => rt.SourceRequirement)
                .Include(rt => rt.TargetRequirement)
                .Include(rt => rt.Creator)
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Id == id);
            
            return entity == null ? null : EntityToDto(entity);
        }

        /// <summary>
        /// Creates a new requirement trace link.
        /// </summary>
        /// <param name="trace">The requirement trace DTO to create.</param>
        /// <returns>The created requirement trace DTO with assigned ID, or null if creation failed.</returns>
        public async Task<RequirementTraceDto?> CreateAsync(RequirementTraceDto trace)
        {
            try
            {
                // Validate the trace before creating
                var isValid = await ValidateTraceAsync(trace.SourceRequirementId, trace.TargetRequirementId, trace.TraceType);
                if (!isValid)
                    return null;

                var entity = DtoToEntity(trace);
                entity.CreatedAt = DateTime.UtcNow;
                
                _context.RequirementTraces.Add(entity);
                await _context.SaveChangesAsync();
                
                // Reload with includes
                var createdEntity = await _context.RequirementTraces
                    .Include(rt => rt.SourceRequirement)
                    .Include(rt => rt.TargetRequirement)
                    .Include(rt => rt.Creator)
                    .FirstOrDefaultAsync(rt => rt.Id == entity.Id);
                
                return createdEntity == null ? null : EntityToDto(createdEntity);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Deletes a requirement trace by its ID.
        /// </summary>
        /// <param name="id">The ID of the requirement trace to delete.</param>
        /// <returns>True if the deletion was successful, false otherwise.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var entity = await _context.RequirementTraces.FindAsync(id);
                if (entity == null)
                    return false;

                _context.RequirementTraces.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates whether a trace link can be created between two requirements.
        /// </summary>
        /// <param name="sourceRequirementId">The ID of the source requirement.</param>
        /// <param name="targetRequirementId">The ID of the target requirement.</param>
        /// <param name="traceType">The type of trace relationship.</param>
        /// <returns>True if the trace is valid, false otherwise.</returns>
        public async Task<bool> ValidateTraceAsync(int sourceRequirementId, int targetRequirementId, TraceType traceType)
        {
            // Check if both requirements exist
            var sourceExists = await _context.Requirements.AnyAsync(r => r.Id == sourceRequirementId);
            var targetExists = await _context.Requirements.AnyAsync(r => r.Id == targetRequirementId);
            
            if (!sourceExists || !targetExists)
                return false;

            // Prevent self-referencing traces
            if (sourceRequirementId == targetRequirementId)
                return false;

            // Check if trace already exists
            var existingTrace = await _context.RequirementTraces
                .AnyAsync(rt => rt.SourceRequirementId == sourceRequirementId && 
                               rt.TargetRequirementId == targetRequirementId && 
                               rt.TraceType == traceType);
            
            if (existingTrace)
                return false;

            // Check for circular references (basic check - could be enhanced for deeper cycles)
            var reverseTrace = await _context.RequirementTraces
                .AnyAsync(rt => rt.SourceRequirementId == targetRequirementId && 
                               rt.TargetRequirementId == sourceRequirementId);
            
            if (reverseTrace)
                return false;

            return true;
        }

        /// <summary>
        /// Converts a RequirementTrace entity to a RequirementTraceDto.
        /// </summary>
        /// <param name="entity">The RequirementTrace entity to convert.</param>
        /// <returns>The converted RequirementTraceDto.</returns>
        private static RequirementTraceDto EntityToDto(RequirementTrace entity)
        {
            return new RequirementTraceDto
            {
                Id = entity.Id,
                SourceRequirementId = entity.SourceRequirementId,
                TargetRequirementId = entity.TargetRequirementId,
                TraceType = entity.TraceType,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                SourceRequirement = entity.SourceRequirement == null ? null : new RequirementDto
                {
                    Id = entity.SourceRequirement.Id,
                    Title = entity.SourceRequirement.Title,
                    Type = entity.SourceRequirement.Type,
                    Status = entity.SourceRequirement.Status,
                    ProjectId = entity.SourceRequirement.ProjectId,
                    ProjectCode = entity.SourceRequirement.ProjectCode ?? string.Empty,
                    ProjectName = string.Empty, // Would need to include Project navigation
                    CreatedBy = entity.SourceRequirement.CreatedBy,
                    CreatedAt = entity.SourceRequirement.CreatedAt,
                    UpdatedAt = entity.SourceRequirement.UpdatedAt,
                    Version = entity.SourceRequirement.Version,
                    Description = entity.SourceRequirement.Description,
                    ParentId = entity.SourceRequirement.ParentId,
                    DocumentId = entity.SourceRequirement.DocumentId,
                    SectionId = entity.SourceRequirement.SectionId,
                    FullRequirementId = string.Empty // Would need calculation logic
                },
                TargetRequirement = entity.TargetRequirement == null ? null : new RequirementDto
                {
                    Id = entity.TargetRequirement.Id,
                    Title = entity.TargetRequirement.Title,
                    Type = entity.TargetRequirement.Type,
                    Status = entity.TargetRequirement.Status,
                    ProjectId = entity.TargetRequirement.ProjectId,
                    ProjectCode = entity.TargetRequirement.ProjectCode ?? string.Empty,
                    ProjectName = string.Empty, // Would need to include Project navigation
                    CreatedBy = entity.TargetRequirement.CreatedBy,
                    CreatedAt = entity.TargetRequirement.CreatedAt,
                    UpdatedAt = entity.TargetRequirement.UpdatedAt,
                    Version = entity.TargetRequirement.Version,
                    Description = entity.TargetRequirement.Description,
                    ParentId = entity.TargetRequirement.ParentId,
                    DocumentId = entity.TargetRequirement.DocumentId,
                    SectionId = entity.TargetRequirement.SectionId,
                    FullRequirementId = string.Empty // Would need calculation logic
                }
            };
        }

        /// <summary>
        /// Converts a RequirementTraceDto to a RequirementTrace entity.
        /// </summary>
        /// <param name="dto">The RequirementTraceDto to convert.</param>
        /// <returns>The converted RequirementTrace entity.</returns>
        private static RequirementTrace DtoToEntity(RequirementTraceDto dto)
        {
            return new RequirementTrace
            {
                Id = dto.Id,
                SourceRequirementId = dto.SourceRequirementId,
                TargetRequirementId = dto.TargetRequirementId,
                TraceType = dto.TraceType,
                CreatedAt = dto.CreatedAt,
                CreatedBy = dto.CreatedBy
            };
        }
    }
}