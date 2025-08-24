using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using RqmtMgmtShared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services
{
    /// <summary>
    /// Service implementation for managing requirements using the database context.
    /// Provides CRUD operations, versioning, and circular reference validation.
    /// </summary>
    public class RequirementService : RqmtMgmtShared.IRequirementService
    {
        private readonly RqmtMgmtDbContext _context;

        /// <summary>
        /// Initializes a new instance of the RequirementService with the specified database context.
        /// </summary>
        /// <param name="context">The database context for requirement operations.</param>
        public RequirementService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all requirements from the database.
        /// </summary>
        /// <returns>A list of all requirements as DTOs.</returns>
        public async Task<List<RequirementDto>> GetAllAsync()
        {
            var entities = await _context.Requirements.ToListAsync();
            return entities.Select(EntityToDto).ToList();
        }

        /// <summary>
        /// Retrieves requirements with pagination, filtering, and sorting capabilities.
        /// </summary>
        /// <param name="parameters">Pagination parameters including page number, size, search term, and sorting options.</param>
        /// <returns>A paginated result containing requirements and pagination metadata.</returns>
        public async Task<PagedResult<RequirementDto>> GetPagedAsync(PaginationParameters parameters)
        {
            var query = _context.Requirements.AsQueryable();

            // Apply filters
            query = ApplyFilters(query, parameters);

            // Apply sorting
            query = ApplySorting(query, parameters);

            // Get total count for pagination metadata
            var totalItems = await query.CountAsync();

            // Apply pagination
            var entities = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<RequirementDto>
            {
                Items = entities.Select(EntityToDto).ToList(),
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalItems = totalItems
            };
        }

        /// <summary>
        /// Applies filtering conditions to the requirements query.
        /// </summary>
        /// <param name="query">The base query to filter.</param>
        /// <param name="parameters">The pagination parameters containing filter criteria.</param>
        /// <returns>The filtered query.</returns>
        private static IQueryable<Requirement> ApplyFilters(IQueryable<Requirement> query, PaginationParameters parameters)
        {
            query = ApplyProjectFilter(query, parameters.ProjectId);
            query = ApplySearchFilter(query, parameters.SearchTerm);
            return query;
        }

        /// <summary>
        /// Applies project filtering to the requirements query.
        /// </summary>
        /// <param name="query">The base query to filter.</param>
        /// <param name="projectId">The project ID to filter by, if provided.</param>
        /// <returns>The filtered query.</returns>
        private static IQueryable<Requirement> ApplyProjectFilter(IQueryable<Requirement> query, int? projectId)
        {
            if (projectId.HasValue)
            {
                query = query.Where(r => r.ProjectId == projectId.Value);
            }
            return query;
        }

        /// <summary>
        /// Applies search filtering to the requirements query.
        /// </summary>
        /// <param name="query">The base query to filter.</param>
        /// <param name="searchTerm">The search term to filter by, if provided.</param>
        /// <returns>The filtered query.</returns>
        private static IQueryable<Requirement> ApplySearchFilter(IQueryable<Requirement> query, string? searchTerm)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchTermLower = searchTerm.ToLower();
                query = query.Where(r => r.Title.ToLower().Contains(searchTermLower) || 
                                        (r.Description != null && r.Description.ToLower().Contains(searchTermLower)));
            }
            return query;
        }

        /// <summary>
        /// Applies sorting to the requirements query based on the specified parameters.
        /// </summary>
        /// <param name="query">The base query to sort.</param>
        /// <param name="parameters">The pagination parameters containing sorting criteria.</param>
        /// <returns>The sorted query.</returns>
        private static IQueryable<Requirement> ApplySorting(IQueryable<Requirement> query, PaginationParameters parameters)
        {
            var sortBy = parameters.SortBy?.ToLower();
            var isDescending = parameters.SortDescending;

            return sortBy switch
            {
                "title" => ApplyTitleSorting(query, isDescending),
                "status" => ApplyStatusSorting(query, isDescending),
                "type" => ApplyTypeSorting(query, isDescending),
                "createdat" => ApplyCreatedAtSorting(query, isDescending),
                "updatedat" => ApplyUpdatedAtSorting(query, isDescending),
                _ => ApplyDefaultSorting(query, isDescending) // Default sort by ID
            };
        }

        /// <summary>
        /// Applies title-based sorting to the query.
        /// </summary>
        /// <param name="query">The query to sort.</param>
        /// <param name="isDescending">Whether to sort in descending order.</param>
        /// <returns>The sorted query.</returns>
        private static IQueryable<Requirement> ApplyTitleSorting(IQueryable<Requirement> query, bool isDescending)
        {
            return isDescending ? query.OrderByDescending(r => r.Title) : query.OrderBy(r => r.Title);
        }

        /// <summary>
        /// Applies status-based sorting to the query.
        /// </summary>
        /// <param name="query">The query to sort.</param>
        /// <param name="isDescending">Whether to sort in descending order.</param>
        /// <returns>The sorted query.</returns>
        private static IQueryable<Requirement> ApplyStatusSorting(IQueryable<Requirement> query, bool isDescending)
        {
            return isDescending ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status);
        }

        /// <summary>
        /// Applies type-based sorting to the query.
        /// </summary>
        /// <param name="query">The query to sort.</param>
        /// <param name="isDescending">Whether to sort in descending order.</param>
        /// <returns>The sorted query.</returns>
        private static IQueryable<Requirement> ApplyTypeSorting(IQueryable<Requirement> query, bool isDescending)
        {
            return isDescending ? query.OrderByDescending(r => r.Type) : query.OrderBy(r => r.Type);
        }

        /// <summary>
        /// Applies creation date-based sorting to the query.
        /// </summary>
        /// <param name="query">The query to sort.</param>
        /// <param name="isDescending">Whether to sort in descending order.</param>
        /// <returns>The sorted query.</returns>
        private static IQueryable<Requirement> ApplyCreatedAtSorting(IQueryable<Requirement> query, bool isDescending)
        {
            return isDescending ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt);
        }

        /// <summary>
        /// Applies update date-based sorting to the query.
        /// </summary>
        /// <param name="query">The query to sort.</param>
        /// <param name="isDescending">Whether to sort in descending order.</param>
        /// <returns>The sorted query.</returns>
        private static IQueryable<Requirement> ApplyUpdatedAtSorting(IQueryable<Requirement> query, bool isDescending)
        {
            return isDescending ? query.OrderByDescending(r => r.UpdatedAt) : query.OrderBy(r => r.UpdatedAt);
        }

        /// <summary>
        /// Applies default ID-based sorting to the query.
        /// </summary>
        /// <param name="query">The query to sort.</param>
        /// <param name="isDescending">Whether to sort in descending order.</param>
        /// <returns>The sorted query.</returns>
        private static IQueryable<Requirement> ApplyDefaultSorting(IQueryable<Requirement> query, bool isDescending)
        {
            return isDescending ? query.OrderByDescending(r => r.Id) : query.OrderBy(r => r.Id);
        }

        /// <summary>
        /// Retrieves all requirements for a specific project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <returns>A list of requirements for the specified project.</returns>
        public async Task<List<RequirementDto>> GetByProjectIdAsync(int projectId)
        {
            var entities = await _context.Requirements
                .Where(r => r.ProjectId == projectId)
                .ToListAsync();
            return entities.Select(EntityToDto).ToList();
        }

        /// <summary>
        /// Retrieves requirements for a specific project with pagination, filtering, and sorting capabilities.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <param name="parameters">Pagination parameters including page number, size, search term, and sorting options.</param>
        /// <returns>A paginated result containing requirements for the specified project and pagination metadata.</returns>
        public async Task<PagedResult<RequirementDto>> GetPagedByProjectIdAsync(int projectId, PaginationParameters parameters)
        {
            var query = _context.Requirements
                .Where(r => r.ProjectId == projectId)
                .AsQueryable();

            // Apply search filter (project filter is already applied above)
            query = ApplySearchFilter(query, parameters.SearchTerm);

            // Apply sorting
            query = ApplySorting(query, parameters);

            // Get total count for pagination metadata
            var totalItems = await query.CountAsync();

            // Apply pagination
            var entities = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<RequirementDto>
            {
                Items = entities.Select(EntityToDto).ToList(),
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalItems = totalItems
            };
        }

        /// <summary>
        /// Retrieves a specific requirement by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement.</param>
        /// <returns>The requirement DTO if found; otherwise, null.</returns>
        public async Task<RequirementDto?> GetByIdAsync(int id)
        {
            var entity = await _context.Requirements.FindAsync(id);
            return entity == null ? null : EntityToDto(entity);
        }

        /// <summary>
        /// Creates a new requirement with validation and initial versioning.
        /// </summary>
        /// <param name="dto">The requirement data to create.</param>
        /// <returns>The created requirement DTO if successful; otherwise, null.</returns>
        public async Task<RequirementDto?> CreateAsync(RequirementDto dto)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.Title))
                return null;
            
            if (dto.CreatedBy <= 0)
                return null;
            
            // Check for circular reference if ParentId is provided
            if (dto.ParentId.HasValue && await WouldCreateCircularReference(dto.Id, dto.ParentId.Value))
                return null;

            var entity = DtoToEntity(dto);
            _context.Requirements.Add(entity);
            await _context.SaveChangesAsync();

            // Save initial version
            var version = new RequirementVersion
            {
                RequirementId = entity.Id,
                Version = 1,
                Type = entity.Type,
                Title = entity.Title,
                Description = entity.Description,
                ParentId = entity.ParentId,
                Status = entity.Status,
                ModifiedBy = entity.CreatedBy,
                ModifiedAt = entity.CreatedAt
            };
            _context.RequirementVersions.Add(version);
            await _context.SaveChangesAsync();
            return EntityToDto(entity);
        }

        /// <summary>
        /// Updates an existing requirement with validation, versioning, and circular reference checking.
        /// </summary>
        /// <param name="dto">The requirement data to update.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(RequirementDto dto)
        {
            var entity = await _context.Requirements.FindAsync(dto.Id);
            if (entity == null)
                return false;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.Title))
                return false;
            
            if (dto.CreatedBy <= 0)
                return false;
            
            // Check for circular reference if ParentId is provided
            if (dto.ParentId.HasValue && await WouldCreateCircularReference(dto.Id, dto.ParentId.Value))
                return false;

            // Save current state as new version BEFORE updating
            int nextVersion = await _context.RequirementVersions.CountAsync(v => v.RequirementId == entity.Id) + 1;
            var version = new RequirementVersion
            {
                RequirementId = entity.Id,
                Version = nextVersion,
                Type = entity.Type,
                Title = entity.Title,
                Description = entity.Description,
                ParentId = entity.ParentId,
                Status = entity.Status,
                ModifiedBy = dto.CreatedBy, // Use the user making the update
                ModifiedAt = DateTime.UtcNow
            };
            _context.RequirementVersions.Add(version);

            // Update entity from DTO (don't change CreatedBy and CreatedAt)
            entity.Type = dto.Type;
            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.ParentId = dto.ParentId;
            entity.Status = dto.Status;
            entity.Version = nextVersion;
            entity.UpdatedAt = DateTime.UtcNow;

            // Save all changes in one transaction
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Deletes a requirement by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var req = await _context.Requirements.FindAsync(id);
            if (req == null) return false;
            _context.Requirements.Remove(req);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves all versions of a specific requirement for audit and change tracking.
        /// </summary>
        /// <param name="requirementId">The unique identifier of the requirement.</param>
        /// <returns>A list of requirement versions ordered by version number.</returns>
        public async Task<List<RequirementVersionDto>> GetVersionsAsync(int requirementId)
        {
            var versions = await _context.RequirementVersions
                .Where(v => v.RequirementId == requirementId)
                .OrderBy(v => v.Version)
                .ToListAsync();
            return versions.Select(EntityToVersionDto).ToList();
        }

        /// <summary>
        /// Checks if setting a parent would create a circular reference in the requirement hierarchy.
        /// Uses depth-first traversal with cycle detection to prevent infinite loops.
        /// </summary>
        /// <param name="requirementId">The ID of the requirement being updated.</param>
        /// <param name="parentId">The proposed parent ID.</param>
        /// <returns>True if a circular reference would be created; otherwise, false.</returns>
        private async Task<bool> WouldCreateCircularReference(int requirementId, int parentId)
        {
            // Check if parentId is a descendant of requirementId
            var current = parentId;
            var visited = new HashSet<int>();
            
            while (current != 0 && !visited.Contains(current))
            {
                if (current == requirementId)
                    return true;
                
                visited.Add(current);
                var parent = await _context.Requirements.FindAsync(current);
                current = parent?.ParentId ?? 0;
            }
            
            return false;
        }

        /// <summary>
        /// Converts a Requirement entity to a RequirementDto for API responses.
        /// </summary>
        /// <param name="r">The requirement entity to convert.</param>
        /// <returns>A RequirementDto with all properties mapped.</returns>
        private static RequirementDto EntityToDto(Requirement r) => new()
        {
            Id = r.Id,
            Type = r.Type,
            Title = r.Title,
            Description = r.Description,
            ParentId = r.ParentId,
            Status = r.Status,
            Version = r.Version,
            CreatedBy = r.CreatedBy,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            ProjectId = r.ProjectId
        };

        /// <summary>
        /// Converts a RequirementVersion entity to a RequirementVersionDto for API responses.
        /// </summary>
        /// <param name="v">The requirement version entity to convert.</param>
        /// <returns>A RequirementVersionDto with all properties mapped.</returns>
        private static RequirementVersionDto EntityToVersionDto(RequirementVersion v) => new()
        {
            Id = v.Id,
            RequirementId = v.RequirementId,
            Version = v.Version,
            Title = v.Title,
            Description = v.Description,
            Type = v.Type,
            Status = v.Status,
            ModifiedBy = v.ModifiedBy,
            ModifiedAt = v.ModifiedAt
        };

        /// <summary>
        /// Converts a RequirementDto to a Requirement entity for database operations.
        /// </summary>
        /// <param name="d">The requirement DTO to convert.</param>
        /// <returns>A Requirement entity with all properties mapped.</returns>
        private static Requirement DtoToEntity(RequirementDto d) => new()
        {
            Id = d.Id,
            Type = d.Type,
            Title = d.Title,
            Description = d.Description,
            ParentId = d.ParentId,
            Status = d.Status,
            Version = d.Version,
            CreatedBy = d.CreatedBy,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt,
            ProjectId = d.ProjectId
        };
    }
}